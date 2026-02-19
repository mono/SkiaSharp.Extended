using SkiaSharp;
using SkiaSharp.Extended.Inking;
using Xunit;

namespace SkiaSharp.Extended.Inking.Tests;

public class SKInkStrokeBrushTests
{
    [Fact]
    public void DefaultBrush_HasDefaultValues()
    {
        var brush = new SKInkStrokeBrush();

        Assert.Equal(SKColors.Black, brush.Color);
        Assert.Equal(new SKSize(2, 2), brush.MinSize);
        Assert.Equal(new SKSize(6, 6), brush.MaxSize);
        Assert.Equal(SKStrokeCapStyle.Round, brush.CapStyle);
        Assert.Equal(SKSmoothingAlgorithm.CatmullRom, brush.SmoothingAlgorithm);
        Assert.Equal(4, brush.SmoothingFactor);
    }

    [Fact]
    public void Constructor_WithColor_SetsColor()
    {
        var brush = new SKInkStrokeBrush(SKColors.Blue);

        Assert.Equal(SKColors.Blue, brush.Color);
    }

    [Fact]
    public void Constructor_WithColorAndSizes_SetsAll()
    {
        var brush = new SKInkStrokeBrush(SKColors.Red, new SKSize(1, 2), new SKSize(10, 12));

        Assert.Equal(SKColors.Red, brush.Color);
        Assert.Equal(new SKSize(1, 2), brush.MinSize);
        Assert.Equal(new SKSize(10, 12), brush.MaxSize);
    }

    [Fact]
    public void Constructor_WithIsotropicSizes_SetsBothWidthAndHeight()
    {
        var brush = new SKInkStrokeBrush(SKColors.Green, 3f, 15f);

        Assert.Equal(SKColors.Green, brush.Color);
        Assert.Equal(new SKSize(3, 3), brush.MinSize);
        Assert.Equal(new SKSize(15, 15), brush.MaxSize);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new SKInkStrokeBrush(SKColors.Blue, 2f, 10f);
        original.CapStyle = SKStrokeCapStyle.Tapered;
        original.SmoothingFactor = 8;
        original.SmoothingAlgorithm = SKSmoothingAlgorithm.QuadraticBezier;

        var clone = original.Clone();

        // Verify values are copied
        Assert.Equal(original.Color, clone.Color);
        Assert.Equal(original.MinSize, clone.MinSize);
        Assert.Equal(original.MaxSize, clone.MaxSize);
        Assert.Equal(original.CapStyle, clone.CapStyle);
        Assert.Equal(original.SmoothingFactor, clone.SmoothingFactor);
        Assert.Equal(original.SmoothingAlgorithm, clone.SmoothingAlgorithm);

        // Verify clone is independent
        clone.Color = SKColors.Yellow;
        Assert.Equal(SKColors.Blue, original.Color);
        Assert.Equal(SKColors.Yellow, clone.Color);
    }

    [Fact]
    public void GetWidthForPressure_InterpolatesCorrectly()
    {
        var brush = new SKInkStrokeBrush(SKColors.Black, 2f, 10f);

        Assert.Equal(2f, brush.GetWidthForPressure(0f));
        Assert.Equal(6f, brush.GetWidthForPressure(0.5f));
        Assert.Equal(10f, brush.GetWidthForPressure(1f));
    }

    [Fact]
    public void GetWidthForPressure_ClampsPressure()
    {
        var brush = new SKInkStrokeBrush(SKColors.Black, 2f, 10f);

        Assert.Equal(2f, brush.GetWidthForPressure(-0.5f)); // Clamped to 0
        Assert.Equal(10f, brush.GetWidthForPressure(1.5f)); // Clamped to 1
    }

    [Fact]
    public void GetSizeForPressure_ReturnsInterpolatedSize()
    {
        var brush = new SKInkStrokeBrush
        {
            MinSize = new SKSize(2, 4),
            MaxSize = new SKSize(10, 20)
        };

        var size = brush.GetSizeForPressure(0.5f);

        Assert.Equal(6f, size.Width);  // 2 + (10-2) * 0.5 = 6
        Assert.Equal(12f, size.Height); // 4 + (20-4) * 0.5 = 12
    }

    [Fact]
    public void SmoothingFactor_ThrowsWhenOutOfRange()
    {
        var brush = new SKInkStrokeBrush();

        Assert.Throws<ArgumentOutOfRangeException>(() => brush.SmoothingFactor = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => brush.SmoothingFactor = 11);
    }

    [Fact]
    public void MinSize_ThrowsWhenNegative()
    {
        var brush = new SKInkStrokeBrush();

        Assert.Throws<ArgumentOutOfRangeException>(() => brush.MinSize = new SKSize(-1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => brush.MinSize = new SKSize(2, -1));
    }

    [Fact]
    public void MaxSize_ThrowsWhenNegative()
    {
        var brush = new SKInkStrokeBrush();

        Assert.Throws<ArgumentOutOfRangeException>(() => brush.MaxSize = new SKSize(-1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => brush.MaxSize = new SKSize(2, -1));
    }
}
