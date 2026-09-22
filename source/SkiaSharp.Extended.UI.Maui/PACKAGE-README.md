# SkiaSharp.Extended.UI.Maui

`SkiaSharp.Extended.UI.Maui` adds polished, SkiaSharp-powered controls to
[.NET MAUI](https://learn.microsoft.com/dotnet/maui/). It includes Lottie
animation playback and configurable confetti effects, with the supporting
`SkiaSharp.Extended` utilities brought in automatically.

## Install

```shell
dotnet add package SkiaSharp.Extended.UI.Maui
```

## Quick start

Add the controls namespace to a XAML page:

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:skia="clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI.Maui">

    <Grid>
        <skia:SKLottieView
            Source="loading.json"
            RepeatCount="-1" />

        <skia:SKConfettiView x:Name="confettiView" />
    </Grid>
</ContentPage>
```

Trigger a burst when your app has something to celebrate:

```csharp
confettiView.Systems.Add(new SKConfettiSystem
{
    Emitter = SKConfettiEmitter.Burst(100),
    EmitterBounds = SKConfettiEmitterBounds.Center,
});
```

## Included controls

- **`SKLottieView`** — plays Lottie JSON animations from app resources,
  streams, or URIs, with repeat and playback controls. DEBUG builds display an
  FPS label after the animation frame; release builds omit the diagnostic.
  Timing is shared with the Blazor animated surface.
- **`SKConfettiView`** — renders configurable particle systems with burst,
  stream, and infinite emission patterns.

## Compatibility

The package targets .NET 10 and supports Android, iOS, Mac Catalyst, Windows,
and the platform-neutral .NET MAUI target. It depends on
`SkiaSharp.Views.Maui.Controls`.

## Documentation and support

- [Lottie animation guide](https://mono.github.io/SkiaSharp.Extended/docs/lottie-maui.html)
- [Confetti effects guide](https://mono.github.io/SkiaSharp.Extended/docs/confetti.html)
- [API reference](https://mono.github.io/SkiaSharp.Extended/api/SkiaSharp.Extended.UI.Controls.html)
- [Source code and issue tracker](https://github.com/mono/SkiaSharp.Extended)

## License

Licensed under the [MIT License](https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE).
