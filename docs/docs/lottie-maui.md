# Lottie Animations for .NET MAUI

`SKLottieView` hosts Lottie animations in a .NET MAUI layout. It preserves MAUI
binding, source-change, handler lifecycle, cancellation, and loaded-animation
events while the core `SKLottiePlayer` performs playback.

```xml
<skia:SKLottieView
    Source="loading.json"
    RepeatCount="-1"
    RepeatMode="Restart" />
```

Use a file, URI, or stream source. Stream inputs passed to
`SKLottieImageSource.FromStream(Stream)` are snapshotted so the view can reload
them after handler reattachment.

`Progress`, `Duration`, and `IsComplete` remain bindable. Set `Progress` to
seek, and handle `AnimationLoaded`, `AnimationFailed`, or `AnimationCompleted`
for MAUI-specific UI work.

For direct hosts or custom frame loops, use the [Lottie Player](lottie-player.md).
