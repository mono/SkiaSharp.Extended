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

- **`SKAnimatedSurfaceView`** — coordinates synchronous update and paint callbacks through a Canvas or OpenGL surface and the Blazor render
  loop. Disable animation and call `Invalidate()` for on-demand rendering. Set `SurfaceType` to `typeof(SKGLView)` to select GPU rendering
  per component. In DEBUG builds, it draws an FPS label after your paint callback; release builds have no diagnostic overlay. Its monotonic
  timing is shared with the .NET MAUI animated surface.
- **`SKLottieView`** — loads and plays Lottie JSON using the shared `SKLottiePlayer` engine on `SKAnimatedSurfaceView`, including its
  `SurfaceType` and `IgnorePixelScaling` options. It supports explicit seeking and opt-in throttled progress reporting. It composes the
  animated surface; it does not inherit from it. Derive from `SKLottieImageSource` to replace the complete loading pipeline.

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
