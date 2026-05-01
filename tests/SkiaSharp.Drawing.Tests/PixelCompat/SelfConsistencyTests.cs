using System.Drawing;
using System.Drawing.Drawing2D;
using SkiaSharp.Drawing.Tests.Infrastructure;

namespace SkiaSharp.Drawing.Tests.PixelCompat;

/// <summary>
/// Self-consistency tests: render → save → reload → compare to self.
/// Validates that our rendering pipeline is deterministic (0% error on self-comparison).
/// </summary>
public class SelfConsistencyTests : PixelCompatibilityTestBase
{
    [Fact]
    public void FilledRect_SaveReload_ExactMatch()
    {
        using var rendered = RenderWithSkiaDrawing(40, 40, g =>
        {
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Red);
            g.FillRectangle(brush, 5, 5, 30, 30);
        });

        using var ms = new MemoryStream();
        SaveToStream(rendered, ms);
        ms.Position = 0;

        using var reloaded = SKBitmap.Decode(ms);
        Assert.NotNull(reloaded);
        Assert.Equal(rendered.Width, reloaded.Width);
        Assert.Equal(rendered.Height, reloaded.Height);

        var result = SkiaSharp.Extended.SKPixelComparer.Compare(rendered, reloaded);
        Assert.True(result.ErrorPixelPercentage <= 0.001,
            $"Self-comparison should be near-exact: error={result.ErrorPixelPercentage:P4}");
    }

    [Fact]
    public void Lines_SaveReload_ExactMatch()
    {
        using var rendered = RenderWithSkiaDrawing(40, 40, g =>
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.None;
            using var pen = new Pen(Color.Blue, 2);
            g.DrawLine(pen, 0, 20, 39, 20);
            g.DrawLine(pen, 20, 0, 20, 39);
        });

        using var ms = new MemoryStream();
        SaveToStream(rendered, ms);
        ms.Position = 0;

        using var reloaded = SKBitmap.Decode(ms);
        var result = SkiaSharp.Extended.SKPixelComparer.Compare(rendered, reloaded);
        Assert.True(result.ErrorPixelPercentage <= 0.001,
            $"Self-comparison should be near-exact: error={result.ErrorPixelPercentage:P4}");
    }

    [Fact]
    public void Ellipse_SaveReload_ExactMatch()
    {
        using var rendered = RenderWithSkiaDrawing(50, 50, g =>
        {
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Green);
            g.FillEllipse(brush, 5, 5, 40, 40);
        });

        using var ms = new MemoryStream();
        SaveToStream(rendered, ms);
        ms.Position = 0;

        using var reloaded = SKBitmap.Decode(ms);
        var result = SkiaSharp.Extended.SKPixelComparer.Compare(rendered, reloaded);
        Assert.True(result.ErrorPixelPercentage <= 0.001,
            $"Self-comparison should be near-exact: error={result.ErrorPixelPercentage:P4}");
    }

    [Fact]
    public void CompositeScene_SaveReload_ExactMatch()
    {
        using var rendered = RenderWithSkiaDrawing(60, 60, g =>
        {
            g.Clear(Color.White);
            using var redBrush = new SolidBrush(Color.Red);
            g.FillRectangle(redBrush, 0, 0, 30, 30);
            using var blueBrush = new SolidBrush(Color.Blue);
            g.FillEllipse(blueBrush, 20, 20, 30, 30);
            using var pen = new Pen(Color.Black, 2);
            g.DrawLine(pen, 0, 0, 59, 59);
        });

        using var ms = new MemoryStream();
        SaveToStream(rendered, ms);
        ms.Position = 0;

        using var reloaded = SKBitmap.Decode(ms);
        var result = SkiaSharp.Extended.SKPixelComparer.Compare(rendered, reloaded);
        Assert.True(result.ErrorPixelPercentage <= 0.001,
            $"Self-comparison should be near-exact: error={result.ErrorPixelPercentage:P4}");
    }

    [Fact]
    public void Polygon_SaveReload_ExactMatch()
    {
        using var rendered = RenderWithSkiaDrawing(50, 50, g =>
        {
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Magenta);
            g.FillPolygon(brush, new PointF[] { new(25, 0), new(0, 49), new(49, 49) });
        });

        using var ms = new MemoryStream();
        SaveToStream(rendered, ms);
        ms.Position = 0;

        using var reloaded = SKBitmap.Decode(ms);
        var result = SkiaSharp.Extended.SKPixelComparer.Compare(rendered, reloaded);
        Assert.True(result.ErrorPixelPercentage <= 0.001,
            $"Self-comparison should be near-exact: error={result.ErrorPixelPercentage:P4}");
    }

    [Fact]
    public void Pie_SaveReload_ExactMatch()
    {
        using var rendered = RenderWithSkiaDrawing(50, 50, g =>
        {
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Teal);
            g.FillPie(brush, 5, 5, 40, 40, 0, 270);
        });

        using var ms = new MemoryStream();
        SaveToStream(rendered, ms);
        ms.Position = 0;

        using var reloaded = SKBitmap.Decode(ms);
        var result = SkiaSharp.Extended.SKPixelComparer.Compare(rendered, reloaded);
        Assert.True(result.ErrorPixelPercentage <= 0.001,
            $"Self-comparison should be near-exact: error={result.ErrorPixelPercentage:P4}");
    }

    private static void SaveToStream(SKBitmap bitmap, MemoryStream ms)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(ms);
    }
}
