using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Scenarios;

public class Boundaries : ScenarioBase
{
    public Boundaries(string outputDir) : base(outputDir) { }

    public void Ellipse_Even_40x40() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 40, 40);
    });

    public void Ellipse_Odd_41x41() => Render(62, 62, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 41, 41);
    });

    public void Ellipse_Even_80x80() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    public void Ellipse_Odd_79x79() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 79, 79);
    });

    public void Ellipse_Small_10x10() => Render(30, 30, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillEllipse(brush, 10, 10, 10, 10);
    });

    public void Ellipse_Small_11x11() => Render(32, 32, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillEllipse(brush, 10, 10, 11, 11);
    });

    public void Ellipse_Tiny_4x4() => Render(20, 20, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillEllipse(brush, 8, 8, 4, 4);
    });

    public void Ellipse_Tiny_5x5() => Render(20, 20, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillEllipse(brush, 8, 8, 5, 5);
    });

    public void Ellipse_Stroke_1px_Even() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawEllipse(pen, 10, 10, 40, 40);
    });

    public void Ellipse_Stroke_1px_Odd() => Render(62, 62, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawEllipse(pen, 10, 10, 41, 41);
    });

    public void Ellipse_Stroke_2px() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        g.DrawEllipse(pen, 10, 10, 40, 40);
    });

    public void Ellipse_Stroke_3px() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3);
        g.DrawEllipse(pen, 10, 10, 40, 40);
    });

    public void Arc_90_Even() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        g.DrawArc(pen, 10, 10, 40, 40, 0, 90);
    });

    public void Arc_180_Odd() => Render(62, 62, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        g.DrawArc(pen, 10, 10, 41, 41, 0, 180);
    });

    public void Pie_Fill_Even() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillPie(brush, 10, 10, 40, 40, 0, 90);
    });

    public void Pie_Fill_Odd() => Render(62, 62, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillPie(brush, 10, 10, 41, 41, 0, 90);
    });

    public void Compare_Rect_Ellipse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, 10, 10, 80, 80);
        using var redPen = new Pen(Color.Red, 1);
        g.DrawEllipse(redPen, 10, 10, 80, 80);
    });
}
