using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Scenarios;

public class PiesAA : ScenarioBase
{
    public PiesAA(string outputDir) : base(outputDir) { }

    public void Pie_Fill_Quarter_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillPie(brush, 10, 10, 80, 80, 0, 90);
    });

    public void Pie_Fill_Half_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillPie(brush, 10, 10, 80, 80, -90, 180);
    });

    public void Pie_Fill_ThreeQuarter_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillPie(brush, 10, 10, 80, 80, 0, 270);
    });

    public void Pie_Multiple_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var r = new SolidBrush(Color.Red);
        using var gr = new SolidBrush(Color.Green);
        using var b = new SolidBrush(Color.Blue);
        using var y = new SolidBrush(Color.Yellow);
        g.FillPie(r, 10, 10, 80, 80, 0, 90);
        g.FillPie(gr, 10, 10, 80, 80, 90, 90);
        g.FillPie(b, 10, 10, 80, 80, 180, 90);
        g.FillPie(y, 10, 10, 80, 80, 270, 90);
    });
}
