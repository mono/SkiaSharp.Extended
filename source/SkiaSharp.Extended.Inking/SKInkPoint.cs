using System;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Represents a single point in an ink stroke with pressure and optional timestamp.
/// </summary>
public readonly struct SKInkPoint : IEquatable<SKInkPoint>
{
    /// <summary>
    /// Creates a new ink point.
    /// </summary>
    /// <param name="location">The point location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="timestamp">Optional timestamp in milliseconds.</param>
    public SKInkPoint(SKPoint location, float pressure, long timestamp = 0)
    {
        Location = location;
        Pressure = Clamp(pressure, 0f, 1f);
        Timestamp = timestamp;
    }

    /// <summary>
    /// Creates a new ink point.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="timestamp">Optional timestamp in milliseconds.</param>
    public SKInkPoint(float x, float y, float pressure, long timestamp = 0)
        : this(new SKPoint(x, y), pressure, timestamp)
    {
    }

    /// <summary>
    /// Gets the location of the point.
    /// </summary>
    public SKPoint Location { get; }

    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public float X => Location.X;

    /// <summary>
    /// Gets the Y coordinate.
    /// </summary>
    public float Y => Location.Y;

    /// <summary>
    /// Gets the pressure value (0.0 to 1.0).
    /// </summary>
    public float Pressure { get; }

    /// <summary>
    /// Gets the timestamp in milliseconds (if available).
    /// </summary>
    public long Timestamp { get; }

    /// <summary>
    /// Returns a string representation of this ink point.
    /// </summary>
    public override string ToString() => $"({X}, {Y}, P={Pressure:F2})";

    /// <summary>
    /// Determines whether the specified object is equal to this ink point.
    /// </summary>
    public override bool Equals(object? obj) => obj is SKInkPoint other && Equals(other);

    /// <summary>
    /// Determines whether the specified ink point is equal to this ink point.
    /// </summary>
    public bool Equals(SKInkPoint other) =>
        Location.Equals(other.Location) &&
        Pressure.Equals(other.Pressure) &&
        Timestamp.Equals(other.Timestamp);

    /// <summary>
    /// Returns the hash code for this ink point.
    /// </summary>
    public override int GetHashCode()
    {
#if NETSTANDARD2_0
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Location.GetHashCode();
            hash = hash * 31 + Pressure.GetHashCode();
            hash = hash * 31 + Timestamp.GetHashCode();
            return hash;
        }
#else
        return HashCode.Combine(Location, Pressure, Timestamp);
#endif
    }

    /// <summary>
    /// Determines whether two ink points are equal.
    /// </summary>
    public static bool operator ==(SKInkPoint left, SKInkPoint right) => left.Equals(right);

    /// <summary>
    /// Determines whether two ink points are not equal.
    /// </summary>
    public static bool operator !=(SKInkPoint left, SKInkPoint right) => !left.Equals(right);

    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
