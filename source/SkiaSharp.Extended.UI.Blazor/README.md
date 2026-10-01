# SkiaSharp.Extended.UI.Blazor source

This project implements the Blazor components shipped in the `SkiaSharp.Extended.UI.Blazor` package.

For package installation, consumer examples, and links to the control guide, see [PACKAGE-README.md](PACKAGE-README.md).

## Source layout

- `Components/` contains reusable Blazor components. `SKAnimatedSurfaceView` owns Canvas/OpenGL rendering and optional frame updates.
- `Properties/` contains assembly-level source configuration.

## Build and test

From the repository root:

```shell
dotnet build source/SkiaSharp.Extended.UI.Blazor/SkiaSharp.Extended.UI.Blazor.csproj --configuration Release
dotnet test tests/SkiaSharp.Extended.UI.Blazor.Tests --configuration Release
```

The project targets .NET 10 for Blazor WebAssembly and Interactive WebAssembly, not Interactive Server or unrestricted Interactive Auto.
`SKAnimatedSurfaceView` depends on `SkiaSharp.Views.Blazor`, shares the core monotonic frame counter with MAUI, and draws an FPS overlay only
in DEBUG builds. Test observable frame and lifecycle behavior.

## Packaging

The shared `source/Directory.Build.props` file supplies package metadata, embeds `PACKAGE-README.md` as the NuGet readme, and adds the shared
package icon. Verify package changes from the repository root with:

```shell
dotnet pack source/SkiaSharp.Extended.UI.Blazor/SkiaSharp.Extended.UI.Blazor.csproj --configuration Release
```
