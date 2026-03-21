#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Orchestrates the Deep Zoom rendering pipeline: viewport management, tile scheduling,
/// cache management, and rendering.
/// </summary>
/// <remarks>
/// <para>
/// This class handles only <em>tile and rendering</em> concerns. Animation, spring physics,
/// and gesture recognition belong in the consuming layer.
/// </para>
/// <para>
/// Typical usage:
/// <list type="number">
///   <item><description>Call <see cref="Load(ISKImagePyramidSource, ISKImagePyramidTileProvider)"/> to load an image.</description></item>
///   <item><description>Call <see cref="SetControlSize"/> when the canvas size changes.</description></item>
///   <item><description>Call <see cref="Update"/> to schedule tile loads each frame.</description></item>
///   <item><description>Call <see cref="Render(ISKImagePyramidRenderer)"/> from your canvas paint callback, passing the renderer for this frame.</description></item>
/// </list>
/// </para>
/// </remarks>
public class SKImagePyramidController : IDisposable
{
    private ISKImagePyramidSource? _tileSource;
    private readonly SKImagePyramidViewport _viewport;
    private readonly SKImagePyramidTileLayout _tileLayout;
    private readonly SKImagePyramidMemoryTileCache _renderBuffer;
    private List<SKImagePyramidSubImage> _subImages = new List<SKImagePyramidSubImage>();
    private ISKImagePyramidTileProvider? _provider;
    private readonly ConcurrentDictionary<SKImagePyramidTileId, byte> _pendingTiles = new ConcurrentDictionary<SKImagePyramidTileId, byte>();
    private readonly ConcurrentDictionary<SKImagePyramidTileId, byte> _failedTiles = new ConcurrentDictionary<SKImagePyramidTileId, byte>();
    private CancellationTokenSource? _cts;
    private volatile bool _disposed;
    private bool _userHasZoomed;
    private IReadOnlyList<SKImagePyramidTileRequest>? _visibleTilesCache;

    /// <summary>
    /// Initializes a new <see cref="SKImagePyramidController"/>.
    /// </summary>
    public SKImagePyramidController()
    {
        _renderBuffer = new SKImagePyramidMemoryTileCache(256);
        _viewport = new SKImagePyramidViewport();
        _tileLayout = new SKImagePyramidTileLayout();
    }

    // ---- Properties ----

    /// <summary>The current viewport.</summary>
    public SKImagePyramidViewport Viewport => _viewport;

    /// <summary>
    /// The internal render buffer holding decoded tiles for the current frame.
    /// This is the controller's in-memory hot cache for the rendering loop.
    /// </summary>
    public SKImagePyramidMemoryTileCache Cache => _renderBuffer;

    /// <summary>The tile layout calculator.</summary>
    public SKImagePyramidTileLayout TileLayout => _tileLayout;

    /// <summary>The loaded tile source, or null if not loaded.</summary>
    public ISKImagePyramidSource? TileSource => _tileSource;

    /// <summary>The sub-images from the loaded DZC, or empty if not loaded from a DZC.</summary>
    public IReadOnlyList<SKImagePyramidSubImage> SubImages => _subImages;

    /// <summary>The aspect ratio of the loaded image (width/height). 0 if not loaded.</summary>
    public double AspectRatio => _tileSource?.AspectRatio ?? 0;

    /// <summary>Whether the controller has no pending tile loads.</summary>
    public bool IsIdle => _pendingTiles.IsEmpty;

    /// <summary>Number of tile fetches currently in flight.</summary>
    public int PendingTileCount => _pendingTiles.Count;

    /// <summary>
    /// Whether to draw lower-resolution fallback tiles while hi-res tiles are loading
    /// (LOD blending). Default is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Enabled (default):</strong> blurry ancestor tiles fill the screen
    /// immediately as you zoom in; they sharpen progressively. Best for interactive use.</para>
    /// <para><strong>Disabled:</strong> missing tiles show as blank until loaded.
    /// Preferred for scientific/medical imaging where blurry placeholders could be misleading.</para>
    /// </remarks>
    public bool EnableLodBlending { get; set; } = true;

    // ---- Events ----

    /// <summary>Fired when the image source is loaded successfully.</summary>
    public event EventHandler? ImageOpenSucceeded;

    /// <summary>Fired when the image source fails to load.</summary>
    public event EventHandler<Exception>? ImageOpenFailed;

