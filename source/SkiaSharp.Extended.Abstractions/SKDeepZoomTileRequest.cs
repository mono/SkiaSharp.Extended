#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// A tile to be fetched, with a priority value (lower = higher priority).
    /// </summary>
    public readonly struct SKDeepZoomTileRequest : System.IEquatable<SKDeepZoomTileRequest>
    {
        /// <summary>Initializes a new <see cref="SKDeepZoomTileRequest"/>.</summary>
        public SKDeepZoomTileRequest(SKDeepZoomTileId tileId, double priority)
        {
            TileId = tileId;
            Priority = priority;
        }

        /// <summary>The tile to fetch.</summary>
        public SKDeepZoomTileId TileId { get; }

        /// <summary>Fetch priority. Lower values are fetched first (higher visual importance).</summary>
        public double Priority { get; }

        public bool Equals(SKDeepZoomTileRequest other) => TileId.Equals(other.TileId);
        public override bool Equals(object? obj) => obj is SKDeepZoomTileRequest r && Equals(r);
        public override int GetHashCode() => TileId.GetHashCode();
    }
}
