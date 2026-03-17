#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Represents a single sub-image within a DZC collection, with its position in the mosaic.
/// Mirrors Silverlight's MultiScaleSubImage with inverted viewport coordinates.
/// </summary>
public class SKDeepZoomSubImage(int id, int mortonIndex, double aspectRatio, string? source)
{
    private double _viewportOriginX;
    private double _viewportOriginY;
    private double _viewportWidth;
    private double _opacity = 1.0;
    private int _zIndex;

    /// <summary>Item ID from the DZC.</summary>
    public int Id { get; } = id;

    /// <summary>Morton (Z-order) index of this item in the DZC mosaic grid.</summary>
    public int MortonIndex { get; } = mortonIndex;

    /// <summary>Aspect ratio (width / height) of this sub-image.</summary>
    public double AspectRatio { get; } = aspectRatio;

    /// <summary>Optional URI to the individual DZI file for deep zoom into this item.</summary>
    public string? Source { get; } = source;

    /// <summary>
    /// X position of the sub-image in the mosaic, using the Silverlight inverted coordinate system.
    /// A sub-image at mosaic grid position (col, row) has ViewportOriginX = -col * subImageLogicalWidth.
    /// </summary>
    public double ViewportOriginX
    {
        get => _viewportOriginX;
        set => _viewportOriginX = value;
    }

    /// <summary>
    /// Y position of the sub-image in the mosaic, using the Silverlight inverted coordinate system.
    /// </summary>
    public double ViewportOriginY
    {
        get => _viewportOriginY;
        set => _viewportOriginY = value;
    }

    /// <summary>
    /// Width of the sub-image in the mosaic, using the Silverlight inverted coordinate system.
    /// Larger values = smaller on screen. This is the inverse of the visible fraction.
    /// </summary>
    public double ViewportWidth
    {
        get => _viewportWidth;
        set => _viewportWidth = value;
    }

    /// <summary>Opacity of this sub-image (0–1). Set to 0 to hide without removing.</summary>
    public double Opacity
    {
        get => _opacity;
        set => _opacity = Math.Max(0, Math.Min(1, value));
    }

    /// <summary>Z-index for rendering order. Higher = on top.</summary>
    public int ZIndex
    {
        get => _zIndex;
        set => _zIndex = value;
    }

    /// <summary>
    /// Converts the Silverlight inverted viewport coordinates to the actual mosaic position.
    /// Returns the top-left corner and size in the parent's logical space.
    /// </summary>
    public (double X, double Y, double Width, double Height) GetMosaicBounds()
    {
        if (_viewportWidth == 0)
            return (0, 0, 0, 0);

        // Silverlight uses inverted coordinates:
        // The sub-image's ViewportWidth is the inverse of how much of the parent it occupies
        // ViewportOrigin is the negated position in parent space
        double actualWidth = 1.0 / _viewportWidth;
        double actualHeight = AspectRatio > 0 ? actualWidth / AspectRatio : 0;
        double actualX = -_viewportOriginX / _viewportWidth;
        double actualY = -_viewportOriginY / _viewportWidth;

        return (actualX, actualY, actualWidth, actualHeight);
    }

    /// <summary>
    /// Converts a point in the parent's logical space to this sub-image's local logical space.
    /// </summary>
    public (double X, double Y) ParentToLocal(double parentX, double parentY)
    {
        var (mx, my, mw, mh) = GetMosaicBounds();
        if (mw == 0 || mh == 0)
            return (0, 0);
        return ((parentX - mx) / mw, (parentY - my) / mh);
    }

    /// <summary>
    /// Converts a point in this sub-image's local logical space to the parent's logical space.
    /// </summary>
    public (double X, double Y) LocalToParent(double localX, double localY)
    {
        var (mx, my, mw, mh) = GetMosaicBounds();
        return (localX * mw + mx, localY * mh + my);
    }
}
