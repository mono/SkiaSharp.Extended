using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;

namespace SkiaSharp.Extended.UI.Controls
{
    /// <summary>
    /// A MAUI view that combines an <see cref="SKCanvasView"/> with an <see cref="SKGestureTracker"/>,
    /// enabling pan, pinch, fling, double-tap zoom, and scroll zoom on any SkiaSharp content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this control when you want gesture-driven pan/zoom over custom-drawn content.
    /// For deep zoom image support, see <c>SKDeepZoomView</c> in the <c>SkiaSharp.Extended.UI.Maui.DeepZoom</c> package.
    /// </para>
    /// <para>Example usage:</para>
    /// <code>
    /// var gestureView = new SKGestureView();
    /// gestureView.PaintSurface += (s, e) =>
    /// {
    ///     var canvas = e.Surface.Canvas;
    ///     canvas.Clear(SKColors.White);
    ///     canvas.SetMatrix(gestureView.GestureTracker.GetMatrix());
    ///     // draw your content here
    /// };
    /// </code>
    /// </remarks>
    public class SKGestureView : ContentView, IDisposable
    {
        private readonly SKCanvasView _canvasView;
        private readonly SKGestureTracker _tracker;
        private bool _disposed;
        private bool _isVisible = true;

        /// <summary>Initializes a new <see cref="SKGestureView"/> with default gesture options.</summary>
        public SKGestureView() : this(new SKGestureTrackerOptions()) { }

        /// <summary>Initializes a new <see cref="SKGestureView"/> with the specified gesture options.</summary>
        /// <param name="options">Options controlling which gestures are recognized and their parameters.</param>
        public SKGestureView(SKGestureTrackerOptions options)
        {
            _canvasView = new SKCanvasView { EnableTouchEvents = true };
            _canvasView.PaintSurface += OnCanvasPaintSurface;
            _canvasView.Touch += OnCanvasTouch;
            Content = _canvasView;

            _tracker = new SKGestureTracker(options);
            _tracker.TransformChanged += OnTrackerTransformChanged;

            Loaded += (_, _) => _isVisible = true;
            Unloaded += (_, _) => _isVisible = false;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// The underlying gesture tracker. Use this to configure gesture options, subscribe to
        /// individual gesture events (tap, long press, etc.), or read the current transform.
        /// </summary>
        public SKGestureTracker GestureTracker => _tracker;

        /// <summary>
        /// Raised when the surface needs to be painted. Handle this to draw your content.
        /// Use <see cref="SKGestureTracker.Matrix"/> to apply the current pan/zoom transform.
        /// </summary>
        public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

        // ── Gesture Handling ───────────────────────────────────────────────────

        private void OnCanvasTouch(object? sender, SKTouchEventArgs e)
        {
            if (!_isVisible) return;
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

        private void OnTrackerTransformChanged(object? sender, EventArgs e)
        {
            if (!_isVisible) return;
            _canvasView.InvalidateSurface();
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void OnCanvasPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isVisible) return;
            PaintSurface?.Invoke(this, e);
        }

        // ── Disposal ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _canvasView.PaintSurface -= OnCanvasPaintSurface;
            _canvasView.Touch -= OnCanvasTouch;

            _tracker.TransformChanged -= OnTrackerTransformChanged;
            _tracker.Dispose();
        }
    }
}
