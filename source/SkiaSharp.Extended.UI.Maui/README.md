# SkiaSharp.Extended.UI.Maui source

This project implements the .NET MAUI controls shipped in the
`SkiaSharp.Extended.UI.Maui` package, including Lottie playback and confetti
effects.

For package installation, consumer examples, and links to the control guides,
see [PACKAGE-README.md](PACKAGE-README.md).

## Source layout

- `Controls/Lottie/` contains `SKLottieView` and its MAUI image sources. The
  core project owns the shared playback and source-loading implementation.
- `Controls/Confetti/` contains `SKConfettiView` and its particle-system
  types.
- `Controls/` also contains the shared surface and animation infrastructure.

Platform-independent sources use the `.shared.cs` suffix. Platform-specific
files use the appropriate `.android.cs`, `.ios.cs`, `.macos.cs`, or
`.windows.cs` suffix and are included conditionally by the project file.

## Build and test

Install the MAUI workloads specified by the repository before building. From
the repository root, build a supported target, for example:

```shell
dotnet build source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj \
  --configuration Release \
  --framework net10.0-android36.0
```

The project targets .NET 10 for Android, iOS, Mac Catalyst, Windows, and the
platform-neutral MAUI target. Keep control behavior consistent across each
supported platform.

## Packaging

The shared `source/Directory.Build.props` file supplies package metadata,
embeds `PACKAGE-README.md` as the NuGet readme, and adds the shared package
icon. Verify package changes from the repository root with:

```shell
dotnet pack source/SkiaSharp.Extended.UI.Maui/SkiaSharp.Extended.UI.Maui.csproj \
  --configuration Release
```
