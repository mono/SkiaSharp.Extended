using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

public class Rectangles : ScenarioBase
{
    [Fact] public void Rect_Stroke_1px() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, 10, 10, 80, 80);
    });

    [Fact] public void Rect_Stroke_3px() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3);
        g.DrawRectangle(pen, 10, 10, 80, 80);
    });

    [Fact] public void Rect_Fill_Red() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void Rect_Fill_Small() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillRectangle(brush, 40, 40, 20, 20);
    });

    [Fact] public void Rect_StrokeAndFill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 10, 10, 80, 80);
        using var pen = new Pen(Color.Black, 2);
        g.DrawRectangle(pen, 10, 10, 80, 80);
    });

    [Fact] public void Rect_Multiple() => Render(200, 200, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var redBrush = new SolidBrush(Color.Red);
        using var greenBrush = new SolidBrush(Color.Green);
        using var blueBrush = new SolidBrush(Color.Blue);
        g.FillRectangle(redBrush, 10, 10, 80, 80);
        g.FillRectangle(greenBrush, 60, 60, 80, 80);
        g.FillRectangle(blueBrush, 110, 110, 80, 80);
    });
}
