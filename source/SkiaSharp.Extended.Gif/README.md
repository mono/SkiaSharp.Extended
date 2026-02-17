# SkiaSharp.Extended.Gif

GIF encoder and decoder for SkiaSharp.

## Features

- **Full GIF87a support** - Read and write classic GIF files
- **Full GIF89a support** - All extensions including animation, transparency, disposal methods
- **High compatibility** - Behavior validated against giflib, libnsgif, and cgif
- **Easy to use** - Simple API for encoding and decoding GIFs

## Usage

### Decoding GIFs

```csharp
using SkiaSharp.Extended.Gif;

// Decode a GIF file
using var stream = File.OpenRead("animation.gif");
using var decoder = SKGifDecoder.Create(stream);

// Get metadata
var metadata = decoder.Metadata;
Console.WriteLine($"Size: {metadata.Width}x{metadata.Height}");
Console.WriteLine($"Frames: {metadata.FrameCount}");

// Decode frames
for (int i = 0; i < metadata.FrameCount; i++)
{
    using var frame = decoder.GetFrame(i);
    var bitmap = frame.Bitmap;
    var delayMs = frame.DelayMs;
    
    // Use the bitmap...
}
```

### Encoding GIFs

```csharp
using SkiaSharp.Extended.Gif;

// Create an encoder
using var stream = File.Create("output.gif");
using var encoder = new SKGifEncoder(stream);

// Configure animation
encoder.SetLoopCount(0); // 0 = infinite loop

// Add frames
encoder.AddFrame(bitmap1, delayMs: 100);
encoder.AddFrame(bitmap2, delayMs: 100);
encoder.AddFrame(bitmap3, delayMs: 100);

// Save the GIF
encoder.Save();
```

## Documentation

For detailed documentation, see:
- [Implementation Plan](../../docs/gif/implementation-plan.md)
- [Compatibility Decision Library](../../docs/gif/compatibility-decision-library.md)
- [Disagreement Matrix](../../docs/gif/disagreement-matrix.md)

## License

This library is licensed under the MIT License.

## References

This implementation validates behavior against three MIT-licensed reference libraries:
- **giflib** - Classic C library for GIF encoding/decoding
- **libnsgif** - NetSurf's GIF decoder
- **cgif** - Modern C library for GIF encoding
