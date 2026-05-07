using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class GraphicsPathAdvanced : ScenarioBase
{
    [Fact] public void Path_AddString() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        try {
            path.AddString("Hi", FontFamily.GenericSansSerif, (int)FontStyle.Bold, 40,
                new Point(10, 20), StringFormat.GenericDefault);
            using var pen = new Pen(Color.Black, 1);
            g.DrawPath(pen, path);
            g.FillPath(Brushes.Blue, path);
        } catch (PlatformNotSupportedException) {
            DrawNotSupported(g, 150, 100);
        }
    });

    [Fact] public void Path_Flatten() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddBezier(10, 90, 30, 10, 70, 10, 90, 90);
        path.Flatten();
        using var pen = new Pen(Color.Black, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Path_Widen() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddLine(20, 80, 50, 20);
        path.AddLine(50, 20, 80, 80);
        using var widenPen = new Pen(Color.Black, 8);
        try {
            path.Widen(widenPen);
            g.FillPath(Brushes.DarkBlue, path);
        } catch (PlatformNotSupportedException) {
            DrawNotSupported(g, 100, 100);
        }
    });

    [Fact] public void Path_FillModes() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        // Winding mode
        using var windPath = new GraphicsPath(FillMode.Winding);
        windPath.AddRectangle(new RectangleF(5, 10, 40, 40));
        windPath.AddRectangle(new RectangleF(25, 30, 40, 40));
        g.FillPath(Brushes.Blue, windPath);
        // Alternate mode
        using var altPath = new GraphicsPath(FillMode.Alternate);
        altPath.AddRectangle(new RectangleF(80, 10, 40, 40));
        altPath.AddRectangle(new RectangleF(100, 30, 40, 40));
        g.FillPath(Brushes.Red, altPath);
    });

    [Fact] public void Path_CloseFigure() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddLine(10, 10, 50, 10);
        path.AddLine(50, 10, 50, 50);
        path.CloseFigure();
        path.StartFigure();
        path.AddLine(60, 60, 90, 60);
        path.AddLine(90, 60, 90, 90);
        path.CloseFigure();
        using var pen = new Pen(Color.Black, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Path_StartNewFigure() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddLine(10, 10, 40, 10);
        path.AddLine(40, 10, 40, 40);
        path.StartFigure();
        path.AddLine(60, 60, 90, 60);
        path.AddLine(90, 60, 90, 90);
        using var pen = new Pen(Color.Black, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Path_Transform() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(0, 0, 30, 30));
        using var matrix = new Matrix();
        matrix.Translate(35, 35);
        matrix.Rotate(30);
        path.Transform(matrix);
        using var pen = new Pen(Color.Black, 2);
        g.DrawPath(pen, path);
        g.FillPath(Brushes.Orange, path);
    });

    [Fact] public void Path_Reverse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddLine(10, 10, 90, 10);
        path.AddLine(90, 10, 90, 90);
        path.AddLine(90, 90, 10, 90);
        path.Reverse();
        using var pen = new Pen(Color.Black, 2) {
            StartCap = LineCap.Round,
            EndCap = LineCap.ArrowAnchor
        };
        g.DrawPath(pen, path);
    });

    private static void DrawNotSupported(Graphics g, int w, int h)
    {
        using var pen = new Pen(Color.Red, 3);
        g.DrawLine(pen, 0, 0, w, h);
        g.DrawLine(pen, w, 0, 0, h);
    }
}
