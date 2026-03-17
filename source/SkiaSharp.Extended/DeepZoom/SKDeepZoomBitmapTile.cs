#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// A <see cref="ISKDeepZoomTile"/> backed by an <see cref="SKBitmap"/>.
/// This is the SkiaSharp-specific implementation of the opaque tile interface.
/// </summary>
public sealed class SKDeepZoomBitmapTile(SKBitmap bitmap) : ISKDeepZoomTile
{
    /// <summary>The underlying decoded bitmap.</summary>
    public SKBitmap Bitmap { get; } = bitmap;

    /// <inheritdoc />
    public void Dispose() => Bitmap.Dispose();
}
