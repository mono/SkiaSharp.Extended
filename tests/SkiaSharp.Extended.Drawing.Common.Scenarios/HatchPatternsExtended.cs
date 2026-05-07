using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class HatchPatternsExtended : ScenarioBase
{
    [Fact] public void Hatch_LightHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LightHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DarkHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DarkHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_NarrowVertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.NarrowVertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_NarrowHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.NarrowHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent05() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent05, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent10() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent10, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent20() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent20, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent25() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent25, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent75() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent75, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent90() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent90, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Plaid() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Plaid, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_SmallCheckerBoard() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.SmallCheckerBoard, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_LargeCheckerBoard() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_ZigZag() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.ZigZag, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Wave() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Wave, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Sphere() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Sphere, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Trellis() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Trellis, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });
}
