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
///   <item><description>Call <see cref="Load(SKDeepZoomImageSource, ISKDeepZoomTileFetcher)"/> to load an image.</description></item>
///   <item><description>Call <see cref="SetControlSize"/> when the canvas size changes.</description></item>
///   <item><description>Call <see cref="Update"/> to schedule tile loads each frame.</description></item>
///   <item><description>Call <see cref="Render(ISKDeepZoomRenderer)"/> from your canvas paint callback, passing the renderer for this frame.</description></item>
/// </list>
/// </para>
/// </remarks>
public class SKDeepZoomController : IDisposable
{
    private SKDeepZoomImageSource? _tileSource;
    private readonly SKDeepZoomViewport _viewport;
    private readonly SKDeepZoomTileLayout _tileLayout;
    private readonly ISKDeepZoomTileCache _cache;
    private List<SKDeepZoomSubImage> _subImages = new List<SKDeepZoomSubImage>();
    private ISKDeepZoomTileFetcher? _fetcher;
    private readonly ConcurrentDictionary<SKDeepZoomTileId, byte> _pendingTiles = new ConcurrentDictionary<SKDeepZoomTileId, byte>();
    private CancellationTokenSource? _cts;
    private bool _disposed;
    private bool _userHasZoomed;

    /// <summary>
    /// Initializes a new <see cref="SKDeepZoomController"/> with an optional custom cache.
    /// When <paramref name="cache"/> is <see langword="null"/>, a default
    /// <see cref="SKDeepZoomMemoryTileCache"/> is used.
    /// </summary>
    public SKDeepZoomController(ISKDeepZoomTileCache? cache = null, int defaultCacheCapacity = 1024)
    {
        _viewport = new SKDeepZoomViewport();
        _tileLayout = new SKDeepZoomTileLayout();
        _cache = cache ?? new SKDeepZoomMemoryTileCache(defaultCacheCapacity);
    }

    // ---- Properties ----

    /// <summary>The current viewport.</summary>
    public SKDeepZoomViewport Viewport => _viewport;

    /// <summary>The tile cache.</summary>
    public ISKDeepZoomTileCache Cache => _cache;

    /// <summary>The tile layout calculator.</summary>
    public SKDeepZoomTileLayout TileLayout => _tileLayout;

    /// <summary>The loaded tile source, or null if not loaded.</summary>
    public SKDeepZoomImageSource? TileSource => _tileSource;

    /// <summary>The sub-images from the loaded DZC, or empty if not loaded from a DZC.</summary>
    public IReadOnlyList<SKDeepZoomSubImage> SubImages => _subImages;

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
    public event EventHandler<SKDeepZoomTileFailedEventArgs>? TileFailed;

    /// <summary>Fired when new tiles are loaded and the view needs repainting.</summary>
    public event EventHandler? InvalidateRequired;

    // ---- Load ----

    /// <summary>Loads a DZI tile source. Resets the viewport to show the full image.</summary>
    public void Load(SKDeepZoomImageSource tileSource, ISKDeepZoomTileFetcher fetcher)
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _cache.Clear();
            _subImages.Clear();

            if (_fetcher != null && !ReferenceEquals(_fetcher, fetcher))
                (_fetcher as IDisposable)?.Dispose();

            _tileSource = tileSource;
            _fetcher = fetcher;

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

    /// <summary>Loads a DZC tile source and sets up sub-images.</summary>
    public void Load(SKDeepZoomCollectionSource dzcTileSource, ISKDeepZoomTileFetcher fetcher)
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _cache.Clear();

            _tileSource = null!;
            _subImages = new List<SKDeepZoomSubImage>();
            foreach (var item in dzcTileSource.Items)
            {
                var sub = new SKDeepZoomSubImage(item.Id, item.MortonIndex, item.AspectRatio, item.Source)
                {
                    ViewportWidth = item.ViewportWidth,
                    ViewportOriginX = item.ViewportX,
                    ViewportOriginY = item.ViewportY,
                };
                _subImages.Add(sub);
            }

