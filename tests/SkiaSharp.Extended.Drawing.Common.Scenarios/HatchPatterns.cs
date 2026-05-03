using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class HatchPatterns : ScenarioBase
{
    [Fact] public void Hatch_Horizontal() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Horizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Vertical() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Vertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_ForwardDiagonal() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Cross() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Cross, Color.Red, Color.Yellow);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DiagonalCross() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DiagonalCross, Color.Blue, Color.LightGray);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent50() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DashedHorizontal() => Render(100, 100, g => {
        g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DashedHorizontal, Color.DarkBlue, Color.LightBlue);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });
}
