#nullable enable

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>Immutable snapshot of viewport position and zoom.</summary>
    public readonly struct SKDeepZoomViewportState : System.IEquatable<SKDeepZoomViewportState>
    {
        public SKDeepZoomViewportState(double viewportWidth, double originX, double originY)
        {
            ViewportWidth = viewportWidth;
            OriginX = originX;
            OriginY = originY;
        }

        public double ViewportWidth { get; }
        public double OriginX { get; }
        public double OriginY { get; }

        public bool Equals(SKDeepZoomViewportState other)
            => ViewportWidth == other.ViewportWidth && OriginX == other.OriginX && OriginY == other.OriginY;

        public override bool Equals(object? obj) => obj is SKDeepZoomViewportState s && Equals(s);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ViewportWidth.GetHashCode();
                hash = hash * 31 + OriginX.GetHashCode();
                hash = hash * 31 + OriginY.GetHashCode();
                return hash;
            }
        }
        public static bool operator ==(SKDeepZoomViewportState left, SKDeepZoomViewportState right) => left.Equals(right);
        public static bool operator !=(SKDeepZoomViewportState left, SKDeepZoomViewportState right) => !left.Equals(right);
    }
}
