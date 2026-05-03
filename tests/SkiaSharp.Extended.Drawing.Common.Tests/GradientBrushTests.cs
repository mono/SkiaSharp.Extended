using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class GradientBrushTests
{
    // --- LinearGradientBrush constructors ---

    [Fact]
    public void Constructor_TwoPoints_TwoColors()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        Assert.NotNull(brush);
    }

    [Fact]
    public void Constructor_Rectangle_TwoColors()
    {
        using var brush = new LinearGradientBrush(
            new RectangleF(0, 0, 100, 50),
            Color.Green, Color.Yellow,
            LinearGradientMode.Horizontal);
        Assert.NotNull(brush);
    }

    [Fact]
    public void Constructor_Rectangle_Angle()
    {
        using var brush = new LinearGradientBrush(
            new RectangleF(0, 0, 100, 100),
            Color.Red, Color.Blue, 45f);
        Assert.NotNull(brush);
    }

    // --- Properties ---

    [Fact]
    public void LinearColors_ReturnsSetColors()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        var colors = brush.LinearColors;
        Assert.Equal(2, colors.Length);
    }

    [Fact]
    public void WrapMode_DefaultIsTile()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        Assert.Equal(WrapMode.Tile, brush.WrapMode);
    }

    [Fact]
    public void WrapMode_CanBeSet()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.WrapMode = WrapMode.Clamp;
        Assert.Equal(WrapMode.Clamp, brush.WrapMode);
    }

    [Fact]
    public void Rectangle_Property()
    {
        var rect = new RectangleF(10, 20, 100, 50);
        using var brush = new LinearGradientBrush(
            rect, Color.Red, Color.Blue, LinearGradientMode.Horizontal);
        Assert.Equal(rect, brush.Rectangle);
    }

    // --- Transform methods ---

    [Fact]
    public void ResetTransform_DoesNotThrow()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.ResetTransform();
    }

    [Fact]
    public void RotateTransform_DoesNotThrow()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.RotateTransform(45);
    }

    [Fact]
    public void ScaleTransform_DoesNotThrow()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.ScaleTransform(2, 2);
    }

    [Fact]
    public void TranslateTransform_DoesNotThrow()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.TranslateTransform(10, 20);
    }

    // --- Clone ---

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        using var clone = (LinearGradientBrush)brush.Clone();
        Assert.NotNull(clone);
        Assert.Equal(brush.WrapMode, clone.WrapMode);
    }

    // --- Drawing with gradient ---

    [Fact]
    public void FillRectangle_WithGradient_DoesNotThrow()
    {
        using var bmp = new Bitmap(100, 50);
        using var g = Graphics.FromImage(bmp);
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        g.FillRectangle(brush, 0, 0, 100, 50);
    }

    [Fact]
    public void FillRectangle_GradientProducesVariedPixels()
    {
        using var bmp = new Bitmap(100, 10);
        using var g = Graphics.FromImage(bmp);
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        g.FillRectangle(brush, 0, 0, 100, 10);

        var left = bmp.GetPixel(5, 5);
        var right = bmp.GetPixel(95, 5);
        // Left should be more red, right more blue
        Assert.True(left.R > left.B);
        Assert.True(right.B > right.R);
    }

    // --- GammaCorrection ---

    [Fact]
    public void GammaCorrection_GetSet()
    {
        using var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.GammaCorrection = true;
        Assert.True(brush.GammaCorrection);
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var brush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(100, 0),
            Color.Red, Color.Blue);
        brush.Dispose();
    }
}
