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
    /// A MAUI view that renders Deep Zoom images with gesture navigation and spring animation.
    /// </summary>
    /// <remarks>
    /// <para>This control composes three independent layers:</para>
    /// <list type="bullet">
    ///   <item><term><see cref="SKGestureTracker"/></term><description>
    ///     Detects pan, pinch, double-tap, scroll, and fling gestures.
    ///   </description></item>
    ///   <item><term><see cref="ViewportSpring"/></term><description>
    ///     Animates the viewport with spring physics. Owned entirely by this view.
    ///   </description></item>
    ///   <item><term><see cref="DeepZoomController"/></term><description>
    ///     Handles tile loading and rendering — no animation, no gesture awareness.
    ///   </description></item>
    /// </list>
    /// <para>
    /// The data flow is: <c>Touch events → SKGestureTracker → ViewportSpring → DeepZoomController → Render</c>
    /// </para>
    /// </remarks>
    public class SKDeepZoomView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly DeepZoomController _controller;
        private readonly SKGestureTracker _tracker;
        private readonly ViewportSpring _spring;
        private readonly Stopwatch _stopwatch;
        private TimeSpan _lastFrameTime;
        private bool _disposed;
        private bool _isVisible = true;
        private IDispatcherTimer? _animationTimer;
        private bool _isAnimating;
        private float _dpiScale = 1f;

        // Named event handlers for proper cleanup
        private readonly EventHandler _onInvalidateRequired;
        private readonly EventHandler _onImageOpenSucceeded;
        private readonly EventHandler<Exception> _onImageOpenFailed;

        /// <summary>Initializes a new <see cref="SKDeepZoomView"/>.</summary>
        public SKDeepZoomView()
        {
            _controller = new DeepZoomController();
            _spring = new ViewportSpring();
            _stopwatch = Stopwatch.StartNew();
            _lastFrameTime = _stopwatch.Elapsed;

            _canvasView = new SKCanvasView
            {
                EnableTouchEvents = true,
            };
            _canvasView.PaintSurface += OnPaintSurface;
            _canvasView.Touch += OnCanvasTouch;
            Content = _canvasView;

            // Wire controller events that don't involve animation
            _onInvalidateRequired = (s, e) =>
                MainThread.BeginInvokeOnMainThread(() => StartAnimation());
            _onImageOpenSucceeded = (s, e) =>
            {
                // Reset spring to initial state when a new image loads
                _spring.Reset(0, 0, 1.0);
                ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
            };
            _onImageOpenFailed = (s, e) => ImageOpenFailed?.Invoke(this, e);

            _controller.InvalidateRequired += _onInvalidateRequired;
            _controller.ImageOpenSucceeded += _onImageOpenSucceeded;
            _controller.ImageOpenFailed += _onImageOpenFailed;

            // Gesture tracker — handles touch recognition only; animation is via ViewportSpring above
            _tracker = new SKGestureTracker(new SKGestureTrackerOptions
            {
                IsRotateEnabled = false,        // Deep zoom doesn't use rotation
                IsDoubleTapZoomEnabled = false,  // We handle double-tap zoom via spring ourselves
                IsScrollZoomEnabled = false,     // We handle scroll zoom ourselves
                IsFlingEnabled = true,           // Fling provides momentum via FlingUpdated frames
            });
            _tracker.PanDetected += OnTrackerPan;
            _tracker.PinchDetected += OnTrackerPinch;
            _tracker.DoubleTapDetected += OnTrackerDoubleTap;
            _tracker.ScrollDetected += OnTrackerScroll;
            _tracker.FlingUpdated += OnTrackerFlingUpdated;

            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void OnViewLoaded(object? sender, EventArgs e) => _isVisible = true;
        private void OnViewUnloaded(object? sender, EventArgs e)
        {
            _isVisible = false;
            StopAnimation();
        }

        private void StartAnimation()
        {
            if (_isAnimating || _disposed) return;
            _isAnimating = true;
            _lastFrameTime = _stopwatch.Elapsed;

            if (_animationTimer == null)
            {
                _animationTimer = Dispatcher.CreateTimer();
                _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 fps
                _animationTimer.Tick += (s, e) =>
                {
                    if (!_isVisible || _disposed) { StopAnimation(); return; }
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

        // ── BindableProperties ─────────────────────────────────────────────────

        /// <summary>When <see langword="false"/>, viewport changes are applied instantly (no spring animation).</summary>
        public static readonly BindableProperty UseSpringsProperty =
            BindableProperty.Create(nameof(UseSprings), typeof(bool), typeof(SKDeepZoomView), true);

        /// <summary>Spring stiffness. Higher = faster snap, lower = slower/smoother. Default 100.0.</summary>
        public static readonly BindableProperty SpringStiffnessProperty =
            BindableProperty.Create(nameof(SpringStiffness), typeof(double), typeof(SKDeepZoomView), 100.0,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._spring.Stiffness = (double)n,
                validateValue: (_, v) => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d > 0);

        /// <summary>Spring damping ratio. 1.0 = no overshoot, &lt;1.0 = bouncy, &gt;1.0 = sluggish. Default 1.0.</summary>
        public static readonly BindableProperty SpringDampingRatioProperty =
            BindableProperty.Create(nameof(SpringDampingRatio), typeof(double), typeof(SKDeepZoomView), 1.0,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._spring.DampingRatio = (double)n,
                validateValue: (_, v) => v is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d > 0);

        /// <summary>Show tile borders for debugging.</summary>
        public static readonly BindableProperty ShowTileBordersProperty =
            BindableProperty.Create(nameof(ShowTileBorders), typeof(bool), typeof(SKDeepZoomView), false,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._controller.ShowTileBorders = (bool)n);

        /// <summary>Show a debug statistics overlay.</summary>
        public static readonly BindableProperty ShowDebugStatsProperty =
            BindableProperty.Create(nameof(ShowDebugStats), typeof(bool), typeof(SKDeepZoomView), false,
                propertyChanged: (b, o, n) =>
                {
                    var view = (SKDeepZoomView)b;
                    view._controller.ShowDebugStats = (bool)n;
                    view.StartAnimation();
                });

        /// <summary>URI to a .dzi file. Setting this fetches and loads the image automatically.</summary>
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(string), typeof(SKDeepZoomView), null,
                BindingMode.OneWay, propertyChanged: OnSourceChanged);

        /// <inheritdoc cref="UseSpringsProperty"/>
        public bool UseSprings
        {
            get => (bool)GetValue(UseSpringsProperty);
            set => SetValue(UseSpringsProperty, value);
        }

        /// <inheritdoc cref="SpringStiffnessProperty"/>
        public double SpringStiffness
        {
            get => (double)GetValue(SpringStiffnessProperty);
            set => SetValue(SpringStiffnessProperty, value);
        }

        /// <inheritdoc cref="SpringDampingRatioProperty"/>
        public double SpringDampingRatio
        {
            get => (double)GetValue(SpringDampingRatioProperty);
            set => SetValue(SpringDampingRatioProperty, value);
        }

        /// <inheritdoc cref="ShowTileBordersProperty"/>
        public bool ShowTileBorders
        {
            get => (bool)GetValue(ShowTileBordersProperty);
            set => SetValue(ShowTileBordersProperty, value);
        }

        /// <inheritdoc cref="ShowDebugStatsProperty"/>
        public bool ShowDebugStats
        {
            get => (bool)GetValue(ShowDebugStatsProperty);
            set => SetValue(ShowDebugStatsProperty, value);
        }

        /// <inheritdoc cref="SourceProperty"/>
        public string? Source
        {
            get => (string?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        // ── Viewport properties ────────────────────────────────────────────────

        /// <summary>
        /// Target viewport width (1.0 = full image fits). Setting this triggers an animated
        /// transition if <see cref="UseSprings"/> is <see langword="true"/>.
        /// </summary>
        public double ViewportWidth
        {
            get => _spring.Width.Target;
            set => SetViewport(value, _spring.OriginX.Target, _spring.OriginY.Target);
        }

        /// <summary>Target viewport origin X. Setting triggers an animated transition.</summary>
        public double ViewportOriginX
        {
            get => _spring.OriginX.Target;
            set => SetViewport(_spring.Width.Target, value, _spring.OriginY.Target);
        }

        /// <summary>Target viewport origin Y. Setting triggers an animated transition.</summary>
        public double ViewportOriginY
        {
            get => _spring.OriginY.Target;
            set => SetViewport(_spring.Width.Target, _spring.OriginX.Target, value);
        }

        /// <summary>Image aspect ratio (width/height), or 0 if not loaded.</summary>
        public double AspectRatio => _controller.AspectRatio;

        /// <summary>Whether the view is idle (no pending tile loads and spring settled).</summary>
        public bool IsIdle => _spring.IsSettled && _controller.IsIdle;

        /// <summary>The underlying controller for advanced tile/render access.</summary>
        public DeepZoomController Controller => _controller;

        /// <summary>The underlying gesture tracker for advanced configuration.</summary>
        public SKGestureTracker GestureTracker => _tracker;

        /// <summary>The viewport spring animator for direct inspection or manipulation.</summary>
        public ViewportSpring Spring => _spring;

        // ── Events ─────────────────────────────────────────────────────────────

        /// <summary>Fired when the image source loads successfully.</summary>
        public event EventHandler? ImageOpenSucceeded;

        /// <summary>Fired when the image source fails to load.</summary>
        public event EventHandler<Exception>? ImageOpenFailed;

        /// <summary>Fired when the spring animation completes (viewport settles).</summary>
        public event EventHandler? MotionFinished;

        /// <summary>Fired when the viewport position or zoom level changes.</summary>
        public event EventHandler? ViewportChanged;

        // ── Source loading ─────────────────────────────────────────────────────

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

            if (string.IsNullOrWhiteSpace(uri)) return;

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

                var parsed = new Uri(uri, UriKind.Absolute);
                var pathPart = parsed.GetLeftPart(UriPartial.Path);
                var dotIdx = pathPart.LastIndexOf('.');
                var tilesBase = (dotIdx >= 0 ? pathPart.Substring(0, dotIdx) : pathPart) + "_files/";

                var tileSource = DziTileSource.Parse(stream);
                tileSource.TilesBaseUri = tilesBase;
                if (!string.IsNullOrEmpty(parsed.Query))
                    tileSource.TilesQueryString = parsed.Query;

                if (_disposed || _sourceCts != activeCts || ct.IsCancellationRequested) return;

                fetcher = new HttpTileFetcher();
                Load(tileSource, fetcher);
                loadedOk = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!_disposed) ImageOpenFailed?.Invoke(this, ex);
            }
            finally
            {
                httpClient?.Dispose();
                if (!loadedOk) fetcher?.Dispose();
            }
        }

        // ── Public methods ─────────────────────────────────────────────────────

        /// <summary>Loads a DZI tile source with the specified fetcher.</summary>
        public void Load(DziTileSource tileSource, ITileFetcher fetcher)
        {
            _controller.Load(tileSource, fetcher);
            // ImageOpenSucceeded resets the spring; StartAnimation drives the first render
            StartAnimation();
        }

        /// <summary>
        /// Loads a DZC collection source with the specified fetcher.
        /// Populates <see cref="SubImages"/> for multi-image mosaic display.
        /// </summary>
        public void Load(DzcTileSource tileSource, ITileFetcher fetcher)
        {
            _controller.Load(tileSource, fetcher);
            StartAnimation();
        }

        /// <summary>Sub-images when a DZC collection is loaded. Empty for single DZI images.</summary>
        public IReadOnlyList<DeepZoomSubImage> SubImages => _controller.SubImages;

        /// <summary>
        /// Sets the viewport. Animates via spring if <see cref="UseSprings"/> is <see langword="true"/>.
        /// </summary>
        public void SetViewport(double viewportWidth, double originX, double originY)
        {
            // Compute constrained state via controller, then take over with spring
            _controller.SetViewport(viewportWidth, originX, originY);
            var vp = _controller.Viewport;
            _spring.SetTarget(vp.ViewportOriginX, vp.ViewportOriginY, vp.ViewportWidth);
            if (!UseSprings) _spring.SnapToTarget();
            StartAnimation();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms to show the entire image. Animates if <see cref="UseSprings"/> is <see langword="true"/>.
        /// </summary>
        public void ResetView()
        {
            _spring.SetTarget(0, 0, 1.0);
            if (!UseSprings) _spring.SnapToTarget();
            StartAnimation();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zooms about a logical point. Animates if <see cref="UseSprings"/> is <see langword="true"/>.
        /// </summary>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            SyncControllerViewportToSpringTarget();
            _controller.ZoomAboutLogicalPoint(factor, logicalX, logicalY);
            CaptureControllerViewportToSpringTarget();
            if (!UseSprings) _spring.SnapToTarget();
            StartAnimation();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Converts screen-space coordinates to logical (0–1 normalised) coordinates.</summary>
        public (double X, double Y) ElementToLogicalPoint(double screenX, double screenY)
            => _controller.Viewport.ElementToLogicalPoint(screenX, screenY);

        /// <summary>Converts logical coordinates to screen-space coordinates.</summary>
        public (double X, double Y) LogicalToElementPoint(double logicalX, double logicalY)
            => _controller.Viewport.LogicalToElementPoint(logicalX, logicalY);

        // ── Spring helpers ─────────────────────────────────────────────────────

        /// <summary>Writes the spring target state into the controller viewport (no events).</summary>
        private void SyncControllerViewportToSpringTarget()
        {
            _controller.Viewport.ViewportWidth = _spring.Width.Target;
            _controller.Viewport.ViewportOriginX = _spring.OriginX.Target;
            _controller.Viewport.ViewportOriginY = _spring.OriginY.Target;
        }

        /// <summary>Reads the controller viewport back into the spring target.</summary>
        private void CaptureControllerViewportToSpringTarget()
        {
            var vp = _controller.Viewport;
            _spring.SetTarget(vp.ViewportOriginX, vp.ViewportOriginY, vp.ViewportWidth);
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isVisible) return;

            var now = _stopwatch.Elapsed;
            var delta = now - _lastFrameTime;
            _lastFrameTime = now;
            if (delta.TotalMilliseconds > 100) delta = TimeSpan.FromMilliseconds(100);

            _dpiScale = _canvasView.CanvasSize.Width > 0 && Width > 0
                ? _canvasView.CanvasSize.Width / (float)Width
                : 1f;

            _controller.SetControlSize(e.Info.Width, e.Info.Height);

            // 1. Advance spring
            bool wasSettled = _spring.IsSettled;
            _spring.Update(delta.TotalSeconds);
            bool isSettled = _spring.IsSettled;

            // 2. Push spring current state into the controller viewport
            var (ox, oy, w) = _spring.GetCurrentState();
            _controller.Viewport.ViewportOriginX = ox;
            _controller.Viewport.ViewportOriginY = oy;
            _controller.Viewport.ViewportWidth = w;

            // 3. Fire ViewportChanged when spring is moving
            if (!isSettled || !wasSettled)
                ViewportChanged?.Invoke(this, EventArgs.Empty);

            // 4. Fire MotionFinished when spring just settled
            if (!wasSettled && isSettled)
                MotionFinished?.Invoke(this, EventArgs.Empty);

            // 5. Schedule tile loads and render
            bool hasPendingTiles = _controller.Update();
            _controller.Render(e.Surface.Canvas);

            // 6. Stop animation loop when fully idle
            if (isSettled && !hasPendingTiles)
                StopAnimation();
        }

        // ── Gesture Handling ───────────────────────────────────────────────────

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
            // Pan is direct manipulation — apply instantly (no spring delay)
            e.Handled = true;
            SyncControllerViewportToSpringTarget();
            _controller.Pan(e.Delta.X, e.Delta.Y);
            CaptureControllerViewportToSpringTarget();
            _spring.SnapToTarget();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
            StartAnimation();
        }

        private void OnTrackerPinch(object? sender, SKPinchGestureEventArgs e)
        {
            // Pinch is direct manipulation — apply instantly
            SyncControllerViewportToSpringTarget();
            _controller.ZoomAboutScreenPoint(e.ScaleDelta, e.FocalPoint.X, e.FocalPoint.Y);
            CaptureControllerViewportToSpringTarget();
            _spring.SnapToTarget();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
            StartAnimation();
        }

        private void OnTrackerDoubleTap(object? sender, SKTapGestureEventArgs e)
        {
            // Double-tap is animated — set spring target and let it animate
            SyncControllerViewportToSpringTarget();
            double factor = _spring.Width.Target < 0.95 ? 0.5 : 2.0;
            _controller.ZoomAboutScreenPoint(factor, e.Location.X, e.Location.Y);
            CaptureControllerViewportToSpringTarget();
            if (!UseSprings) _spring.SnapToTarget();
            StartAnimation();
        }

        private void OnTrackerScroll(object? sender, SKScrollGestureEventArgs e)
        {
            // Scroll zoom is animated
            double factor = e.Delta.Y > 0 ? 1.2 : 1.0 / 1.2;
            SyncControllerViewportToSpringTarget();
            _controller.ZoomAboutScreenPoint(factor, e.Location.X, e.Location.Y);
            CaptureControllerViewportToSpringTarget();
            if (!UseSprings) _spring.SnapToTarget();
            StartAnimation();
        }

        private void OnTrackerFlingUpdated(object? sender, SKFlingGestureEventArgs e)
        {
            // Fling provides per-frame deltas from the gesture tracker — apply instantly
            if (e.Delta == SKPoint.Empty) return;
            SyncControllerViewportToSpringTarget();
            _controller.Pan(e.Delta.X, e.Delta.Y);
            CaptureControllerViewportToSpringTarget();
            _spring.SnapToTarget();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
            StartAnimation();
        }

        // ── Keyboard Navigation ────────────────────────────────────────────────

        /// <summary>
        /// Handles keyboard navigation: arrow keys pan, +/= zoom in, -/_ zoom out, Home resets.
        /// </summary>
        public bool HandleKeyPress(string key)
        {
            if (_disposed) return false;

            double panStep = 50 * _dpiScale;
            const double zoomFactor = 1.5;

            switch (key)
            {
                case "Left": case "ArrowLeft":
                    SyncControllerViewportToSpringTarget();
                    _controller.Pan(panStep, 0);
                    CaptureControllerViewportToSpringTarget();
                    break;
                case "Right": case "ArrowRight":
                    SyncControllerViewportToSpringTarget();
                    _controller.Pan(-panStep, 0);
                    CaptureControllerViewportToSpringTarget();
                    break;
                case "Up": case "ArrowUp":
                    SyncControllerViewportToSpringTarget();
                    _controller.Pan(0, panStep);
                    CaptureControllerViewportToSpringTarget();
                    break;
                case "Down": case "ArrowDown":
                    SyncControllerViewportToSpringTarget();
                    _controller.Pan(0, -panStep);
                    CaptureControllerViewportToSpringTarget();
                    break;
                case "Add": case "OemPlus": case "+": case "=":
                    SyncControllerViewportToSpringTarget();
                    _controller.ZoomAboutScreenPoint(zoomFactor,
                        _canvasView.CanvasSize.Width / 2,
                        _canvasView.CanvasSize.Height / 2);
                    CaptureControllerViewportToSpringTarget();
                    break;
                case "Subtract": case "OemMinus": case "-": case "_":
                    SyncControllerViewportToSpringTarget();
                    _controller.ZoomAboutScreenPoint(1.0 / zoomFactor,
                        _canvasView.CanvasSize.Width / 2,
                        _canvasView.CanvasSize.Height / 2);
                    CaptureControllerViewportToSpringTarget();
                    break;
                case "Home":
                    ResetView();
                    return true;
                default:
                    return false;
            }

            if (!UseSprings) _spring.SnapToTarget();
            StartAnimation();
            return true;
        }

        // ── Disposal ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
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
            _controller.ImageOpenSucceeded -= _onImageOpenSucceeded;
            _controller.ImageOpenFailed -= _onImageOpenFailed;

            _tracker.PanDetected -= OnTrackerPan;
            _tracker.PinchDetected -= OnTrackerPinch;
            _tracker.DoubleTapDetected -= OnTrackerDoubleTap;
            _tracker.ScrollDetected -= OnTrackerScroll;
            _tracker.FlingUpdated -= OnTrackerFlingUpdated;
            _tracker.Dispose();

            Loaded -= OnViewLoaded;
            Unloaded -= OnViewUnloaded;

            _controller.Dispose();
        }
    }
}
