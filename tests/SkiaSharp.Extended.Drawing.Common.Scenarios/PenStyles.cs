using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class PenStyles : ScenarioBase
{
    [Fact] public void Pen_Dash() => Render(100, 40, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, 5, 20, 95, 20);
    });

    [Fact] public void Pen_DashDot() => Render(100, 40, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.DashDot };
        g.DrawLine(pen, 5, 20, 95, 20);
    });

    [Fact] public void Pen_Custom_DashPattern() => Render(100, 40, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashPattern = new float[] { 6, 2, 2, 2 } };
        g.DrawLine(pen, 5, 20, 95, 20);
    });

    [Fact] public void Pen_RoundCap() => Render(100, 40, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, 20, 20, 80, 20);
    });

    [Fact] public void Pen_SquareCap() => Render(100, 40, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 8) { StartCap = LineCap.Square, EndCap = LineCap.Square };
        g.DrawLine(pen, 20, 20, 80, 20);
    });

    [Fact] public void Pen_MiterJoin() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Miter };
        g.DrawRectangle(pen, 10, 10, 40, 40);
    });

    [Fact] public void Pen_RoundJoin() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round };
        g.DrawRectangle(pen, 10, 10, 40, 40);
    });

    [Fact] public void Pen_BevelJoin() => Render(60, 60, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Bevel };
        g.DrawRectangle(pen, 10, 10, 40, 40);
    });

    [Fact] public void Pen_CompoundArray() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 12);
        try {
            pen.CompoundArray = new[] { 0f, 0.3f, 0.7f, 1f };
        } catch (PlatformNotSupportedException) { }
        g.DrawLine(pen, 10, 50, 90, 50);
        g.DrawRectangle(pen, 15, 15, 70, 70);
    });

    [Fact] public void Pen_Alignment_Center() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 8) { Alignment = PenAlignment.Center };
        g.DrawRectangle(pen, 20, 20, 60, 60);
        using var thinPen = new Pen(Color.Red, 1);
        g.DrawRectangle(thinPen, 20, 20, 60, 60);
    });

    [Fact] public void Pen_Alignment_Inset() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 8) { Alignment = PenAlignment.Inset };
        g.DrawRectangle(pen, 20, 20, 60, 60);
        using var thinPen = new Pen(Color.Red, 1);
        g.DrawRectangle(thinPen, 20, 20, 60, 60);
    });

    [Fact] public void Pen_Transform() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2);
        try {
            using var matrix = new Matrix();
            matrix.Scale(2, 1);
            pen.Transform = matrix;
        } catch (PlatformNotSupportedException) { }
        g.DrawRectangle(pen, 20, 20, 60, 60);
    });

    [Fact] public void Pen_Width_Fractional() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen1 = new Pen(Color.Black, 1.5f);
        g.DrawLine(pen1, 10, 20, 90, 20);
        using var pen2 = new Pen(Color.Blue, 2.5f);
        g.DrawLine(pen2, 10, 40, 90, 40);
        using var pen3 = new Pen(Color.Red, 3.5f);
        g.DrawLine(pen3, 10, 60, 90, 60);
        using var pen4 = new Pen(Color.Green, 0.5f);
        g.DrawLine(pen4, 10, 80, 90, 80);
    });

    [Fact] public void Pen_BrushBacked() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new LinearGradientBrush(new Point(10, 0), new Point(90, 0), Color.Red, Color.Blue);
        using var pen = new Pen(brush, 6);
        g.DrawLine(pen, 10, 20, 90, 20);
        g.DrawRectangle(pen, 15, 40, 70, 50);
    });

    [Fact] public void Pen_DashOffset() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen1 = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash, DashOffset = 0f };
        g.DrawLine(pen1, 10, 20, 140, 20);
        using var pen2 = new Pen(Color.Blue, 2) { DashStyle = DashStyle.Dash, DashOffset = 2f };
        g.DrawLine(pen2, 10, 40, 140, 40);
        using var pen3 = new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash, DashOffset = 5f };
        g.DrawLine(pen3, 10, 60, 140, 60);
        using var pen4 = new Pen(Color.Green, 2) { DashStyle = DashStyle.Dash, DashOffset = 10f };
        g.DrawLine(pen4, 10, 80, 140, 80);
    });

    [Fact] public void Pen_StartEndCap() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 8) {
            StartCap = LineCap.Round,
            EndCap = LineCap.ArrowAnchor
        };
        g.DrawLine(pen, 20, 30, 130, 30);
        using var pen2 = new Pen(Color.Blue, 8) {
            StartCap = LineCap.Square,
            EndCap = LineCap.DiamondAnchor
        };
        g.DrawLine(pen2, 20, 70, 130, 70);
    });
}
