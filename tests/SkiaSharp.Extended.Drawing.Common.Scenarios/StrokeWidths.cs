using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class StrokeWidths : ScenarioBase
{
    [Fact] public void Line_Width1() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 10, 50, 90, 50);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Line_Width3() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3);
        g.DrawLine(pen, 10, 50, 90, 50);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Line_Width5() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 5);
        g.DrawLine(pen, 10, 50, 90, 50);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Line_Width10() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 10);
        g.DrawLine(pen, 10, 50, 90, 50);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Rect_Width1() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, 15, 15, 70, 70);
    });

    [Fact] public void Rect_Width3() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3);
        g.DrawRectangle(pen, 15, 15, 70, 70);
    });

    [Fact] public void Rect_Width5() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 5);
        g.DrawRectangle(pen, 15, 15, 70, 70);
    });

    [Fact] public void Ellipse_Width1() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawEllipse(pen, 10, 10, 80, 80);
    });

    [Fact] public void Ellipse_Width3() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3);
        g.DrawEllipse(pen, 10, 10, 80, 80);
    });

    [Fact] public void Ellipse_Width5() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 5);
        g.DrawEllipse(pen, 10, 10, 80, 80);
    });
}
