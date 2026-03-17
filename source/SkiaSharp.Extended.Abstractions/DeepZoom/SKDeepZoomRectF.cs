#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A floating-point axis-aligned rectangle used for screen-space tile geometry.
/// Replaces anonymous tuples like <c>(float SrcX, float SrcY, float SrcW, float SrcH)</c>
/// and <see cref="SKRect"/> usages in the SkiaSharp-agnostic DeepZoom pipeline.
/// </summary>
public readonly struct SKDeepZoomRectF : IEquatable<SKDeepZoomRectF>
{
    /// <summary>Left edge.</summary>
    public float X { get; }
    /// <summary>Top edge.</summary>
    public float Y { get; }
    /// <summary>Width.</summary>
    public float Width { get; }
    /// <summary>Height.</summary>
    public float Height { get; }

    /// <summary>Right edge (X + Width).</summary>
    public float Right => X + Width;
    /// <summary>Bottom edge (Y + Height).</summary>
    public float Bottom => Y + Height;

    /// <summary>Initializes a new <see cref="SKDeepZoomRectF"/>.</summary>
    public SKDeepZoomRectF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <inheritdoc/>
    public bool Equals(SKDeepZoomRectF other) =>
        X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SKDeepZoomRectF r && Equals(r);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (int)(((X * 31 + Y) * 31 + Width) * 31 + Height);
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"({X},{Y},{Width}×{Height})";

    /// <summary>Returns <see langword="true"/> if the two rects are equal.</summary>
    public static bool operator ==(SKDeepZoomRectF a, SKDeepZoomRectF b) => a.Equals(b);
    /// <summary>Returns <see langword="true"/> if the two rects differ.</summary>
    public static bool operator !=(SKDeepZoomRectF a, SKDeepZoomRectF b) => !a.Equals(b);
}
