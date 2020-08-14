# SkiaSharp.Extended

[![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/SkiaSharp.Extended?branchName=main)](https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=10846&branchName=main)  [![Build Status](https://dev.azure.com/xamarin/public/_apis/build/status/mono/SkiaSharp/SkiaSharp.Extended%20(Public)?branchName=main)](https://dev.azure.com/xamarin/public/_build/latest?definitionId=6&branchName=main)

**SkiaSharp.Extended** is a collection some cool libraries that may be
useful to some apps. There are several repositories that may have
interesting projects:

 - [SkiaSharp][skiasharp] _(the engine)_
 - [SkiaSharp.Extended][extended] _(additional APIs)_
 - [SkiaSharp.Extended.Iconify][iconify] _(iconify library)_
 - [SkiaSharp.Extended.Svg][svg] _(lightweight SVG loader)_

## Building

To build the projects and samples, just open `SkiaSharp.Extended.sln` 
in Visual Studio 2019.

The CI server just runs `dotnet cake`` and outputs all the packages,
assemblies and test results. This can also be used to build everything
locally.

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE
[netcore]: https://www.microsoft.com/net/core

[skiasharp]: https://github.com/mono/SkiaSharp
[extended]: https://github.com/mono/SkiaSharp.Extended/wiki/SkiaSharp.Extended
[iconify]: https://github.com/mono/SkiaSharp.Extended/wiki/SkiaSharp.Extended.Iconify
[svg]: https://github.com/mono/SkiaSharp.Extended/wiki/SkiaSharp.Extended.Svg
