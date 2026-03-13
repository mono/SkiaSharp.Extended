using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Extended.DeepZoom;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;

namespace SkiaSharp.Extended.UI.Maui.DeepZoom
{
    /// <summary>
    /// A MAUI view that renders Deep Zoom images with gesture navigation.
    /// </summary>
    /// <remarks>
    /// <para>This control composes two independent layers:</para>
    /// <list type="bullet">
    ///   <item><term><see cref="SKGestureTracker"/></term><description>
    ///     Detects and animates pan, pinch, double-tap zoom, scroll zoom, and fling gestures.
    ///     All animation (fling deceleration, double-tap zoom easing) lives here.
    ///   </description></item>
    ///   <item><term><see cref="SKDeepZoomController"/></term><description>
    ///     Handles tile loading and rendering — no animation, no gesture awareness.
    ///   </description></item>
    /// </list>
    /// <para>
    /// The data flow is: <c>Touch events → SKGestureTracker → TransformChanged → SKDeepZoomController.Viewport → Render</c>
    /// </para>
    /// </remarks>
    public class SKDeepZoomView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly SKDeepZoomController _controller;
        private readonly SKGestureTracker _tracker;
        private bool _suppressTransformSync;
        private bool _disposed;
        private bool _isVisible = true;
        private float _dpiScale = 1f;

        // Named event handlers for proper cleanup
        private readonly EventHandler _onInvalidateRequired;
        private readonly EventHandler _onImageOpenSucceeded;
        private readonly EventHandler<Exception> _onImageOpenFailed;

