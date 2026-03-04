# Animated Surface View

[`SKAnimatedSurfaceView`](xref:SkiaSharp.Extended.UI.Blazor.Controls.SKAnimatedSurfaceView) is a Blazor component that drives a ~60 fps frame-update loop and renders onto a SkiaSharp canvas. It mirrors the MAUI [`SKAnimatedSurfaceView`](xref:SkiaSharp.Extended.UI.Controls.SKAnimatedSurfaceView) pattern and — like MAUI's control-template approach — lets the consumer choose the rendering backend at runtime.

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

    private void HandlePaint(SKSurface surface, SKSize size)
    {
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.RotateDegrees(angle, size.Width / 2f, size.Height / 2f);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue };
        canvas.DrawRect(size.Width / 2f - 50, size.Height / 2f - 50, 100, 100, paint);
    }
}
```

## Switching Rendering Backends

Set `UseGL="true"` to switch to GPU-accelerated rendering via [`SKGLView`](https://learn.microsoft.com/dotnet/api/skiasharp.views.blazor.skglview) (WebGL). The default is software rendering via [`SKCanvasView`](https://learn.microsoft.com/dotnet/api/skiasharp.views.blazor.skcanvasview).

This is the Blazor equivalent of the MAUI `PART_DrawingSurface` control-template pattern: the consumer decides the backend; the component works identically regardless.

```cshtml-razor
@* Software rendering (default) *@
<SKAnimatedSurfaceView UseGL="false"
                       OnUpdate="HandleUpdate"
                       OnPaintSurface="HandlePaint"
                       style="width: 400px; height: 400px;" />

@* GPU rendering (WebGL) *@
<SKAnimatedSurfaceView UseGL="true"
                       OnUpdate="HandleUpdate"
                       OnPaintSurface="HandlePaint"
                       style="width: 400px; height: 400px;" />
```

You can toggle `UseGL` at runtime:

```cshtml-razor
<div class="form-check form-switch">
    <input class="form-check-input" type="checkbox" @bind="useGL" />
    <label class="form-check-label">
        @(useGL ? "GPU (WebGL)" : "Software")
    </label>
</div>

<SKAnimatedSurfaceView UseGL="@useGL"
                       OnUpdate="HandleUpdate"
                       OnPaintSurface="HandlePaint"
                       style="width: 400px; height: 400px;" />

@code {
    private bool useGL = false;
    // ...
}
```

When `UseGL` changes, the component swaps the underlying canvas element and restarts the animation loop automatically.

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
| `UseGL` | `bool` | `false` | `true` = GPU rendering (`SKGLView`); `false` = software (`SKCanvasView`) |
| `OnUpdate` | `Action<TimeSpan>?` | `null` | Called each frame with the elapsed time since the previous frame |
| `OnPaintSurface` | `Action<SKSurface, SKSize>?` | `null` | Called to render each frame; works identically for both backends |

Additional HTML attributes (like `style` and `class`) are forwarded to the underlying canvas element.

## Notes

- The frame loop targets ~60 fps using a `PeriodicTimer`. Actual frame rate depends on the browser and workload.
- `OnPaintSurface` receives `SKSurface` and `SKSize` regardless of the rendering backend, making it easy to switch between software and GL without changing your drawing code.
- The component implements `IAsyncDisposable` and cleans up the frame loop automatically when removed from the page.

## Learn More

- [Lottie Animations (Blazor)](lottie-blazor.md) — Lottie playback built on top of this component
- [SkiaSharp Blazor Views](https://learn.microsoft.com/dotnet/api/skiasharp.views.blazor) — Underlying `SKCanvasView` and `SKGLView` documentation
