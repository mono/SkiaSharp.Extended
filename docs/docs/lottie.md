# Lottie Animations

Lottie brings designer-created animations to your .NET MAUI apps. Instead of manually coding complex animations, designers export their After Effects animations as JSON files, and `SKLottieView` plays them natively with smooth, scalable vector graphics.

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
lottieView.Pause();

// Resume
lottieView.Resume();

// Jump to specific progress
lottieView.Progress = TimeSpan.FromSeconds(1.5);

// Check if complete
if (lottieView.IsComplete)
{
    // Animation finished
}
```

### Frame range control

Restrict playback to a sub-range of the animation using `FrameStart` and `FrameEnd`.
Both are zero-based offsets from the animation's InPoint. `Duration`, `FrameCount`, `Progress`,
and `CurrentFrame` all reflect the active range.

- **`FrameStart`** — first frame to play (default `0` = InPoint)
- **`FrameEnd`** — end frame (exclusive, default `-1` = OutPoint)

```csharp
// Play only frames 0–29 (30 frames)
lottieView.FrameEnd = 30;

// Play only frames 10–39 (30 frames, offset start)
lottieView.FrameStart = 10;
lottieView.FrameEnd = 40;

// Restore the full InPoint→OutPoint range
lottieView.FrameStart = 0;
lottieView.FrameEnd = -1;
```

A multi-state animation where each state lives in a different frame range:

```csharp
// Three states: idle (0-30), pressed (30-50), release (50-80)
void SetState(AnimationState state)
{
    switch (state)
    {
        case AnimationState.Idle:
            lottieView.FrameStart = 0;
            lottieView.FrameEnd = 30;
            break;
        case AnimationState.Pressed:
            lottieView.FrameStart = 30;
            lottieView.FrameEnd = 50;
            break;
        case AnimationState.Release:
            lottieView.FrameStart = 50;
            lottieView.FrameEnd = 80;
            break;
    }
}
```

XAML binding works directly on `FrameStart` and `FrameEnd`:

```xml
<Entry Text="{Binding FrameStart, Source={Reference lottieView}}" Keyboard="Numeric" />
<Entry Text="{Binding FrameEnd, Source={Reference lottieView}}" Keyboard="Numeric" />
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
| `Source` | `SKLottieImageSource` | The Lottie JSON file to play |
| `Duration` | `TimeSpan` | Duration of the active frame range (read-only, updates when `FrameStart`/`FrameEnd` changes) |
| `Progress` | `TimeSpan` | Current playback position within the active frame range |
| `Fps` | `double` | Frames per second of the animation (read-only) |
| `FrameCount` | `int` | Number of frames in the active range (read-only) |
| `CurrentFrame` | `int` | Current frame within the active range, zero-based (read-only) |
| `FrameStart` | `int` | First frame to play (zero-based from InPoint, default `0`) |
| `FrameEnd` | `int` | End frame — exclusive (zero-based from InPoint, default `-1` = OutPoint) |
| `RepeatCount` | `int` | Times to repeat (0 = once, -1 = forever) |
| `RepeatMode` | `SKLottieRepeatMode` | `Restart` or `Reverse` (ping-pong) |
| `IsAnimationEnabled` | `bool` | Play/pause the animation |
| `IsComplete` | `bool` | Whether playback has finished |

## Methods Reference

There are no additional public methods beyond setting properties. Use `Progress` and `IsAnimationEnabled` directly for playback control.

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

## Learn More

- [Lottie by Airbnb](https://airbnb.design/lottie/) — Official project page
- [Lottie Documentation](https://airbnb.io/lottie/) — Format specification and guides
- [LottieFiles](https://lottiefiles.com/) — Animation marketplace
- [API Reference](xref:SkiaSharp.Extended.UI.Controls.SKLottieView) — Full method documentation

[lottie-preview]: ../images/ui/controls/sklottieview/lottie.gif
