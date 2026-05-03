using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class EllipsesAA : ScenarioBase
{
    [Fact] public void Ellipse_AA_Circle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    [Fact] public void Ellipse_AA_Wide() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Magenta);
        g.FillEllipse(brush, 5, 25, 90, 50);
    });
}
