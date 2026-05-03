using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class CompositesAA : ScenarioBase
{
    [Fact] public void Composite_RectOverEllipse_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var blueBrush = new SolidBrush(Color.Blue);
        g.FillEllipse(blueBrush, 10, 10, 80, 80);
        using var redBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        g.FillRectangle(redBrush, 25, 25, 50, 50);
    });

    [Fact] public void Composite_MultipleShapes_AA() => Render(200, 200, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var redBrush = new SolidBrush(Color.Red);
        using var greenBrush = new SolidBrush(Color.Green);
        using var blueBrush = new SolidBrush(Color.Blue);
        using var pen = new Pen(Color.Black, 3);
        g.FillRectangle(redBrush, 10, 10, 80, 80);
        g.FillRectangle(greenBrush, 60, 60, 80, 80);
        g.FillEllipse(blueBrush, 110, 10, 80, 80);
        g.DrawLine(pen, 0, 0, 199, 199);
        g.DrawLine(pen, 199, 0, 0, 199);
    });

    [Fact] public void Composite_ConcentricCircles_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var r = new SolidBrush(Color.Red);
        using var gr = new SolidBrush(Color.Green);
        using var b = new SolidBrush(Color.Blue);
        using var y = new SolidBrush(Color.Yellow);
        g.FillEllipse(r, 5, 5, 90, 90);
        g.FillEllipse(gr, 15, 15, 70, 70);
        g.FillEllipse(b, 25, 25, 50, 50);
        g.FillEllipse(y, 35, 35, 30, 30);
    });
}
