#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// A <see cref="ISKImagePyramidTile"/> backed by an <see cref="SKImage"/>.
/// This is the SkiaSharp-specific implementation of the opaque tile interface.
/// Using <see cref="SKImage"/> instead of <see cref="SKBitmap"/> allows GPU-accelerated rendering.
/// </summary>
public sealed class SKImagePyramidImageTile(SKImage image) : ISKImagePyramidTile
{
    /// <summary>The underlying decoded image.</summary>
    public SKImage Image { get; } = image;

    /// <inheritdoc />
    public void Dispose() => Image.Dispose();
}
