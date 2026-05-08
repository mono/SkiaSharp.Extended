using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class ArcsAA : ScenarioBase
{
    [Fact] public void Arc_Quarter_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        g.DrawArc(pen, 10, 10, 80, 80, 0, 90);
    });

    [Fact] public void Arc_Half_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 2);
        g.DrawArc(pen, 10, 10, 80, 80, 0, 180);
    });

    [Fact] public void Arc_ThreeQuarter_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 2);
        g.DrawArc(pen, 10, 10, 80, 80, 45, 270);
    });

    [Fact] public void Arc_NegativeStart_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Green, 2);
        g.DrawArc(pen, 10, 10, 80, 80, -45, 180);
    });

    [Fact] public void Arc_Thick_AA() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        using var pen = new Pen(Color.DarkRed, 5);
        g.DrawArc(pen, 10, 10, 80, 80, 30, 120);
    });
}
