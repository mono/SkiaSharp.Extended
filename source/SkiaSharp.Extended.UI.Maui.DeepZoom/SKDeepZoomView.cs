using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Diagnostics;

namespace SkiaSharp.Extended.UI.Maui.DeepZoom
{
    /// <summary>
    /// A MAUI view that renders Deep Zoom images using SkiaSharp.
    /// Wraps DeepZoomController with gesture handling and BindableProperties.
    /// </summary>
    public class SKDeepZoomView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly DeepZoomController _controller;
        private readonly Stopwatch _stopwatch;
        private TimeSpan _lastFrameTime;
        private bool _disposed;
        private bool _isVisible = true;

        // Gesture state
        private double _lastPanX, _lastPanY;
        private double _lastPinchScale = 1.0;
        private float _dpiScale = 1f;

        // Named event handlers for proper cleanup
        private readonly EventHandler _onInvalidateRequired;
        private readonly EventHandler _onMotionFinished;
        private readonly EventHandler _onViewportChanged;
        private readonly EventHandler _onImageOpenSucceeded;
        private readonly EventHandler<Exception> _onImageOpenFailed;

        // Gesture recognizer references for disposal
        private PanGestureRecognizer? _panGesture;
        private PinchGestureRecognizer? _pinchGesture;
        private TapGestureRecognizer? _doubleTapGesture;

        public SKDeepZoomView()
        {
            _controller = new DeepZoomController();
            _stopwatch = Stopwatch.StartNew();
            _lastFrameTime = _stopwatch.Elapsed;

            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnPaintSurface;
            Content = _canvasView;

            // Wire up controller events using named handlers for proper cleanup
            _onInvalidateRequired = (s, e) =>
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            _onMotionFinished = (s, e) => MotionFinished?.Invoke(this, EventArgs.Empty);
            _onViewportChanged = (s, e) => ViewportChanged?.Invoke(this, EventArgs.Empty);
            _onImageOpenSucceeded = (s, e) => ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
            _onImageOpenFailed = (s, e) => ImageOpenFailed?.Invoke(this, e);

            _controller.InvalidateRequired += _onInvalidateRequired;
            _controller.MotionFinished += _onMotionFinished;
            _controller.ViewportChanged += _onViewportChanged;
            _controller.ImageOpenSucceeded += _onImageOpenSucceeded;
            _controller.ImageOpenFailed += _onImageOpenFailed;

            // Set up gestures
            SetupGestures();

            // Suppress animation when not visible
            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        private void OnViewLoaded(object? sender, EventArgs e) => _isVisible = true;
        private void OnViewUnloaded(object? sender, EventArgs e) => _isVisible = false;

        // --- BindableProperties ---

        public static readonly BindableProperty UseSpringsProperty =
            BindableProperty.Create(nameof(UseSprings), typeof(bool), typeof(SKDeepZoomView), true,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._controller.UseSprings = (bool)n);

        public static readonly BindableProperty ShowTileBordersProperty =
            BindableProperty.Create(nameof(ShowTileBorders), typeof(bool), typeof(SKDeepZoomView), false,
                propertyChanged: (b, o, n) => ((SKDeepZoomView)b)._controller.ShowTileBorders = (bool)n);

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

        // --- Read-only properties ---

        /// <summary>Current viewport width (1.0 = full image fits).</summary>
        public double ViewportWidth => _controller.Viewport.ViewportWidth;

        /// <summary>Current viewport origin X.</summary>
        public double ViewportOriginX => _controller.Viewport.ViewportOriginX;

        /// <summary>Current viewport origin Y.</summary>
        public double ViewportOriginY => _controller.Viewport.ViewportOriginY;

        /// <summary>Image aspect ratio (width/height), or 0 if not loaded.</summary>
        public double AspectRatio => _controller.AspectRatio;

        /// <summary>Whether the view is idle (no pending loads, no animation).</summary>
        public bool IsIdle => _controller.IsIdle;

        /// <summary>The underlying controller for advanced usage.</summary>
        public DeepZoomController Controller => _controller;

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
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Zooms to show the entire image.
        /// </summary>
        public void ResetView()
        {
            _controller.ResetView();
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Zooms about a logical point.
        /// </summary>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            _controller.ZoomAboutLogicalPoint(factor, logicalX, logicalY);
            _canvasView.InvalidateSurface();
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

            // Store scale factor for gesture coordinate conversion
            _dpiScale = _canvasView.CanvasSize.Width > 0 && Width > 0
                ? _canvasView.CanvasSize.Width / (float)Width
                : 1f;

            _controller.SetControlSize(e.Info.Width, e.Info.Height);
            bool needsRepaint = _controller.Update(delta);
            _controller.Render(e.Surface.Canvas);

            if (needsRepaint && _isVisible)
            {
                MainThread.BeginInvokeOnMainThread(() => _canvasView.InvalidateSurface());
            }
        }

        // --- Gestures ---

        private void SetupGestures()
        {
            _panGesture = new PanGestureRecognizer();
            _panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(_panGesture);

            _pinchGesture = new PinchGestureRecognizer();
            _pinchGesture.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(_pinchGesture);

            _doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            _doubleTapGesture.Tapped += OnDoubleTapped;
            GestureRecognizers.Add(_doubleTapGesture);
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _lastPanX = 0;
                    _lastPanY = 0;
                    break;
                case GestureStatus.Running:
                    double dx = (e.TotalX - _lastPanX) * _dpiScale;
                    double dy = (e.TotalY - _lastPanY) * _dpiScale;
                    _lastPanX = e.TotalX;
                    _lastPanY = e.TotalY;
                    _controller.Pan(dx, dy);
                    _canvasView.InvalidateSurface();
                    break;
            }
        }

        private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _lastPinchScale = 1.0;
                    break;
                case GestureStatus.Running:
                    double scaleChange = e.Scale / _lastPinchScale;
                    _lastPinchScale = e.Scale;

                    // Zoom about the pinch center (convert DIPs to pixels)
                    double centerX = Width * e.ScaleOrigin.X * _dpiScale;
                    double centerY = Height * e.ScaleOrigin.Y * _dpiScale;
                    _controller.ZoomAboutScreenPoint(scaleChange, centerX, centerY);
                    _canvasView.InvalidateSurface();
                    break;
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.HasValue)
            {
                _controller.ZoomAboutScreenPoint(2.0, point.Value.X * _dpiScale, point.Value.Y * _dpiScale);
                _canvasView.InvalidateSurface();
            }
        }

        // --- Disposal ---

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _canvasView.PaintSurface -= OnPaintSurface;
            _controller.InvalidateRequired -= _onInvalidateRequired;
            _controller.MotionFinished -= _onMotionFinished;
            _controller.ViewportChanged -= _onViewportChanged;
            _controller.ImageOpenSucceeded -= _onImageOpenSucceeded;
            _controller.ImageOpenFailed -= _onImageOpenFailed;

            // Unsubscribe gesture recognizer events
            if (_panGesture != null) _panGesture.PanUpdated -= OnPanUpdated;
            if (_pinchGesture != null) _pinchGesture.PinchUpdated -= OnPinchUpdated;
            if (_doubleTapGesture != null) _doubleTapGesture.Tapped -= OnDoubleTapped;

            // Unsubscribe lifecycle events
            Loaded -= OnViewLoaded;
            Unloaded -= OnViewUnloaded;

            _controller.Dispose();
        }
    }
}
