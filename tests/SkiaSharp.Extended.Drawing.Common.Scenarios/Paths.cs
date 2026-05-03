using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class Paths : ScenarioBase
{
    [Fact] public void Path_MultiShape() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(10, 10, 30, 30));
        path.AddEllipse(50, 10, 40, 40);
        path.AddLine(10, 60, 90, 90);
        using var pen = new Pen(Color.Black, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Path_Filled() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddEllipse(10, 10, 80, 80);
        path.AddRectangle(new RectangleF(30, 30, 40, 40));
        using var brush = new SolidBrush(Color.Red);
        g.FillPath(brush, path);
    });

    [Fact] public void Path_WithBezier() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddBezier(10, 50, 30, 10, 70, 90, 90, 50);
        using var pen = new Pen(Color.Blue, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Path_WithCurve() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddCurve(new PointF[] { new(10,50), new(30,10), new(50,90), new(70,10), new(90,50) });
        using var pen = new Pen(Color.DarkGreen, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Path_FillModeWinding() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath(FillMode.Winding);
        // Overlapping rectangles - winding fills intersection
        path.AddRectangle(new RectangleF(10, 10, 60, 60));
        path.AddRectangle(new RectangleF(30, 30, 60, 60));
        using var brush = new SolidBrush(Color.FromArgb(128, Color.Blue));
        g.FillPath(brush, path);
    });
}
