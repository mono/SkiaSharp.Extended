#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// Orchestrates the image pyramid rendering pipeline: viewport management, tile scheduling,
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
///   <item><description>Call <see cref="SetProvider"/> to configure the tile fetch/cache pipeline.</description></item>
///   <item><description>Call <see cref="Load(ISKImagePyramidSource)"/> to load an image.</description></item>
///   <item><description>Call <see cref="SetControlSize"/> when the canvas size changes.</description></item>
///   <item><description>Call <see cref="Update"/> to schedule tile loads each frame.</description></item>
///   <item><description>Call <see cref="Render(ISKImagePyramidRenderer)"/> from your canvas paint callback.</description></item>
/// </list>
/// </para>
/// </remarks>
public class SKImagePyramidController : IDisposable
{
    private readonly SKImagePyramidViewport _viewport;
    private readonly SKImagePyramidTileLayout _tileLayout;
    private readonly ISKImagePyramidTileCache _renderBuffer;
    private readonly TileFailureTracker _failures;
    private List<SKImagePyramidSubImage> _subImages = new List<SKImagePyramidSubImage>();
    private readonly ConcurrentDictionary<SKImagePyramidTileId, byte> _pendingTiles = new ConcurrentDictionary<SKImagePyramidTileId, byte>();
    private CancellationTokenSource? _cts;
    private volatile bool _disposed;
    private bool _userHasZoomed;
    private IReadOnlyList<SKImagePyramidTileRequest>? _visibleTilesCache;

    /// <summary>
    /// Initializes a new <see cref="SKImagePyramidController"/>.
    /// </summary>
    /// <param name="memoryCache">
    /// Optional in-memory tile cache for the render loop. If <see langword="null"/>,
    /// a default <see cref="SKImagePyramidMemoryTileCache"/> with 256 entries is created.
    /// The controller owns the cache lifecycle and disposes it on <see cref="Dispose"/>.
    /// </param>
    public SKImagePyramidController(ISKImagePyramidTileCache? memoryCache = null)
    {
        _renderBuffer = memoryCache ?? new SKImagePyramidMemoryTileCache(256);
        _viewport = new SKImagePyramidViewport();
        _tileLayout = new SKImagePyramidTileLayout();
        _failures = new TileFailureTracker();
    }

    // ---- Properties ----

    /// <summary>The current viewport.</summary>
    public SKImagePyramidViewport Viewport => _viewport;

    /// <summary>
    /// The in-memory render cache. Exposed for observability (count, diagnostics).
    /// </summary>
    public ISKImagePyramidTileCache Cache => _renderBuffer;

    /// <summary>The tile layout calculator.</summary>
    public SKImagePyramidTileLayout TileLayout => _tileLayout;

    /// <summary>The active tile source, or null if not loaded.</summary>
    public ISKImagePyramidSource? Source { get; private set; }

    /// <summary>The active tile provider, or null if not set.</summary>
    public ISKImagePyramidTileProvider? Provider { get; private set; }

    /// <summary>The sub-images from the loaded DZC, or empty if not loaded from a DZC.</summary>
    public IReadOnlyList<SKImagePyramidSubImage> SubImages => _subImages;

    /// <summary>The aspect ratio of the loaded image (width/height). 0 if not loaded.</summary>
    public double AspectRatio => Source?.AspectRatio ?? 0;

    /// <summary>Whether the controller has no pending tile loads.</summary>
    public bool IsIdle => _pendingTiles.IsEmpty;

    /// <summary>Number of tile fetches currently in flight.</summary>
    public int PendingTileCount => _pendingTiles.Count;

    /// <summary>
    /// Whether to draw lower-resolution fallback tiles while hi-res tiles are loading
    /// (LOD blending). Default is <see langword="true"/>.
    /// </summary>
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
    /// </summary>
    public event EventHandler? CollectionOpenSucceeded;

    // ---- Provider ----

    /// <summary>
    /// Sets the tile provider (fetch + cache pipeline). Cancels pending loads, clears
    /// failed tiles and render buffer, and fires <see cref="InvalidateRequired"/>.
    /// The controller does NOT own the provider lifecycle — the caller manages disposal.
    /// </summary>
    public void SetProvider(ISKImagePyramidTileProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _pendingTiles.Clear();
        _failures.ResetAll();
        _visibleTilesCache = null;

        Provider = provider;
        _renderBuffer.Clear();

        InvalidateRequired?.Invoke(this, EventArgs.Empty);
    }

    // ---- Load ----

