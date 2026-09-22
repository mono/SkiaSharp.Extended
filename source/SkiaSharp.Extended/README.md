# SkiaSharp.Extended source

This project implements the core `SkiaSharp.Extended` library. The package adds
image-processing and drawing utilities to SkiaSharp and is shipped as
`SkiaSharp.Extended`.

For the package landing page, installation instructions, and consumer examples,
see [PACKAGE-README.md](PACKAGE-README.md).

## Source layout

- `BlurHash/` contains the BlurHash serializer, deserializer, and helpers.
- `Comparer/` contains pixel-comparison APIs.
- `Geometry/` contains shape and path helpers.
- `Lottie/` contains the shared playback engine and internal source-loading helper.
- `PathInterpolation/` contains path interpolation support.
- `Internal/` contains shared non-public timing support used by the MAUI and
  Blazor animation surfaces.

## Build and test

From the repository root:

```shell
dotnet build source/SkiaSharp.Extended/SkiaSharp.Extended.csproj --configuration Release
dotnet test tests/SkiaSharp.Extended.Tests --configuration Release
```

The project targets `netstandard2.0` and `net10.0`. Keep public APIs compatible
with both targets, and add tests that mirror the source area being changed.

## Packaging

The shared `source/Directory.Build.props` file supplies package metadata,
embeds `PACKAGE-README.md` as the NuGet readme, and adds the shared package
icon. Verify package changes from the repository root with:

```shell
dotnet pack source/SkiaSharp.Extended/SkiaSharp.Extended.csproj --configuration Release
```
