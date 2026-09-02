#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Identifies a single tile in the pyramid.
    /// </summary>
    public readonly struct SKDeepZoomTileId : System.IEquatable<SKDeepZoomTileId>
    {
        public SKDeepZoomTileId(int level, int col, int row)
        {
            Level = level;
            Col = col;
            Row = row;
        }

        public int Level { get; }
        public int Col { get; }
        public int Row { get; }

        public bool Equals(SKDeepZoomTileId other) => Level == other.Level && Col == other.Col && Row == other.Row;
        public override bool Equals(object? obj) => obj is SKDeepZoomTileId id && Equals(id);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Level;
                hash = hash * 31 + Col;
                hash = hash * 31 + Row;
                return hash;
            }
        }
        public override string ToString() => $"({Level},{Col},{Row})";

        public static bool operator ==(SKDeepZoomTileId left, SKDeepZoomTileId right) => left.Equals(right);
        public static bool operator !=(SKDeepZoomTileId left, SKDeepZoomTileId right) => !left.Equals(right);
    }
}
