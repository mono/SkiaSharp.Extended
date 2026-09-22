# Lottie Player

[`SKLottiePlayer`](xref:SkiaSharp.Extended.SKLottiePlayer) is the platform-agnostic engine that drives Lottie animation playback. It manages timing, repeat logic, and rendering — you just feed it frames. Both the [MAUI `SKLottieView`](lottie-maui.md) and the [Blazor `SKLottieView`](lottie-blazor.md) are built on top of this player.

## When to Use the Player Directly

Most apps should use the higher-level view components for [MAUI](lottie-maui.md) or [Blazor](lottie-blazor.md). Use `SKLottiePlayer` directly when you need:

- A custom rendering host (WPF, Avalonia, console, tests)
- Full control over the frame loop
- Headless animation processing (e.g., exporting frames to images)

## Quick Start

```csharp
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Skottie;

// 1. Create the player
var player = new SKLottiePlayer
{
    Repeat = SKLottieRepeat.Restart(),   // loop forever
    AnimationSpeed = 1.0
};

// 2. Load a Lottie animation
var json = File.ReadAllText("animation.json");
var animation = Animation.Parse(json);
player.SetAnimation(animation);

// 3. On each frame tick, advance and render
player.Update(deltaTime);               // deltaTime = time since last frame
player.Render(canvas, destRect);         // draw current frame
```

## The Frame Loop

`SKLottiePlayer` follows a simple two-step pattern on every frame:

1. **`Update(deltaTime)`** — Advances the playback position by the elapsed time, applying speed, direction, and repeat logic.
2. **`Render(canvas, rect)`** — Draws the current frame onto the canvas within the given rectangle.

```csharp
// Typical game/render loop integration
var stopwatch = Stopwatch.StartNew();
var lastTick = stopwatch.Elapsed;

void OnFrame()
{
    var now = stopwatch.Elapsed;
    var delta = now - lastTick;
    lastTick = now;

    player.Update(delta);
    canvas.Clear(SKColors.Transparent);
    player.Render(canvas, SKRect.Create(0, 0, width, height));
}
```

## Playback Speed

The `AnimationSpeed` property multiplies the time delta on every frame:

| Speed | Effect |
| :---: | :----- |
| `1.0` | Normal speed |
| `2.0` | Double speed |
| `0.5` | Half speed |
| `-1.0` | Play in reverse |
| `-2.0` | Double speed, reversed |

```csharp
player.AnimationSpeed = -1.5;   // 1.5× speed, playing backward
```

## Repeat Modes

The [`SKLottieRepeat`](xref:SkiaSharp.Extended.SKLottieRepeat) struct controls what happens when the animation reaches a boundary:

```csharp
// Play once, then stop
player.Repeat = SKLottieRepeat.Never;

// Loop forever, restarting from the beginning each time
player.Repeat = SKLottieRepeat.Restart();

// Loop 3 additional times (4 total plays), restarting each time
player.Repeat = SKLottieRepeat.Restart(3);

// Ping-pong forever (forward → backward → forward → …)
player.Repeat = SKLottieRepeat.Reverse();

// Ping-pong 2 additional times (3 forward/back cycles)
player.Repeat = SKLottieRepeat.Reverse(2);
```

You can change the repeat mode mid-playback. The player preserves the current direction until the animation hits its next boundary, preventing abrupt mid-animation direction changes.
`SKLottieRepeat.Reverse(0)` is one forward/back cycle; `Never` is the
single-direction, play-once mode.

## Seeking

Use `Seek` to jump to an absolute position (clamped to `[0, Duration]`):

```csharp
// Jump to the 1-second mark
player.Seek(TimeSpan.FromSeconds(1.0));

// Jump to the beginning
player.Seek(TimeSpan.Zero);
```

> **Note:** `Seek` does not increment the internal repeat counter or trigger
> completion logic. It clears `IsComplete`, so seeking to a frame can resume
> playback with the remaining repeat state intact.

## Events

| Event | When It Fires |
| :---- | :------------ |
| `AnimationCompleted` | All repeats finished (never fires for infinite loops) |
| `AnimationUpdated` | Once after every `Seek` or `Update` call |

```csharp
player.AnimationCompleted += (s, e) =>
{
    Console.WriteLine("Animation finished!");
};

player.AnimationUpdated += (s, e) =>
{
    Console.WriteLine($"Progress: {player.Progress.TotalSeconds:0.00}s");
};
```

## Properties Reference

| Property | Type | Description |
| :------- | :--- | :---------- |
| `Duration` | `TimeSpan` | Total animation duration (read-only, set when animation loads) |
| `Progress` | `TimeSpan` | Current playback position (read-only, advances via `Update`) |
| `IsComplete` | `bool` | Whether all repeats have finished |
| `HasAnimation` | `bool` | Whether an animation is currently loaded |
| `Repeat` | [`SKLottieRepeat`](xref:SkiaSharp.Extended.SKLottieRepeat) | How the animation repeats |
| `AnimationSpeed` | `double` | Playback speed multiplier (negative = reverse) |

## Loading and Resetting

```csharp
// Load a new animation (resets progress, completion, and repeat counters)
var animation = Animation.Parse(json);
player.SetAnimation(animation);

// Clear the animation
player.SetAnimation(null);
```

The caller owns the `Skottie.Animation` instance and is responsible for disposing it when no longer needed.

## Learn More

- [MAUI Lottie Animations](lottie-maui.md) — Use Lottie in .NET MAUI with `SKLottieView`
- [Blazor Lottie Animations](lottie-blazor.md) — Use Lottie in Blazor WebAssembly
- [Lottie by Airbnb](https://airbnb.design/lottie/) — Official project page
- [LottieFiles](https://lottiefiles.com/) — Free and premium animations
- [API Reference](xref:SkiaSharp.Extended.SKLottiePlayer) — Full method documentation
