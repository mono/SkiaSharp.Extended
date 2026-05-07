using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class StringFormatting : ScenarioBase
{
    [Fact] public void StringAlign_Center() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat { Alignment = StringAlignment.Center };
        var rect = new RectangleF(10, 10, 130, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("Centered", font, Brushes.Black, rect, sf);
    });

    [Fact] public void StringAlign_Far() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat { Alignment = StringAlignment.Far };
        var rect = new RectangleF(10, 10, 130, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("Right", font, Brushes.Black, rect, sf);
    });

    [Fact] public void StringLineAlign_Center() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat { LineAlignment = StringAlignment.Center };
        var rect = new RectangleF(10, 10, 130, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("VCenter", font, Brushes.Black, rect, sf);
    });

    [Fact] public void StringTrimming_Ellipsis() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
        var rect = new RectangleF(10, 10, 130, 30);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("This is a very long string that should be trimmed with ellipsis", font, Brushes.Black, rect, sf);
    });

    [Fact] public void StringTrimming_Word() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat { Trimming = StringTrimming.Word };
        var rect = new RectangleF(10, 10, 130, 30);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("This is a very long string that should be trimmed by word", font, Brushes.Black, rect, sf);
    });

    [Fact] public void StringFormat_RightToLeft() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat { FormatFlags = StringFormatFlags.DirectionRightToLeft };
        var rect = new RectangleF(10, 10, 130, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("RTL Text", font, Brushes.Black, rect, sf);
    });

    [Fact] public void StringMultiline() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 10);
        var rect = new RectangleF(10, 10, 130, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("This is a long paragraph of text that should automatically wrap within the bounds of the rectangle.", font, Brushes.Black, rect);
    });

    [Fact] public void StringMeasure_Comparison() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var font = new Font("Arial", 14);
        var text = "Measure Me";
        var size = g.MeasureString(text, font);
        var x = 10f;
        var y = 30f;
        g.DrawString(text, font, Brushes.Black, x, y);
        using var pen = new Pen(Color.Red, 1);
        g.DrawRectangle(pen, x, y, size.Width, size.Height);
    });
}
