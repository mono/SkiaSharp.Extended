using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Diagnostics;

namespace SkiaSharp.Extended.UI.Maui.DeepZoom
{
    /// <summary>
    /// A MAUI view that renders Deep Zoom images using SkiaSharp.
    /// Uses <see cref="SKGestureTracker"/> for gesture recognition (pan, pinch, double-tap, scroll, fling).
    /// </summary>
    public class SKDeepZoomView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly DeepZoomController _controller;
        private readonly SKGestureTracker _tracker;
        private readonly Stopwatch _stopwatch;
        private TimeSpan _lastFrameTime;
        private bool _disposed;
        private bool _isVisible = true;
        private IDispatcherTimer? _animationTimer;
        private bool _isAnimating;
        private float _dpiScale = 1f;

        // Named event handlers for proper cleanup
        private readonly EventHandler _onInvalidateRequired;
        private readonly EventHandler _onMotionFinished;
        private readonly EventHandler _onViewportChanged;
        private readonly EventHandler _onImageOpenSucceeded;
        private readonly EventHandler<Exception> _onImageOpenFailed;

        public SKDeepZoomView()
        {
            _controller = new DeepZoomController();
            _stopwatch = Stopwatch.StartNew();
            _lastFrameTime = _stopwatch.Elapsed;

            _canvasView = new SKCanvasView
            {
                EnableTouchEvents = true,
            };
            _canvasView.PaintSurface += OnPaintSurface;
            _canvasView.Touch += OnCanvasTouch;
            Content = _canvasView;

            // Wire up controller events using named handlers for proper cleanup
            _onInvalidateRequired = (s, e) =>
                MainThread.BeginInvokeOnMainThread(() => StartAnimation());
            _onMotionFinished = (s, e) => MotionFinished?.Invoke(this, EventArgs.Empty);
            _onViewportChanged = (s, e) => ViewportChanged?.Invoke(this, EventArgs.Empty);
            _onImageOpenSucceeded = (s, e) => ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
            _onImageOpenFailed = (s, e) => ImageOpenFailed?.Invoke(this, e);

            _controller.InvalidateRequired += _onInvalidateRequired;
            _controller.MotionFinished += _onMotionFinished;
            _controller.ViewportChanged += _onViewportChanged;
            _controller.ImageOpenSucceeded += _onImageOpenSucceeded;
            _controller.ImageOpenFailed += _onImageOpenFailed;

            // Set up unified gesture tracker (from SkiaSharp.Extended)
            _tracker = new SKGestureTracker(new SKGestureTrackerOptions
            {
                IsRotateEnabled = false,        // Deep zoom doesn't support rotation
                IsDoubleTapZoomEnabled = false,  // Deep zoom handles zoom animation via spring physics
                IsScrollZoomEnabled = false,     // We forward scroll to controller ourselves
                IsFlingEnabled = true,           // Add momentum scrolling via fling
            });
            _tracker.PanDetected += OnTrackerPan;
            _tracker.PinchDetected += OnTrackerPinch;
            _tracker.DoubleTapDetected += OnTrackerDoubleTap;
            _tracker.ScrollDetected += OnTrackerScroll;
            _tracker.FlingUpdated += OnTrackerFlingUpdated;

            // Suppress animation when not visible
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        private void OnViewLoaded(object? sender, EventArgs e) => _isVisible = true;
        private void OnViewUnloaded(object? sender, EventArgs e)
        {
            _isVisible = false;
            StopAnimation();
        }

        /// <summary>
        /// Ensures the animation timer is running. The timer fires at ~60fps,
        /// invalidating the surface each tick so the spring advances smoothly.
        /// Stops automatically when the spring settles and no tiles are pending.
        /// </summary>
        private void StartAnimation()
        {
            if (_isAnimating || _disposed) return;
            _isAnimating = true;
            _lastFrameTime = _stopwatch.Elapsed;

            if (_animationTimer == null)
            {
                _animationTimer = Dispatcher.CreateTimer();
                _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
                _animationTimer.Tick += (s, e) =>
                {
                    if (!_isVisible || _disposed)
                    {
                        StopAnimation();
                        return;
                    }
                    _canvasView.InvalidateSurface();
                };
            }

            _animationTimer.Start();
        }

        private void StopAnimation()
        {
            _isAnimating = false;
            _animationTimer?.Stop();
        }

        // --- BindableProperties ---

        public static readonly BindableProperty UseSpringsProperty =
            BindableProperty.Create(nameof(UseSprings), typeof(bool), typeof(SKDeepZoomView), true,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._controller.UseSprings = (bool)n);

        public static readonly BindableProperty ShowTileBordersProperty =
            BindableProperty.Create(nameof(ShowTileBorders), typeof(bool), typeof(SKDeepZoomView), false,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._controller.ShowTileBorders = (bool)n);

        public static readonly BindableProperty ShowDebugStatsProperty =
            BindableProperty.Create(nameof(ShowDebugStats), typeof(bool), typeof(SKDeepZoomView), false,
                propertyChanged: (b, o, n) =>
                {
                    var view = (SKDeepZoomView)b;
                    view._controller.ShowDebugStats = (bool)n;
                    view.StartAnimation();
                });

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(string), typeof(SKDeepZoomView), null,
                BindingMode.OneWay, propertyChanged: OnSourceChanged);

