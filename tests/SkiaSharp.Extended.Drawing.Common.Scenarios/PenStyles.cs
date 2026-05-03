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
}
