#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// Describes an image pyramid source — provides tile pyramid math for computing
/// tile URLs and dimensions. Implement this to support new image formats (DZI, IIIF, Zoomify).
/// </summary>
public interface ISKImagePyramidSource
{
    /// <summary>Full image width in pixels at maximum resolution.</summary>
    int ImageWidth { get; }

    /// <summary>Full image height in pixels at maximum resolution.</summary>
    int ImageHeight { get; }

    /// <summary>Maximum pyramid level index. Level 0 is lowest resolution.</summary>
    int MaxLevel { get; }

    /// <summary>Image aspect ratio (width / height).</summary>
    double AspectRatio { get; }

    /// <summary>Gets the image width at a given pyramid level.</summary>
    int GetLevelWidth(int level);

    /// <summary>Gets the image height at a given pyramid level.</summary>
    int GetLevelHeight(int level);

    /// <summary>Gets the number of tile columns at a given level.</summary>
    int GetTileCountX(int level);

    /// <summary>Gets the number of tile rows at a given level.</summary>
    int GetTileCountY(int level);

    /// <summary>
    /// Gets the pixel bounds of a tile within its pyramid level.
    /// Returns an <see cref="SKImagePyramidRectI"/> in level-pixel coordinates.
    /// For formats without tile overlap (e.g., IIIF), the bounds are the exact tile dimensions.
    /// For DZI, the bounds include the configured overlap pixels.
    /// </summary>
    SKImagePyramidRectI GetTileBounds(int level, int col, int row);

    /// <summary>
    /// Gets the full URL for a specific tile. Returns null if the source
    /// cannot resolve the URL (e.g., not yet loaded).
    /// </summary>
    string? GetFullTileUrl(int level, int col, int row);

    /// <summary>Gets the optimal pyramid level for the given viewport and control dimensions.</summary>
    int GetOptimalLevel(double viewportWidth, double controlWidth);
}
