using System;
using System.IO;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif
{
/// <summary>
/// Decodes GIF files (GIF87a and GIF89a) to SKBitmap frames.
/// API aligned with SKCodec patterns for consistency.
/// </summary>
public class SKGifDecoder : IDisposable
{
private readonly Stream stream;
private readonly GifReader reader;
private bool disposed;

private SKGifDecoder(Stream stream)
{
this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
this.reader = new GifReader(stream);
}

/// <summary>
/// Gets the image information (width, height, color type, alpha type).
/// Aligned with SKCodec.Info.
/// </summary>
public SKImageInfo Info { get; private set; }

/// <summary>
/// Gets GIF-specific metadata including loop count and extensions.
/// </summary>
public SKGifInfo GifInfo { get; private set; } = null!;

/// <summary>
/// Gets the number of frames in the GIF.
/// Aligned with SKCodec.FrameCount.
/// </summary>
public int FrameCount => GifInfo.FrameCount;

/// <summary>
/// Gets information about all frames in the GIF.
/// Aligned with SKCodec.FrameInfo.
/// </summary>
public SKGifFrameInfo[] FrameInfo { get; private set; } = null!;

/// <summary>
/// Creates a new GIF decoder from a stream.
/// Factory pattern aligned with SKCodec.Create.
/// </summary>
/// <param name="stream">The stream containing the GIF data.</param>
/// <returns>A new SKGifDecoder instance.</returns>
/// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
/// <exception cref="InvalidDataException">Thrown when the stream does not contain valid GIF data.</exception>
public static SKGifDecoder Create(Stream stream)
{
var decoder = new SKGifDecoder(stream);
decoder.Initialize();
return decoder;
}

/// <summary>
/// Gets information about a specific frame.
/// </summary>
/// <param name="index">The zero-based frame index.</param>
/// <returns>Frame information.</returns>
/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
public SKGifFrameInfo GetFrameInfo(int index)
{
if (index < 0 || index >= FrameCount)
throw new ArgumentOutOfRangeException(nameof(index));

return FrameInfo[index];
}

/// <summary>
/// Gets the specified frame from the GIF as a decoded bitmap.
/// </summary>
/// <param name="index">The zero-based index of the frame to retrieve.</param>
/// <returns>The decoded frame with bitmap and metadata.</returns>
/// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
public SKGifFrame GetFrame(int index)
{
if (index < 0 || index >= FrameCount)
throw new ArgumentOutOfRangeException(nameof(index));

// TODO: Implement frame decoding
throw new NotImplementedException("Frame decoding not yet implemented");
}

/// <summary>
/// Disposes the decoder and releases resources.
/// </summary>
public void Dispose()
{
if (!disposed)
{
disposed = true;
}

GC.SuppressFinalize(this);
}

private void Initialize()
{
// Read GIF header
var header = reader.ReadHeader();
if (!header.IsValid)
throw new InvalidDataException("Invalid GIF header");

// Read logical screen descriptor
var screenDescriptor = reader.ReadLogicalScreenDescriptor();

// Set up minimal image info
Info = new SKImageInfo(
screenDescriptor.Width,
screenDescriptor.Height,
SKColorType.Rgba8888,
SKAlphaType.Premul);

// For now, just create minimal GifInfo
GifInfo = new SKGifInfo
{
ImageInfo = Info,
FrameCount = 1 // TODO: Parse actual frame count
};

// Create minimal frame info
FrameInfo = new[]
{
new SKGifFrameInfo
{
Duration = 0,
DisposalMethod = SKGifDisposalMethod.None,
RequiredFrame = -1,
FrameRect = new SKRectI(0, 0, screenDescriptor.Width, screenDescriptor.Height),
HasTransparency = false,
TransparentColor = null
}
};
}
}
}
