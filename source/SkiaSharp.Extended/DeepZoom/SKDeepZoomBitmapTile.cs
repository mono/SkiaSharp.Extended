#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// A <see cref="ISKDeepZoomTile"/> backed by an <see cref="SKBitmap"/>.
    /// This is the SkiaSharp-specific implementation of the opaque tile interface.
    /// </summary>
    public sealed class SKDeepZoomBitmapTile : ISKDeepZoomTile
    {
        /// <summary>The underlying decoded bitmap.</summary>
        public SKBitmap Bitmap { get; }

        /// <summary>Wraps <paramref name="bitmap"/> as an <see cref="ISKDeepZoomTile"/>.</summary>
        public SKDeepZoomBitmapTile(SKBitmap bitmap)
        {
            Bitmap = bitmap;
        }

        /// <inheritdoc />
        public void Dispose() => Bitmap.Dispose();
    }
}
