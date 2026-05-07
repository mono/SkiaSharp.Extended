using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class DrawStringAdvanced : ScenarioBase
{
    [Fact] public void Text_RightAligned() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 14);
        using var sf = new StringFormat { Alignment = StringAlignment.Far };
        var rect = new RectangleF(10, 10, 130, 40);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("Right", font, Brushes.Black, rect, sf);
    });

    [Fact] public void Text_CenterAligned() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 14);
        using var sf = new StringFormat { Alignment = StringAlignment.Center };
        var rect = new RectangleF(10, 10, 130, 40);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("Center", font, Brushes.Black, rect, sf);
    });

    [Fact] public void Text_VerticalCenter() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        var rect = new RectangleF(10, 10, 130, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("Centered", font, Brushes.Black, rect, sf);
    });

    [Fact] public void Text_Rotated() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.RotateTransform(45);
        using var font = new Font("Arial", 12);
        g.DrawString("Rotated", font, Brushes.Black, -25, -8);
    });

    [Fact] public void Text_Scaled() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(1.5f, 1.5f);
        using var font = new Font("Arial", 10);
        g.DrawString("Scaled", font, Brushes.Black, 5, 10);
    });

    [Fact] public void Text_MultipleFonts() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        float y = 5;
        foreach (var size in new[] { 8f, 12f, 18f, 24f }) {
            using var font = new Font("Arial", size);
            g.DrawString($"{size}pt", font, Brushes.Black, 5, y);
            y += size + 4;
        }
    });

    [Fact] public void Text_FontStyles() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        float y = 5;
        foreach (var style in new[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Bold | FontStyle.Italic }) {
            using var font = new Font("Arial", 12, style);
            g.DrawString(style.ToString(), font, Brushes.Black, 5, y);
            y += 22;
        }
    });

    [Fact] public void Text_LongWrapped() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 9);
        var rect = new RectangleF(5, 5, 140, 90);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", font, Brushes.Black, rect);
    });
}
