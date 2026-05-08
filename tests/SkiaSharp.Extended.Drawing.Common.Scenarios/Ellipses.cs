using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class Ellipses : ScenarioBase
{
    [Fact] public void Ellipse_Stroke_Circle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawEllipse(pen, 10, 10, 80, 80);
    });

    [Fact] public void Ellipse_Fill_Circle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    [Fact] public void Ellipse_Wide() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Magenta);
        g.FillEllipse(brush, 5, 25, 90, 50);
    });

    [Fact] public void Ellipse_Tall() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Cyan);
        g.FillEllipse(brush, 25, 5, 50, 90);
    });

    [Fact] public void Ellipse_StrokeAndFill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Yellow);
        g.FillEllipse(brush, 10, 10, 80, 80);
        using var pen = new Pen(Color.Black, 2);
        g.DrawEllipse(pen, 10, 10, 80, 80);
    });
}
