# Blazor Lottie Animations

Play Lottie animations in Blazor WebAssembly apps with [`SKLottieView`](xref:SkiaSharp.Extended.UI.Blazor.Controls.SKLottieView). This component wraps the shared [`SKLottiePlayer`](lottie-player.md) engine with a Blazor-friendly API — just point it at a JSON URL and it handles loading, frame updates, and rendering.

For building custom animated canvases beyond Lottie, see [`SKAnimatedCanvasView`](#custom-animations-with-skanimatedcanvasview) below.

## Quick Start

### 1. Add the NuGet package

```bash
dotnet add package SkiaSharp.Extended.UI.Blazor
```

### 2. Add the namespace

In your `_Imports.razor`:

```razor
@using SkiaSharp.Extended.UI.Blazor.Controls
```

### 3. Add the component

```razor
<SKLottieView Source="animations/trophy.json"
              RepeatCount="-1"
              style="width: 300px; height: 300px;" />
```

That's it — the animation loads from the URL and starts playing automatically.

## Playback Controls

### Repeat modes

```razor
@* Loop forever, restarting each time *@
<SKLottieView Source="animation.json"
              RepeatMode="SKLottieRepeatMode.Restart"
              RepeatCount="-1" />

@* Ping-pong 3 times *@
<SKLottieView Source="animation.json"
              RepeatMode="SKLottieRepeatMode.Reverse"
              RepeatCount="3" />

@* Play once *@
<SKLottieView Source="animation.json"
              RepeatCount="0" />
```

### Speed and direction

```razor
<SKLottieView Source="animation.json"
              AnimationSpeed="2.0"
              RepeatCount="-1" />

@* Negative speed plays in reverse *@
<SKLottieView Source="animation.json"
              AnimationSpeed="-1.0"
              RepeatCount="-1" />
```

### Pause and resume

```razor
<SKLottieView Source="animation.json"
              IsAnimationEnabled="@isPlaying"
              RepeatCount="-1" />

<button @onclick="() => isPlaying = !isPlaying">
    @(isPlaying ? "Pause" : "Play")
</button>

@code {
    private bool isPlaying = true;
}
```

### Restart programmatically

Use an `@ref` to call `Restart()`:

```razor
<SKLottieView @ref="lottieView"
              Source="animation.json"
              RepeatCount="-1" />

<button @onclick="() => lottieView?.Restart()">Restart</button>

@code {
    private SKLottieView? lottieView;
}
```

## Reading Playback State

Access read-only state through the component reference:

```razor
<SKLottieView @ref="lottieView"
              Source="animation.json"
              RepeatCount="-1"
              AnimationUpdated="StateHasChanged" />

@if (lottieView?.HasAnimation == true)
{
    <p>
        Duration: @lottieView.Duration.TotalSeconds.ToString("0.00")s |
        Progress: @lottieView.Progress.TotalSeconds.ToString("0.00")s |
        @(lottieView.IsComplete ? "Complete ✓" : "Playing")
    </p>
}

@code {
    private SKLottieView? lottieView;
}
```

> **Tip:** Bind `AnimationUpdated` to `StateHasChanged` so the UI refreshes with each frame. Without this, the displayed progress won't update live.

## Events

| Event | Type | Description |
| :---- | :--- | :---------- |
| `AnimationLoaded` | `EventCallback` | The animation JSON was loaded and parsed successfully |
| `AnimationCompleted` | `EventCallback` | All repeats finished (never fires for infinite loops) |
| `AnimationFailed` | `EventCallback<Exception?>` | Loading or parsing failed |
| `AnimationUpdated` | `EventCallback` | Fires after each frame update |

```razor
<SKLottieView Source="animation.json"
              AnimationLoaded="OnLoaded"
              AnimationFailed="OnFailed"
              AnimationCompleted="OnCompleted" />

@code {
    private void OnLoaded() => Console.WriteLine("Loaded!");
    private void OnFailed(Exception? ex) => Console.WriteLine($"Failed: {ex?.Message}");
    private void OnCompleted() => Console.WriteLine("Done!");
}
```

## Parameters Reference

| Parameter | Type | Default | Description |
| :-------- | :--- | :------ | :---------- |
| `Source` | `string?` | `null` | URL of the Lottie JSON file to load |
| `RepeatMode` | `SKLottieRepeatMode` | `Restart` | `Restart` or `Reverse` (ping-pong) |
| `RepeatCount` | `int` | `-1` | Additional plays after the first (`0` = once, `-1` = infinite) |
| `AnimationSpeed` | `double` | `1.0` | Speed multiplier (negative = reverse) |
| `IsAnimationEnabled` | `bool` | `true` | Play/pause the animation loop |

Additional HTML attributes (like `style` and `class`) are forwarded to the underlying canvas element.

## Read-Only State (via `@ref`)

| Property | Type | Description |
| :------- | :--- | :---------- |
| `IsLoading` | `bool` | Whether the animation is currently being fetched |
| `HasAnimation` | `bool` | Whether an animation is loaded and ready |
| `Duration` | `TimeSpan` | Total animation duration |
| `Progress` | `TimeSpan` | Current playback position |
| `IsComplete` | `bool` | Whether all repeats have finished |

## Sizing

The component renders an HTML `<canvas>` element. Set the size using standard CSS:

```razor
@* Fixed size *@
<SKLottieView Source="animation.json" style="width: 300px; height: 300px;" />

@* Fill parent container *@
<div style="width: 100%; height: 400px;">
    <SKLottieView Source="animation.json" style="width: 100%; height: 100%;" />
</div>
```

## Custom Animations with SKAnimatedCanvasView

For animations beyond Lottie, [`SKAnimatedCanvasView`](xref:SkiaSharp.Extended.UI.Blazor.Controls.SKAnimatedCanvasView) provides a frame loop with a SkiaSharp canvas. It runs at ~60 fps and calls your update and paint callbacks each frame.

```razor
@using SkiaSharp
@using SkiaSharp.Extended.UI.Blazor.Controls

<SKAnimatedCanvasView OnUpdate="HandleUpdate"
                      OnPaintSurface="HandlePaint"
                      style="width: 400px; height: 400px;" />

@code {
    private float angle = 0;

    private void HandleUpdate(TimeSpan delta)
    {
        angle += (float)(delta.TotalSeconds * 90);  // 90° per second
    }

    private void HandlePaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.RotateDegrees(angle, e.Info.Width / 2f, e.Info.Height / 2f);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue };
        canvas.DrawRect(
            e.Info.Width / 2f - 50, e.Info.Height / 2f - 50,
            100, 100, paint);
    }
}
```

### SKAnimatedCanvasView Parameters

| Parameter | Type | Default | Description |
| :-------- | :--- | :------ | :---------- |
| `IsAnimationEnabled` | `bool` | `true` | Start/stop the frame loop |
| `OnUpdate` | `EventCallback<TimeSpan>` | — | Called each frame with delta time |
| `OnPaintSurface` | `EventCallback<SKPaintSurfaceEventArgs>` | — | Called to render each frame |

> **Important:** `OnPaintSurface` handlers must be synchronous. The canvas is only valid during the paint callback.

## Learn More

- [Lottie Player](lottie-player.md) — The shared playback engine behind both MAUI and Blazor views
- [MAUI Lottie Animations](lottie.md) — Use Lottie in .NET MAUI apps
- [Lottie by Airbnb](https://airbnb.design/lottie/) — Official project page
- [LottieFiles](https://lottiefiles.com/) — Free and premium animations
- [API Reference](xref:SkiaSharp.Extended.UI.Blazor.Controls.SKLottieView) — Full method documentation
