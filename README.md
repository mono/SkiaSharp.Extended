# SkiaSharp.Extended

[![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/SkiaSharp.Extended?branchName=main)](https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=10846&branchName=main)  [![Build Status](https://dev.azure.com/xamarin/public/_apis/build/status/mono/SkiaSharp/SkiaSharp.Extended%20(Public)?branchName=main)](https://dev.azure.com/xamarin/public/_build/latest?definitionId=6&branchName=main)

**SkiaSharp.Extended** is a collection some cool libraries that may be
useful to some apps. There are several repositories that may have
interesting projects:

 - [SkiaSharp][skiasharp] _(the engine)_
 - [SkiaSharp.Extended][extended] _(additional APIs)_
 - [SkiaSharp.Extended.UI.Forms][ui-forms] _(additional Xamarin.Forms controls)_
 - [SkiaSharp.Extended.UI.Maui][ui-maui] _(additional .NET MAUI controls)_

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
[ui-forms]: https://mono.github.io/SkiaSharp.Extended/api/ui-forms
[ui-maui]: https://mono.github.io/SkiaSharp.Extended/api/ui-maui
