#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Identifies a single tile in the pyramid.
    /// </summary>
    public readonly record struct SKDeepZoomTileId(int Level, int Col, int Row)
    {
        // Custom hash keeps netstandard2.0 compatibility (HashCode.Combine not available there).
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
    }
}
