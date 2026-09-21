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

Builds use the .NET Arcade SDK. The SDK and workload versions are pinned in
`global.json`; the build scripts bootstrap the required .NET SDK when needed.
Install the pinned workloads before your first full build:

```bash
# On macOS; use maui-android instead of maui on Linux.
./eng/common/dotnet.sh workload install maui wasm-tools --source https://api.nuget.org/v3/index.json

# Restore, build, run tests, and create NuGet packages.
./build.sh -configuration Release -test -pack
```

On Windows, use `eng\common\dotnet.cmd workload install maui wasm-tools --source https://api.nuget.org/v3/index.json`,
then `build.cmd -configuration Release -test -pack`. You can also open
`SkiaSharp.Extended.sln` in Visual Studio.

Outputs use Arcade's standard `artifacts/` layout: assemblies in `bin/`,
shipping packages in `packages/Release/Shipping/`, test results in
`TestResults/Release/`, and build logs in `log/Release/`. Package versions are
defined in `eng/Versions.props`. Arcade owns CI versioning, Source Link, symbol
generation, signing, and asset manifests; no Cake tools are required.

Public CI builds and tests without signing credentials or BAR access. Official
internal builds use the 1ES/Arcade templates for signing, Build Asset Registry
registration, validation, and promotion through configured Maestro channels.
NuGet.org publication remains a separate protected release operation.

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://mono.github.io/SkiaSharp.Extended/api/extended
[ui-maui]: https://mono.github.io/SkiaSharp.Extended/api/ui-maui
