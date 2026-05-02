using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

public class Curves : ScenarioBase
{
    [Fact] public void Curve_Open() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        g.DrawCurve(pen, new PointF[] { new(10,50), new(30,20), new(50,70), new(70,30), new(90,50) });
    });

    [Fact] public void Curve_Closed_Fill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillClosedCurve(brush, new PointF[] { new(50,10), new(90,40), new(80,90), new(20,90), new(10,40) });
    });

    [Fact] public void Curve_HighTension() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Green, 2);
        g.DrawCurve(pen, new PointF[] { new(10,50), new(30,10), new(50,90), new(70,10), new(90,50) }, 1.0f);
    });
}
