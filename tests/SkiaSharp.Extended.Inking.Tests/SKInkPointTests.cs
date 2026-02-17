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
        Assert.Equal(12345, point.Timestamp);
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
}