            if (_fetcher != null && !ReferenceEquals(_fetcher, fetcher))
                (_fetcher as IDisposable)?.Dispose();

            _fetcher = fetcher;

            _viewport.ControlWidth = _viewport.ControlWidth > 0 ? _viewport.ControlWidth : 800;
            _viewport.ControlHeight = _viewport.ControlHeight > 0 ? _viewport.ControlHeight : 600;
            _userHasZoomed = false;
            _viewport.ViewportOriginX = 0;
            _viewport.ViewportOriginY = 0;
            _viewport.ViewportWidth = 1.0;

            ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
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
        double newViewportWidth = Math.Max(SKDeepZoomViewport.MinViewportWidth, 1.0 / zoom);
        double factor = _viewport.ViewportWidth / newViewportWidth;
        _viewport.ZoomAboutLogicalPoint(factor, lx, ly);
        _viewport.Constrain();
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Returns the logical rectangle visible at the current viewport state.</summary>
    public (double X, double Y, double Width, double Height) GetZoomRect()
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
        if (_tileSource != null && _fetcher != null)
            ScheduleTileLoads();

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
    public void Render(ISKDeepZoomRenderer renderer)
    {
        if (_tileSource == null) return;

        _cache.FlushEvicted();

        var visibleTiles = _tileLayout.GetVisibleTiles(_tileSource, _viewport);

        renderer.BeginRender();

        // Pass 1: LOD fallback tiles (blurry placeholder while hi-res tiles load)
        if (EnableLodBlending)
        {
            foreach (var request in visibleTiles)
            {
                var tileId = request.TileId;
                if (!_cache.Contains(tileId))
                {
                    var fallback = _tileLayout.FindBestFallback(tileId, _cache);
                    if (fallback.HasValue)
                    {
                        _cache.TryGet(fallback.Value, out ISKDeepZoomTile? parentTile);
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
            _cache.TryGet(tileId, out ISKDeepZoomTile? tile);
            if (tile != null)
            {
                var dest = _tileLayout.GetTileDestRect(_tileSource, _viewport, tileId);
                renderer.DrawTile(dest, tile);
            }
        }

        renderer.EndRender();
    }

    // ---- Private ----

    private void ScheduleTileLoads()
    {
        if (_tileSource == null || _fetcher == null) return;

        var visibleTiles = _tileLayout.GetVisibleTiles(_tileSource, _viewport);
        var ct = _cts?.Token ?? CancellationToken.None;

        foreach (var request in visibleTiles)
        {
            var tileId = request.TileId;
            if (_cache.Contains(tileId) || _pendingTiles.ContainsKey(tileId))
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

    private async Task LoadTileAsync(SKDeepZoomTileId tileId, CancellationToken ct)
    {
        ISKDeepZoomTile? tile = null;
        try
        {
            if (_tileSource == null || _fetcher == null) return;

            tile = await _cache.TryGetAsync(tileId, ct).ConfigureAwait(false);

            if (tile == null && !ct.IsCancellationRequested)
            {
                string url = _tileSource.GetFullTileUrl(tileId.Level, tileId.Col, tileId.Row)
                    ?? _tileSource.GetTileUrl(tileId.Level, tileId.Col, tileId.Row);
                tile = await _fetcher.FetchTileAsync(url, ct).ConfigureAwait(false);
            }

            if (tile != null && !ct.IsCancellationRequested && !_disposed)
            {
                await _cache.PutAsync(tileId, tile, ct).ConfigureAwait(false);
                tile = null;
                InvalidateRequired?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            TileFailed?.Invoke(this, new SKDeepZoomTileFailedEventArgs(tileId, ex));
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
        (_fetcher as IDisposable)?.Dispose();
        _cache.Dispose();
    }
}
