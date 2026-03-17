#nullable enable

using System.IO;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Decodes raw image data from a stream into an <see cref="ISKDeepZoomTile"/>.
    /// Implement this interface to plug in a custom image decoder (e.g. SkiaSharp, ImageSharp).
    /// </summary>
    public interface ISKDeepZoomTileDecoder
    {
        /// <summary>
        /// Decodes the image data in <paramref name="data"/> and returns the resulting tile,
        /// or <see langword="null"/> if decoding fails.
        /// </summary>
        /// <param name="data">A fully-buffered stream containing the raw image bytes.</param>
        ISKDeepZoomTile? Decode(Stream data);
    }
}
