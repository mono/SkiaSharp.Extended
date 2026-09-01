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

To build the projects and samples, just open `SkiaSharp.Extended.sln` 
in Visual Studio.

The CI server just runs `dotnet cake` and outputs all the packages,
assemblies and test results. This can also be used to build everything
locally.

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://mono.github.io/SkiaSharp.Extended/api/extended
[ui-maui]: https://mono.github.io/SkiaSharp.Extended/api/ui-maui
