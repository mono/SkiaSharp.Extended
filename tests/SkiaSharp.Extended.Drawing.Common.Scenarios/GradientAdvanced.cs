using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class GradientAdvanced : ScenarioBase
{
    [Fact] public void Gradient_WrapMode_Tile() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(30, 0), Color.Red, Color.Blue);
        brush.WrapMode = WrapMode.Tile;
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_WrapMode_TileFlipX() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(30, 0), Color.Red, Color.Blue);
        brush.WrapMode = WrapMode.TileFlipX;
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_WrapMode_TileFlipXY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(30, 30), Color.Green, Color.Yellow);
        brush.WrapMode = WrapMode.TileFlipXY;
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_ThreeColor() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(100, 0), Color.Red, Color.Blue);
        try {
            var blend = new ColorBlend(3);
            blend.Colors = new[] { Color.Red, Color.Green, Color.Blue };
            blend.Positions = new[] { 0f, 0.5f, 1f };
            brush.InterpolationColors = blend;
        } catch (PlatformNotSupportedException) { }
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_Blend() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(100, 0), Color.Black, Color.White);
        try {
            var blend = new Blend(3);
            blend.Factors = new[] { 0f, 0.8f, 1f };
            blend.Positions = new[] { 0f, 0.3f, 1f };
            brush.Blend = blend;
        } catch (PlatformNotSupportedException) { }
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_PathGradient() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var points = new PointF[] { new(10, 10), new(90, 10), new(90, 90), new(10, 90) };
        try {
            using var brush = new PathGradientBrush(points);
            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };
            g.FillRectangle(brush, 0, 0, 100, 100);
        } catch (PlatformNotSupportedException) {
            DrawNotSupported(g, 100, 100);
        }
    });

    [Fact] public void Gradient_Radial() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddEllipse(10, 10, 80, 80);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.Yellow;
            brush.SurroundColors = new[] { Color.DarkBlue };
            g.FillEllipse(brush, 10, 10, 80, 80);
        } catch (PlatformNotSupportedException) {
            DrawNotSupported(g, 100, 100);
        }
    });

    [Fact] public void Gradient_WideArea() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(150, 100), Color.DarkRed, Color.LightBlue);
        g.FillRectangle(brush, 0, 0, 150, 100);
    });

    private static void DrawNotSupported(Graphics g, int w, int h)
    {
        using var pen = new Pen(Color.Red, 3);
        g.DrawLine(pen, 0, 0, w, h);
        g.DrawLine(pen, w, 0, 0, h);
    }
}
