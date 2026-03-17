#nullable enable

using System.IO;

namespace SkiaSharp.Extended;

/// <summary>
/// Decodes raw image data from a stream into an <see cref="ISKImagePyramidTile"/>.
/// Implement this interface to plug in a custom image decoder (e.g. SkiaSharp, ImageSharp).
/// </summary>
public interface ISKImagePyramidTileDecoder
{
    /// <summary>
    /// Decodes the image data in <paramref name="data"/> and returns the resulting tile,
    /// or <see langword="null"/> if decoding fails.
    /// </summary>
    /// <param name="data">A fully-buffered stream containing the raw image bytes.</param>
    ISKImagePyramidTile? Decode(Stream data);
}
