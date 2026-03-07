# Deep Zoom for Blazor

In Blazor WebAssembly, you compose `DeepZoomController` and `SKGestureTracker` yourself — there is no single wrapper component like the MAUI `SKDeepZoomView`. This gives you full control over the rendering loop, pointer event wiring, and layout.

## Quick Start

```razor
@page "/deepzoom"
@implements IDisposable
@inject HttpClient Http
@using SkiaSharp.Extended
@using SkiaSharp.Extended.DeepZoom

<div @onpointerdown="OnPointerDown"
     @onpointermove="OnPointerMove"
     @onpointerup="OnPointerUp"
     @onpointercancel="OnPointerCancel"
     @onwheel="OnWheel"
     @onwheel:preventDefault
     style="touch-action: none; cursor: grab; border: 1px solid #ccc; user-select: none;">
    <SKCanvasView @ref="_canvas" OnPaintSurface="OnPaintSurface"
                  style="width: 100%; height: 600px;" />
</div>

@code {
    private SKCanvasView? _canvas;
    private DeepZoomController? _controller;
    private bool _suppressSync;
    private float _displayScale = 1f;

    private readonly SKGestureTracker _tracker = new(new SKGestureTrackerOptions
    {
        IsRotateEnabled = false,
        IsDoubleTapZoomEnabled = true,
        IsScrollZoomEnabled = true,
        IsFlingEnabled = true,
        MinScale = 1f,
        MaxScale = 32f,
    });

    protected override void OnInitialized()
    {
        _tracker.TransformChanged += OnTransformChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _controller = new DeepZoomController();
        _controller.ImageOpenSucceeded += (_, _) => _tracker.Reset();
        _controller.InvalidateRequired += (_, _) => InvokeAsync(() => _canvas?.Invalidate());

        var xml = await Http.GetStringAsync("deepzoom/image.dzi");
        var baseUrl = new Uri(Http.BaseAddress!, "deepzoom/image_files/").ToString();
        var tileSource = DziTileSource.Parse(xml, baseUrl);
        _controller.Load(tileSource, new HttpTileFetcher(new HttpClient()));

        await InvokeAsync(StateHasChanged);
    }

    // ── Tracker → Viewport sync ────────────────────────────────────────

    private void OnTransformChanged(object? sender, EventArgs e)
    {
        if (_suppressSync || _controller == null) return;
        var controlW = _controller.Viewport.ControlWidth;
        if (controlW <= 0) { InvokeAsync(() => _canvas?.Invalidate()); return; }

        // Translate tracker (scale + pixel offset) → deep zoom viewport
        double scale = _tracker.Scale;
        _controller.Viewport.ViewportWidth = 1.0 / scale;
        _controller.Viewport.ViewportOriginX = -_tracker.Offset.X / (controlW * scale);
        _controller.Viewport.ViewportOriginY = -_tracker.Offset.Y / (controlW * scale);
        _controller.Viewport.Constrain();

        // Sync tracker back to the (possibly constrained) viewport
        var vp = _controller.Viewport;
        float ns = (float)(1.0 / vp.ViewportWidth);
        _suppressSync = true;
        _tracker.SetTransform(ns, 0f, new SKPoint(
            (float)(-vp.ViewportOriginX * controlW * ns),
            (float)(-vp.ViewportOriginY * controlW * ns)));
        _suppressSync = false;

        InvokeAsync(() => _canvas?.Invalidate());
    }

    // ── Rendering ──────────────────────────────────────────────────────

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        if (_controller == null) return;
        _displayScale = e.Info.Height / 600f; // CSS height
        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        e.Surface.Canvas.Clear(SKColors.White);
        _controller.Render(e.Surface.Canvas);
    }

    // ── Pointer events ─────────────────────────────────────────────────

    private SKPoint ToCanvas(PointerEventArgs e) =>
        new((float)e.OffsetX * _displayScale, (float)e.OffsetY * _displayScale);

    private void OnPointerDown(PointerEventArgs e) =>
        _tracker.ProcessTouchDown(e.PointerId, ToCanvas(e), e.PointerType == "mouse");
    private void OnPointerMove(PointerEventArgs e) =>
        _tracker.ProcessTouchMove(e.PointerId, ToCanvas(e), e.Buttons > 0 || e.PointerType != "mouse");
    private void OnPointerUp(PointerEventArgs e) =>
        _tracker.ProcessTouchUp(e.PointerId, ToCanvas(e), e.PointerType == "mouse");
    private void OnPointerCancel(PointerEventArgs e) =>
        _tracker.ProcessTouchCancel(e.PointerId);
    private void OnWheel(WheelEventArgs e) =>
        _tracker.ProcessMouseWheel(
            new((float)e.OffsetX * _displayScale, (float)e.OffsetY * _displayScale),
            0, e.DeltaY < 0 ? 1f : -1f);

    public void Dispose()
    {
        _tracker.TransformChanged -= OnTransformChanged;
        _tracker.Dispose();
        _controller?.Dispose();
    }
}
```

