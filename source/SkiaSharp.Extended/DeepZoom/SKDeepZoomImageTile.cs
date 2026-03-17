#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// A <see cref="ISKDeepZoomTile"/> backed by an <see cref="SKImage"/>.
/// This is the SkiaSharp-specific implementation of the opaque tile interface.
/// Using <see cref="SKImage"/> instead of <see cref="SKBitmap"/> allows GPU-accelerated rendering.
/// </summary>
public sealed class SKDeepZoomImageTile(SKImage image) : ISKDeepZoomTile
{
    /// <summary>The underlying decoded image.</summary>
    public SKImage Image { get; } = image;

    /// <inheritdoc />
    public void Dispose() => Image.Dispose();
}
