using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class HatchPatterns : ScenarioBase
{
    [Fact] public void Hatch_Horizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Horizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Vertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Vertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_ForwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_BackwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Cross() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Cross, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DiagonalCross() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DiagonalCross, Color.Black, Color.White);
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

    [Fact] public void Hatch_Percent30() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent30, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent40() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent40, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent50() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent60() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent60, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent70() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent70, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent75() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent75, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent80() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent80, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Percent90() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Percent90, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_LightDownwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LightDownwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_LightUpwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_LightHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LightHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_LightVertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LightVertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DarkDownwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DarkUpwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DarkHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DarkHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DarkVertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DarkVertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_WideDownwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_WideUpwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_NarrowHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.NarrowHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_NarrowVertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.NarrowVertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DashedDownwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DashedDownwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DashedUpwardDiagonal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DashedUpwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DashedHorizontal() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DashedHorizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DashedVertical() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DashedVertical, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_SmallConfetti() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.SmallConfetti, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_LargeConfetti() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.LargeConfetti, Color.Black, Color.White);
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

    [Fact] public void Hatch_DiagonalBrick() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DiagonalBrick, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_HorizontalBrick() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.HorizontalBrick, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Weave() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Weave, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Plaid() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Plaid, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Divot() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Divot, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DottedGrid() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DottedGrid, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_DottedDiamond() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.DottedDiamond, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Shingle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Shingle, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Trellis() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Trellis, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_Sphere() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.Sphere, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_SmallGrid() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.SmallGrid, Color.Black, Color.White);
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

    [Fact] public void Hatch_OutlinedDiamond() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.OutlinedDiamond, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Hatch_SolidDiamond() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var brush = new HatchBrush(HatchStyle.SolidDiamond, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

}