        /// <summary>Initializes a new <see cref="SKDeepZoomView"/>.</summary>
        public SKDeepZoomView()
        {
            _controller = new SKDeepZoomController();

            _canvasView = new SKCanvasView
            {
                EnableTouchEvents = true,
            };
            _canvasView.PaintSurface += OnPaintSurface;
            _canvasView.Touch += OnCanvasTouch;
            Content = _canvasView;

            // Wire controller events
            _onInvalidateRequired = (s, e) => _canvasView.InvalidateSurface();
            _onImageOpenSucceeded = (s, e) =>
            {
                // Sync tracker from the fit viewport the controller set up (don't reset to scale=1)
                SyncTrackerFromViewport();
                ImageOpenSucceeded?.Invoke(this, EventArgs.Empty);
            };
            _onImageOpenFailed = (s, e) => ImageOpenFailed?.Invoke(this, e);

            _controller.InvalidateRequired += _onInvalidateRequired;
            _controller.ImageOpenSucceeded += _onImageOpenSucceeded;
            _controller.ImageOpenFailed += _onImageOpenFailed;

            // Gesture tracker — owns all gesture detection AND gesture animation
            // (fling deceleration, double-tap zoom easing, scroll zoom)
            _tracker = new SKGestureTracker(new SKGestureTrackerOptions
            {
                IsRotateEnabled = false,       // Deep zoom doesn't use rotation
                IsDoubleTapZoomEnabled = true, // Tracker animates double-tap zoom via SKAnimationTimer
                IsScrollZoomEnabled = true,    // Tracker handles scroll/wheel zoom
                IsFlingEnabled = true,         // Tracker animates fling deceleration
                MinScale = 0.001f,             // Zoom-out floor enforced by viewport MaxViewportWidth via Constrain()
                MaxScale = 32f,                // 32× zoom limit
            });
            _tracker.TransformChanged += OnTrackerTransformChanged;

            Loaded += OnViewLoaded;
            Unloaded += OnViewUnloaded;
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void OnViewLoaded(object? sender, EventArgs e) => _isVisible = true;
        private void OnViewUnloaded(object? sender, EventArgs e) => _isVisible = false;

        // ── BindableProperties ─────────────────────────────────────────────────

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
                    view._canvasView.InvalidateSurface();
                });

        /// <summary>URI to a .dzi file. Setting this fetches and loads the image automatically.</summary>
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(string), typeof(SKDeepZoomView), null,
                BindingMode.OneWay, propertyChanged: OnSourceChanged);

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
        /// Viewport width (1.0 = full image fits). Setting this instantly updates the viewport.
        /// </summary>
        public double ViewportWidth
        {
            get => _controller.Viewport.ViewportWidth;
            set => SetViewport(value, _controller.Viewport.ViewportOriginX, _controller.Viewport.ViewportOriginY);
        }

        /// <summary>Current viewport origin X.</summary>
        public double ViewportOriginX
        {
            get => _controller.Viewport.ViewportOriginX;
            set => SetViewport(_controller.Viewport.ViewportWidth, value, _controller.Viewport.ViewportOriginY);
        }

        /// <summary>Current viewport origin Y.</summary>
        public double ViewportOriginY
        {
            get => _controller.Viewport.ViewportOriginY;
            set => SetViewport(_controller.Viewport.ViewportWidth, _controller.Viewport.ViewportOriginX, value);
        }

        /// <summary>Image aspect ratio (width/height), or 0 if not loaded.</summary>
        public double AspectRatio => _controller.AspectRatio;

        /// <summary>Whether the view is idle (no pending tile loads and no active gesture animation).</summary>
        public bool IsIdle => !_tracker.IsZoomAnimating && !_tracker.IsFlinging && _controller.IsIdle;

        /// <summary>The underlying controller for advanced tile/render access.</summary>
        public SKDeepZoomController Controller => _controller;

        /// <summary>The underlying gesture tracker for advanced configuration.</summary>
        public SKGestureTracker GestureTracker => _tracker;

        // ── Events ─────────────────────────────────────────────────────────────

        /// <summary>Fired when the image source loads successfully.</summary>
        public event EventHandler? ImageOpenSucceeded;

        /// <summary>Fired when the image source fails to load.</summary>
        public event EventHandler<Exception>? ImageOpenFailed;

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

            SKDeepZoomHttpTileFetcher? fetcher = null;
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

                var tileSource = SKDeepZoomImageSource.Parse(stream);
                tileSource.TilesBaseUri = tilesBase;
                if (!string.IsNullOrEmpty(parsed.Query))
                    tileSource.TilesQueryString = parsed.Query;

                if (_disposed || _sourceCts != activeCts || ct.IsCancellationRequested) return;

                fetcher = new SKDeepZoomHttpTileFetcher();
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
        public void Load(SKDeepZoomImageSource tileSource, ISKDeepZoomTileFetcher fetcher)
        {
            _controller.Load(tileSource, fetcher);
            // ImageOpenSucceeded resets the tracker; trigger an initial render
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Loads a DZC collection source with the specified fetcher.
        /// Populates <see cref="SubImages"/> for multi-image mosaic display.
        /// </summary>
        public void Load(SKDeepZoomCollectionSource tileSource, ISKDeepZoomTileFetcher fetcher)
        {
            _controller.Load(tileSource, fetcher);
            _canvasView.InvalidateSurface();
        }

        /// <summary>Sub-images when a DZC collection is loaded. Empty for single DZI images.</summary>
        public IReadOnlyList<SKDeepZoomSubImage> SubImages => _controller.SubImages;

        /// <summary>
        /// Sets the viewport instantly (no animation).
        /// </summary>
        public void SetViewport(double viewportWidth, double originX, double originY)
        {
            _controller.Viewport.ViewportWidth = viewportWidth;
            _controller.Viewport.ViewportOriginX = originX;
            _controller.Viewport.ViewportOriginY = originY;
            _controller.Viewport.Constrain();
            SyncTrackerFromViewport();
            ViewportChanged?.Invoke(this, EventArgs.Empty);
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Zooms to show the entire image instantly.
        /// </summary>
        public void ResetView()
        {
            _controller.ResetView();
            SyncTrackerFromViewport();
        }

        /// <summary>
        /// Zooms by <paramref name="factor"/> about a logical point, with smooth animation.
        /// </summary>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            var (sx, sy) = _controller.Viewport.LogicalToElementPoint(logicalX, logicalY);
            _tracker.ZoomTo((float)factor, new SKPoint((float)sx, (float)sy));
            // Tracker fires TransformChanged on each animation frame
        }

        /// <summary>Converts screen-space coordinates to logical (0–1 normalised) coordinates.</summary>
        public (double X, double Y) ElementToLogicalPoint(double screenX, double screenY)
            => _controller.Viewport.ElementToLogicalPoint(screenX, screenY);

        /// <summary>Converts logical coordinates to screen-space coordinates.</summary>
        public (double X, double Y) LogicalToElementPoint(double logicalX, double logicalY)
            => _controller.Viewport.LogicalToElementPoint(logicalX, logicalY);

        // ── Tracker → Viewport sync ────────────────────────────────────────────

        /// <summary>
        /// Translates the gesture tracker's current scale/offset into deep zoom viewport coordinates,
        /// applies constraints, then syncs the tracker back to the constrained state.
        /// </summary>
        private void OnTrackerTransformChanged(object? sender, EventArgs e)
        {
            if (_suppressTransformSync) return;

            var controlW = _controller.Viewport.ControlWidth;
            if (controlW <= 0)
            {
                // Control size not known yet; just invalidate for first paint
                _canvasView.InvalidateSurface();
                return;
            }

            // Translate tracker (scale + pixel offset) → deep zoom viewport (logical coordinates)
            double scale = _tracker.Scale;
            _controller.Viewport.ViewportWidth = 1.0 / scale;
            _controller.Viewport.ViewportOriginX = -_tracker.Offset.X / (controlW * scale);
            _controller.Viewport.ViewportOriginY = -_tracker.Offset.Y / (controlW * scale);
            _controller.Viewport.Constrain();

            // Sync tracker back to the (possibly constrained) viewport so the tracker's
            // accumulated transform doesn't drift beyond the image bounds.
            SyncTrackerFromViewport();

            ViewportChanged?.Invoke(this, EventArgs.Empty);
            _canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Writes the current controller viewport back into the gesture tracker, suppressing the
        /// resulting <see cref="SKGestureTracker.TransformChanged"/> event to avoid re-entry.
        /// </summary>
        private void SyncTrackerFromViewport()
        {
            var vp = _controller.Viewport;
            float newScale = (float)(1.0 / vp.ViewportWidth);
            float offsetX = (float)(-vp.ViewportOriginX * vp.ControlWidth * newScale);
            float offsetY = (float)(-vp.ViewportOriginY * vp.ControlWidth * newScale);

            _suppressTransformSync = true;
            _tracker.SetTransform(newScale, 0f, new SKPoint(offsetX, offsetY));
            _suppressTransformSync = false;
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isVisible) return;

            _dpiScale = _canvasView.CanvasSize.Width > 0 && Width > 0
                ? _canvasView.CanvasSize.Width / (float)Width
                : 1f;

            _controller.SetControlSize(e.Info.Width, e.Info.Height);
            _controller.Update();
            _controller.Render(e.Surface.Canvas);
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

        // ── Keyboard Navigation ────────────────────────────────────────────────

        /// <summary>
        /// Handles keyboard navigation: arrow keys pan, +/= zoom in, -/_ zoom out, Home resets.
        /// </summary>
        public bool HandleKeyPress(string key)
        {
            if (_disposed) return false;

            var controlW = _controller.Viewport.ControlWidth;
            var controlH = _controller.Viewport.ControlHeight;

            double panStep = 50 * _dpiScale;
            const double zoomFactor = 1.5;

            switch (key)
            {
                case "Left": case "ArrowLeft":
                    _controller.Pan(panStep, 0);
                    SyncTrackerFromViewport();
                    break;
                case "Right": case "ArrowRight":
                    _controller.Pan(-panStep, 0);
                    SyncTrackerFromViewport();
                    break;
                case "Up": case "ArrowUp":
                    _controller.Pan(0, panStep);
                    SyncTrackerFromViewport();
                    break;
                case "Down": case "ArrowDown":
                    _controller.Pan(0, -panStep);
                    SyncTrackerFromViewport();
                    break;
                case "Add": case "OemPlus": case "+": case "=":
                    _tracker.ZoomTo((float)zoomFactor, new SKPoint((float)(controlW / 2), (float)(controlH / 2)));
                    return true;
                case "Subtract": case "OemMinus": case "-": case "_":
                    _tracker.ZoomTo((float)(1.0 / zoomFactor), new SKPoint((float)(controlW / 2), (float)(controlH / 2)));
                    return true;
                case "Home":
                    ResetView();
                    return true;
                default:
                    return false;
            }

            ViewportChanged?.Invoke(this, EventArgs.Empty);
            _canvasView.InvalidateSurface();
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

            _tracker.TransformChanged -= OnTrackerTransformChanged;
            _tracker.Dispose();

            Loaded -= OnViewLoaded;
            Unloaded -= OnViewUnloaded;

            _controller.Dispose();
        }
    }
}
