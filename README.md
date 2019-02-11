# SkiaSharp.Extended

[![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/Xamarin/Components/SkiaSharp.Extended?branchName=master)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=10846&branchName=master)  [![Build Status](https://dev.azure.com/SkiaSharp/SkiaSharp/_apis/build/status/SkiaSharp.Extended%20(Public)?branchName=master)](https://dev.azure.com/SkiaSharp/SkiaSharp/_build/latest?definitionId=4&branchName=master)

**SkiaSharp.Extended** is a collection some cool libraries that may be
useful to some apps. There are several repositories that may have
interesting projects:

 - [SkiaSharp][skiasharp] _(the engine)_
 - [SkiaSharp.Extended][extended] _(additional APIs)_
 - [SkiaSharp.Extended.Iconify][iconify] _(iconify library)_
 - [SkiaSharp.Extended.Svg][svg] _(lightweight SVG loader)_

## Building

Each sub-directory has a solution file that can be opened in Visual Studio or
built by MSBuild. All scripting and tasks are performed by MSBuild, so no
external tooling is needed.

There is a single PowerShell/bash script that can be used to build the entire
repository:

    > .\build.ps1
    $ ./build.sh

The CI server just runs that single file and outputs all the packages,
assemblies and test results.

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/master/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended
[iconify]: https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended.Iconify
[svg]: https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended.Svg
