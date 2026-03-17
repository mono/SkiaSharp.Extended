#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// Represents a single sub-image within a DZC collection.
/// Maps to Silverlight's MultiScaleSubImage.
/// </summary>
public class SKImagePyramidDziCollectionSubImage(int id, int mortonIndex, int width, int height, string? source)
{
    /// <summary>Item ID from the DZC.</summary>
    public int Id { get; } = id;

    /// <summary>Morton (Z-order) index in the mosaic grid.</summary>
    public int MortonIndex { get; } = mortonIndex;

    /// <summary>Full image width in pixels.</summary>
    public int Width { get; } = width;

    /// <summary>Full image height in pixels.</summary>
    public int Height { get; } = height;

    /// <summary>Optional individual DZI source path.</summary>
    public string? Source { get; } = source;

    /// <summary>Aspect ratio (width / height).</summary>
    public double AspectRatio => Height == 0 ? 1.0 : (double)Width / Height;

    /// <summary>Viewport width in the DZC mosaic coordinate system.</summary>
    public double ViewportWidth { get; set; }

    /// <summary>Viewport X origin in the DZC mosaic coordinate system.</summary>
    public double ViewportX { get; set; }

    /// <summary>Viewport Y origin in the DZC mosaic coordinate system.</summary>
    public double ViewportY { get; set; }
}
