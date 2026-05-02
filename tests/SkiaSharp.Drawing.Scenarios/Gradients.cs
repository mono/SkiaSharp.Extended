using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

public class Gradients : ScenarioBase
{
    [Fact] public void Gradient_Horizontal() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0,0), new Point(99,0), Color.Red, Color.Blue);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_Vertical() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0,0), new Point(0,99), Color.Green, Color.Yellow);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_Diagonal() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(0,0), new Point(99,99), Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Gradient_InEllipse() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(10,10), new Point(90,90), Color.Red, Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });
}
