# SkiaSharp.Extended.UI.Blazor

`SkiaSharp.Extended.UI.Blazor` provides reusable Blazor WebAssembly and Interactive WebAssembly controls built on
[SkiaSharp](https://github.com/mono/SkiaSharp). Its animated surface component uses the browser's rendering loop so your update and paint
callbacks stay in sync with displayed frames.

## Install

```shell
dotnet add package SkiaSharp.Extended.UI.Blazor
```

## Quick start

```razor
@using SkiaSharp
@using SkiaSharp.Views.Blazor
@using SkiaSharp.Extended.UI.Blazor.Components

<SKAnimatedSurfaceView OnPaintSurface="Paint" style="width: 320px; height: 200px;" />

@code {
    private void Paint(SKCanvas canvas, SKSize size)
    {
        canvas.Clear(SKColors.CornflowerBlue);
    }
}
```

## Included component

**`SKAnimatedSurfaceView`** coordinates synchronous update and paint callbacks through a Canvas or OpenGL surface and the Blazor render loop.
Disable animation and call `Invalidate()` for on-demand rendering. Set `SurfaceType` to `typeof(SKGLView)` to select GPU rendering per
component. In DEBUG builds, it draws an FPS label after your paint callback; release builds have no diagnostic overlay. Its monotonic timing
is shared with the .NET MAUI animated surface.

## Compatibility

The package targets .NET 10 for Blazor WebAssembly and Interactive WebAssembly applications, not Interactive Server or unrestricted
Interactive Auto. The animated surface uses `SkiaSharp.Views.Blazor`.

## Documentation and support

- [Animated Surface View guide](https://mono.github.io/SkiaSharp.Extended/docs/animated-surface-blazor.html)
- [API reference](https://mono.github.io/SkiaSharp.Extended/api/SkiaSharp.Extended.UI.Blazor.Components.html)
- [Source code and issue tracker](https://github.com/mono/SkiaSharp.Extended)

## License

Licensed under the [MIT License](https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE).
