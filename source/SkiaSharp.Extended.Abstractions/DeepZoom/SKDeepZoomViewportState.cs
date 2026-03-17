#nullable enable

namespace SkiaSharp.Extended.DeepZoom;

/// <summary>Immutable snapshot of viewport position and zoom.</summary>
public readonly record struct SKDeepZoomViewportState(double ViewportWidth, double OriginX, double OriginY)
{
    // Custom hash for netstandard2.0 compatibility.
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
}
