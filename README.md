# SkiaSharp.Extended

[![Build Status](https://dev.azure.com/dnceng-public/public/_apis/build/status/346?branchName=main)](https://dev.azure.com/dnceng-public/public/_build?definitionId=346&branchName=main)

**SkiaSharp.Extended** is a collection some cool libraries that may be
useful to some apps. There are several repositories that may have
interesting projects:

 - [SkiaSharp][skiasharp] _(the engine)_
 - [SkiaSharp.Extended][extended] _(additional APIs)_
 - [SkiaSharp.Extended.UI.Maui][ui-maui] _(additional .NET MAUI controls)_

## ⚠️ Important Notice for SVG Users

**SkiaSharp.Extended.Svg has been deprecated**. If you're using `SkiaSharp.Extended.Svg` and encountering errors like `MissingMethodException: Method not found: SKMatrix.MakeTranslation`, please see our [SVG Migration Guide](https://mono.github.io/SkiaSharp.Extended/docs/svg-migration.html) for instructions on migrating to [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia).

## Building

Install the .NET SDK specified by `global.json` and the required MAUI workloads
globally on your machine. The repository does not install an SDK or package
cache into your checkout.

```bash
# Each command restores and builds what it needs.
dotnet build SkiaSharp.Extended.sln --configuration Release
dotnet test SkiaSharp.Extended.sln --configuration Release
dotnet pack SkiaSharp.Extended.sln --configuration Release
```

Run the Blazor sample with:

```bash
dotnet run --project samples/SkiaSharpDemo.Blazor
```

Run the MAUI sample on a supported platform with its target framework:

```bash
# Android device or emulator
dotnet run --project samples/SkiaSharpDemo -f net10.0-android

# iOS simulator or device (macOS)
dotnet run --project samples/SkiaSharpDemo -f net10.0-ios

# Mac Catalyst (macOS)
dotnet run --project samples/SkiaSharpDemo -f net10.0-maccatalyst

# Windows
dotnet run --project samples/SkiaSharpDemo -f net10.0-windows10.0.19041.0
```

Outputs use Arcade's standard `artifacts/` layout: assemblies in `bin/`,
shipping packages in `packages/Release/Shipping/`, test results in
`TestResults/Release/`, and build logs in `log/Release/`. Package versions are
defined in `eng/Versions.props`. Arcade supplies repository and CI build
infrastructure; it does not require a repository-local .NET SDK.

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://mono.github.io/SkiaSharp.Extended/api/extended
[ui-maui]: https://mono.github.io/SkiaSharp.Extended/api/ui-maui
