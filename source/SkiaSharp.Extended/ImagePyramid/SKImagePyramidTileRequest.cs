#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// A tile to be fetched, with a priority value (lower = higher priority).
/// </summary>
public readonly struct SKImagePyramidTileRequest : System.IEquatable<SKImagePyramidTileRequest>
{
    /// <summary>Initializes a new <see cref="SKImagePyramidTileRequest"/>.</summary>
    public SKImagePyramidTileRequest(SKImagePyramidTileId tileId, double priority)
    {
        TileId = tileId;
        Priority = priority;
    }

    /// <summary>The tile to fetch.</summary>
    public SKImagePyramidTileId TileId { get; }

    /// <summary>Fetch priority. Lower values are fetched first (higher visual importance).</summary>
    public double Priority { get; }

    public bool Equals(SKImagePyramidTileRequest other) => TileId.Equals(other.TileId);
    public override bool Equals(object? obj) => obj is SKImagePyramidTileRequest r && Equals(r);
    public override int GetHashCode() => TileId.GetHashCode();
}
