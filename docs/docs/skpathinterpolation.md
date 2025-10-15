# SKPathInterpolation

**SKPathInterpolation** can be used to create interpolated paths. This is awesome when creating animated shapes or transitions between two paths.

| Path Interpolation |
| :----------------: |
| ![Path Interpolation][interpolation] |

The code is also very simple, just create a `SKPathInterpolation` instance and then ask for each step:

```csharp
var interpolation = new SKPathInterpolation(startPath, endPath);

var halfWayPath = interpolation.Interpolate(0.5f);
var almostTherePath = interpolation.Interpolate(0.9f);
```

## Methods

There are a few helper methods that can be used to create geometric shapes in the `SKGeometry` type:

| Method           | Description |
| :--------------- | :---------- |
| **Interpolate**  | Creates a new path interpolated between the start and end paths. |


[interpolation]: ../images/extended/skpathinterpolation/interpolation.gif
