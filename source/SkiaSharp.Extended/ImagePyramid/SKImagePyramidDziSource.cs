#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SkiaSharp.Extended;

/// <summary>
/// Represents a Deep Zoom Image (DZI) tile source. Parses the DZI XML descriptor
/// and provides tile pyramid math for computing tile URLs and dimensions.
/// </summary>
public class SKImagePyramidDziSource : ISKImagePyramidSource
{
    private const string DeepZoomNamespace2008 = "http://schemas.microsoft.com/deepzoom/2008";
    private const string DeepZoomNamespace2009 = "http://schemas.microsoft.com/deepzoom/2009";

    public SKImagePyramidDziSource(int imageWidth, int imageHeight, int tileSize, int overlap, string format)
    {
        if (imageWidth <= 0) throw new ArgumentOutOfRangeException(nameof(imageWidth));
        if (imageHeight <= 0) throw new ArgumentOutOfRangeException(nameof(imageHeight));
        if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));
        if (overlap < 0) throw new ArgumentOutOfRangeException(nameof(overlap));
        if (string.IsNullOrEmpty(format)) throw new ArgumentException("Format cannot be null or empty.", nameof(format));

        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
        TileSize = tileSize;
        Overlap = overlap;
        Format = format;
        MaxLevel = (int)Math.Ceiling(Math.Log(Math.Max(imageWidth, imageHeight)) / Math.Log(2));
    }

    /// <summary>Full image width in pixels at maximum resolution.</summary>
    public int ImageWidth { get; }

    /// <summary>Full image height in pixels at maximum resolution.</summary>
    public int ImageHeight { get; }

    /// <summary>Tile size in pixels (typically 254 or 256).</summary>
    public int TileSize { get; }

    /// <summary>Overlap in pixels between adjacent tiles.</summary>
    public int Overlap { get; }

    /// <summary>Image format (e.g., "jpg", "png").</summary>
    public string Format { get; }

    /// <summary>Maximum pyramid level. Level 0 is 1×1 pixel.</summary>
    public int MaxLevel { get; }

    /// <summary>Image aspect ratio (width / height).</summary>
    public double AspectRatio => (double)ImageWidth / ImageHeight;

    /// <summary>
    /// Display rectangles for sparse images. If empty, the image is not sparse
    /// and all pixels are available at all levels.
    /// </summary>
    public IReadOnlyList<SKImagePyramidDisplayRect> DisplayRects { get; internal set; } = Array.Empty<SKImagePyramidDisplayRect>();

    /// <summary>
    /// Gets or sets the base URI used to resolve tile URLs.
    /// Typically derived from the DZI file path: "{name}_files/".
    /// </summary>
    public string? TilesBaseUri { get; set; }

    /// <summary>
    /// Gets or sets a query string (e.g., "?sig=ABC") appended to each tile URL.
    /// Used for signed/SAS URLs where authentication tokens must be preserved.
    /// </summary>
    public string? TilesQueryString { get; set; }

    /// <summary>Computes the image width at a given pyramid level.</summary>
    public int GetLevelWidth(int level)
    {
        if (level < 0 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
        return (int)Math.Ceiling((double)ImageWidth / (1L << (MaxLevel - level)));
    }

    /// <summary>Computes the image height at a given pyramid level.</summary>
    public int GetLevelHeight(int level)
    {
        if (level < 0 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
        return (int)Math.Ceiling((double)ImageHeight / (1L << (MaxLevel - level)));
    }

    /// <summary>Number of tile columns at a given level.</summary>
    public int GetTileCountX(int level)
    {
        return (int)Math.Ceiling((double)GetLevelWidth(level) / TileSize);
    }

    /// <summary>Number of tile rows at a given level.</summary>
    public int GetTileCountY(int level)
    {
        return (int)Math.Ceiling((double)GetLevelHeight(level) / TileSize);
    }

    /// <summary>
    /// Gets the pixel bounds of a tile within its pyramid level, including overlap.
    /// Returns a <see cref="Rect{T}"/> of <see cref="int"/> in level-pixel coordinates (XYWH).
    /// </summary>
    public Rect<int> GetTileBounds(int level, int col, int row)
    {
        if (level < 0 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));

        int levelWidth = GetLevelWidth(level);
        int levelHeight = GetLevelHeight(level);

        int x = col == 0 ? 0 : col * TileSize - Overlap;
        int y = row == 0 ? 0 : row * TileSize - Overlap;

        int right;
        if (col > 0) right = Math.Min((col + 1) * TileSize + Overlap, levelWidth);
        else right = Math.Min(TileSize + Overlap, levelWidth);

        int bottom;
        if (row > 0) bottom = Math.Min((row + 1) * TileSize + Overlap, levelHeight);
        else bottom = Math.Min(TileSize + Overlap, levelHeight);

        return new Rect<int>(x, y, right - x, bottom - y);
    }

    /// <summary>
    /// Selects the optimal pyramid level for rendering, where one tile pixel ≈ one screen pixel.
    /// </summary>
    /// <param name="viewportWidth">The viewport width (0-1 normalized, where 1.0 = full image fits).</param>
    /// <param name="controlWidth">The control width in screen pixels.</param>
    public int GetOptimalLevel(double viewportWidth, double controlWidth)
    {
        if (viewportWidth <= 0) return MaxLevel;

        // Pixels per logical unit
        double scale = controlWidth / viewportWidth;
        // The visible area is viewportWidth logical units wide, rendered into controlWidth pixels.
        // We need a level where levelWidth * viewportWidth ≈ controlWidth,
        // i.e. levelWidth ≈ controlWidth / viewportWidth = scale
        double neededWidth = scale;
        if (neededWidth <= 0) return 0;

        for (int level = MaxLevel; level >= 0; level--)
        {
            int lw = GetLevelWidth(level);
            if (lw <= neededWidth * 1.01) // slight tolerance
                return Math.Min(level + 1, MaxLevel);
        }

        return 0;
    }

    /// <summary>Gets the relative tile URL (e.g., "8/3_2.jpg").</summary>
    public string GetTileUrl(int level, int col, int row)
    {
        return $"{level}/{col}_{row}.{Format}";
    }

    /// <summary>Gets the full tile URL using TilesBaseUri and optional TilesQueryString.</summary>
    public string? GetFullTileUrl(int level, int col, int row)
    {
        if (TilesBaseUri == null) return null;
        var url = TilesBaseUri + GetTileUrl(level, col, row);
        if (!string.IsNullOrEmpty(TilesQueryString))
            url += TilesQueryString;
        return url;
    }

    /// <summary>Parses a DZI XML string.</summary>
    public static SKImagePyramidDziSource Parse(string xml)
    {
        var doc = XDocument.Parse(xml);
        return ParseDocument(doc, null);
    }

    /// <summary>Parses a DZI XML from a stream.</summary>
    public static SKImagePyramidDziSource Parse(Stream stream)
    {
        var doc = XDocument.Load(stream);
        return ParseDocument(doc, null);
    }

    /// <summary>Parses a DZI XML from a stream with a base URI for computing tile URLs.</summary>
    public static SKImagePyramidDziSource Parse(Stream stream, string? baseUri)
    {
        var doc = XDocument.Load(stream);
        return ParseDocument(doc, baseUri);
    }

    /// <summary>Parses a DZI XML string with a base URI.</summary>
    public static SKImagePyramidDziSource Parse(string xml, string? baseUri)
    {
        var doc = XDocument.Parse(xml);
        return ParseDocument(doc, baseUri);
    }

    private static SKImagePyramidDziSource ParseDocument(XDocument doc, string? baseUri)
    {
        // Try both 2008 and 2009 namespaces
        var ns = XNamespace.Get(DeepZoomNamespace2008);
        var imageElement = doc.Element(ns + "Image");
        if (imageElement == null)
        {
            ns = XNamespace.Get(DeepZoomNamespace2009);
            imageElement = doc.Element(ns + "Image");
        }
        if (imageElement == null)
            throw new FormatException("Invalid DZI: missing <Image> element with Deep Zoom namespace.");

        var sizeElement = imageElement.Element(ns + "Size");
        if (sizeElement == null)
            throw new FormatException("Invalid DZI: missing <Size> element.");

        int tileSize = int.Parse(imageElement.Attribute("TileSize")?.Value ?? throw new FormatException("Missing TileSize attribute."));
        int overlap = int.Parse(imageElement.Attribute("Overlap")?.Value ?? "0");
        string format = imageElement.Attribute("Format")?.Value ?? throw new FormatException("Missing Format attribute.");
        int width = int.Parse(sizeElement.Attribute("Width")?.Value ?? throw new FormatException("Missing Width attribute."));
        int height = int.Parse(sizeElement.Attribute("Height")?.Value ?? throw new FormatException("Missing Height attribute."));

        var source = new SKImagePyramidDziSource(width, height, tileSize, overlap, format);
        source.TilesBaseUri = baseUri;

        // Parse optional DisplayRects (sparse image support)
        var displayRectsElement = imageElement.Element(ns + "DisplayRects");
        if (displayRectsElement != null)
        {
            var rects = new List<SKImagePyramidDisplayRect>();
            foreach (var drElement in displayRectsElement.Elements(ns + "DisplayRect"))
            {
                int minLevel = int.Parse(drElement.Attribute("MinLevel")?.Value ?? "0");
                int maxLevel = int.Parse(drElement.Attribute("MaxLevel")?.Value ?? "0");

                var rectElement = drElement.Element(ns + "Rect");
                if (rectElement != null)
                {
                    int rx = int.Parse(rectElement.Attribute("X")?.Value ?? "0");
                    int ry = int.Parse(rectElement.Attribute("Y")?.Value ?? "0");
                    int rw = int.Parse(rectElement.Attribute("Width")?.Value ?? "0");
                    int rh = int.Parse(rectElement.Attribute("Height")?.Value ?? "0");
                    rects.Add(new SKImagePyramidDisplayRect(rx, ry, rw, rh, minLevel, maxLevel));
                }
            }

            source.DisplayRects = rects.AsReadOnly();
        }

        return source;
    }
}
