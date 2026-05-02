using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Scenarios;

public class LinesAA : ScenarioBase
{
    public LinesAA(string outputDir) : base(outputDir) { }

    public void Line_Diagonal_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    public void Line_Thick_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 5);
        g.DrawLine(pen, 10, 10, 90, 90);
    });
}
