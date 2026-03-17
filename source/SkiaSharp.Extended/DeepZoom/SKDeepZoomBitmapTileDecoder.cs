#nullable enable

using System.IO;

namespace SkiaSharp.Extended;

/// <summary>
/// Decodes image streams into <see cref="SKDeepZoomBitmapTile"/> using SkiaSharp.
/// </summary>
public class SKDeepZoomBitmapTileDecoder : ISKDeepZoomTileDecoder
{
    /// <inheritdoc />
    public ISKDeepZoomTile? Decode(Stream data)
    {
        var bitmap = SKBitmap.Decode(data);
        return bitmap != null ? new SKDeepZoomBitmapTile(bitmap) : null;
    }
}
