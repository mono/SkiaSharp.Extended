using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class GradientFills : ScenarioBase
{
    [Fact] public void GradFill_Rectangle_Horiz() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(10, 0), new Point(90, 0), Color.Red, Color.Blue);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void GradFill_Rectangle_Vert() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 10), new Point(0, 90), Color.Green, Color.Yellow);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void GradFill_Ellipse_Horiz() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(10, 0), new Point(90, 0), Color.Cyan, Color.Magenta);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    [Fact] public void GradFill_Ellipse_Diag() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(10, 10), new Point(90, 90), Color.Orange, Color.Purple);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    [Fact] public void GradFill_Polygon() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(99, 0), Color.Red, Color.Green);
        g.FillPolygon(brush, new PointF[] { new(50, 10), new(90, 90), new(10, 90) });
    });

    [Fact] public void GradFill_Pie() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(99, 99), Color.Blue, Color.Yellow);
        g.FillPie(brush, 10, 10, 80, 80, 0, 270);
    });

    [Fact] public void GradFill_MultiColor() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush1 = new LinearGradientBrush(new Point(0, 0), new Point(49, 0), Color.Red, Color.Blue);
        g.FillRectangle(brush1, 0, 10, 50, 80);
        using var brush2 = new LinearGradientBrush(new Point(50, 0), new Point(99, 0), Color.Green, Color.Yellow);
        g.FillRectangle(brush2, 50, 10, 50, 80);
        using var brush3 = new LinearGradientBrush(new Point(100, 0), new Point(149, 0), Color.Cyan, Color.Magenta);
        g.FillRectangle(brush3, 100, 10, 50, 80);
    });

    [Fact] public void GradFill_Rectangle_WideStroke() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(10, 10), new Point(90, 90), Color.Red, Color.Blue);
        g.FillRectangle(brush, 10, 10, 80, 80);
        using var pen = new Pen(Color.Black, 3);
        g.DrawRectangle(pen, 10, 10, 80, 80);
    });
}
