# SkiaSharp.Extended

[![Build Status](https://jenkins.mono-project.com/buildStatus/icon?job=Components-SkiaSharpExtended)](https://jenkins.mono-project.com/view/Components/job/Components-SkiaSharpExtended/)  [![Build Status](https://jenkins.mono-project.com/buildStatus/icon?job=Components-SkiaSharpExtended-Windows)](https://jenkins.mono-project.com/view/Components/job/Components-SkiaSharpExtended-Windows/)

**SkiaSharp.Extended** is a collection some cool libraries that may be 
useful to some apps. There are several repositories that may have 
interesting projects:

 - [SkiaSharp][skiasharp] _(the engine)_
 - [SkiaSharp.Extended][extended] _(additional APIs)_
 - [SkiaSharp.Extended.Iconify][iconify] _(iconify library)_
 - [SkiaSharp.Extended.Svg][svg] _(lightweight SVG loader)_

## Building

Each sub-directory has a solution file that can be opened in Visual Studio or
built by MSBuild. All stripting and tasks are performed by MSBuild, so no 
external tooling is needed.

There is a single PowerShell script that can be used to build the entire 
repository:

    > .\build.ps1

The CI server just runs that single file and outputs all the packages, 
assemblies and test results.

_NOTE: for macOS, you may need to [first install PowerShell][pwsh]._

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/master/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended
[iconify]: https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended.Iconify
[svg]: https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended.Svg
[pwsh]: https://github.com/PowerShell/PowerShell