    /// <summary>
    /// Loads a tile source. Resets the viewport to show the full image.
    /// <see cref="Provider"/> must be set first via <see cref="SetProvider"/>.
    /// </summary>
    public void Load(ISKImagePyramidSource source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _failures.ResetAll();
            _renderBuffer.Clear();
            _subImages.Clear();
            _visibleTilesCache = null;

            Source = source;

            _viewport.AspectRatio = source.AspectRatio;
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
    /// Convenience: sets the provider and loads a source in one call.
    /// </summary>
    public void Load(ISKImagePyramidSource source, ISKImagePyramidTileProvider provider)
    {
        SetProvider(provider);
        Load(source);
    }

    /// <summary>
    /// Loads a DZC collection source. Populates <see cref="SubImages"/> with the items in
    /// the collection. Does NOT set a renderable tile source — listen for
    /// <see cref="CollectionOpenSucceeded"/> and then call
    /// <see cref="Load(ISKImagePyramidSource)"/> with a specific sub-image source to render.
    /// </summary>
    public void Load(SKImagePyramidDziCollectionSource dzcTileSource, ISKImagePyramidTileProvider provider)
    {
        try
        {
            SetProvider(provider);

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _failures.ResetAll();
            _renderBuffer.Clear();
            _visibleTilesCache = null;

            Source = null;
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

    // ---- Failure management ----

    /// <summary>
    /// Clears all failure state, allowing failed tiles to be re-fetched.
    /// Call after network recovery or when retrying is appropriate.
    /// </summary>
    public void RetryFailedTiles()
    {
        _failures.ResetAll();
        InvalidateRequired?.Invoke(this, EventArgs.Empty);
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

        if (sizeChanged && Source != null && !_userHasZoomed)
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
        (Source != null && _viewport.ControlWidth > 0)
            ? (double)Source.ImageWidth / _viewport.ControlWidth
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
        if (Source != null && Provider != null)
        {
            _visibleTilesCache = _tileLayout.GetVisibleTiles(Source, _viewport);
            ScheduleTileLoads(_visibleTilesCache);
        }

        return _pendingTiles.Count > 0;
    }

    // ---- Rendering ----

    /// <summary>
    /// Executes the two-pass LOD rendering pipeline using the provided renderer.
    /// </summary>
    public void Render(ISKImagePyramidRenderer renderer)
    {
        if (Source == null) return;

        _renderBuffer.FlushEvicted();

        var visibleTiles = _visibleTilesCache ?? _tileLayout.GetVisibleTiles(Source, _viewport);

        renderer.BeginRender();

        // Pass 1: LOD fallback tiles
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
                            var src  = _tileLayout.GetFallbackSourceRect(tileId, fallback.Value, Source);
                            var dest = _tileLayout.GetTileDestRect(Source, _viewport, tileId);
                            renderer.DrawFallbackTile(dest, src, parentTile);
                        }
                    }
                }
            }
        }

        // Pass 2: Hi-res tiles
        foreach (var request in visibleTiles)
        {
            var tileId = request.TileId;
            _renderBuffer.TryGet(tileId, out SKImagePyramidTile? tile);
            if (tile != null)
            {
                var dest = _tileLayout.GetTileDestRect(Source, _viewport, tileId);
                renderer.DrawTile(dest, tile);
            }
        }

        renderer.EndRender();
    }

    // ---- Private ----

    private void ScheduleTileLoads(IReadOnlyList<SKImagePyramidTileRequest> visibleTiles)
    {
        if (Source == null || Provider == null) return;

        var ct = _cts?.Token ?? CancellationToken.None;

        foreach (var request in visibleTiles)
        {
            var tileId = request.TileId;
            if (_renderBuffer.Contains(tileId) || _pendingTiles.ContainsKey(tileId) || _failures.ShouldSkip(tileId))
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
            var source = Source;
            var provider = Provider;
            if (source == null || provider == null) return;

            if (_renderBuffer.TryGet(tileId, out tile) && tile != null)
            {
                if (!_disposed && !ct.IsCancellationRequested)
                    InvalidateRequired?.Invoke(this, EventArgs.Empty);
                tile = null;
                return;
            }
            tile = null;

            if (ct.IsCancellationRequested) return;

            string? url = source.GetFullTileUrl(tileId.Level, tileId.Col, tileId.Row);
            if (url == null) return;

            tile = await provider.GetTileAsync(url, ct).ConfigureAwait(false);

            if (tile == null)
            {
                _failures.RecordFailure(tileId);
                return;
            }

            if (!ct.IsCancellationRequested && !_disposed)
            {
                _renderBuffer.Put(tileId, tile);
                tile = null;
                if (!_disposed && !ct.IsCancellationRequested)
                    InvalidateRequired?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _failures.RecordFailure(tileId);
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
        _renderBuffer.Dispose();
        // Note: Provider is NOT disposed — caller manages its lifecycle
    }
}
