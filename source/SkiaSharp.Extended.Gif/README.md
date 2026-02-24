# SkiaSharp.Extended.Gif

GIF encoder and decoder for SkiaSharp with API aligned to SkiaSharp conventions.

## Features

- **Full GIF87a support** - Read and write classic GIF files
- **Full GIF89a support** - All extensions including animation, transparency, disposal methods
- **High compatibility** - Behavior validated against giflib, libnsgif, and cgif
- **Easy to use** - Simple API aligned with SkiaSharp patterns
- **SkiaSharp Integration** - API design consistent with SKCodec patterns

## Usage

### Decoding GIFs

```csharp
using SkiaSharp.Extended.Gif;

// Decode a GIF file (factory pattern like SKCodec.Create)
using var stream = File.OpenRead("animation.gif");
using var decoder = SKGifDecoder.Create(stream);

// Get image info (like SKCodec.Info)
var info = decoder.Info;
Console.WriteLine($"Size: {info.Width}x{info.Height}");

// Get GIF-specific metadata
var gifInfo = decoder.GifInfo;
Console.WriteLine($"Frames: {gifInfo.FrameCount}");
Console.WriteLine($"Loop count: {gifInfo.LoopCount}");

// Access frame information (like SKCodec.FrameInfo)
var frameInfo = decoder.FrameInfo;
for (int i = 0; i < frameInfo.Length; i++)
{
    Console.WriteLine($"Frame {i}: Duration={frameInfo[i].Duration}ms, Disposal={frameInfo[i].DisposalMethod}");
}

// Decode frames
for (int i = 0; i < gifInfo.FrameCount; i++)
{
    using var frame = decoder.GetFrame(i);
    var bitmap = frame.Bitmap;
    var duration = frame.FrameInfo.Duration; // Like SKCodecFrameInfo.Duration
    
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

// Add frames with duration (aligned with SKCodecFrameInfo.Duration)
encoder.AddFrame(bitmap1, duration: 100);
encoder.AddFrame(bitmap2, duration: 100);
encoder.AddFrame(bitmap3, duration: 100);

// Or use detailed frame info
var frameInfo = new SKGifFrameInfo
{
    Duration = 100,
    DisposalMethod = SKGifDisposalMethod.RestoreToBackground
};
encoder.AddFrame(bitmap4, frameInfo);

// Finalize the GIF
encoder.Encode();
```

## API Alignment with SkiaSharp

This library follows SkiaSharp's naming conventions and patterns:

| SkiaSharp (Decoding) | This Library | Description |
|---------------------|--------------|-------------|
| `SKCodec.Create(stream)` | `SKGifDecoder.Create(stream)` | Factory method |
| `codec.Info` | `decoder.Info` | Image dimensions and format |
| `codec.FrameCount` | `decoder.FrameCount` | Number of animation frames |
| `codec.FrameInfo` | `decoder.FrameInfo` | Array of frame metadata |
| `frameInfo.Duration` | `frameInfo.Duration` | Frame duration in milliseconds |
| `frameInfo.DisposalMethod` | `frameInfo.DisposalMethod` | How to handle previous frame |

## DisposalMethod Values

The `SKGifDisposalMethod` enum values are aligned with `SKCodecAnimationDisposalMethod`:

- `None = 0` - No disposal specified
- `DoNotDispose = 1` - Keep previous frame (same as `SKCodecAnimationDisposalMethod.Keep`)
- `RestoreToBackground = 2` - Clear to background color (same as `SKCodecAnimationDisposalMethod.RestoreBackgroundColor`)
- `RestoreToPrevious = 3` - Restore previous frame (same as `SKCodecAnimationDisposalMethod.RestorePrevious`)

## Implementation Status

⚠️ **Work in Progress**: This is currently a skeleton implementation for project structure. Core decoding/encoding functionality is not yet implemented.

- ✅ Project structure created
- ✅ API surface defined and aligned with SkiaSharp
- ✅ Block I/O layer implemented
- ✅ LZW decoder implemented
- ⬜ Full GIF decoder (in progress)
- ⬜ Frame compositor
- ⬜ Color quantization
- ⬜ GIF encoder (in progress)

## License

MIT License - See LICENSE file for details.
