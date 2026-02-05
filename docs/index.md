# SkiaSharp.Extended

## ⚠️ Important Notice for SVG Users

**SkiaSharp.Extended.Svg has been deprecated**. If you're using `SkiaSharp.Extended.Svg` and encountering errors with .NET 9 or recent SkiaSharp versions (such as `MissingMethodException: Method not found: SKMatrix.MakeTranslation`), please see the [SVG Migration Guide](docs/svg-migration.md) for instructions on migrating to [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia).

---

## SkiaSharp.Extended

**SkiaSharp.Extended** is a collection some cool libraries.

### SKBlurHash

**SKBlurHash** is a compact representation of a placeholder for an image.

| Preview |
| :-----: |
| ![BlurHash][blur-img] |

### SKGeometry

**SKGeometry** provides several helper methods that can be used to create common geometric shapes.

### SKPathInterpolation

SKPathInterpolation can be used to create interpolated paths. This is awesome when creating animated shapes or transitions between two paths.

| Preview |
| :-----: |
| ![Path Interpolation][interpolation-img] |


## SkiaSharp.Extended.UI.Maui

**SkiaSharp.Extended.UI.Maui** is a collection some great .NET MAUI controls.

### SKConfettiView

The confetti view is a container for one or more systems of confetti particles.

| Preview |
| :-----: |
| ![top-stream][confetti-img] |

### SKLottieView

The Lottie view is a animated view that can playback Lottie files.

| Preview |
| :-----: |
| ![lottie][lottie-img] |


[blur-img]: images/extended/skblurhash/blur-small.png
[interpolation-img]: images/extended/skpathinterpolation/interpolation.gif
[lottie-img]: images/ui/controls/sklottieview/lottie.gif
[confetti-img]: images/ui/controls/skconfettiview/top-stream.gif
