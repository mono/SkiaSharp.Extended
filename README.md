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

    MIT License

    Copyright (c) 2017 Matthew Leibowitz

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

[netcore]: https://www.microsoft.com/net/core