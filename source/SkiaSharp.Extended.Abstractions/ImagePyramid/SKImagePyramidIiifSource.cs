#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SkiaSharp.Extended;

/// <summary>
/// An <see cref="ISKImagePyramidSource"/> backed by an IIIF Image API v2/v3 info.json.
/// Parses the IIIF descriptor and maps IIIF scale factors to pyramid levels,
/// where level 0 = lowest resolution (largest scale factor) and level MaxLevel = full resolution.
/// </summary>
public class SKImagePyramidIiifSource : ISKImagePyramidSource
{
    private readonly string _baseId;
    private readonly int[] _scaleFactorsDescending; // sorted descending: [32, 16, 8, 4, 2, 1]
    private readonly int _tileWidth;
    private readonly int _tileHeight;
    private readonly string _format;
    private readonly string _quality;

    /// <summary>
    /// Constructs an IIIF source from parsed parameters.
    /// </summary>
    /// <param name="baseId">The IIIF base identifier (@id) used to construct tile URLs.</param>
    /// <param name="imageWidth">Full image width in pixels.</param>
    /// <param name="imageHeight">Full image height in pixels.</param>
    /// <param name="tileWidth">Tile width in pixels.</param>
    /// <param name="tileHeight">Tile height in pixels.</param>
    /// <param name="scaleFactorsDescending">Scale factors sorted descending (e.g. [32, 16, 8, 4, 2, 1]).</param>
    /// <param name="format">Image format (e.g. "jpg").</param>
    /// <param name="quality">IIIF quality parameter (e.g. "default").</param>
    public SKImagePyramidIiifSource(
        string baseId,
        int imageWidth,
        int imageHeight,
        int tileWidth,
        int tileHeight,
        int[] scaleFactorsDescending,
        string format = "jpg",
        string quality = "default")
    {
        _baseId = baseId?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseId));
        ImageWidth = imageWidth > 0 ? imageWidth : throw new ArgumentOutOfRangeException(nameof(imageWidth));
        ImageHeight = imageHeight > 0 ? imageHeight : throw new ArgumentOutOfRangeException(nameof(imageHeight));
        _tileWidth = tileWidth > 0 ? tileWidth : throw new ArgumentOutOfRangeException(nameof(tileWidth));
        _tileHeight = tileHeight > 0 ? tileHeight : throw new ArgumentOutOfRangeException(nameof(tileHeight));
        _scaleFactorsDescending = scaleFactorsDescending?.Length > 0
            ? scaleFactorsDescending
            : throw new ArgumentException("Must have at least one scale factor.", nameof(scaleFactorsDescending));
        _format = format;
        _quality = quality;
        MaxLevel = _scaleFactorsDescending.Length - 1;
    }

    /// <inheritdoc/>
    public int ImageWidth { get; }

    /// <inheritdoc/>
    public int ImageHeight { get; }

    /// <inheritdoc/>
    public int MaxLevel { get; }

    /// <inheritdoc/>
    public double AspectRatio => (double)ImageWidth / ImageHeight;

    /// <summary>The IIIF base ID (@id) used to construct tile URLs.</summary>
    public string BaseId => _baseId;

    /// <summary>Tile width in pixels.</summary>
    public int TileWidth => _tileWidth;

    /// <summary>Tile height in pixels.</summary>
    public int TileHeight => _tileHeight;

    /// <summary>Image format (e.g. "jpg").</summary>
    public string Format => _format;

    /// <summary>Scale factors in descending order (index 0 = lowest resolution level).</summary>
    public IReadOnlyList<int> ScaleFactorsDescending => _scaleFactorsDescending;

    /// <inheritdoc/>
    public int GetLevelWidth(int level)
    {
        ValidateLevel(level);
        int scaleFactor = _scaleFactorsDescending[level];
        return (int)Math.Ceiling((double)ImageWidth / scaleFactor);
    }

    /// <inheritdoc/>
    public int GetLevelHeight(int level)
    {
        ValidateLevel(level);
        int scaleFactor = _scaleFactorsDescending[level];
        return (int)Math.Ceiling((double)ImageHeight / scaleFactor);
    }

    /// <inheritdoc/>
    public int GetTileCountX(int level)
    {
        return (int)Math.Ceiling((double)GetLevelWidth(level) / _tileWidth);
    }

    /// <inheritdoc/>
    public int GetTileCountY(int level)
    {
        return (int)Math.Ceiling((double)GetLevelHeight(level) / _tileHeight);
    }

    /// <inheritdoc/>
    public SKImagePyramidRectI GetTileBounds(int level, int col, int row)
    {
        ValidateLevel(level);
        int levelWidth = GetLevelWidth(level);
        int levelHeight = GetLevelHeight(level);

        int x = col * _tileWidth;
        int y = row * _tileHeight;
        int w = Math.Min(_tileWidth, levelWidth - x);
        int h = Math.Min(_tileHeight, levelHeight - y);

        return new SKImagePyramidRectI(x, y, w, h);
    }

    /// <inheritdoc/>
    public string? GetFullTileUrl(int level, int col, int row)
    {
        ValidateLevel(level);
        int scaleFactor = _scaleFactorsDescending[level];

        // Full-resolution region
        int regX = col * _tileWidth * scaleFactor;
        int regY = row * _tileHeight * scaleFactor;
        int regW = Math.Min(_tileWidth * scaleFactor, ImageWidth - regX);
        int regH = Math.Min(_tileHeight * scaleFactor, ImageHeight - regY);

        if (regW <= 0 || regH <= 0) return null;

        // Output size (at level resolution)
        int outW = (int)Math.Ceiling((double)regW / scaleFactor);
        int outH = (int)Math.Ceiling((double)regH / scaleFactor);

        // IIIF URL: {baseId}/{region}/{size}/{rotation}/{quality}.{format}
        return $"{_baseId}/{regX},{regY},{regW},{regH}/{outW},{outH}/0/{_quality}.{_format}";
    }

    /// <inheritdoc/>
    public int GetOptimalLevel(double viewportWidth, double controlWidth)
    {
        if (viewportWidth <= 0) return MaxLevel;
        double scale = controlWidth / viewportWidth;
        if (scale <= 0) return 0;

        for (int level = MaxLevel; level >= 0; level--)
        {
            int lw = GetLevelWidth(level);
            if (lw <= scale * 1.01)
                return Math.Min(level + 1, MaxLevel);
        }
        return 0;
    }

    private void ValidateLevel(int level)
    {
        if (level < 0 || level > MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(level), $"Level must be 0–{MaxLevel}.");
    }

    /// <summary>
    /// Parses a IIIF Image API v2 or v3 info.json string.
    /// </summary>
    /// <param name="infoJson">The JSON content of the info.json endpoint.</param>
    /// <returns>A configured <see cref="SKImagePyramidIiifSource"/>.</returns>
    public static SKImagePyramidIiifSource Parse(string infoJson)
    {
        using var doc = JsonDocument.Parse(infoJson);
        return ParseDocument(doc.RootElement);
    }

    private static SKImagePyramidIiifSource ParseDocument(JsonElement root)
    {
        // Support both IIIF v2 (@id) and v3 (id)
        string? baseId = null;
        if (root.TryGetProperty("@id", out var idProp))
            baseId = idProp.GetString();
        else if (root.TryGetProperty("id", out var id3Prop))
            baseId = id3Prop.GetString();

        if (string.IsNullOrEmpty(baseId))
            throw new FormatException("Invalid IIIF info.json: missing '@id' or 'id'.");

        if (!root.TryGetProperty("width", out var widthProp) ||
            !root.TryGetProperty("height", out var heightProp))
            throw new FormatException("Invalid IIIF info.json: missing 'width' or 'height'.");

        int width = widthProp.GetInt32();
        int height = heightProp.GetInt32();

        // Parse tiles (prefer first entry)
        int tileWidth = 256;
        int tileHeight = 256;
        var scaleFactors = new List<int> { 1 };

        if (root.TryGetProperty("tiles", out var tilesProp) && tilesProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var tile in tilesProp.EnumerateArray())
            {
                if (tile.TryGetProperty("width", out var twProp))
                    tileWidth = twProp.GetInt32();
                tileHeight = tile.TryGetProperty("height", out var thProp) ? thProp.GetInt32() : tileWidth;

                if (tile.TryGetProperty("scaleFactors", out var sfProp) && sfProp.ValueKind == JsonValueKind.Array)
                {
                    scaleFactors = sfProp.EnumerateArray().Select(e => e.GetInt32()).ToList();
                }
                break; // Use first tile definition only
            }
        }
        else
        {
            // No tiles — use whole-image as a single tile
            tileWidth = width;
            tileHeight = height;
        }

        // Sort descending so level 0 = lowest resolution
        scaleFactors.Sort((a, b) => b.CompareTo(a));

        // Pick best format: prefer jpg
        string format = "jpg";
        if (root.TryGetProperty("profile", out var profileProp))
        {
            if (profileProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in profileProp.EnumerateArray())
                {
                    if (entry.ValueKind == JsonValueKind.Object &&
                        entry.TryGetProperty("formats", out var fmtsProp))
                    {
                        var formats = fmtsProp.EnumerateArray().Select(f => f.GetString() ?? "").ToList();
                        if (formats.Contains("jpg")) { format = "jpg"; break; }
                        if (formats.Count > 0) format = formats[0];
                    }
                }
            }
        }

        return new SKImagePyramidIiifSource(
            baseId: baseId,
            imageWidth: width,
            imageHeight: height,
            tileWidth: tileWidth,
            tileHeight: tileHeight,
            scaleFactorsDescending: scaleFactors.ToArray(),
            format: format);
    }
}
