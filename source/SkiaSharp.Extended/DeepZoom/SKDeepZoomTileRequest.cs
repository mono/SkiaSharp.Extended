#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// A tile to be fetched, with a priority value (lower = higher priority).
    /// </summary>
    public readonly struct SKDeepZoomTileRequest : System.IEquatable<SKDeepZoomTileRequest>
    {
        public SKDeepZoomTileRequest(SKDeepZoomTileId tileId, double priority)
        {
            TileId = tileId;
            Priority = priority;
        }

        public SKDeepZoomTileId TileId { get; }
        public double Priority { get; }

        public bool Equals(SKDeepZoomTileRequest other) => TileId.Equals(other.TileId);
        public override bool Equals(object? obj) => obj is SKDeepZoomTileRequest r && Equals(r);
        public override int GetHashCode() => TileId.GetHashCode();
    }
}
