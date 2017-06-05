# SkiaSharp.Extended

**SkiaSharp.Extended** is a collection some cool libraries that may be 
useful to some apps. There are several repositories that may have 
interesting projects:

 - [SkiaSharp](https://github.com/mono/SkiaSharp) _(the engine)_
 - [SkiaSharp.Extended](https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended) _(additional APIs)_
 - [SkiaSharp.Extended.Iconify](https://github.com/mono/SkiaSharp.Extended/tree/master/SkiaSharp.Extended.Iconify) _(iconify library)_

## Building

_Make sure [.NET Core][netcore] is installed._

The root just contains a build script that will build all the other 
scripts. To build everything, just run the command-line:

Mac/Linux:

    $ ./build.sh

Windows:

    > .\build.ps1

If only a specific project, or a set of projects, are to be built, 
then pass a value to the `names` argument:


Mac/Linux:

    $ ./build.sh -names=SkiaSharp.Extended.Iconify

Windows:

    > .\build.ps1 -Names=SkiaSharp.Extended.Iconify

## License

The code in this repository is licensed under the [MIT License][license].

[license]: https://github.com/mono/SkiaSharp.Extended/blob/master/LICENSE
[netcore]: https://www.microsoft.com/net/core
