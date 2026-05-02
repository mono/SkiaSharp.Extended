using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

public class Beziers : ScenarioBase
{
    [Fact] public void Bezier_Simple() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        g.DrawBezier(pen, 10f, 50f, 30f, 10f, 70f, 90f, 90f, 50f);
    });

    [Fact] public void Bezier_Multiple() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 2);
        g.DrawBeziers(pen, new PointF[] { new(10,50), new(30,10), new(50,90), new(70,10), new(90,50), new(80,90), new(60,50) });
    });
}
