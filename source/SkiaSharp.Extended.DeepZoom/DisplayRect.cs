using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Represents a display rectangle in a sparse Deep Zoom Image.
    /// Defines a region of available pixels and the pyramid levels at which it is visible.
    /// </summary>
    public readonly struct DisplayRect : IEquatable<DisplayRect>
    {
        /// <summary>X coordinate of the rectangle in full-image pixels.</summary>
        public int X { get; }

        /// <summary>Y coordinate of the rectangle in full-image pixels.</summary>
        public int Y { get; }

        /// <summary>Width of the rectangle in full-image pixels.</summary>
        public int Width { get; }

        /// <summary>Height of the rectangle in full-image pixels.</summary>
        public int Height { get; }

        /// <summary>Minimum pyramid level at which this rectangle is displayed.</summary>
        public int MinLevel { get; }

        /// <summary>Maximum pyramid level at which this rectangle is displayed.</summary>
        public int MaxLevel { get; }

        public DisplayRect(int x, int y, int width, int height, int minLevel, int maxLevel)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }

        /// <summary>
        /// Returns true if this display rect is visible at the specified pyramid level.
        /// </summary>
        public bool IsVisibleAtLevel(int level) => level >= MinLevel && level <= MaxLevel;

        public bool Equals(DisplayRect other) =>
            X == other.X && Y == other.Y && Width == other.Width && Height == other.Height &&
            MinLevel == other.MinLevel && MaxLevel == other.MaxLevel;

        public override bool Equals(object? obj) => obj is DisplayRect other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height, MinLevel, MaxLevel);

        public static bool operator ==(DisplayRect left, DisplayRect right) => left.Equals(right);
        public static bool operator !=(DisplayRect left, DisplayRect right) => !left.Equals(right);

        public override string ToString() =>
            $"DisplayRect({X}, {Y}, {Width}x{Height}, Levels {MinLevel}-{MaxLevel})";
    }
}
