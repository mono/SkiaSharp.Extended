#nullable enable

using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// An integer-precision axis-aligned rectangle used for tile pixel bounds within a pyramid level.
    /// Replaces anonymous tuples like <c>(int X, int Y, int Width, int Height)</c> throughout the DeepZoom pipeline.
    /// </summary>
    public readonly struct SKDeepZoomRectI : IEquatable<SKDeepZoomRectI>
    {
        /// <summary>Left edge in pixels.</summary>
        public int X { get; }
        /// <summary>Top edge in pixels.</summary>
        public int Y { get; }
        /// <summary>Width in pixels.</summary>
        public int Width { get; }
        /// <summary>Height in pixels.</summary>
        public int Height { get; }

        /// <summary>Right edge (X + Width).</summary>
        public int Right => X + Width;
        /// <summary>Bottom edge (Y + Height).</summary>
        public int Bottom => Y + Height;

        /// <summary>Initializes a new <see cref="SKDeepZoomRectI"/>.</summary>
        public SKDeepZoomRectI(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <inheritdoc/>
        public bool Equals(SKDeepZoomRectI other) =>
            X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SKDeepZoomRectI r && Equals(r);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((X * 31 + Y) * 31 + Width) * 31 + Height;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"({X},{Y},{Width}×{Height})";

        /// <summary>Returns <see langword="true"/> if the two rects are equal.</summary>
        public static bool operator ==(SKDeepZoomRectI a, SKDeepZoomRectI b) => a.Equals(b);
        /// <summary>Returns <see langword="true"/> if the two rects differ.</summary>
        public static bool operator !=(SKDeepZoomRectI a, SKDeepZoomRectI b) => !a.Equals(b);
    }
}
