# SkiaSharp.Extended.UI.Blazor

`SkiaSharp.Extended.UI.Blazor` provides reusable Blazor controls built on
[SkiaSharp](https://github.com/mono/SkiaSharp). Its animated surface component
uses the browser's rendering loop so your update and paint callbacks stay in
sync with displayed frames.

## Install

```shell
dotnet add package SkiaSharp.Extended.UI.Blazor
```

## Quick start

Import the component namespace:

```razor
@using SkiaSharp
@using SkiaSharp.Views.Blazor
@using SkiaSharp.Extended.UI.Blazor.Controls
```

Then draw with the component:

```razor
<SKAnimatedSurfaceView OnPaintSurface="Paint" style="width: 320px; height: 200px;" />

@code {
    private void Paint(SKCanvas canvas, SKSize size)
    {
        canvas.Clear(SKColors.CornflowerBlue);
    }
}
```

## Included controls

- **`SKAnimatedSurfaceView`** — coordinates synchronous update and paint
  callbacks through SkiaSharp's Canvas or OpenGL Blazor render loop. Set
  `SurfaceType` to `typeof(SKGLView)` to select GPU rendering per control.

## Compatibility

The package targets .NET 10 for Blazor applications and depends on
`SkiaSharp.Views.Blazor`.

## Documentation and support

- [Animated Surface View guide](https://mono.github.io/SkiaSharp.Extended/docs/animated-surface-blazor.html)
- [API reference](https://mono.github.io/SkiaSharp.Extended/api/SkiaSharp.Extended.UI.Blazor.Controls.html)
- [Source code and issue tracker](https://github.com/mono/SkiaSharp.Extended)

## License

Licensed under the [MIT License](https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE).