        /// <summary>Whether spring animations are used for transitions.</summary>
        public bool UseSprings
        {
            get => (bool)GetValue(UseSpringsProperty);
            set => SetValue(UseSpringsProperty, value);
        }

        /// <summary>Whether to show tile borders for debugging.</summary>
        public bool ShowTileBorders
        {
            get => (bool)GetValue(ShowTileBordersProperty);
            set => SetValue(ShowTileBordersProperty, value);
        }

        /// <summary>Whether to show the debug statistics overlay.</summary>
        public bool ShowDebugStats
        {
            get => (bool)GetValue(ShowDebugStatsProperty);
            set => SetValue(ShowDebugStatsProperty, value);
        }

        /// <summary>
        /// URI to a .dzi file. Setting this automatically fetches the DZI metadata
        /// and loads the image using HTTP. Use <see cref="Load"/> for non-HTTP sources.
        /// </summary>
        public string? Source
        {
            get => (string?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private CancellationTokenSource? _sourceCts;

        private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SKDeepZoomView view)
                view.LoadFromSource(newValue as string);
        }

        private async void LoadFromSource(string? uri)
        {
            _sourceCts?.Cancel();
            _sourceCts?.Dispose();
            _sourceCts = null;

            if (string.IsNullOrWhiteSpace(uri))
                return;

            _sourceCts = new CancellationTokenSource();
            var activeCts = _sourceCts;
            var ct = activeCts.Token;

            HttpTileFetcher? fetcher = null;
            HttpClient? httpClient = null;
            bool loadedOk = false;

            try
            {
                httpClient = new HttpClient();

                using var response = await httpClient.GetAsync(new Uri(uri, UriKind.Absolute), ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var xml = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                ct.ThrowIfCancellationRequested();

                using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));

                // Derive the base URL preserving query string (for signed URLs)
                var parsed = new Uri(uri, UriKind.Absolute);
                var pathPart = parsed.GetLeftPart(UriPartial.Path);
                var dotIdx = pathPart.LastIndexOf('.');
                var tilesBase = (dotIdx >= 0 ? pathPart.Substring(0, dotIdx) : pathPart) + "_files/";

                var tileSource = DziTileSource.Parse(stream);
                tileSource.TilesBaseUri = tilesBase;
                if (!string.IsNullOrEmpty(parsed.Query))
                    tileSource.TilesQueryString = parsed.Query;

                // Verify this is still the active request before mutating state
                if (_disposed || _sourceCts != activeCts || ct.IsCancellationRequested)
                    return;

                fetcher = new HttpTileFetcher();
                Load(tileSource, fetcher);
                loadedOk = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!_disposed)
                    ImageOpenFailed?.Invoke(this, ex);
            }
            finally
            {
                httpClient?.Dispose();
                if (!loadedOk)
                    fetcher?.Dispose();
            }
        }

        // --- Viewport properties (read-write for programmatic control) ---

        /// <summary>Current viewport width (1.0 = full image fits). Setting triggers animated transition if UseSprings is true.</summary>
        public double ViewportWidth
        {
            get => _controller.TargetViewportWidth;
            set
            {
                _controller.SetViewport(value, _controller.TargetOriginX, _controller.TargetOriginY);
                StartAnimation();
            }
        }

        /// <summary>Current viewport origin X (target value). Setting triggers animated transition if UseSprings is true.</summary>
        public double ViewportOriginX
        {
            get => _controller.TargetOriginX;
            set
            {
                _controller.SetViewport(_controller.TargetViewportWidth, value, _controller.TargetOriginY);
                StartAnimation();
            }
        }

        /// <summary>Current viewport origin Y (target value). Setting triggers animated transition if UseSprings is true.</summary>
        public double ViewportOriginY
        {
            get => _controller.TargetOriginY;
            set
            {
                _controller.SetViewport(_controller.TargetViewportWidth, _controller.TargetOriginX, value);
                StartAnimation();
            }
        }

        /// <summary>Image aspect ratio (width/height), or 0 if not loaded.</summary>
        public double AspectRatio => _controller.AspectRatio;

        /// <summary>Whether the view is idle (no pending loads, no animation).</summary>
        public bool IsIdle => _controller.IsIdle;

        /// <summary>The underlying controller for advanced usage.</summary>
        public DeepZoomController Controller => _controller;

        /// <summary>The underlying gesture tracker for advanced configuration.</summary>
        public SKGestureTracker GestureTracker => _tracker;

        // --- Events ---

        /// <summary>Fired when the image source loads successfully.</summary>
        public event EventHandler? ImageOpenSucceeded;

        /// <summary>Fired when the image source fails to load.</summary>
        public event EventHandler<Exception>? ImageOpenFailed;

        /// <summary>Fired when spring animation completes.</summary>
        public event EventHandler? MotionFinished;

        /// <summary>Fired when the viewport position or zoom level changes.</summary>
        public event EventHandler? ViewportChanged;

        // --- Methods ---

        /// <summary>
        /// Loads a DZI tile source with the specified fetcher.
        /// </summary>
        public void Load(DziTileSource tileSource, ITileFetcher fetcher)
        {
            _controller.Load(tileSource, fetcher);
            StartAnimation();
        }

        /// <summary>
        /// Loads a DZC collection source with the specified fetcher.
        /// Populates SubImages for multi-image mosaic display.
        /// </summary>
        public void Load(DzcTileSource tileSource, ITileFetcher fetcher)
        {
            _controller.Load(tileSource, fetcher);
            StartAnimation();
        }

        /// <summary>
        /// Sub-images when a DZC collection is loaded. Empty for single DZI images.
        /// </summary>
        public IReadOnlyList<DeepZoomSubImage> SubImages => _controller.SubImages;

        /// <summary>
        /// Zooms to show the entire image. Animates if UseSprings is true.
        /// </summary>
        public void ResetView()
        {
            _controller.ResetView();
            StartAnimation();
        }

        /// <summary>
        /// Zooms about a logical point. Animates if UseSprings is true.
        /// </summary>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            _controller.ZoomAboutLogicalPoint(factor, logicalX, logicalY);
            StartAnimation();
        }

        /// <summary>
        /// Converts screen-space point to logical coordinates.
        /// </summary>
        public (double X, double Y) ElementToLogicalPoint(double screenX, double screenY)
            => _controller.Viewport.ElementToLogicalPoint(screenX, screenY);

        /// <summary>
        /// Converts logical coordinates to screen-space point.
        /// </summary>
        public (double X, double Y) LogicalToElementPoint(double logicalX, double logicalY)
            => _controller.Viewport.LogicalToElementPoint(logicalX, logicalY);

        // --- Rendering ---

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isVisible) return;

            var now = _stopwatch.Elapsed;
            var delta = now - _lastFrameTime;
            _lastFrameTime = now;

            // Cap delta to prevent physics instability after app pause/background
            if (delta.TotalMilliseconds > 100)
                delta = TimeSpan.FromMilliseconds(100);

            // Store scale factor for keyboard navigation coordinate conversion
            _dpiScale = _canvasView.CanvasSize.Width > 0 && Width > 0
                ? _canvasView.CanvasSize.Width / (float)Width
                : 1f;

            _controller.SetControlSize(e.Info.Width, e.Info.Height);
            bool needsRepaint = _controller.Update(delta);
            _controller.Render(e.Surface.Canvas);

            // Stop the animation timer when spring has settled and all tiles are loaded
            if (!needsRepaint)
            {
                StopAnimation();
            }
        }

        // --- Gesture Handling (via SKGestureTracker) ---

        /// <summary>
        /// Forwards <see cref="SKCanvasView.Touch"/> events to the gesture tracker.
        /// <see cref="SKTouchEventArgs.Location"/> is already in device-pixel coordinates,
        /// matching the canvas coordinate space used by <see cref="DeepZoomController"/>.
        /// </summary>
        private void OnCanvasTouch(object? sender, SKTouchEventArgs e)
        {
            var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;
            e.Handled = e.ActionType switch
            {
                SKTouchAction.Pressed => _tracker.ProcessTouchDown(e.Id, e.Location, isMouse),
                SKTouchAction.Moved => _tracker.ProcessTouchMove(e.Id, e.Location, e.InContact),
                SKTouchAction.Released => _tracker.ProcessTouchUp(e.Id, e.Location, isMouse),
                SKTouchAction.Cancelled => _tracker.ProcessTouchCancel(e.Id),
                SKTouchAction.WheelChanged => _tracker.ProcessMouseWheel(e.Location, 0, e.WheelDelta),
                _ => false,
            };
        }

        private void OnTrackerPan(object? sender, SKPanGestureEventArgs e)
        {
            // Mark handled so the tracker doesn't also update its own matrix offset
            e.Handled = true;
            _controller.Pan(e.Delta.X, e.Delta.Y);
            _controller.SnapSpringToTarget();
            StartAnimation();
        }

        private void OnTrackerPinch(object? sender, SKPinchGestureEventArgs e)
        {
            _controller.ZoomAboutScreenPoint(e.ScaleDelta, e.FocalPoint.X, e.FocalPoint.Y);
            _controller.SnapSpringToTarget();
            StartAnimation();
        }

        private void OnTrackerDoubleTap(object? sender, SKTapGestureEventArgs e)
        {
            // Toggle: if zoomed in, zoom out to fit; otherwise zoom in 2x
            if (_controller.TargetViewportWidth < 0.95)
                _controller.ZoomAboutScreenPoint(0.5, e.Location.X, e.Location.Y);
            else
                _controller.ZoomAboutScreenPoint(2.0, e.Location.X, e.Location.Y);
            StartAnimation();
        }

        private void OnTrackerScroll(object? sender, SKScrollGestureEventArgs e)
        {
            // Positive Y = scroll up = zoom in
            double factor = e.Delta.Y > 0 ? 1.2 : 1.0 / 1.2;
            _controller.ZoomAboutScreenPoint(factor, e.Location.X, e.Location.Y);
            StartAnimation();
        }

        private void OnTrackerFlingUpdated(object? sender, SKFlingGestureEventArgs e)
        {
            // Apply per-frame fling displacement as a pan translation
            if (e.Delta == SKPoint.Empty) return;
            _controller.Pan(e.Delta.X, e.Delta.Y);
            _controller.SnapSpringToTarget();
            StartAnimation();
        }

        // --- Keyboard Navigation ---

        /// <summary>
        /// Handles keyboard input for navigation:
        /// Arrow keys → pan, +/= → zoom in, -/_ → zoom out, Home → reset view.
        /// Call from platform key event handler.
        /// </summary>
        public bool HandleKeyPress(string key)
        {
            if (_disposed) return false;

            double panStep = 50 * _dpiScale;
            const double zoomFactor = 1.5;

            switch (key)
            {
                case "Left":
                case "ArrowLeft":
                    _controller.Pan(panStep, 0);
                    break;
                case "Right":
                case "ArrowRight":
                    _controller.Pan(-panStep, 0);
                    break;
                case "Up":
                case "ArrowUp":
                    _controller.Pan(0, panStep);
                    break;
                case "Down":
                case "ArrowDown":
                    _controller.Pan(0, -panStep);
                    break;
                case "Add":
                case "OemPlus":
                case "+":
                case "=":
                    _controller.ZoomAboutScreenPoint(zoomFactor,
                        _canvasView.CanvasSize.Width / 2,
                        _canvasView.CanvasSize.Height / 2);
                    break;
                case "Subtract":
                case "OemMinus":
                case "-":
                case "_":
                    _controller.ZoomAboutScreenPoint(1.0 / zoomFactor,
                        _canvasView.CanvasSize.Width / 2,
                        _canvasView.CanvasSize.Height / 2);
                    break;
                case "Home":
                    _controller.ResetView();
                    break;
                default:
                    return false;
            }

            StartAnimation();
            return true;
        }

        // --- Disposal ---

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _sourceCts?.Cancel();
            _sourceCts?.Dispose();
            _sourceCts = null;

            _canvasView.PaintSurface -= OnPaintSurface;
            _canvasView.Touch -= OnCanvasTouch;
            _controller.InvalidateRequired -= _onInvalidateRequired;
            _controller.MotionFinished -= _onMotionFinished;
            _controller.ViewportChanged -= _onViewportChanged;
            _controller.ImageOpenSucceeded -= _onImageOpenSucceeded;
            _controller.ImageOpenFailed -= _onImageOpenFailed;

            _tracker.PanDetected -= OnTrackerPan;
            _tracker.PinchDetected -= OnTrackerPinch;
            _tracker.DoubleTapDetected -= OnTrackerDoubleTap;
            _tracker.ScrollDetected -= OnTrackerScroll;
            _tracker.FlingUpdated -= OnTrackerFlingUpdated;
            _tracker.Dispose();

            // Unsubscribe lifecycle events
            Loaded -= OnViewLoaded;
            Unloaded -= OnViewUnloaded;

            _controller.Dispose();
        }
    }
}
