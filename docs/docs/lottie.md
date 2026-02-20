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

### Frame-based control

Seek to a specific frame and stop (instant, like Lottie's `goToAndStop`):

```csharp
// Instantly jump to frame 30 and pause
lottieView.SeekToFrame(30, stopPlayback: true);

// Instantly jump to a normalized position and keep playing
lottieView.SeekToProgress(0.5);
```

Smoothly animate to a position (plays the frames, then stops):

```csharp
// Play forward from current position to frame 50, then stop
lottieView.PlayToFrame(50);

// Play backward from frame 50 to frame 0 (direction auto-detected)
lottieView.SeekToFrame(50, stopPlayback: true);  // jump to frame 50 first
lottieView.PlayToFrame(0);                        // then animate back to 0

// Animate to the 75% position
lottieView.PlayToProgress(0.75);
```

Create a toggle-switch animation using `PlayToFrame`:

```csharp
private bool isOn = false;

private void OnSwitchToggled(object sender, EventArgs e)
{
    // PlayToFrame auto-detects direction: forward when off→on, backward when on→off
    lottieView.PlayToFrame(isOn ? 0 : lottieView.FrameCount - 1);
    isOn = !isOn;
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
| `Source` | `SKLottieImageSource` | The Lottie JSON file to play |
| `Duration` | `TimeSpan` | Total duration of the animation (read-only) |
| `Progress` | `TimeSpan` | Current playback position |
| `Fps` | `double` | Frames per second of the animation (read-only) |
| `FrameCount` | `int` | Total number of frames (read-only) |
| `CurrentFrame` | `int` | Current frame number, zero-based (read-only) |
| `RepeatCount` | `int` | Times to repeat (0 = once, -1 = forever) |
| `RepeatMode` | `SKLottieRepeatMode` | `Restart` or `Reverse` (ping-pong) |
| `IsAnimationEnabled` | `bool` | Play/pause the animation |
| `IsComplete` | `bool` | Whether playback has finished |

## Methods Reference

| Method | Description |
| :----- | :---------- |
| `SeekToFrame(int, bool)` | Instantly seeks to a frame (zero-based). Pass `stopPlayback: true` to pause. |
| `SeekToTime(TimeSpan, bool)` | Instantly seeks to a time position. Pass `stopPlayback: true` to pause. |
| `SeekToProgress(double, bool)` | Instantly seeks to a normalized position [0.0, 1.0]. Pass `stopPlayback: true` to pause. |
| `PlayToFrame(int)` | Smoothly animates from current position to the target frame, then stops. Direction is auto-detected. |
| `PlayToProgress(double)` | Smoothly animates from current position to a normalized position [0.0, 1.0], then stops. |
| `PlayToTime(TimeSpan)` | Smoothly animates from current position to a target time, then stops. |
| `Pause()` | Pauses the animation. Equivalent to `IsAnimationEnabled = false`. |
| `Resume()` | Resumes the animation. Equivalent to `IsAnimationEnabled = true`. |

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
