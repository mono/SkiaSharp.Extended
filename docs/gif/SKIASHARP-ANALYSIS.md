# SkiaSharp GIF Capabilities Analysis

## Overview

This document analyzes SkiaSharp's existing GIF support to inform our implementation strategy for `SkiaSharp.Extended.Gif`.

## SkiaSharp's Current GIF Support

### What SkiaSharp Provides

SkiaSharp includes GIF decoding through the native Skia library via the `SKCodec` API.

#### API Surface

```csharp
// Create codec from stream
using var stream = new SKManagedStream(gifStream, true);
using var codec = SKCodec.Create(stream);

// Access frame information
int frameCount = codec.FrameCount;
SKCodecFrameInfo[] frames = codec.FrameInfo;
SKImageInfo info = codec.Info; // Width, Height, ColorType, AlphaType

// Decode specific frame
using var bitmap = new SKBitmap(info);
var options = new SKCodecOptions(frameIndex);
SKCodecResult result = codec.GetPixels(info, bitmap.GetPixels(), options);
```

#### Key Classes and Properties

**SKCodec:**
- `Create(Stream)` - Factory method for creating codec
- `FrameCount` - Number of frames in the image
- `FrameInfo` - Array of `SKCodecFrameInfo` for each frame
- `Info` - `SKImageInfo` with dimensions and color information
- `GetPixels(info, pixels, options)` - Decode frame to pixel buffer

**SKCodecFrameInfo (struct):**
- `Duration` - Frame duration in milliseconds
- `DisposalMethod` - `SKCodecAnimationDisposalMethod` enum
- `RequiredFrame` - Index of frame needed for compositing (-1 if independent)
- `FrameRect` - Rectangle occupied by the frame (`SKRectI`)
- `AlphaType` - Alpha channel information
- `Blend` - Blend mode for compositing
- `HasAlphaWithinBounds` - Whether frame has transparency
- `FullyReceived` - Whether frame data is complete

**SKCodecAnimationDisposalMethod (enum):**
```csharp
public enum SKCodecAnimationDisposalMethod
{
    Keep = 1,                    // Next frame draws on top
    RestoreBackgroundColor = 2,  // Clear to background before next
    RestorePrevious = 3          // Restore to previous frame
}
```

**SKCodecOptions (struct):**
- Constructor: `SKCodecOptions(int frameIndex)`
- `FrameIndex` - Which frame to decode
- `PriorFrame` - Frame index to use as base for compositing
- `HasFrameIndex`, `HasPriorFrame` - Property flags

### What SkiaSharp Does NOT Provide

❌ **GIF Encoding** - No encoder API at all  
❌ **Loop Count** - NETSCAPE extension not exposed  
❌ **Comment Extensions** - Text comments not accessible  
❌ **Application Extensions** - Custom app data not exposed  
❌ **Plain Text Extensions** - Not accessible  
❌ **Convenience Wrappers** - Low-level API only  
❌ **Direct Frame Access** - Must decode each time  
❌ **Metadata Helpers** - No high-level metadata object  

## Implications for Our Implementation

### Decoder Strategy Options

#### Option A: Wrap SKCodec (Hybrid Approach)

**Pros:**
- ✅ Leverage native performance (C++ libgif via P/Invoke)
- ✅ Automatic format updates as Skia updates
- ✅ Battle-tested implementation
- ✅ Smaller code maintenance burden
- ✅ Memory-efficient (native heap)

**Cons:**
- ❌ Limited to what Skia exposes
- ❌ Loop count not available
- ❌ Extension metadata not accessible
- ❌ Cannot fix Skia bugs without upstream changes

#### Option B: Pure C# Implementation

**Pros:**
- ✅ Full control over all features
- ✅ Access to all GIF extensions
- ✅ Loop count support
- ✅ Comment/application extension support
- ✅ Can optimize for specific scenarios
- ✅ Behavior matches reference libraries exactly

