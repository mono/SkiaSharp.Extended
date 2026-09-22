# SkiaSharp.Extended

`SkiaSharp.Extended` provides practical image-processing and drawing utilities
that complement [SkiaSharp](https://github.com/mono/SkiaSharp). Use it when you
need compact image placeholders, geometry helpers, path interpolation, or
pixel-level image comparison without building those features yourself.

## Install

```shell
dotnet add package SkiaSharp.Extended
```

## Quick start

Create a compact [BlurHash](https://blurha.sh/) placeholder for an image:

```csharp
using SkiaSharp;
using SkiaSharp.Extended;

using var bitmap = SKBitmap.Decode("photo.jpg");
var hash = SKBlurHash.Serialize(bitmap, componentsX: 4, componentsY: 3);

using var placeholder = SKBlurHash.DeserializeBitmap(hash, width: 32, height: 32);
```

Show the small decoded image while the full image loads, then replace it with
the original. A `4 x 3` component hash is a useful default for most images.

## Included features

- **BlurHash** — encode and decode compact image placeholders with
  `SKBlurHash`.
- **Geometry** — create shapes and work with paths through `SKGeometry` and
  `SKGeometryExtensions`.
- **Lottie playback** — drive loaded animations with `SKLottiePlayer` and `SKLottieRepeat`. The MAUI and Blazor hosts share the same player,
  internal loader configuration, rendering behavior, and monotonic frame timing.
- **Path interpolation** — animate between compatible paths with
  `SKPathInterpolation`.
- **Pixel comparison** — compare rendered images with `SKPixelComparer`.

## Compatibility

The package supports `netstandard2.0` and `net10.0`, and depends on `SkiaSharp` and `SkiaSharp.Skottie`.

## Documentation and support

- [Guides and API reference](https://mono.github.io/SkiaSharp.Extended/)
- [Interactive samples](https://mono.github.io/SkiaSharp.Extended/sample/)
- [Source code and issue tracker](https://github.com/mono/SkiaSharp.Extended)

## License

Licensed under the [MIT License](https://github.com/mono/SkiaSharp.Extended/blob/main/LICENSE).
