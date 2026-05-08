using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class Lines : ScenarioBase
{
    [Fact] public void Line_Horizontal_1px() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 10, 50, 90, 50);
    });

    [Fact] public void Line_Vertical_1px() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 50, 10, 50, 90);
    });

    [Fact] public void Line_Diagonal_1px() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Line_Thick_5px() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 5);
        g.DrawLine(pen, 10, 50, 90, 50);
    });

    [Fact] public void Line_Colored_Blue() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 2);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Line_Multiple() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 10, 10, 90, 10);
        g.DrawLine(pen, 10, 30, 90, 30);
        g.DrawLine(pen, 10, 50, 90, 50);
        g.DrawLine(pen, 10, 70, 90, 70);
        g.DrawLine(pen, 10, 90, 90, 90);
    });
}
