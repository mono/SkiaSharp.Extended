# SkiaSharp.Extended.UI.Blazor

A touch-enabled SkiaSharp canvas view for Blazor WebAssembly and Blazor Server.

## SKTouchCanvasView

Wraps the SkiaSharp Blazor canvas with unified touch/pointer event support. Browser pointer events (mouse, touch, stylus) are translated into `SKTouchEventArgs`, matching the MAUI `SKTouchAction` API for shared source compatibility.

```razor
<SKTouchCanvasView
    OnPaintSurface="OnPaint"
    Touch="OnTouch"
    EnableTouchEvents="true"
    style="width: 100%; height: 400px;" />

@code {
    private void OnPaint(SKPaintSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear(SKColors.White);
    }

    private void OnTouch(SKTouchEventArgs e)
    {
        Console.WriteLine($"{e.ActionType} at {e.Location} via {e.DeviceType}");
        e.Handled = true;
    }
}
```
