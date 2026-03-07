# Animated Surface View

[`SKAnimatedSurfaceView`](xref:SkiaSharp.Extended.UI.Blazor.Controls.SKAnimatedSurfaceView) is a Blazor component that drives a ~60 fps frame-update loop and renders onto a SkiaSharp canvas. It mirrors the MAUI [`SKAnimatedSurfaceView`](xref:SkiaSharp.Extended.UI.Controls.SKAnimatedSurfaceView) pattern.

## Quick Start

### 1. Add the NuGet package

```bash
dotnet add package SkiaSharp.Extended.UI.Blazor
```

### 2. Add the namespace

In your `_Imports.razor`:

```cshtml-razor
@using SkiaSharp.Extended.UI.Blazor.Controls
```

### 3. Add the component

```cshtml-razor
<SKAnimatedSurfaceView OnUpdate="HandleUpdate"
                       OnPaintSurface="HandlePaint"
                       style="width: 400px; height: 400px;" />

@code {
    private float angle = 0f;

    private void HandleUpdate(TimeSpan delta)
    {
        angle += (float)(delta.TotalSeconds * 90);  // 90° per second
    }

    private void HandlePaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var size = new SKSize(e.Info.Width, e.Info.Height);

        canvas.Clear(SKColors.White);
        canvas.RotateDegrees(angle, size.Width / 2f, size.Height / 2f);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue };
        canvas.DrawRect(size.Width / 2f - 50, size.Height / 2f - 50, 100, 100, paint);
    }
}
```

## Pausing and Resuming

```cshtml-razor
<SKAnimatedSurfaceView IsAnimationEnabled="@isPlaying"
                       OnUpdate="HandleUpdate"
                       OnPaintSurface="HandlePaint"
                       style="width: 400px; height: 400px;" />

<button @onclick="() => isPlaying = !isPlaying">
    @(isPlaying ? "Pause" : "Play")
</button>

@code {
    private bool isPlaying = true;
}
```

## Subclassing

For reusable animated components, subclass `SKAnimatedSurfaceView` and override `UpdateAsync`:

```csharp
public class SpinningSquare : SKAnimatedSurfaceView
{
    private float angle = 0f;

    protected override Task UpdateAsync(TimeSpan deltaTime)
    {
        angle += (float)(deltaTime.TotalSeconds * 90);
        return base.UpdateAsync(deltaTime);  // fires OnUpdate callback
    }
}
```

## Parameters Reference

| Parameter | Type | Default | Description |
| :-------- | :--- | :------ | :---------- |
| `IsAnimationEnabled` | `bool` | `true` | Start/stop the frame loop |
| `OnUpdate` | `Action<TimeSpan>?` | `null` | Called each frame with the elapsed time since the previous frame |
| `OnPaintSurface` | `Action<SKPaintSurfaceEventArgs>?` | `null` | Called to render each frame |

Additional HTML attributes (like `style` and `class`) are forwarded to the underlying canvas element.

## Notes

- The frame loop targets ~60 fps using a `PeriodicTimer`. Actual frame rate depends on the browser and workload.
- The component implements `IAsyncDisposable` and cleans up the frame loop automatically when removed from the page.

## Learn More

- [SkiaSharp Blazor Views](https://learn.microsoft.com/dotnet/api/skiasharp.views.blazor) — Underlying `SKCanvasView` documentation
