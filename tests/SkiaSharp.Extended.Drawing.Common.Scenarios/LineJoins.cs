using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class LineJoins : ScenarioBase
{
    [Fact] public void LineJoin_Miter() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Miter };
        g.DrawPolygon(pen, new PointF[] { new(20, 80), new(50, 20), new(80, 80) });
    });

    [Fact] public void LineJoin_Bevel() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Bevel };
        g.DrawPolygon(pen, new PointF[] { new(20, 80), new(50, 20), new(80, 80) });
    });

    [Fact] public void LineJoin_Round() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round };
        g.DrawPolygon(pen, new PointF[] { new(20, 80), new(50, 20), new(80, 80) });
    });

    [Fact] public void LineJoin_MiterClipped() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.MiterClipped };
        g.DrawPolygon(pen, new PointF[] { new(20, 80), new(50, 20), new(80, 80) });
    });

    [Fact] public void LineJoin_Miter_LowLimit() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Miter, MiterLimit = 1.0f };
        g.DrawPolygon(pen, new PointF[] { new(20, 80), new(50, 20), new(80, 80) });
    });
}
