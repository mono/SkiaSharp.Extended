# Lottie Player

`SKLottiePlayer` is the platform-neutral playback engine for loaded Lottie
animations. It owns timing, seeking, repeat behavior, and rendering state, but
not the native Skottie animation itself.

## Quick Start

```csharp
using SkiaSharp.Extended;
using SkiaSharp.Skottie;

using var animation = Animation.Parse(File.ReadAllText("animation.json"));
var player = new SKLottiePlayer
{
    Animation = animation,
    Repeat = SKLottieRepeat.Restart(),
};

player.Update(deltaTime);
player.Render(canvas, destination);
```

Set `Animation` to a caller-owned `Animation` instance. The player seeks and
renders that instance but does not dispose it, so dispose the animation after
the player no longer uses it.

## Playback

Use `AnimationSpeed` to change the playback rate; negative values play in
reverse. `Seek` clamps an absolute position to the animation duration and lets
playback resume from the requested frame.

| Repeat value | Behavior |
| --- | --- |
| `SKLottieRepeat.Never` | Play once. |
| `SKLottieRepeat.Restart()` | Restart indefinitely. |
| `SKLottieRepeat.Restart(2)` | Play three times total. |
| `SKLottieRepeat.Reverse()` | Ping-pong indefinitely. |

`AnimationUpdated` reports state changes and `AnimationCompleted` fires once
when a finite playback completes. The player is not thread-safe; call it from
one rendering thread.

## Learn More

- [Lottie](https://airbnb.design/lottie/) — the animation format created by Airbnb
- [LottieFiles](https://lottiefiles.com/) — animation resources
- [API Reference](xref:SkiaSharp.Extended.SKLottiePlayer)
