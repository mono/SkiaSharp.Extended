#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Orchestrates the Deep Zoom rendering pipeline: viewport management, tile scheduling,
    /// cache management, and rendering.
    /// </summary>
    /// <remarks>
    /// This class handles only the <em>tile and rendering</em> concerns. Animation (spring physics,
    /// easing) and gesture recognition belong in the consuming layer (e.g. a MAUI view).
    /// <para>
    /// Typical usage:
    /// <list type="number">
    ///   <item><description>Call <see cref="Load(SKDeepZoomImageSource, ISKDeepZoomTileFetcher)"/> to load an image.</description></item>
    ///   <item><description>Call <see cref="SetControlSize"/> when the canvas size changes.</description></item>
    ///   <item><description>Call <see cref="SetViewport"/> / <see cref="Pan"/> / <see cref="ZoomAboutScreenPoint"/> to navigate.</description></item>
    ///   <item><description>Call <see cref="Update"/> and <see cref="Render"/> from your render loop.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class SKDeepZoomController : IDisposable
    {
        private SKDeepZoomImageSource? _tileSource;
        private readonly SKDeepZoomViewport _viewport;
        private readonly SKDeepZoomTileScheduler _scheduler;
        private readonly SKDeepZoomTileCache _cache;
        private readonly SKDeepZoomRenderer _renderer;
        private List<SKDeepZoomSubImage> _subImages = new List<SKDeepZoomSubImage>();
        private ISKDeepZoomTileFetcher? _fetcher;
        private readonly ConcurrentDictionary<SKDeepZoomTileId, byte> _pendingTiles = new ConcurrentDictionary<SKDeepZoomTileId, byte>();
        private CancellationTokenSource? _cts;
        private bool _disposed;
        private bool _userHasZoomed;

        /// <summary>Initializes a new <see cref="SKDeepZoomController"/> with an optional tile cache capacity.</summary>
        /// <param name="cacheCapacity">Maximum number of tiles to cache. Default is 1024.</param>
        public SKDeepZoomController(int cacheCapacity = 1024)
        {
            _viewport = new SKDeepZoomViewport();
            _scheduler = new SKDeepZoomTileScheduler();
            _cache = new SKDeepZoomTileCache(cacheCapacity);
            _renderer = new SKDeepZoomRenderer();
        }

        /// <summary>The current viewport. Use this to read the current position and zoom level.</summary>
        public SKDeepZoomViewport Viewport => _viewport;

        /// <summary>The tile cache.</summary>
        public SKDeepZoomTileCache Cache => _cache;

        /// <summary>The tile scheduler.</summary>
        public SKDeepZoomTileScheduler Scheduler => _scheduler;

        /// <summary>The renderer.</summary>
        public SKDeepZoomRenderer Renderer => _renderer;

        /// <summary>The loaded tile source, or null if not loaded.</summary>
        public SKDeepZoomImageSource? TileSource => _tileSource;

        /// <summary>The sub-images from the loaded DZC, or empty if not loaded from a DZC.</summary>
        public IReadOnlyList<SKDeepZoomSubImage> SubImages => _subImages;

        /// <summary>
        /// The aspect ratio of the loaded image (width/height). 0 if not loaded.
        /// </summary>
        public double AspectRatio => _tileSource?.AspectRatio ?? 0;

        /// <summary>
        /// Whether the controller is idle (no pending tile loads).
        /// </summary>
        public bool IsIdle => _pendingTiles.IsEmpty;

        /// <summary>Number of tile fetches currently in flight.</summary>
        public int PendingTileCount => _pendingTiles.Count;

        /// <summary>
        /// Returns the logical rectangle visible at the current viewport state.
        /// </summary>
        public (double X, double Y, double Width, double Height) GetZoomRect()
            => _viewport.GetZoomRect(_viewport.ViewportWidth);

        /// <summary>
        /// The zoom level at which one image pixel maps to exactly one screen pixel (native 1:1 resolution).
        /// Returns 0 if no image is loaded.
        /// </summary>
        public double NativeZoom =>
            (_tileSource != null && _viewport.ControlWidth > 0)
                ? (double)_tileSource.ImageWidth / _viewport.ControlWidth
                : 0.0;

        /// <summary>Show tile borders for debugging.</summary>
        public bool ShowTileBorders
        {
            get => _renderer.ShowTileBorders;
            set => _renderer.ShowTileBorders = value;
        }

        /// <summary>Show a debug statistics overlay with viewport, level, cache, and tile info.</summary>
        public bool ShowDebugStats
        {
            get => _renderer.ShowDebugStats;
            set => _renderer.ShowDebugStats = value;
        }

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

        /// <summary>
        /// Loads a DZI tile source and sets up the tile fetcher.
        /// Resets the viewport to show the full image.
        /// </summary>
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

        /// <summary>
        /// Loads a DZC tile source, populates SubImages, and sets up the tile fetcher.
        /// </summary>
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

        /// <summary>
        /// Sets the control (canvas) size. Call whenever the canvas is resized.
        /// When a source is loaded and the user has not manually zoomed, automatically
        /// refits the viewport so the image remains fully visible in the new size.
        /// </summary>
        public void SetControlSize(double width, double height)
        {
            bool sizeChanged = Math.Abs(_viewport.ControlWidth - width) > 0.5 ||
                               Math.Abs(_viewport.ControlHeight - height) > 0.5;

            _viewport.ControlWidth = width;
            _viewport.ControlHeight = height;

            // Only refit on resize when the user hasn't manually zoomed/panned.
            if (sizeChanged && _tileSource != null && !_userHasZoomed)
                _viewport.FitToView();
        }

        /// <summary>
        /// Sets the viewport directly to the given state and constrains it.
        /// </summary>
        public void SetViewport(double viewportWidth, double originX, double originY)
        {
            _viewport.ViewportWidth = viewportWidth;
            _viewport.ViewportOriginX = originX;
            _viewport.ViewportOriginY = originY;
            _viewport.Constrain();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms about a logical point. Factor &gt; 1 zooms in, &lt; 1 zooms out.
        /// </summary>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            _userHasZoomed = true;
            _viewport.ZoomAboutLogicalPoint(factor, logicalX, logicalY);
            _viewport.Constrain();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms about a screen-space point. Factor &gt; 1 zooms in, &lt; 1 zooms out.
        /// </summary>
        public void ZoomAboutScreenPoint(double factor, double screenX, double screenY)
        {
            _userHasZoomed = true;
            var (lx, ly) = _viewport.ElementToLogicalPoint(screenX, screenY);
            _viewport.ZoomAboutLogicalPoint(factor, lx, ly);
            _viewport.Constrain();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Pans by the given screen-space delta.
        /// </summary>
        public void Pan(double deltaScreenX, double deltaScreenY)
        {
            _userHasZoomed = true;
            _viewport.PanByScreenDelta(deltaScreenX, deltaScreenY);
            _viewport.Constrain();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resets the viewport to show the entire image and clears the manual-zoom flag.
        /// </summary>
        public void ResetView()
        {
            _userHasZoomed = false;
            _viewport.FitToView();
            _viewport.Constrain();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets an absolute zoom level (1.0 = image fills the control width).
        /// Zooms about the center of the control.
        /// </summary>
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

        /// <summary>
        /// Schedules loading for visible tiles and returns whether any tiles are still pending.
        /// Call from your render loop on every frame.
        /// </summary>
        /// <returns><see langword="true"/> if tile loads are still in progress; otherwise <see langword="false"/>.</returns>
        public bool Update()
        {
            if (_tileSource != null && _fetcher != null)
                ScheduleTileLoads();

            return _pendingTiles.Count > 0;
        }

        /// <summary>
        /// Renders the current viewport state onto the given canvas.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            if (_tileSource == null) return;

            canvas.Clear(SKColors.White);
            _renderer.Render(canvas, _tileSource, _viewport, _cache, _scheduler);
        }

        private void ScheduleTileLoads()
        {
            if (_tileSource == null || _fetcher == null) return;

            var visibleTiles = _scheduler.GetVisibleTiles(_tileSource, _viewport);
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
            SKBitmap? bitmap = null;
            try
            {
                if (_tileSource == null || _fetcher == null) return;

                string url = _tileSource.GetFullTileUrl(tileId.Level, tileId.Col, tileId.Row)
                    ?? _tileSource.GetTileUrl(tileId.Level, tileId.Col, tileId.Row);
                bitmap = await _fetcher.FetchTileAsync(url, ct).ConfigureAwait(false);

                if (bitmap != null && !ct.IsCancellationRequested && !_disposed)
                {
                    _cache.Put(tileId, bitmap);
                    bitmap = null;
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
                bitmap?.Dispose();
                _pendingTiles.TryRemove(tileId, out _);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts?.Cancel();
            _cts?.Dispose();
            (_fetcher as IDisposable)?.Dispose();
            _cache.Dispose();
            _renderer.Dispose();
        }
    }
}