## How It Works

The pattern is the same one used inside `SKDeepZoomView` on MAUI — you're just doing the wiring yourself:

1. **Pointer events** are routed to `SKGestureTracker` via `ProcessTouchDown/Move/Up` and `ProcessMouseWheel`.
2. The tracker detects gestures and animates fling/zoom. On each frame it fires **`TransformChanged`**.
3. In your `TransformChanged` handler you translate the tracker's `Scale` and `Offset` into the controller's viewport coordinates, call `Viewport.Constrain()`, then sync the constrained result back to the tracker.
4. Call `_canvas.Invalidate()` to schedule a repaint, which calls `controller.Update()` and `controller.Render()`.

### Coordinate Translation

The tracker works in pixel-space (scale factor + pixel offset), while the deep zoom viewport works in normalised image-space (viewport width 0–1 + origin). The translation is:

```csharp
// Tracker → Viewport
viewportWidth  = 1.0 / scale
viewportOriginX = -offset.X / (controlWidth * scale)
viewportOriginY = -offset.Y / (controlWidth * scale)

// Viewport → Tracker (inverse)
scale   = 1.0 / viewportWidth
offsetX = -viewportOriginX * controlWidth * scale
offsetY = -viewportOriginY * controlWidth * scale
```

Both X and Y use `controlWidth` (not height) because deep zoom normalises coordinates to the image width.

### The `_suppressSync` Flag

When you call `_tracker.SetTransform(...)` to sync the constrained viewport back, the tracker fires `TransformChanged` again. The `_suppressSync` flag prevents this from re-entering your handler. Since Blazor component code runs on a single synchronisation context, this is safe without locks.

## Display Scale

Blazor pointer events report coordinates in CSS pixels, but the SkiaSharp canvas renders in physical (device) pixels. You need to scale pointer coordinates:

```csharp
// Compute scale: physical canvas height / expected CSS height
_displayScale = canvasInfo.Height / 600f; // 600 = your CSS height in px

// Apply to every pointer event
var canvasPoint = new SKPoint((float)e.OffsetX * _displayScale, (float)e.OffsetY * _displayScale);
```

## Programmatic Navigation

```csharp
// Zoom in 2× at the center (animated via tracker)
var cx = (float)(_controller.Viewport.ControlWidth / 2);
var cy = (float)(_controller.Viewport.ControlHeight / 2);
_tracker.ZoomTo(2f, new SKPoint(cx, cy));

// Reset to full image
_tracker.Reset();
```

## Using SKGestureView

If you prefer a reusable component that handles the pointer-event wiring for you, the Blazor sample includes `SKGestureView.razor`:

```razor
<SKGestureView @ref="_gestureView" OnPaint="OnPaint" CssHeight="600" />

@code {
    private SKGestureView? _gestureView;

    private void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var tracker = _gestureView!.GestureTracker;
        // Use tracker.Scale, tracker.Offset, tracker.Matrix
        // to transform your drawing
    }
}
```

`SKGestureView` wraps an `SKCanvasView` with pointer-event routing and automatic invalidation on `TransformChanged`. You can use it for any gesture-driven SkiaSharp content, not just deep zoom.

## Next Steps

- [Deep Zoom Core](deep-zoom.md) — Core classes shared by MAUI and Blazor
- [Deep Zoom for MAUI](deep-zoom-maui.md) — MAUI `SKDeepZoomView` control
- [Gestures](gestures.md) — SKGestureTracker documentation
- [Animation](animation.md) — Animation utilities used by the gesture system
- [API Reference — DeepZoomController](xref:SkiaSharp.Extended.DeepZoom.DeepZoomController)
- [Blazor Sample](https://github.com/mono/SkiaSharp.Extended/tree/main/samples/SkiaSharpDemo.Blazor/Pages/DeepZoom.razor)
