# SkiaSharp.Extended.UI.Blazor source

This project implements the Blazor controls shipped in the
`SkiaSharp.Extended.UI.Blazor` package.

For package installation, consumer examples, and links to the control guide,
see [PACKAGE-README.md](PACKAGE-README.md).

## Source layout

- `Controls/` contains reusable Blazor controls and their Razor markup.
- `Properties/` contains assembly-level source configuration.

## Build and test

From the repository root:

```shell
dotnet build source/SkiaSharp.Extended.UI.Blazor/SkiaSharp.Extended.UI.Blazor.csproj --configuration Release
dotnet test tests/SkiaSharp.Extended.UI.Blazor.Tests --configuration Release
```

The project targets .NET 10 and depends on `SkiaSharp.Views.Blazor`. Keep
components independent from application-specific features and test their
observable frame and lifecycle behavior.

## Packaging

The shared `source/Directory.Build.props` file supplies package metadata,
embeds `PACKAGE-README.md` as the NuGet readme, and adds the shared package
icon. Verify package changes from the repository root with:

```shell
dotnet pack source/SkiaSharp.Extended.UI.Blazor/SkiaSharp.Extended.UI.Blazor.csproj --configuration Release
```
