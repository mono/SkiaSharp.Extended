# Lottie Animations

Lottie brings designer-created animations to your .NET MAUI apps. Instead of manually coding complex animations, designers export their After Effects animations as JSON files, and [`SKLottieView`](xref:SkiaSharp.Extended.UI.Controls.SKLottieView) plays them natively with smooth, scalable vector graphics.

![Lottie animation preview][lottie-preview]

## What is Lottie?

[Lottie](https://airbnb.design/lottie/) is an animation format created by Airbnb. Animations are designed in Adobe After Effects, exported as JSON using the [Bodymovin](https://github.com/airbnb/lottie-web) plugin, and rendered natively on mobile and web. The name honors Lotte Reiniger, a pioneer of silhouette animation.

**Why Lottie?**
- 🎨 Designers create animations visually in After Effects
- 📦 JSON files are tiny compared to GIFs or video
- 🔍 Vector-based, so animations scale to any resolution
- ⚡ Renders in real-time with smooth performance

## Quick Start

### Add to your XAML

```xml
<ContentPage xmlns:skia="clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI.Maui">
    
    <skia:SKLottieView 
        Source="animation.json"
        RepeatCount="-1"
        WidthRequest="200"
        HeightRequest="200" />
        
</ContentPage>
```

### Load from different sources

```csharp
// From app resources (recommended)
lottieView.Source = new SKFileLottieImageSource { File = "animation.json" };

// From a URL
lottieView.Source = new SKUriLottieImageSource { Uri = new Uri("https://...") };

// From a stream
lottieView.Source = new SKStreamLottieImageSource { Stream = myStream };
```

## Playback Control

### Repeat modes

```xml
<!-- Play once and stop -->
<skia:SKLottieView Source="animation.json" RepeatCount="0" />

<!-- Repeat 3 times -->
<skia:SKLottieView Source="animation.json" RepeatCount="3" />

<!-- Loop forever -->
<skia:SKLottieView Source="animation.json" RepeatCount="-1" />

<!-- Ping-pong (play forward, then backward) -->
<skia:SKLottieView Source="animation.json" RepeatCount="-1" RepeatMode="Reverse" />
```

### Control playback programmatically

```csharp
// Pause the animation
lottieView.IsAnimationEnabled = false;

// Resume
lottieView.IsAnimationEnabled = true;

// Jump to specific progress
lottieView.Progress = TimeSpan.FromSeconds(1.5);

// Check if complete
if (lottieView.IsComplete)
{
    // Animation finished
}
```

## Events

Handle animation lifecycle events:

```csharp
lottieView.AnimationLoaded += (s, e) =>
{
    Console.WriteLine($"Loaded! Duration: {lottieView.Duration}");
};

lottieView.AnimationFailed += (s, e) =>
{
    Console.WriteLine($"Failed to load animation");
};

lottieView.AnimationCompleted += (s, e) =>
{
    Console.WriteLine("Animation finished playing");
};
```

> **Note:** `AnimationCompleted` only fires for finite animations. Infinite loops (`RepeatCount="-1"`) never complete.

## Properties Reference

| Property | Type | Description |
| :------- | :--- | :---------- |
| `Source` | [`SKLottieImageSource`](xref:SkiaSharp.Extended.UI.Controls.SKLottieImageSource) | The Lottie JSON file to play |
| `Duration` | `TimeSpan` | Total duration of the animation (read-only) |
| `Progress` | `TimeSpan` | Current playback position |
| `RepeatCount` | `int` | Times to repeat (0 = once, -1 = forever) |
| `RepeatMode` | [`SKLottieRepeatMode`](xref:SkiaSharp.Extended.UI.Controls.SKLottieRepeatMode) | `Restart` or `Reverse` (ping-pong) |
| `IsAnimationEnabled` | `bool` | Play/pause the animation |
| `IsComplete` | `bool` | Whether playback has finished |

## Where to Find Animations

- [LottieFiles](https://lottiefiles.com/) — Free and premium Lottie animations
- [IconScout Lottie](https://iconscout.com/lottie-animations) — Animated icons
- [Lordicon](https://lordicon.com/) — Animated icon library

Or create your own in Adobe After Effects using the [Bodymovin plugin](https://github.com/airbnb/lottie-web).

## Template Customization

You can customize the rendering surface by overriding the control template:

```xml
<skia:SKLottieView Source="animation.json">
    <skia:SKLottieView.ControlTemplate>
        <ControlTemplate>
            <!-- Use SKGLView for GPU-accelerated rendering -->
            <skia:SKGLView x:Name="PART_DrawingSurface" />
        </ControlTemplate>
    </skia:SKLottieView.ControlTemplate>
</skia:SKLottieView>
```

The `PART_DrawingSurface` name is required—it can be either `SKCanvasView` (software) or `SKGLView` (GPU).

## Cross-Platform Usage with SKLottiePlayer

The `SKLottiePlayer` class in `SkiaSharp.Extended` provides all the playback logic without any MAUI or Blazor dependency, so you can integrate Lottie animations into any .NET host — including Blazor WebAssembly, console renderers, or custom frameworks.

### SKLottieRepeat

`SKLottieRepeat` is a lightweight value type (struct) that describes how an animation repeats:

| Factory | Behaviour |
| :------ | :-------- |
| `SKLottieRepeat.Never` | Play once, then stop (default). |
| `SKLottieRepeat.Restart(count)` | Restart from the beginning. `count = -1` for infinite. |
| `SKLottieRepeat.Reverse(count)` | Ping-pong (play forward, then backward). `count = -1` for infinite. |

### Using SKLottiePlayer directly

```csharp
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Skottie;

// 1. Load the animation from a stream
using var stream = File.OpenRead("animation.json");
var animation = Animation.Create(stream);

// 2. Create a player and configure it
var player = new SKLottiePlayer
{
    AnimationSpeed = 1.5,
    Repeat = SKLottieRepeat.Restart(count: -1)  // loop forever
};

player.AnimationCompleted += (_, _) => Console.WriteLine("Done!");
player.SetAnimation(animation);

// 3. In your frame/paint loop:
//    a) advance the playback position
player.Update(deltaTime);

//    b) draw the current frame
player.Render(canvas, SKRect.Create(0, 0, 400, 400));
```

> **Blazor note:** The Blazor sample at `samples/SkiaSharpDemo.Blazor/` demonstrates `SKLottiePlayer` inside a `PeriodicTimer`-based animation loop. The `SKAnimatedCanvasView` component in `SkiaSharp.Extended.UI.Blazor` wraps this pattern as a reusable Blazor component.

## Learn More

- [Lottie by Airbnb](https://airbnb.design/lottie/) — Official project page
- [Lottie Documentation](https://airbnb.io/lottie/) — Format specification and guides
- [LottieFiles](https://lottiefiles.com/) — Animation marketplace
- [API Reference](xref:SkiaSharp.Extended.UI.Controls.SKLottieView) — Full method documentation

[lottie-preview]: ../images/ui/controls/sklottieview/lottie.gif
