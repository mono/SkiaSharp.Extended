using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class PenVariations : ScenarioBase
{
    [Fact] public void Cap_Flat() => Render(100, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 8) { StartCap = LineCap.Flat, EndCap = LineCap.Flat };
        g.DrawLine(pen, 15, 20, 85, 20);
        using var pen2 = new Pen(Color.Red, 8) { StartCap = LineCap.Flat, EndCap = LineCap.Flat };
        g.DrawLine(pen2, 15, 40, 85, 40);
    });

    [Fact] public void Cap_Round() => Render(100, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, 15, 20, 85, 20);
        using var pen2 = new Pen(Color.Blue, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen2, 15, 40, 85, 40);
    });

    [Fact] public void Cap_Square() => Render(100, 60, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 8) { StartCap = LineCap.Square, EndCap = LineCap.Square };
        g.DrawLine(pen, 15, 20, 85, 20);
        using var pen2 = new Pen(Color.Green, 8) { StartCap = LineCap.Square, EndCap = LineCap.Square };
        g.DrawLine(pen2, 15, 40, 85, 40);
    });

    [Fact] public void Join_Miter() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Miter };
        g.DrawPolygon(pen, new PointF[] { new(50, 10), new(90, 90), new(10, 90) });
    });

    [Fact] public void Join_Round() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round };
        g.DrawPolygon(pen, new PointF[] { new(50, 10), new(90, 90), new(10, 90) });
    });

    [Fact] public void Join_Bevel() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Bevel };
        g.DrawPolygon(pen, new PointF[] { new(50, 10), new(90, 90), new(10, 90) });
    });

    [Fact] public void DashDot_Width3() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashStyle = DashStyle.DashDot };
        g.DrawLine(pen, 10, 25, 140, 25);
        g.DrawRectangle(pen, 10, 40, 130, 50);
    });

    [Fact] public void CustomDash_Width2() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.DarkBlue, 2) { DashPattern = new float[] { 8, 3, 2, 3 } };
        g.DrawLine(pen, 10, 25, 140, 25);
        g.DrawEllipse(pen, 20, 35, 110, 55);
    });
}
