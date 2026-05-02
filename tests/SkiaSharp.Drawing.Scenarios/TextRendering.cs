using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

public class TextRendering : ScenarioBase
{
    [Fact] public void Text_Simple() => Render(200, 50, g => {
        g.Clear(Color.White);
        using var font = new Font("Arial", 20);
        g.DrawString("Hello World", font, Brushes.Black, 10, 10);
    });

    [Fact] public void Text_Bold() => Render(200, 50, g => {
        g.Clear(Color.White);
        using var font = new Font("Arial", 20, FontStyle.Bold);
        g.DrawString("Bold Text", font, Brushes.Black, 10, 10);
    });

    [Fact] public void Text_Colored() => Render(200, 50, g => {
        g.Clear(Color.White);
        using var font = new Font("Arial", 16);
        g.DrawString("Red Text", font, Brushes.Red, 10, 10);
    });

    [Fact] public void Text_InRectangle() => Render(200, 100, g => {
        g.Clear(Color.White);
        using var font = new Font("Arial", 12);
        var rect = new RectangleF(10, 10, 180, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("This text should wrap inside the rectangle bounds", font, Brushes.Black, rect);
    });
}
