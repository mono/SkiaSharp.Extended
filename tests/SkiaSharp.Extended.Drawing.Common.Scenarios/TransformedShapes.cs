using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class TransformedShapes : ScenarioBase
{
    [Fact] public void Trans_Translate_Rect() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(20, 20);
        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 0, 0, 50, 50);
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, 0, 0, 50, 50);
    });

    [Fact] public void Trans_Scale_Ellipse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.ScaleTransform(2f, 1.5f);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 5, 10, 30, 30);
    });

    [Fact] public void Trans_Rotate45_Line() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.RotateTransform(45);
        using var pen = new Pen(Color.Black, 2);
        g.DrawLine(pen, -40, 0, 40, 0);
    });

    [Fact] public void Trans_Rotate30_Rect() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.RotateTransform(30);
        using var brush = new SolidBrush(Color.Green);
        g.FillRectangle(brush, -25, -25, 50, 50);
    });

    [Fact] public void Trans_Combined_Fill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.ScaleTransform(1.5f, 1.5f);
        g.RotateTransform(20);
        using var brush = new SolidBrush(Color.Purple);
        g.FillRectangle(brush, -15, -15, 30, 30);
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, -15, -15, 30, 30);
    });

    [Fact] public void Trans_Scale2x_Stroke() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.ScaleTransform(2f, 2f);
        using var pen = new Pen(Color.DarkRed, 2);
        g.DrawRectangle(pen, 5, 5, 35, 35);
        g.DrawEllipse(pen, 10, 10, 25, 25);
    });

    [Fact] public void Trans_Rotate_Gradient() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.RotateTransform(45);
        using var brush = new LinearGradientBrush(new Point(-30, -30), new Point(30, 30), Color.Red, Color.Blue);
        g.FillRectangle(brush, -30, -30, 60, 60);
    });

    [Fact] public void Trans_Translate_Polygon() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(25, 10);
        using var brush = new SolidBrush(Color.Orange);
        g.FillPolygon(brush, new PointF[] { new(25, 5), new(45, 40), new(5, 40) });
        using var pen = new Pen(Color.Black, 1);
        g.DrawPolygon(pen, new PointF[] { new(25, 5), new(45, 40), new(5, 40) });
    });
}
