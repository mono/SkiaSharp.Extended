using System;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Represents a single point in an ink stroke with pressure, tilt, and timestamp.
/// </summary>
public readonly struct SKInkPoint : IEquatable<SKInkPoint>
{
    /// <summary>
    /// Creates a new ink point with full parameters.
    /// </summary>
    /// <param name="location">The point location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="tiltX">The tilt on X axis in degrees (-90 to 90).</param>
    /// <param name="tiltY">The tilt on Y axis in degrees (-90 to 90).</param>
    /// <param name="timestampMicroseconds">Timestamp in microseconds.</param>
    public SKInkPoint(SKPoint location, float pressure, float tiltX, float tiltY, long timestampMicroseconds)
    {
        Location = location;
        Pressure = Clamp(pressure, 0f, 1f);
        TiltX = Clamp(tiltX, -90f, 90f);
        TiltY = Clamp(tiltY, -90f, 90f);
        TimestampMicroseconds = timestampMicroseconds;
    }

    /// <summary>
    /// Creates a new ink point (without tilt).
    /// </summary>
    /// <param name="location">The point location.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="timestampMicroseconds">Timestamp in microseconds (default 0).</param>
    public SKInkPoint(SKPoint location, float pressure, long timestampMicroseconds = 0)
        : this(location, pressure, 0f, 0f, timestampMicroseconds)
    {
    }

    /// <summary>
    /// Creates a new ink point with coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="timestampMicroseconds">Timestamp in microseconds (default 0).</param>
    public SKInkPoint(float x, float y, float pressure, long timestampMicroseconds = 0)
        : this(new SKPoint(x, y), pressure, 0f, 0f, timestampMicroseconds)
    {
    }

    /// <summary>
    /// Creates a new ink point with coordinates and tilt.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="pressure">The pressure value (0.0 to 1.0).</param>
    /// <param name="tiltX">The tilt on X axis in degrees (-90 to 90).</param>
    /// <param name="tiltY">The tilt on Y axis in degrees (-90 to 90).</param>
    /// <param name="timestampMicroseconds">Timestamp in microseconds.</param>
    public SKInkPoint(float x, float y, float pressure, float tiltX, float tiltY, long timestampMicroseconds)
        : this(new SKPoint(x, y), pressure, tiltX, tiltY, timestampMicroseconds)
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
    /// Gets the tilt on the X axis in degrees (-90 to 90).
    /// 0 means the pen is perpendicular to the surface.
    /// Positive values indicate tilt to the right.
    /// </summary>
    public float TiltX { get; }

    /// <summary>
    /// Gets the tilt on the Y axis in degrees (-90 to 90).
    /// 0 means the pen is perpendicular to the surface.
    /// Positive values indicate tilt away from the user.
    /// </summary>
    public float TiltY { get; }

    /// <summary>
    /// Gets the timestamp in microseconds (if available).
    /// For milliseconds, divide by 1000.
    /// </summary>
    public long TimestampMicroseconds { get; }

    /// <summary>
    /// Returns a string representation of this ink point.
    /// </summary>
    public override string ToString() => $"({X}, {Y}, P={Pressure:F2}, Tilt=({TiltX:F1}, {TiltY:F1}))";

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
        TiltX.Equals(other.TiltX) &&
        TiltY.Equals(other.TiltY) &&
        TimestampMicroseconds.Equals(other.TimestampMicroseconds);

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
            hash = hash * 31 + TiltX.GetHashCode();
            hash = hash * 31 + TiltY.GetHashCode();
            hash = hash * 31 + TimestampMicroseconds.GetHashCode();
            return hash;
        }
#else
        return HashCode.Combine(Location, Pressure, TiltX, TiltY, TimestampMicroseconds);
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
