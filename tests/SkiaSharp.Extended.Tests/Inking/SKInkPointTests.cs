using SkiaSharp;
using SkiaSharp.Extended.Inking;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

public class SKInkPointTests
{
    [Fact]
    public void Constructor_CreatesPointWithCorrectValues()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f, 12345);

        Assert.Equal(10f, point.X);
        Assert.Equal(20f, point.Y);
        Assert.Equal(0.5f, point.Pressure);
        Assert.Equal(12345, point.TimestampMicroseconds);
    }

    [Fact]
    public void Constructor_WithSKPoint_CreatesPointCorrectly()
    {
        var location = new SKPoint(15f, 25f);
        var point = new SKInkPoint(location, 0.75f);

        Assert.Equal(15f, point.X);
        Assert.Equal(25f, point.Y);
        Assert.Equal(location, point.Location);
        Assert.Equal(0.75f, point.Pressure);
    }

    [Theory]
    [InlineData(-0.5f, 0f)]
    [InlineData(1.5f, 1f)]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    [InlineData(0.5f, 0.5f)]
    public void Constructor_ClampsPressureToValidRange(float input, float expected)
    {
        var point = new SKInkPoint(0f, 0f, input);

        Assert.Equal(expected, point.Pressure);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        var point2 = new SKInkPoint(10f, 20f, 0.5f, 100);

        Assert.Equal(point1, point2);
        Assert.True(point1 == point2);
        Assert.False(point1 != point2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        var point2 = new SKInkPoint(10f, 21f, 0.5f, 100);

        Assert.NotEqual(point1, point2);
        Assert.False(point1 == point2);
        Assert.True(point1 != point2);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f);

        var result = point.ToString();

        Assert.Contains("10", result);
        Assert.Contains("20", result);
        Assert.Contains("0.50", result);
    }

    [Fact]
    public void Equals_WithObject_ReturnsTrue_WhenEqual()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        object point2 = new SKInkPoint(10f, 20f, 0.5f, 100);

        Assert.True(point1.Equals(point2));
    }

    [Fact]
    public void Equals_WithObject_ReturnsFalse_WhenNotEqual()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        object point2 = new SKInkPoint(10f, 21f, 0.5f, 100);

        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f, 100);

        Assert.False(point.Equals(null));
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f, 100);

        Assert.False(point.Equals("not a point"));
    }

    [Fact]
    public void GetHashCode_SamePoints_ReturnSameHashCode()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        var point2 = new SKInkPoint(10f, 20f, 0.5f, 100);

        Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentPoints_ReturnDifferentHashCode()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        var point2 = new SKInkPoint(30f, 40f, 0.8f, 200);

        // Hash codes can theoretically collide, but these should be different
        Assert.NotEqual(point1.GetHashCode(), point2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentPressure_AreNotEqual()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        var point2 = new SKInkPoint(10f, 20f, 0.7f, 100);

        Assert.NotEqual(point1, point2);
    }

    [Fact]
    public void Equality_DifferentTimestamp_AreNotEqual()
    {
        var point1 = new SKInkPoint(10f, 20f, 0.5f, 100);
        var point2 = new SKInkPoint(10f, 20f, 0.5f, 200);

        Assert.NotEqual(point1, point2);
    }

    [Fact]
    public void Constructor_DefaultTimestamp_IsZero()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f);

        Assert.Equal(0, point.TimestampMicroseconds);
    }

    [Fact]
    public void Constructor_WithTilt_CreatesPointWithTiltValues()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f, 30f, -15f, 12345);

        Assert.Equal(10f, point.X);
        Assert.Equal(20f, point.Y);
        Assert.Equal(0.5f, point.Pressure);
        Assert.Equal(30f, point.TiltX);
        Assert.Equal(-15f, point.TiltY);
        Assert.Equal(12345, point.TimestampMicroseconds);
    }

    [Fact]
    public void Constructor_TiltValuesClamped()
    {
        var point = new SKInkPoint(10f, 20f, 0.5f, 100f, -100f, 0);

        Assert.Equal(90f, point.TiltX);  // Clamped from 100
        Assert.Equal(-90f, point.TiltY); // Clamped from -100
    }

    [Fact]
    public void CalculateVelocity_ReturnsCorrectVelocity()
    {
        var p1 = new SKInkPoint(0f, 0f, 0.5f, 0f, 0f, 0L);
        var p2 = new SKInkPoint(100f, 0f, 0.5f, 0f, 0f, 1000L); // 100 pixels in 1ms = 100 px/ms

        var velocity = SKInkPoint.CalculateVelocity(p1, p2);

        Assert.Equal(100f, velocity, 0.01f);
    }

    [Fact]
    public void CalculateVelocity_ReturnsZero_WhenTimeDeltaIsZero()
    {
        var p1 = new SKInkPoint(0f, 0f, 0.5f, 0f, 0f, 1000L);
        var p2 = new SKInkPoint(100f, 0f, 0.5f, 0f, 0f, 1000L);

        var velocity = SKInkPoint.CalculateVelocity(p1, p2);

        Assert.Equal(0f, velocity);
    }

    [Fact]
    public void CalculateVelocity_ReturnsZero_WhenTimeDeltaIsNegative()
    {
        var p1 = new SKInkPoint(0f, 0f, 0.5f, 0f, 0f, 2000L);
        var p2 = new SKInkPoint(100f, 0f, 0.5f, 0f, 0f, 1000L);

        var velocity = SKInkPoint.CalculateVelocity(p1, p2);

        Assert.Equal(0f, velocity);
    }

    [Fact]
    public void WithVelocity_CreatesNewPointWithVelocity()
    {
        var p1 = new SKInkPoint(10f, 20f, 0.5f, 5f, 10f, 1000L);
        var p2 = p1.WithVelocity(2.5f);

        Assert.Equal(p1.Location, p2.Location);
        Assert.Equal(p1.Pressure, p2.Pressure);
        Assert.Equal(p1.TiltX, p2.TiltX);
        Assert.Equal(p1.TiltY, p2.TiltY);
        Assert.Equal(p1.TimestampMicroseconds, p2.TimestampMicroseconds);
        Assert.Equal(2.5f, p2.Velocity);
    }

    [Fact]
    public void Constructor_WithVelocity_SetsVelocity()
    {
        var point = new SKInkPoint(new SKPoint(10f, 20f), 0.5f, 5f, 10f, 1000L, 3.5f);

        Assert.Equal(3.5f, point.Velocity);
    }

    [Fact]
    public void Equals_IncludesVelocity()
    {
        var p1 = new SKInkPoint(new SKPoint(10f, 20f), 0.5f, 5f, 10f, 1000L, 3.5f);
        var p2 = new SKInkPoint(new SKPoint(10f, 20f), 0.5f, 5f, 10f, 1000L, 3.5f);
        var p3 = new SKInkPoint(new SKPoint(10f, 20f), 0.5f, 5f, 10f, 1000L, 4.5f);

        Assert.Equal(p1, p2);
        Assert.NotEqual(p1, p3);
    }

    [Fact]
    public void Velocity_ClampedToNonNegative()
    {
        var point = new SKInkPoint(new SKPoint(10f, 20f), 0.5f, 5f, 10f, 1000L, -5f);

        Assert.Equal(0f, point.Velocity);
    }
}
