# SkiaSharp.Extended.UI.Blazor

`SkiaSharp.Extended.UI.Blazor` provides reusable Blazor WebAssembly and
Interactive WebAssembly controls built on
[SkiaSharp](https://github.com/mono/SkiaSharp). Its animated surface component
uses the browser's rendering loop so your update and paint callbacks stay in
sync with displayed frames, and its Lottie component uses the shared
`SKLottiePlayer` engine for repeat, speed, and completion behavior.

## Install

```shell
dotnet add package SkiaSharp.Extended.UI.Blazor
```

## Quick start

Import the component namespace:

```razor
@using SkiaSharp
@using SkiaSharp.Views.Blazor
@using SkiaSharp.Extended
@using SkiaSharp.Extended.UI.Blazor.Components
```

Draw a custom animated surface:

```razor
<SKAnimatedSurfaceView OnPaintSurface="Paint" style="width: 320px; height: 200px;" />

@code {
    private void Paint(SKCanvas canvas, SKSize size)
    {
        canvas.Clear(SKColors.CornflowerBlue);
    }
}
```

Or play a Lottie animation:

```razor
<SKLottieView Source="animations/loading.json"
              Repeat="SKLottieRepeat.Restart()"
              style="width: 300px; height: 300px;" />
```

## Included components

- **`SKSurfaceView`** — provides validated Canvas or OpenGL rendering with a
  synchronous paint callback and explicit `Invalidate()` redraws.
- **`SKAnimatedSurfaceView`** — coordinates synchronous update and paint
  callbacks through the inherited Canvas or OpenGL surface and the Blazor
  render loop. Set `SurfaceType` to `typeof(SKGLView)` to select GPU rendering
  per control. In DEBUG builds, it draws an FPS label after your paint
  callback; release builds have no diagnostic overlay. Its monotonic timing is
  shared with the .NET MAUI animated surface.
- **`SKLottieView`** — loads and plays Lottie JSON using the shared
  `SKLottiePlayer` engine on `SKAnimatedSurfaceView`, including its
  `SurfaceType` and `IgnorePixelScaling` options. It composes the animated
  surface; it does not inherit from it.

## Compatibility

The package targets .NET 10 for Blazor WebAssembly and Interactive WebAssembly
applications, not Interactive Server or unrestricted Interactive Auto. The animated surface uses
`SkiaSharp.Views.Blazor`; the Lottie control also uses `SkiaSharp.Extended` and
`SkiaSharp.Skottie`.

## Documentation and support

- [Animated Surface View guide](https://mono.github.io/SkiaSharp.Extended/docs/animated-surface-blazor.html)
- [Lottie guide](https://mono.github.io/SkiaSharp.Extended/docs/lottie-blazor.html)
- [API reference](https://mono.github.io/SkiaSharp.Extended/api/SkiaSharp.Extended.UI.Blazor.Components.html)
- [Source code and issue tracker](https://github.com/mono/SkiaSharp.Extended)

## License

Licensed under the [MIT License](https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE).