**Cons:**
- ❌ More code to write and maintain
- ❌ Must ensure competitive performance
- ❌ Need comprehensive testing
- ❌ Managed heap pressure for large GIFs

### Recommended Hybrid Approach

**For Decoding:**
1. **Primary decoder**: Use `SKCodec` wrapper for common cases
2. **Extended decoder**: Pure C# implementation for advanced scenarios
3. **API Design**: Single facade that uses SKCodec when possible

```csharp
public class SKGifDecoder
{
    // Factory automatically chooses best implementation
    public static SKGifDecoder Create(Stream stream, SKGifDecoderOptions? options = null)
    {
        if (options?.UseNativeCodec ?? true)
            return new SKGifCodecDecoder(stream); // Wraps SKCodec
        else
            return new SKGifManagedDecoder(stream); // Pure C# implementation
    }
}
```

**For Encoding:**
- Must implement from scratch (no native support)
- Follow SkiaSharp naming conventions

### API Design Principles

To maintain consistency with SkiaSharp:

1. **Naming Conventions:**
   - ✅ Use `Duration` instead of `DelayMs` (matches `SKCodecFrameInfo.Duration`)
   - ✅ Use `DisposalMethod` enum (similar to `SKCodecAnimationDisposalMethod`)
   - ✅ Use `Create` factory pattern (matches `SKCodec.Create`)
   - ✅ Use `GetPixels` or `GetFrame` for frame access
   - ✅ Use `Info` for metadata (matches `SKCodec.Info`)

2. **Type Patterns:**
   - ✅ Return `SKBitmap` for frames
   - ✅ Use `SKImageInfo` for dimensions/color
   - ✅ Implement `IDisposable` for resources
   - ✅ Use `Stream` for I/O

3. **Enum Values:**
   - Match `SKCodecAnimationDisposalMethod` numbering if possible
   - Keep = 1, RestoreBackgroundColor = 2, RestorePrevious = 3
   - Add None = 0 for no disposal specified (GIF default)

## Proposed API Alignment

### Updated SKGifDecoder

```csharp
public class SKGifDecoder : IDisposable
{
    // Factory pattern (like SKCodec.Create)
    public static SKGifDecoder Create(Stream stream, SKGifDecoderOptions? options = null);
    
    // Metadata (like SKCodec.Info, .FrameCount)
    public SKImageInfo Info { get; }
    public int FrameCount { get; }
    public SKGifInfo GifInfo { get; } // Extended metadata
    
    // Frame access (like SKCodec.FrameInfo)
    public SKGifFrameInfo[] FrameInfo { get; }
    public SKGifFrameInfo GetFrameInfo(int index);
    
    // Frame decoding (like SKCodec.GetPixels)
    public SKBitmap GetFrame(int index);
    public SKCodecResult GetPixels(int index, SKImageInfo info, IntPtr pixels);
}

public class SKGifInfo
{
    public int Width { get; }
    public int Height { get; }
    public int FrameCount { get; }
    public int LoopCount { get; } // 0 = infinite, -1 = no loop
    public SKColor BackgroundColor { get; }
    public string? Comment { get; } // From comment extension
    public byte[]? ApplicationData { get; } // From application extension
}

public struct SKGifFrameInfo
{
    public int Duration { get; } // milliseconds (matches SKCodecFrameInfo)
    public SKGifDisposalMethod DisposalMethod { get; }
    public int RequiredFrame { get; } // -1 if independent (matches SKCodecFrameInfo)
    public SKRectI FrameRect { get; } // Frame bounds (matches SKCodecFrameInfo)
    public bool HasTransparency { get; }
    public SKColor? TransparentColor { get; }
}

public enum SKGifDisposalMethod
{
    None = 0,                      // No disposal specified
    DoNotDispose = 1,              // Keep (same as SKCodecAnimationDisposalMethod.Keep)
    RestoreToBackground = 2,       // Clear (same as SKCodecAnimationDisposalMethod.RestoreBackgroundColor)
    RestoreToPrevious = 3          // Restore (same as SKCodecAnimationDisposalMethod.RestorePrevious)
}

public class SKGifDecoderOptions
{
    public bool UseNativeCodec { get; set; } = true; // Use SKCodec when possible
    public bool StrictMode { get; set; } = false;    // Strict spec compliance
}
```

