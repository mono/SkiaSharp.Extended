# SkiaSharp.Extended.Svg

[![SkiaSharp](https://img.shields.io/nuget/vpre/SkiaSharp.Svg.svg?maxAge=2592000)](https://www.nuget.org/packages/SkiaSharp.Svg)

**SkiaSharp.Svg** is lightweight SVG parser that can be used for 
most SVG needs.

Support for SVG has been a hot topic, but [Google has stated][google-svg] 
that this is not going to be a feature coming soon. However, we do want to 
support SVG. To this end, we are trying out a lightweight SVG parser. 
This is a pure managed code parser, that actually lives in a single file.

This implementation of SVG is fairly limited, but supports all the features 
supported by the alternate [NGraphics][ngraphics] library (and a bit more).
We are looking to add new features, so please do create issues when you need 
a feature that does not exist yet.

```csharp
// create a new SVG object
var svg = new SKSvg();

// load the SVG document
svg.Load("image.svg");

// draw the svg
SKCanvas canvas = ...
canvas.DrawPicture(svg.Picture);
```

This will draw the SVG at the size that it was created. To control the 
size, you can make use of a scale `SKMatrix`:

```csharp
// get the rectangle that the SVG is defined in
var svgSize = svg.Picture.CullRect;
float svgMax = Math.Max(svgSize.Width, svgSize.Height);

// calculate the scaling need to fit
float canvasMin = Math.Min(width, height);
float scale = canvasMin / svgMax;
var matrix = SKMatrix.MakeScale(scale, scale);

// draw the svg
canvas.DrawPicture(svg.Picture, ref matrix);
```

_NOTE: although this library is in the `SkiaSharp.Extended.XXX` repository, this  
library retains the `SkiaSharp.Svg` assembly name and legacy namespace to avoid 
any breaking changes._


[google-svg]: https://groups.google.com/d/msg/skia-discuss/8grSzbS0GnI/GxsAdCCUU9cJ
[ngraphics]: https://github.com/praeclarum/NGraphics
