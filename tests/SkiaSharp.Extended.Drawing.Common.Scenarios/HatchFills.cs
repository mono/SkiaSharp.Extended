using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class HatchFills : ScenarioBase
{
    [Fact] public void HatchFill_Rect_Horizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Horizontal, Color.Black, Color.LightYellow);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void HatchFill_Rect_Cross() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Cross, Color.DarkRed, Color.LightGray);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void HatchFill_Rect_DiagonalCross() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DiagonalCross, Color.Navy, Color.Ivory);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void HatchFill_Ellipse_Vertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Vertical, Color.DarkGreen, Color.LightGreen);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    [Fact] public void HatchFill_Polygon_ForwardDiag() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.Maroon, Color.Wheat);
        g.FillPolygon(brush, new PointF[] { new(50, 10), new(90, 90), new(10, 90) });
    });

    [Fact] public void HatchFill_Percent50() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.White);
        g.FillRectangle(brush, 5, 5, 90, 90);
    });

    [Fact] public void HatchFill_Rect_BackwardDiag() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Blue, Color.LightCyan);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void HatchFill_Pie_DashedHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DashedHorizontal, Color.DarkBlue, Color.LightBlue);
        g.FillPie(brush, 10, 10, 80, 80, 0, 270);
    });
}
