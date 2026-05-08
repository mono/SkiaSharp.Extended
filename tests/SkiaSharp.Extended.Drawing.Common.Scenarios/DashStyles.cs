using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class DashStyles : ScenarioBase
{
    // --- Solid ---

    [Fact] public void DashStyle_Solid_Width1() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1) { DashStyle = DashStyle.Solid };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Solid_Width2() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.Solid };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Solid_Width3() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashStyle = DashStyle.Solid };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    // --- Dash ---

    [Fact] public void DashStyle_Dash_Width1() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Dash_Width2() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Dash_Width3() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    // --- Dot ---

    [Fact] public void DashStyle_Dot_Width1() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1) { DashStyle = DashStyle.Dot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Dot_Width2() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Dot_Width3() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashStyle = DashStyle.Dot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    // --- DashDot ---

    [Fact] public void DashStyle_DashDot_Width1() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1) { DashStyle = DashStyle.DashDot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_DashDot_Width2() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.DashDot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_DashDot_Width3() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashStyle = DashStyle.DashDot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    // --- DashDotDot ---

    [Fact] public void DashStyle_DashDotDot_Width1() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1) { DashStyle = DashStyle.DashDotDot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_DashDotDot_Width2() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.DashDotDot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_DashDotDot_Width3() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashStyle = DashStyle.DashDotDot };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    // --- Custom ---

    [Fact] public void DashStyle_Custom_Width1() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1) { DashPattern = new float[] { 5, 2, 1, 2 } };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Custom_Width2() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 2) { DashPattern = new float[] { 5, 2, 1, 2 } };
        g.DrawLine(pen, 5, 15, 145, 15);
    });

    [Fact] public void DashStyle_Custom_Width3() => Render(150, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 3) { DashPattern = new float[] { 5, 2, 1, 2 } };
        g.DrawLine(pen, 5, 15, 145, 15);
    });
}
