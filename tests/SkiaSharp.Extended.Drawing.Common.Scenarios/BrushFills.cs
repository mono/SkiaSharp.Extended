using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class BrushFills : ScenarioBase
{
    [Fact] public void SolidFill_Red() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void SolidFill_SemiTransparent() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var bgBrush = new SolidBrush(Color.Blue);
        g.FillRectangle(bgBrush, 5, 5, 60, 60);
        using var brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        g.FillRectangle(brush, 30, 30, 60, 60);
    });

    [Fact] public void SolidFill_DarkBlue() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.DarkBlue);
        g.FillRectangle(brush, 10, 10, 80, 80);
    });

    [Fact] public void SolidFill_Named_Colors() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        Color[] colors = { Color.Coral, Color.Teal, Color.Gold, Color.Indigo, Color.Salmon };
        for (int i = 0; i < colors.Length; i++)
        {
            using var brush = new SolidBrush(colors[i]);
            g.FillRectangle(brush, i * 30, 10, 28, 80);
        }
    });

    [Fact] public void SolidFill_AlphaLevels() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var bgBrush = new SolidBrush(Color.Black);
        g.FillRectangle(bgBrush, 0, 0, 100, 100);
        int[] alphas = { 32, 64, 128, 192, 255 };
        for (int i = 0; i < alphas.Length; i++)
        {
            using var brush = new SolidBrush(Color.FromArgb(alphas[i], 0, 200, 0));
            g.FillRectangle(brush, i * 20, 10, 18, 80);
        }
    });

    [Fact] public void Fill_CompareTypes() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        // Solid
        using var solid = new SolidBrush(Color.Red);
        g.FillRectangle(solid, 5, 10, 40, 80);
        // Hatch
        using var hatch = new HatchBrush(HatchStyle.Cross, Color.Blue, Color.LightBlue);
        g.FillRectangle(hatch, 55, 10, 40, 80);
        // Gradient
        using var grad = new LinearGradientBrush(new Point(105, 10), new Point(145, 90), Color.Green, Color.Yellow);
        g.FillRectangle(grad, 105, 10, 40, 80);
    });

    [Fact] public void SolidFill_Ellipse_Colors() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush1 = new SolidBrush(Color.FromArgb(180, 255, 0, 0));
        g.FillEllipse(brush1, 5, 5, 55, 55);
        using var brush2 = new SolidBrush(Color.FromArgb(180, 0, 0, 255));
        g.FillEllipse(brush2, 40, 40, 55, 55);
    });

    [Fact] public void SolidFill_FullAlpha_Overwrite() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush1 = new SolidBrush(Color.Red);
        g.FillRectangle(brush1, 10, 10, 80, 80);
        using var brush2 = new SolidBrush(Color.Blue);
        g.FillRectangle(brush2, 30, 30, 40, 40);
    });
}
