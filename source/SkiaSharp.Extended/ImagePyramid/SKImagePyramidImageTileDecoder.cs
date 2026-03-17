#nullable enable

using System.IO;

namespace SkiaSharp.Extended;

/// <summary>
/// Decodes image streams into <see cref="SKImagePyramidImageTile"/> using SkiaSharp.
/// </summary>
public class SKImagePyramidImageTileDecoder : ISKImagePyramidTileDecoder
{
    /// <inheritdoc />
    public ISKImagePyramidTile? Decode(Stream data)
    {
        var image = SKImage.FromEncodedData(data);
        return image != null ? new SKImagePyramidImageTile(image) : null;
    }
}
