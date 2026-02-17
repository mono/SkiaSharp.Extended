# SkiaSharp.Extended.Gif

GIF encoder and decoder for SkiaSharp with API aligned to SkiaSharp conventions.

## Features

- **Full GIF87a support** - Read and write classic GIF files
- **Full GIF89a support** - All extensions including animation, transparency, disposal methods
- **High compatibility** - Behavior validated against giflib, libnsgif, and cgif
- **Easy to use** - Simple API aligned with SkiaSharp patterns
- **SkiaSharp Integration** - API design consistent with SKCodec and SKRuntimeEffect patterns
- **Flexible Error Handling** - Create* methods for graceful handling, Build* methods for fail-fast

## Usage

### Decoding GIFs - Two Error Handling Options

#### Option 1: Fail-Fast (Build* Pattern - Like SKRuntimeEffect)

```csharp
using SkiaSharp.Extended.Gif;

// BuildDecoder throws exception if GIF is invalid (recommended for most cases)
using var stream = File.OpenRead("animation.gif");
using var decoder = SKGifDecoder.BuildDecoder(stream);  // Throws on error

// Get image info (like SKCodec.Info)
var info = decoder.Info;
Console.WriteLine($"Size: {info.Width}x{info.Height}");
```

#### Option 2: Graceful Error Handling (Create* Pattern - Like SKRuntimeEffect)

```csharp
using SkiaSharp.Extended.Gif;

// CreateDecoder returns null with error details if GIF is invalid
using var stream = File.OpenRead("animation.gif");
var decoder = SKGifDecoder.CreateDecoder(stream, out var errors);

if (decoder == null)
{
    Console.WriteLine($"Failed to decode GIF: {errors}");
    return;
}

using (decoder)
{
    // Use decoder...
}
```

#### Option 3: Original Create Method (Backwards Compatible)

```csharp
// Original Create method still works (delegates to BuildDecoder)
using var decoder = SKGifDecoder.Create(stream);  // Throws on error
```

### Working with Decoded GIFs
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

### Encoding GIFs - Two Error Handling Options

#### Option 1: Fail-Fast (Build* Pattern - Like SKRuntimeEffect)

```csharp
using SkiaSharp.Extended.Gif;

// BuildEncoder throws exception if stream is invalid
using var stream = File.Create("output.gif");
using var encoder = SKGifEncoder.BuildEncoder(stream);  // Throws on error

// Configure animation
encoder.SetLoopCount(0); // 0 = infinite loop

// Add frames with duration (aligned with SKCodecFrameInfo.Duration)
encoder.AddFrame(bitmap1, duration: 100);
encoder.AddFrame(bitmap2, duration: 100);

// Finalize the GIF
encoder.Encode();
```

#### Option 2: Graceful Error Handling (Create* Pattern - Like SKRuntimeEffect)

```csharp
using SkiaSharp.Extended.Gif;

// CreateEncoder returns null with error details if stream is invalid
using var stream = File.Create("output.gif");
var encoder = SKGifEncoder.CreateEncoder(stream, out var errors);

if (encoder == null)
{
    Console.WriteLine($"Failed to create encoder: {errors}");
    return;
}

using (encoder)
{
    encoder.SetLoopCount(0);
    encoder.AddFrame(bitmap, duration: 100);
    encoder.Encode();
}
```

#### Option 3: Constructor (Traditional)

```csharp
// Create an encoder using constructor (throws on error)
using var stream = File.Create("output.gif");
using var encoder = new SKGifEncoder(stream);

// Configure animation
encoder.SetLoopCount(0); // 0 = infinite loop

// Add frames
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

## API Pattern - Following SKRuntimeEffect

This library follows SkiaSharp's `SKRuntimeEffect` pattern for operations that can fail:

### Create* Methods (Graceful Error Handling)
- Return `null` if operation fails
- Provide detailed error message via `out string? errors` parameter
- Use when you want to handle errors gracefully
- Better for user input validation

```csharp
var decoder = SKGifDecoder.CreateDecoder(stream, out var errors);
if (decoder == null)
{
    // Handle error gracefully
    ShowError(errors);
}
```

### Build* Methods (Fail-Fast)
- Throw exception if operation fails
- Use when errors are exceptional
- Cleaner code when you expect success
- Better for internal operations

```csharp
var decoder = SKGifDecoder.BuildDecoder(stream);  // Throws on error
// Continue with valid decoder
```

## API Alignment with SkiaSharp

This library follows SkiaSharp's naming conventions and patterns:

- **Factory Pattern**: `SKGifDecoder.Create(stream)` matches `SKCodec.Create(stream)`
- **Info Properties**: `decoder.Info` and `decoder.FrameInfo` match `SKCodec.Info` and `SKCodec.FrameInfo`
- **Duration**: Uses milliseconds like `SKCodecFrameInfo.Duration` (not "delay")
- **Disposal Methods**: Enum values align with `SKCodecAnimationDisposalMethod`
- **Types**: Uses `SKImageInfo`, `SKRectI`, `SKColor`, etc.

### Disposal Method Mapping

| SKGifDisposalMethod | Value | SKCodecAnimationDisposalMethod |
|---------------------|-------|--------------------------------|
| None | 0 | (not in SKCodec) |
| DoNotDispose | 1 | Keep |
| RestoreToBackground | 2 | RestoreBackgroundColor |
| RestoreToPrevious | 3 | RestorePrevious |

## Documentation

For detailed documentation, see:
- [SkiaSharp Analysis](../../docs/gif/SKIASHARP-ANALYSIS.md) - How this API aligns with SkiaSharp
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

The API design is aligned with SkiaSharp's patterns for consistency and familiarity.

