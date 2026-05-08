using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class CombinedOperations : ScenarioBase
{
    [Fact] public void DrawAndFill_Rect() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.LightBlue);
        g.FillRectangle(brush, 15, 15, 70, 70);
        using var pen = new Pen(Color.DarkBlue, 3);
        g.DrawRectangle(pen, 15, 15, 70, 70);
    });

    [Fact] public void DrawAndFill_Ellipse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.LightGreen);
        g.FillEllipse(brush, 10, 10, 80, 80);
        using var pen = new Pen(Color.DarkGreen, 2);
        g.DrawEllipse(pen, 10, 10, 80, 80);
    });

    [Fact] public void Overlap_GradientRect_StrokedEllipse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var gradBrush = new LinearGradientBrush(new Point(5, 5), new Point(65, 65), Color.Red, Color.Yellow);
        g.FillRectangle(gradBrush, 5, 5, 60, 60);
        using var pen = new Pen(Color.Black, 2);
        g.DrawEllipse(pen, 30, 30, 60, 60);
    });

    [Fact] public void Clip_GradientFill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddEllipse(10, 10, 80, 80);
        g.SetClip(path);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(99, 99), Color.Red, Color.Blue);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Transform_GradientStroke() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.RotateTransform(30);
        using var brush = new LinearGradientBrush(new Point(-30, -30), new Point(30, 30), Color.Green, Color.Yellow);
        g.FillRectangle(brush, -30, -30, 60, 60);
        using var pen = new Pen(Color.Black, 2);
        g.DrawRectangle(pen, -30, -30, 60, 60);
    });

    [Fact] public void Hatch_Clipped_Ellipse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddEllipse(10, 10, 80, 80);
        g.SetClip(path);
        using var brush = new HatchBrush(HatchStyle.DiagonalCross, Color.DarkRed, Color.LightYellow);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void MultiLayer_Shapes() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        // Layer 1: filled rectangle
        using var brush1 = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        g.FillRectangle(brush1, 10, 10, 60, 60);
        // Layer 2: filled ellipse overlapping
        using var brush2 = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
        g.FillEllipse(brush2, 30, 30, 60, 60);
        // Layer 3: stroked triangle on top
        using var pen = new Pen(Color.Black, 2);
        g.DrawPolygon(pen, new PointF[] { new(50, 5), new(95, 85), new(5, 85) });
    });

    [Fact] public void Scaled_Gradient_Stroke() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        g.ScaleTransform(1.5f, 1.5f);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(60, 60), Color.Cyan, Color.Magenta);
        g.FillEllipse(brush, 5, 5, 50, 50);
        using var pen = new Pen(Color.Black, 1);
        g.DrawEllipse(pen, 5, 5, 50, 50);
    });
}