### Updated SKGifEncoder

```csharp
public class SKGifEncoder : IDisposable
{
    public SKGifEncoder(Stream stream, SKGifEncoderOptions? options = null);
    
    // Encoding
    public void AddFrame(SKBitmap bitmap, SKGifFrameInfo? frameInfo = null);
    public void AddFrame(SKBitmap bitmap, int duration = 100);
    
    // Configuration
    public void SetLoopCount(int count); // 0 = infinite, -1 = no loop
    public void SetComment(string comment);
    public void SetApplicationData(string applicationId, byte[] data);
    
    // Finalization
    public void Encode(); // or Save()
}

public class SKGifEncoderOptions
{
    public int LoopCount { get; set; } = -1;
    public int DefaultDuration { get; set; } = 100;
    public SKGifQuantizationMethod Quantization { get; set; } = SKGifQuantizationMethod.Octree;
    public int MaxColors { get; set; } = 256;
    public bool Dithering { get; set; } = false;
}
```

## Performance Comparison Plan

To decide between wrapper vs pure implementation:

1. **Benchmark Decoding:**
   - SKCodec wrapper vs pure C# implementation
   - Measure: frame decode time, memory usage, startup cost
   - Test with: small GIFs, large GIFs, many-frame animations

2. **Feature Matrix:**
   - Create table of what each approach can/cannot do
   - Identify scenarios where pure C# is required

3. **Hybrid Strategy:**
   - Use SKCodec by default for better performance
   - Fall back to pure C# when extensions needed
   - Make it seamless to the user

## Recommendations

### Immediate Actions

1. **Update API Design** ✅
   - Rename properties to match SkiaSharp conventions
   - Align enum values with `SKCodecAnimationDisposalMethod`
   - Use `SKImageInfo`, `SKRectI`, and other Skia types

2. **Create Prototype Wrapper** 🔄
   - Build simple SKCodec wrapper
   - Test with sample GIFs
   - Identify limitations

3. **Implement Pure Decoder** 🔄
   - Start with block I/O and LZW
   - Build decoder with full extension support
   - Benchmark against SKCodec

4. **Document Decision** 📝
   - Record performance results
   - List feature trade-offs
   - Choose primary approach

### Long-term Strategy

- **v1.0**: Hybrid decoder (SKCodec wrapper + pure C# fallback) + Pure C# encoder
- **v1.1**: Optimize based on real-world usage patterns
- **v2.0**: Consider full pure C# if needed for features/control

## References

- [SKCodec Class Documentation](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcodec)
- [SKCodecFrameInfo Struct](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcodecframeinfo)
- [Animating SkiaSharp Bitmaps](http://mono.github.io/SkiaSharp/docs/bitmaps/animating.html)
- [DecodeGifFramesSample.cs](https://github.com/mono/SkiaSharp/blob/main/samples/Gallery/Shared/Samples/DecodeGifFramesSample.cs)
- [SKCodecAnimationDisposalMethod](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcodecanimationdisposalmethod)

## Conclusion

SkiaSharp provides solid GIF decoding via `SKCodec`, but lacks encoding and extended metadata access. Our implementation should:

1. **Align API naming** with SkiaSharp conventions for consistency
2. **Leverage SKCodec** where possible for performance
3. **Extend functionality** with pure C# for missing features
4. **Implement encoder** from scratch (no native option)
5. **Make it seamless** - users shouldn't need to know about the hybrid approach

This gives us the best of both worlds: native performance when possible, full feature set when needed.
