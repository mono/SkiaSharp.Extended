using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Orchestrates the Deep Zoom rendering pipeline: viewport management, spring animation,
    /// tile scheduling, cache management, and rendering. Platform-agnostic — no MAUI dependency.
    /// </summary>
    public class DeepZoomController : IDisposable
    {
        private DziTileSource? _tileSource;
        private readonly Viewport _viewport;
        private readonly ViewportSpring _spring;
        private readonly TileScheduler _scheduler;
        private readonly TileCache _cache;
        private readonly DeepZoomRenderer _renderer;
        private List<DeepZoomSubImage> _subImages = new List<DeepZoomSubImage>();
        private ITileFetcher? _fetcher;
        private readonly ConcurrentDictionary<TileId, byte> _pendingTiles = new ConcurrentDictionary<TileId, byte>();
        private CancellationTokenSource? _cts;
        private bool _disposed;

        public DeepZoomController(int cacheCapacity = 1024)
        {
            _viewport = new Viewport();
            _spring = new ViewportSpring();
            _scheduler = new TileScheduler();
            _cache = new TileCache(cacheCapacity);
            _renderer = new DeepZoomRenderer();
        }

        /// <summary>The current viewport (read-only access). During animation, this reflects the animated position.</summary>
        public Viewport Viewport => _viewport;

        /// <summary>The target viewport width. During spring animation, this is the destination value.</summary>
        public double TargetViewportWidth => _spring.Width.Target;

        /// <summary>The target viewport origin X. During spring animation, this is the destination value.</summary>
        public double TargetOriginX => _spring.OriginX.Target;

        /// <summary>The target viewport origin Y. During spring animation, this is the destination value.</summary>
        public double TargetOriginY => _spring.OriginY.Target;

        /// <summary>The spring animator for smooth transitions.</summary>
        public ViewportSpring Spring => _spring;

        /// <summary>The tile cache.</summary>
        public TileCache Cache => _cache;

        /// <summary>The tile scheduler.</summary>
        public TileScheduler Scheduler => _scheduler;

        /// <summary>The renderer.</summary>
        public DeepZoomRenderer Renderer => _renderer;

        /// <summary>The loaded tile source, or null if not loaded.</summary>
        public DziTileSource? TileSource => _tileSource;

        /// <summary>The sub-images from the loaded DZC, or empty if not loaded from a DZC.</summary>
        public IReadOnlyList<DeepZoomSubImage> SubImages => _subImages;

        /// <summary>Whether spring animations are enabled. Default true.</summary>
        public bool UseSprings { get; set; } = true;

        /// <summary>
        /// The aspect ratio of the loaded image (width/height). 0 if not loaded.
        /// </summary>
        public double AspectRatio => _tileSource?.AspectRatio ?? 0;

        /// <summary>
        /// Whether the controller is idle (no pending tile loads and no active animation).
        /// </summary>
        public bool IsIdle => _pendingTiles.IsEmpty && _spring.IsSettled;

        /// <summary>
        /// Returns the logical rectangle visible at the current viewport state.
        /// Convenience method that uses the current ViewportWidth.
        /// </summary>
        public (double X, double Y, double Width, double Height) GetZoomRect()
            => _viewport.GetZoomRect(_viewport.ViewportWidth);

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

        /// <summary>Fired when spring animation completes.</summary>
        public event EventHandler? MotionFinished;

        /// <summary>Fired when the viewport position or zoom level changes.</summary>
        public event EventHandler? ViewportChanged;

        /// <summary>Fired when a tile fails to load.</summary>
        public event EventHandler<TileFailedEventArgs>? TileFailed;

        /// <summary>Fired when new tiles are loaded and the view needs repainting.</summary>
        public event EventHandler? InvalidateRequired;

        /// <summary>
        /// Loads a DZI tile source and sets up the tile fetcher.
        /// </summary>
        public void Load(DziTileSource tileSource, ITileFetcher fetcher)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _cache.Clear();
            _subImages.Clear();

            // Dispose previous fetcher if it's a different instance
            if (_fetcher != null && !ReferenceEquals(_fetcher, fetcher))
                (_fetcher as IDisposable)?.Dispose();

            _tileSource = tileSource;
            _fetcher = fetcher;

            // Reset viewport to show the full image
            _viewport.AspectRatio = tileSource.AspectRatio;
            _viewport.ControlWidth = _viewport.ControlWidth > 0 ? _viewport.ControlWidth : 800;
            _viewport.ControlHeight = _viewport.ControlHeight > 0 ? _viewport.ControlHeight : 600;
            _viewport.ViewportOriginX = 0;
            _viewport.ViewportOriginY = 0;
            _viewport.ViewportWidth = 1.0;

            var state = _viewport.GetState();
            _spring.Reset(state.OriginX, state.OriginY, state.ViewportWidth);

            ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads a DZC tile source, populates SubImages, and sets up the tile fetcher.
        /// DZC collections are rendered externally by consuming the <see cref="SubImages"/> property.
        /// Each sub-image can be loaded individually via its Source DZI descriptor.
        /// The PivotViewer's CollectionImageProvider handles composite thumbnail rendering.
        /// </summary>
        public void Load(DzcTileSource dzcTileSource, ITileFetcher fetcher)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _pendingTiles.Clear();
            _cache.Clear();

            // DZC is a collection, not a single image — clear single-image state
            _tileSource = null!;
            _subImages = new List<DeepZoomSubImage>();
            foreach (var item in dzcTileSource.Items)
            {
                var sub = new DeepZoomSubImage(item.Id, item.MortonIndex, item.AspectRatio, item.Source)
                {
                    ViewportWidth = item.ViewportWidth,
                    ViewportOriginX = item.ViewportX,
                    ViewportOriginY = item.ViewportY,
                };
                _subImages.Add(sub);
            }

            // Dispose previous fetcher if it's a different instance
            if (_fetcher != null && !ReferenceEquals(_fetcher, fetcher))
                (_fetcher as IDisposable)?.Dispose();

            _fetcher = fetcher;

            // Reset viewport
            _viewport.ControlWidth = _viewport.ControlWidth > 0 ? _viewport.ControlWidth : 800;
            _viewport.ControlHeight = _viewport.ControlHeight > 0 ? _viewport.ControlHeight : 600;
            _viewport.ViewportOriginX = 0;
            _viewport.ViewportOriginY = 0;
            _viewport.ViewportWidth = 1.0;

            var state = _viewport.GetState();
            _spring.Reset(state.OriginX, state.OriginY, state.ViewportWidth);

            ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the control (canvas) size. Call when the view is resized.
        /// </summary>
        public void SetControlSize(double width, double height)
        {
            _viewport.ControlWidth = width;
            _viewport.ControlHeight = height;
        }

        /// <summary>
        /// Zooms about a logical point. Factor > 1 zooms in, &lt; 1 zooms out.
        /// The logical point stays fixed on screen.
        /// </summary>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            SyncViewportToTarget();
            _viewport.ZoomAboutLogicalPoint(factor, logicalX, logicalY);
            _viewport.Constrain();
            ApplyViewportToSpring();
        }

        /// <summary>
        /// Zooms about a screen-space point. Factor > 1 zooms in, &lt; 1 zooms out.
        /// </summary>
        public void ZoomAboutScreenPoint(double factor, double screenX, double screenY)
        {
            SyncViewportToTarget();
            var (lx, ly) = _viewport.ElementToLogicalPoint(screenX, screenY);
            _viewport.ZoomAboutLogicalPoint(factor, lx, ly);
            _viewport.Constrain();
            ApplyViewportToSpring();
        }

        /// <summary>
        /// Pans by the given screen-space delta.
        /// </summary>
        public void Pan(double deltaScreenX, double deltaScreenY)
        {
            SyncViewportToTarget();
            _viewport.PanByScreenDelta(deltaScreenX, deltaScreenY);
            _viewport.Constrain();
            ApplyViewportToSpring();
        }

        /// <summary>
        /// Sets the viewport to show the entire image.
        /// </summary>
        public void ResetView()
        {
            _viewport.ViewportOriginX = 0;
            _viewport.ViewportOriginY = 0;
            _viewport.ViewportWidth = 1.0;
            _viewport.Constrain();
            ApplyViewportToSpring();
        }

        /// <summary>
        /// Sets the viewport state directly. Springs animate to this state if enabled.
        /// </summary>
        public void SetViewport(double viewportWidth, double originX, double originY)
        {
            _viewport.ViewportWidth = viewportWidth;
            _viewport.ViewportOriginX = originX;
            _viewport.ViewportOriginY = originY;
            _viewport.Constrain();
            ApplyViewportToSpring();
        }

        /// <summary>
        /// Restores the viewport to the spring's target state so that
        /// programmatic operations (Zoom, Pan) compound on the intended
        /// destination rather than the mid-animation position.
        /// </summary>
        private void SyncViewportToTarget()
        {
            _viewport.ViewportWidth = _spring.Width.Target;
            _viewport.ViewportOriginX = _spring.OriginX.Target;
            _viewport.ViewportOriginY = _spring.OriginY.Target;
        }

        private void ApplyViewportToSpring()
        {
            var state = _viewport.GetState();
            if (UseSprings)
            {
                _spring.SetTarget(state.OriginX, state.OriginY, state.ViewportWidth);
            }
            else
            {
                _spring.SetTarget(state.OriginX, state.OriginY, state.ViewportWidth);
                _spring.SnapToTarget();
            }
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Immediately snaps the spring animation to its target.
        /// Useful for direct-manipulation gestures (pan, pinch) where the
        /// user expects instant feedback rather than spring lag.
        /// </summary>
        public void SnapSpringToTarget()
        {
            _spring.SnapToTarget();
            var state = _spring.GetCurrentState();
            _viewport.ViewportOriginX = state.OriginX;
            _viewport.ViewportOriginY = state.OriginY;
            _viewport.ViewportWidth = state.ViewportWidth;
        }

        /// <summary>
        /// Advances animation and returns true if the view needs repainting.
        /// Call from the render loop with the time since last frame.
        /// </summary>
        public bool Update(TimeSpan deltaTime)
        {
            bool wasSettled = _spring.IsSettled;
            _spring.Update(deltaTime.TotalSeconds);

            // Apply spring state to viewport
            var state = _spring.GetCurrentState();
            bool viewportMoved = state.OriginX != _viewport.ViewportOriginX ||
                                 state.OriginY != _viewport.ViewportOriginY ||
                                 state.ViewportWidth != _viewport.ViewportWidth;
            _viewport.ViewportOriginX = state.OriginX;
            _viewport.ViewportOriginY = state.OriginY;
            _viewport.ViewportWidth = state.ViewportWidth;

            if (viewportMoved)
            {
                ViewportChanged?.Invoke(this, EventArgs.Empty);
            }

            if (!wasSettled && _spring.IsSettled)
            {
                MotionFinished?.Invoke(this, EventArgs.Empty);
            }

            // Schedule tile loading
            if (_tileSource != null && _fetcher != null)
            {
                ScheduleTileLoads();
            }

            return !_spring.IsSettled || _pendingTiles.Count > 0;
        }

        /// <summary>
        /// Renders the current state onto the given canvas.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            if (_tileSource == null) return;

            canvas.Clear(SKColors.White);
            _renderer.Render(canvas, _tileSource, _viewport, _cache, _scheduler);
        }

        /// <summary>
        /// Schedules loading for visible tiles that aren't cached.
        /// </summary>
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

        private async Task LoadTileAsync(TileId tileId, CancellationToken ct)
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
                    bitmap = null; // cache owns the bitmap now
                    InvalidateRequired?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during controller dispose or source change
            }
            catch (Exception ex)
            {
                TileFailed?.Invoke(this, new TileFailedEventArgs(tileId, ex));
            }
            finally
            {
                bitmap?.Dispose();
                _pendingTiles.TryRemove(tileId, out _);
            }
        }

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

    /// <summary>
    /// Event args for when a tile fails to load.
    /// </summary>
    public class TileFailedEventArgs : EventArgs
    {
        public TileFailedEventArgs(TileId tileId, Exception exception)
        {
            TileId = tileId;
            Exception = exception;
        }

        /// <summary>The tile that failed to load.</summary>
        public TileId TileId { get; }

        /// <summary>The exception that caused the failure.</summary>
        public Exception Exception { get; }
    }
}
