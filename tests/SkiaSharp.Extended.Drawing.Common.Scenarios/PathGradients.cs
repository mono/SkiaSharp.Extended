using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class PathGradients : ScenarioBase
{
    // Triangle — 3 vertices, 3 surround colors
    [Fact] public void PathGrad_Triangle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] { new(50, 10), new(10, 90), new(90, 90) };
        using var path = new GraphicsPath();
        path.AddPolygon(pts);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Red, Color.Green, Color.Blue };
            g.FillPath(brush, path);
        } catch (PlatformNotSupportedException) { }
    });

    // Square — 4 vertices, 4 colors (same as existing but explicit)
    [Fact] public void PathGrad_Square() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] { new(10, 10), new(90, 10), new(90, 90), new(10, 90) };
        using var path = new GraphicsPath();
        path.AddPolygon(pts);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };
            g.FillPath(brush, path);
        } catch (PlatformNotSupportedException) { }
    });

    // Pentagon — 5 vertices, 5 colors
    [Fact] public void PathGrad_Pentagon() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] { new(50,5), new(95,37), new(77,90), new(23,90), new(5,37) };
        using var path = new GraphicsPath();
        path.AddPolygon(pts);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
            g.FillPath(brush, path);
        } catch (PlatformNotSupportedException) { }
    });

    // Star — 10 points (5 outer + 5 inner), single surround color
    [Fact] public void PathGrad_Star() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] {
            new(50,5), new(61,35), new(95,35), new(68,55), new(79,90),
            new(50,70), new(21,90), new(32,55), new(5,35), new(39,35)
        };
        using var path = new GraphicsPath();
        path.AddPolygon(pts);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.Yellow;
            brush.SurroundColors = new[] { Color.DarkRed };
            g.FillPath(brush, path);
        } catch (PlatformNotSupportedException) { }
    });

    // Ellipse path — radial-like gradient on curved shape
    [Fact] public void PathGrad_Ellipse() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddEllipse(10, 10, 80, 80);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.Yellow;
            brush.SurroundColors = new[] { Color.DarkBlue };
            g.FillEllipse(brush, 10, 10, 80, 80);
        } catch (PlatformNotSupportedException) { }
    });

    // Off-center — CenterPoint moved from centroid
    [Fact] public void PathGrad_OffCenter() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] { new(10, 10), new(90, 10), new(90, 90), new(10, 90) };
        using var path = new GraphicsPath();
        path.AddPolygon(pts);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.White;
            brush.CenterPoint = new PointF(30, 30);
            brush.SurroundColors = new[] { Color.DarkRed };
            g.FillPath(brush, path);
        } catch (PlatformNotSupportedException) { }
    });

    // Single color surround on rectangle — simple center-to-edge gradient
    [Fact] public void PathGrad_SingleColor() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] { new(10, 10), new(90, 10), new(90, 90), new(10, 90) };
        try {
            using var brush = new PathGradientBrush(pts);
            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Black };
            g.FillRectangle(brush, 10, 10, 80, 80);
        } catch (PlatformNotSupportedException) { }
    });

    // L-shape (concave polygon) — tests non-convex path gradient
    [Fact] public void PathGrad_LShape() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var pts = new PointF[] { new(10,10), new(60,10), new(60,50), new(90,50), new(90,90), new(10,90) };
        using var path = new GraphicsPath();
        path.AddPolygon(pts);
        try {
            using var brush = new PathGradientBrush(path);
            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Red };
            g.FillPath(brush, path);
        } catch (PlatformNotSupportedException) { }
    });
}
