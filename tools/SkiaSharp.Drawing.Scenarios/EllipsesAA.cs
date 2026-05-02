using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Scenarios;

public class EllipsesAA : ScenarioBase
{
    public EllipsesAA(string outputDir) : base(outputDir) { }

    public void Ellipse_AA_Circle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    public void Ellipse_AA_Wide() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Magenta);
        g.FillEllipse(brush, 5, 25, 90, 50);
    });
}
