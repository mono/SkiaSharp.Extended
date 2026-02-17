using SkiaSharp;
using SkiaSharp.Extended.Inking;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

public class SKInkStrokeTests
{
    [Fact]
    public void Constructor_CreatesEmptyStroke()
    {
        using var stroke = new SKInkStroke();

        Assert.True(stroke.IsEmpty);
        Assert.Equal(0, stroke.PointCount);
        Assert.Null(stroke.Path);
    }

    [Fact]
    public void Constructor_SetsStrokeWidthRange()
    {
        using var stroke = new SKInkStroke(2f, 10f);

        Assert.Equal(2f, stroke.MinStrokeWidth);
        Assert.Equal(10f, stroke.MaxStrokeWidth);
    }

    [Fact]
    public void Constructor_ThrowsOnNegativeMinWidth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKInkStroke(-1f, 8f));
    }

    [Fact]
    public void Constructor_ThrowsWhenMaxLessThanMin()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SKInkStroke(10f, 5f));
    }

    [Fact]
    public void AddPoint_AddsPointToStroke()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);

        Assert.False(stroke.IsEmpty);
        Assert.Equal(1, stroke.PointCount);
    }

    [Fact]
    public void AddPoint_WithSKInkPoint_AddsPointToStroke()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKInkPoint(10f, 20f, 0.5f));

        Assert.Equal(1, stroke.PointCount);
        Assert.Equal(10f, stroke.Points[0].X);
        Assert.Equal(20f, stroke.Points[0].Y);
    }

    [Fact]
    public void AddPoint_IgnoresPointsTooClose()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(10.5f, 20.5f), 0.5f); // Too close
        stroke.AddPoint(new SKPoint(20f, 30f), 0.5f); // Far enough

        Assert.Equal(2, stroke.PointCount);
    }

    [Fact]
    public void AddPoint_AlwaysAddsLastPoint()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(10.5f, 20.5f), 0.5f, isLastPoint: true);

        Assert.Equal(2, stroke.PointCount);
    }

    [Fact]
    public void AddPoint_UsesDefaultPressureForZeroPressure()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0f); // Zero pressure

        // Should use default pressure (0.5) instead of 0
        Assert.True(stroke.Points[0].Pressure > 0);
    }

    [Fact]
    public void Path_SinglePoint_ReturnsCirclePath()
    {
        using var stroke = new SKInkStroke(4f, 8f);

        stroke.AddPoint(new SKPoint(50f, 50f), 1f); // Full pressure

        var path = stroke.Path;

        Assert.NotNull(path);
        Assert.False(path!.IsEmpty);
        
        // Check the bounds are approximately a circle
        var bounds = path.Bounds;
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    [Fact]
    public void Path_TwoPoints_ReturnsFilledPolygon()
    {
        using var stroke = new SKInkStroke(2f, 8f);

        stroke.AddPoint(new SKPoint(10f, 50f), 0.5f);
        stroke.AddPoint(new SKPoint(100f, 50f), 0.5f);

        var path = stroke.Path;

        Assert.NotNull(path);
        Assert.False(path!.IsEmpty);
        
        var bounds = path.Bounds;
        Assert.True(bounds.Width >= 90); // At least the distance between points
    }

    [Fact]
    public void Path_IsCached()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(50f, 60f), 0.5f);

        var path1 = stroke.Path;
        var path2 = stroke.Path;

        Assert.Same(path1, path2);
    }

    [Fact]
    public void Path_InvalidatesOnNewPoint()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(50f, 60f), 0.5f);

        var path1 = stroke.Path;

        stroke.AddPoint(new SKPoint(100f, 100f), 0.5f);

        var path2 = stroke.Path;

        Assert.NotSame(path1, path2);
    }

    [Fact]
    public void Clear_RemovesAllPoints()
    {
        using var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(50f, 60f), 0.5f);

        stroke.Clear();

        Assert.True(stroke.IsEmpty);
        Assert.Equal(0, stroke.PointCount);
    }

    [Fact]
    public void Bounds_ReturnsPathBounds()
    {
        using var stroke = new SKInkStroke(2f, 4f);

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(100f, 80f), 0.5f);

        var bounds = stroke.Bounds;

        Assert.False(bounds.IsEmpty);
        Assert.True(bounds.Left <= 10f);
        Assert.True(bounds.Right >= 100f);
    }

    [Fact]
    public void PressureSensitivity_HighPressureWidensStroke()
    {
        using var lowPressureStroke = new SKInkStroke(2f, 10f);
        using var highPressureStroke = new SKInkStroke(2f, 10f);

        // Low pressure stroke
        lowPressureStroke.AddPoint(new SKPoint(0f, 50f), 0.1f);
        lowPressureStroke.AddPoint(new SKPoint(100f, 50f), 0.1f);

        // High pressure stroke
        highPressureStroke.AddPoint(new SKPoint(0f, 50f), 0.9f);
        highPressureStroke.AddPoint(new SKPoint(100f, 50f), 0.9f);

        var lowBounds = lowPressureStroke.Bounds;
        var highBounds = highPressureStroke.Bounds;

        // High pressure stroke should be taller (wider perpendicular to stroke direction)
        Assert.True(highBounds.Height > lowBounds.Height);
    }

    [Fact]
    public void Dispose_DisposesPath()
    {
        var stroke = new SKInkStroke();

        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);
        stroke.AddPoint(new SKPoint(50f, 60f), 0.5f);

        var path = stroke.Path;
        Assert.NotNull(path);

        stroke.Dispose();

        // Accessing Path after dispose should throw
        Assert.Throws<ObjectDisposedException>(() => stroke.Path);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var stroke = new SKInkStroke();
        stroke.AddPoint(new SKPoint(10f, 20f), 0.5f);

        stroke.Dispose();
        stroke.Dispose(); // Should not throw
    }
}
