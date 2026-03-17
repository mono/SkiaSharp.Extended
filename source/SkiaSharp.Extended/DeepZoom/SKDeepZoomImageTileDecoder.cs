#nullable enable

using System.IO;

namespace SkiaSharp.Extended;

/// <summary>
/// Decodes image streams into <see cref="SKDeepZoomImageTile"/> using SkiaSharp.
/// </summary>
public class SKDeepZoomImageTileDecoder : ISKDeepZoomTileDecoder
{
    /// <inheritdoc />
    public ISKDeepZoomTile? Decode(Stream data)
    {
        var image = SKImage.FromEncodedData(data);
        return image != null ? new SKDeepZoomImageTile(image) : null;
    }
}
