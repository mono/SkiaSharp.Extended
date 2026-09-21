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

To build and pack only the shipping libraries, without building tests or samples:

```bash
./build.sh -configuration Release -projects "$PWD/scripts/SkiaSharp.Extended-Pack.slnf" -pack
```

## CI pipelines

| Entry point | Responsibility | Platforms |
| --- | --- | --- |
| [azure-pipelines-public.yml](azure-pipelines-public.yml) | Build the full solution, including samples; run tests and pack unsigned packages | Windows, macOS, Linux |
| [azure-pipelines.yml](azure-pipelines.yml) | Build and pack only the shipping libraries, sign, register assets in BAR, and promote through Maestro | Windows |
| [azure-pipelines-tests.yml](azure-pipelines-tests.yml) | Build the full solution, including samples, and run tests; no packaging or promotion | Windows, macOS, Linux |

The internal package pipeline does not build sample or test projects. Its
signing and package-validation stages use the standard 1ES/Arcade templates;
NuGet.org publication remains a separate protected release operation.

The internal tests pipeline is triggered by successful completion of
`\dotnet\skiasharp\skiasharp-extended-package`. Both internal definitions must use
the same `mono-SkiaSharp.Extended` Azure Repos mirror so completion-triggered
validation checks out the producer's branch and commit. Public CI and internal
tests run without signing credentials or BAR access. For a manual internal
validation run, select the package run and its matching source ref; the pipeline
rejects mismatched commits.

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://mono.github.io/SkiaSharp.Extended/api/extended
[ui-maui]: https://mono.github.io/SkiaSharp.Extended/api/ui-maui
