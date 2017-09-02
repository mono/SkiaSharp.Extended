# SkiaSharp.Extended

[![SkiaSharp.Extended](https://img.shields.io/nuget/vpre/SkiaSharp.Extended.svg?maxAge=2592000)](https://www.nuget.org/packages/SkiaSharp.Extended)  [![NuGet](https://img.shields.io/nuget/dt/SkiaSharp.Extended.svg)](https://www.nuget.org/packages/SkiaSharp.Extended)

**SkiaSharp.Extended** is a collection some cool functions that may be 
useful to some apps.

There are a few helper methods that can be used to create geometric 
shapes in the `SKGeometry` type:

 - `CreateSectorPath` - 
   creates a path with the shape of sector/section of a doughnut/pie 
   chart
 - `CreatePiePath` - 
   creates a path with the shape of a doughnut/pie chart
 - `CreateSquarePath` - 
   creates a path with the shape of a square
 - `CreateRectanglePath` - 
   creates a path with the shape of a rectangle
 - `CreateTrianglePath` - 
   creates a path with the shape of a triangle
 - `CreateRegularPolygonPath` - 
   creates a path with the shape of some regular polygon
 - `CreateRegularStarPath` - 
   creates a path with the shape of a square

Some of these shapes can also be draw directly on a `SKCanvas` 
using the extensions methods:

```csharp
SKCanvas canvas = ...

canvas.DrawStar(
    100, 100, // x, y
    100,      // outer radius
    50,       // inner radius
    5);       // points
```
