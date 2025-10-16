# SKGeometry

**SKGeometry** provides several helper methods that can be used to create common geometric shapes.

## Methods

There are a few helper methods that can be used to create geometric shapes in the `SKGeometry` type:

| Method                        | Description |
| :---------------------------- | :---------- |
| **CreateSectorPath**          | Creates a path with the shape of sector/section of a doughnut/pie chart. |
| **CreatePiePath**             | Creates a path with the shape of a doughnut/pie chart. |
| **CreateSquarePath**          | Creates a path with the shape of a square. |
| **CreateRectanglePath**       | Creates a path with the shape of a rectangle. |
| **CreateTrianglePath**        | Creates a path with the shape of a triangle. |
| **CreateRegularPolygonPath**  | Creates a path with the shape of some regular polygon. |
| **CreateRegularStarPath**     | Creates a path with the shape of a square. |

## Extension Methods

Some of these shapes can also be draw directly on a `SKCanvas` using the extensions methods:

```csharp
SKCanvas canvas = ...

canvas.DrawStar(
    100, 100, // x, y
    100,      // outer radius
    50,       // inner radius
    5);       // points
```
