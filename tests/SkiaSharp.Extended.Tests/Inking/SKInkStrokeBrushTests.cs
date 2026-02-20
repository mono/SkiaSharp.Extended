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

    [Fact]
    public void VelocityMode_DefaultsToNone()
    {
        var brush = new SKInkStrokeBrush();

        Assert.Equal(SKVelocityMode.None, brush.VelocityMode);
    }

    [Fact]
    public void VelocityScale_DefaultsToHalf()
    {
        var brush = new SKInkStrokeBrush();

        Assert.Equal(0.5f, brush.VelocityScale);
    }

    [Fact]
    public void VelocityScale_ClampedToRange()
    {
        var brush = new SKInkStrokeBrush();

        brush.VelocityScale = -0.5f;
        Assert.Equal(0f, brush.VelocityScale);

        brush.VelocityScale = 1.5f;
        Assert.Equal(1f, brush.VelocityScale);
    }

    [Fact]
    public void GetWidthForPressureAndVelocity_WithNoVelocityMode_ReturnsPressureWidth()
    {
        var brush = new SKInkStrokeBrush
        {
            MinSize = new SKSize(2f, 2f),
            MaxSize = new SKSize(10f, 10f),
            VelocityMode = SKVelocityMode.None
        };

        // At pressure 0.5, width should be 6
        var width = brush.GetWidthForPressureAndVelocity(0.5f, 5f);
        Assert.Equal(6f, width);
    }

    [Fact]
    public void GetWidthForPressureAndVelocity_BallpointPen_FasterIsThinnerStroke()
    {
        var brush = new SKInkStrokeBrush
        {
            MinSize = new SKSize(2f, 2f),
            MaxSize = new SKSize(10f, 10f),
            VelocityMode = SKVelocityMode.BallpointPen,
            VelocityScale = 1f
        };

        var slowWidth = brush.GetWidthForPressureAndVelocity(0.5f, 0.5f);
        var fastWidth = brush.GetWidthForPressureAndVelocity(0.5f, 4f);

        Assert.True(fastWidth < slowWidth, "Faster velocity should result in thinner stroke");
    }

    [Fact]
    public void GetWidthForPressureAndVelocity_Pencil_FasterIsThinnerStroke()
    {
        var brush = new SKInkStrokeBrush
        {
            MinSize = new SKSize(2f, 2f),
            MaxSize = new SKSize(10f, 10f),
            VelocityMode = SKVelocityMode.Pencil,
            VelocityScale = 1f
        };

        var slowWidth = brush.GetWidthForPressureAndVelocity(0.5f, 0.5f);
        var fastWidth = brush.GetWidthForPressureAndVelocity(0.5f, 4f);

        Assert.True(fastWidth < slowWidth, "Faster velocity should result in thinner stroke");
    }

    [Fact]
    public void GetColorForVelocity_NonPencilMode_ReturnsOriginalColor()
    {
        var brush = new SKInkStrokeBrush
        {
            Color = SKColors.Blue,
            VelocityMode = SKVelocityMode.BallpointPen,
            VelocityScale = 1f
        };

        var color = brush.GetColorForVelocity(5f);

        Assert.Equal(SKColors.Blue, color);
    }

    [Fact]
    public void GetColorForVelocity_PencilMode_FasterIsLighterColor()
    {
        var brush = new SKInkStrokeBrush
        {
            Color = SKColors.Black,
            VelocityMode = SKVelocityMode.Pencil,
            VelocityScale = 1f
        };

        var slowColor = brush.GetColorForVelocity(0.5f);
        var fastColor = brush.GetColorForVelocity(4f);

        Assert.True(fastColor.Alpha < slowColor.Alpha, "Faster velocity should result in lighter (more transparent) color");
    }

    [Fact]
    public void Clone_IncludesVelocitySettings()
    {
        var brush = new SKInkStrokeBrush
        {
            VelocityMode = SKVelocityMode.BallpointPen,
            VelocityScale = 0.8f
        };

        var clone = brush.Clone();

        Assert.Equal(SKVelocityMode.BallpointPen, clone.VelocityMode);
        Assert.Equal(0.8f, clone.VelocityScale);
    }
}