    /// <summary>Fired when the viewport position or zoom level changes.</summary>
    public event EventHandler? ViewportChanged;

    /// <summary>Fired when a tile fails to load.</summary>
    public event EventHandler<SKImagePyramidTileFailedEventArgs>? TileFailed;

    /// <summary>Fired when new tiles are loaded and the view needs repainting.</summary>
    public event EventHandler? InvalidateRequired;

    /// <summary>
    /// Fired when a DZC collection is successfully loaded and <see cref="SubImages"/> is populated.
    /// Use this event to know when the collection is ready. To render a specific image from the
    /// collection, call <see cref="Load(ISKImagePyramidSource,ISKImagePyramidTileProvider)"/>
    /// with one of the sub-image sources from <see cref="SubImages"/>.
    /// </summary>
    public event EventHandler? CollectionOpenSucceeded;

    // ---- Provider replacement ----

    /// <summary>
    /// Replaces the active tile provider without disturbing the loaded image, viewport state,
    /// or zoom level. All pending tile loads are cancelled and in-flight tiles are discarded;
    /// the render buffer is cleared and tiles are re-fetched as the view is repainted.
    /// The old provider is disposed by this method.
    /// </summary>
    /// <param name="newProvider">
    /// The new provider to adopt. The controller takes ownership — it will be disposed when
    /// <see cref="Dispose"/> is called or <see cref="ReplaceProvider"/> is called again.
    /// </param>
    public void ReplaceProvider(ISKImagePyramidTileProvider newProvider)
    {
        if (newProvider == null) throw new ArgumentNullException(nameof(newProvider));

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _pendingTiles.Clear();
        _failedTiles.Clear();
        _visibleTilesCache = null;

        var oldProvider = _provider;
        _provider = newProvider;
        oldProvider?.Dispose();

        _renderBuffer.Clear();

        InvalidateRequired?.Invoke(this, EventArgs.Empty);
    }

    // ---- Load ----

    /// <summary>Loads a DZI or IIIF tile source. Resets the viewport to show the full image.</summary>
    public void Load(ISKImagePyramidSource tileSource, ISKImagePyramidTileProvider provider)
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _failedTiles.Clear();
            _renderBuffer.Clear();
            _subImages.Clear();
            _visibleTilesCache = null;

            if (_provider != null && !ReferenceEquals(_provider, provider))
                _provider.Dispose();

            _tileSource = tileSource;
            _provider = provider;

            _viewport.AspectRatio = tileSource.AspectRatio;
            _viewport.ControlWidth = _viewport.ControlWidth > 0 ? _viewport.ControlWidth : 800;
            _viewport.ControlHeight = _viewport.ControlHeight > 0 ? _viewport.ControlHeight : 600;
            _userHasZoomed = false;
            _viewport.FitToView();

            ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ImageOpenFailed?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// Loads a DZC collection source. Populates <see cref="SubImages"/> with the items in
    /// the collection. Does NOT set a renderable tile source — listen for
    /// <see cref="CollectionOpenSucceeded"/> and then call
    /// <see cref="Load(ISKImagePyramidSource,ISKImagePyramidTileProvider)"/> with a specific
    /// sub-image source to actually render.
    /// </summary>
    public void Load(SKImagePyramidDziCollectionSource dzcTileSource, ISKImagePyramidTileProvider provider)
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _failedTiles.Clear();
            _renderBuffer.Clear();
            _visibleTilesCache = null;

            // _tileSource stays null — DZC is a collection, not a single renderable source.
            // Callers should call Load(subImage.Source, provider) to render a specific sub-image.
            _tileSource = null;
            _subImages = new List<SKImagePyramidSubImage>();
            foreach (var item in dzcTileSource.Items)
            {
                var sub = new SKImagePyramidSubImage(item.Id, item.MortonIndex, item.AspectRatio, item.Source)
                {
                    ViewportWidth = item.ViewportWidth,
                    ViewportOriginX = item.ViewportX,
                    ViewportOriginY = item.ViewportY,
                };
                _subImages.Add(sub);
            }

            if (_provider != null && !ReferenceEquals(_provider, provider))
                _provider.Dispose();

            _provider = provider;

            _viewport.ControlWidth = _viewport.ControlWidth > 0 ? _viewport.ControlWidth : 800;
            _viewport.ControlHeight = _viewport.ControlHeight > 0 ? _viewport.ControlHeight : 600;
            _userHasZoomed = false;
            _viewport.ViewportOriginX = 0;
            _viewport.ViewportOriginY = 0;
            _viewport.ViewportWidth = 1.0;

            CollectionOpenSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ImageOpenFailed?.Invoke(this, ex);
        }
    }

    // ---- Viewport ----

    /// <summary>
    /// Sets the control (canvas) size. Refits the viewport when the user has not manually zoomed.
    /// </summary>
    public void SetControlSize(double width, double height)
    {
        bool sizeChanged = Math.Abs(_viewport.ControlWidth - width) > 0.5 ||
                           Math.Abs(_viewport.ControlHeight - height) > 0.5;

        _viewport.ControlWidth = width;
        _viewport.ControlHeight = height;

        if (sizeChanged && _tileSource != null && !_userHasZoomed)
            _viewport.FitToView();
    }

    /// <summary>Sets the viewport directly and constrains it.</summary>
    public void SetViewport(double viewportWidth, double originX, double originY)
    {
        _viewport.ViewportWidth = viewportWidth;
        _viewport.ViewportOriginX = originX;
        _viewport.ViewportOriginY = originY;
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Zooms about a logical point. Factor &gt; 1 zooms in, &lt; 1 zooms out.</summary>
    public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
    {
        _userHasZoomed = true;
        _viewport.ZoomAboutLogicalPoint(factor, logicalX, logicalY);
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Zooms about a screen-space point.</summary>
    public void ZoomAboutScreenPoint(double factor, double screenX, double screenY)
    {
        _userHasZoomed = true;
        var (lx, ly) = _viewport.ElementToLogicalPoint(screenX, screenY);
        _viewport.ZoomAboutLogicalPoint(factor, lx, ly);
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Pans by the given screen-space delta.</summary>
    public void Pan(double deltaScreenX, double deltaScreenY)
    {
        _userHasZoomed = true;
        _viewport.PanByScreenDelta(deltaScreenX, deltaScreenY);
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Resets the viewport to show the entire image.</summary>
    public void ResetView()
    {
        _userHasZoomed = false;
        _viewport.FitToView();
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Sets an absolute zoom level (1.0 = image fills the control width).</summary>
    public void SetZoom(double zoom)
    {
        if (zoom <= 0) throw new ArgumentOutOfRangeException(nameof(zoom));
        _userHasZoomed = true;
        var cx = _viewport.ControlWidth / 2.0;
        var cy = _viewport.ControlHeight / 2.0;
        var (lx, ly) = _viewport.ElementToLogicalPoint(cx, cy);
        double newViewportWidth = Math.Max(SKImagePyramidViewport.MinViewportWidth, 1.0 / zoom);
        double factor = _viewport.ViewportWidth / newViewportWidth;
        _viewport.ZoomAboutLogicalPoint(factor, lx, ly);
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Returns the logical rectangle visible at the current viewport state.</summary>
    public Rect<double> GetZoomRect()
        => _viewport.GetZoomRect(_viewport.ViewportWidth);

    /// <summary>
    /// The zoom level at which one image pixel maps to exactly one screen pixel.
    /// Returns 0 if no image is loaded.
    /// </summary>
    public double NativeZoom =>
        (_tileSource != null && _viewport.ControlWidth > 0)
            ? (double)_tileSource.ImageWidth / _viewport.ControlWidth
            : 0.0;

    // ---- Tile loading ----

    /// <summary>
    /// Schedules loading for visible tiles.
    /// Call from your render loop on every frame.
    /// </summary>
    /// <returns><see langword="true"/> if tile loads are still in progress.</returns>
    public bool Update()
    {
        _visibleTilesCache = null;
        if (_tileSource != null && _provider != null)
        {
            _visibleTilesCache = _tileLayout.GetVisibleTiles(_tileSource, _viewport);
            ScheduleTileLoads(_visibleTilesCache);
        }

        return _pendingTiles.Count > 0;
    }

    // ---- Rendering ----

    /// <summary>
    /// Executes the two-pass LOD rendering pipeline using the provided renderer.
    /// The renderer is transient — it is only used during this call and should not be
    /// stored by the controller. This allows the renderer (and its canvas) to be created
    /// and discarded per frame, which is required by some graphics APIs.
    /// </summary>
    /// <param name="renderer">
    /// The renderer to draw with. Must be fully initialized (e.g. canvas set)
    /// before calling this method. The renderer is responsible for its own lifecycle.
    /// </param>
    public void Render(ISKImagePyramidRenderer renderer)
    {
        if (_tileSource == null) return;

        _renderBuffer.FlushEvicted();

        // Reuse visible-tiles list computed by Update() in the same frame; compute fresh if stale.
        var visibleTiles = _visibleTilesCache ?? _tileLayout.GetVisibleTiles(_tileSource, _viewport);

        renderer.BeginRender();

        // Pass 1: LOD fallback tiles (blurry placeholder while hi-res tiles load)
        if (EnableLodBlending)
        {
            foreach (var request in visibleTiles)
            {
                var tileId = request.TileId;
                if (!_renderBuffer.Contains(tileId))
                {
                    var fallback = _tileLayout.FindBestFallback(tileId, _renderBuffer);
                    if (fallback.HasValue)
                    {
                        _renderBuffer.TryGet(fallback.Value, out SKImagePyramidTile? parentTile);
                        if (parentTile != null)
                        {
                            var src  = _tileLayout.GetFallbackSourceRect(tileId, fallback.Value, _tileSource);
                            var dest = _tileLayout.GetTileDestRect(_tileSource, _viewport, tileId);
                            renderer.DrawFallbackTile(dest, src, parentTile);
                        }
                    }
                }
            }
        }

        // Pass 2: Hi-res tiles (blank spaces when LOD blending is off)
        foreach (var request in visibleTiles)
        {
            var tileId = request.TileId;
            _renderBuffer.TryGet(tileId, out SKImagePyramidTile? tile);
            if (tile != null)
            {
                var dest = _tileLayout.GetTileDestRect(_tileSource, _viewport, tileId);
                renderer.DrawTile(dest, tile);
            }
        }

        renderer.EndRender();
    }

    // ---- Private ----

    private void ScheduleTileLoads(IReadOnlyList<SKImagePyramidTileRequest> visibleTiles)
    {
        if (_tileSource == null || _provider == null) return;

        var ct = _cts?.Token ?? CancellationToken.None;

        foreach (var request in visibleTiles)
        {
            var tileId = request.TileId;
            if (_renderBuffer.Contains(tileId) || _pendingTiles.ContainsKey(tileId) || _failedTiles.ContainsKey(tileId))
                continue;

            _pendingTiles.TryAdd(tileId, 0);
            try
            {
                _ = LoadTileAsync(tileId, ct);
            }
            catch
            {
                _pendingTiles.TryRemove(tileId, out _);
            }
        }
    }

    private async Task LoadTileAsync(SKImagePyramidTileId tileId, CancellationToken ct)
    {
        SKImagePyramidTile? tile = null;
        try
        {
            // Capture source and provider locally — they may be replaced while this task is in flight
            var source = _tileSource;
            var provider = _provider;
            if (source == null || provider == null) return;

            // Skip if another concurrent task already loaded this tile
            if (_renderBuffer.TryGet(tileId, out tile) && tile != null)
            {
                if (!_disposed && !ct.IsCancellationRequested)
                    InvalidateRequired?.Invoke(this, EventArgs.Empty);
                tile = null; // already owned by render buffer — don't dispose in finally
                return;
            }
            tile = null;

            if (ct.IsCancellationRequested) return;

            string? url = source.GetFullTileUrl(tileId.Level, tileId.Col, tileId.Row);
            if (url == null) return;

            // Provider owns the full fetch+persist pipeline (HTTP fetch, disk cache, browser storage, etc.)
            tile = await provider.GetTileAsync(url, ct).ConfigureAwait(false);

            // null means the provider could not obtain the tile (e.g. HTTP 404) — don't retry
            if (tile == null)
            {
                _failedTiles.TryAdd(tileId, 0);
                return;
            }

            // Stamp the source identity for introspection/debugging
            tile.SourceId = source.SourceId;

            if (!ct.IsCancellationRequested && !_disposed)
            {
                _renderBuffer.Put(tileId, tile);
                tile = null; // ownership transferred to render buffer
                if (!_disposed && !ct.IsCancellationRequested)
                    InvalidateRequired?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            TileFailed?.Invoke(this, new SKImagePyramidTileFailedEventArgs(tileId, ex));
        }
        finally
        {
            tile?.Dispose();
            _pendingTiles.TryRemove(tileId, out _);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();
        _provider?.Dispose();
        _renderBuffer.Dispose();
    }
}
