# SkiaSharp.Extended.UI.Blazor source

This project implements the Blazor components shipped in the
`SkiaSharp.Extended.UI.Blazor` package.

For package installation, consumer examples, and links to the control guide,
see [PACKAGE-README.md](PACKAGE-README.md).

## Source layout

- `Components/` contains reusable Blazor components.
  `Components/Lottie/` contains the Lottie component and source implementations.
  `SKSurfaceView` owns native Canvas/OpenGL handling; `SKAnimatedSurfaceView`
  extends it with frame updates and Lottie composes the animated surface.
- `Properties/` contains assembly-level source configuration.

## Build and test

From the repository root:

```shell
dotnet build source/SkiaSharp.Extended.UI.Blazor/SkiaSharp.Extended.UI.Blazor.csproj --configuration Release
dotnet test tests/SkiaSharp.Extended.UI.Blazor.Tests --configuration Release
```

The project targets .NET 10 for Blazor WebAssembly and Interactive WebAssembly,
not Interactive Server or unrestricted Interactive Auto. `SKSurfaceView` and `SKAnimatedSurfaceView`
depend on `SkiaSharp.Views.Blazor`; `SKLottieView` additionally uses the core
`SkiaSharp.Extended` project and `SkiaSharp.Skottie`. Keep native surface
behavior in the base surface, render-loop behavior in the animated surface,
and Lottie loading/playback behavior in the adapter. The animated surface
shares the core monotonic frame counter with MAUI and draws an FPS overlay only
in DEBUG builds. Test observable frame and lifecycle behavior.

## Packaging

The shared `source/Directory.Build.props` file supplies package metadata,
embeds `PACKAGE-README.md` as the NuGet readme, and adds the shared package
icon. Verify package changes from the repository root with:

```shell
dotnet pack source/SkiaSharp.Extended.UI.Blazor/SkiaSharp.Extended.UI.Blazor.csproj --configuration Release
```
