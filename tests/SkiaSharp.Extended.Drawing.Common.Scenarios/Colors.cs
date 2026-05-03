using System.Drawing;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class Colors : ScenarioBase
{
    [Fact] public void Color_AllChannels() => Render(100, 100, g => {
        g.Clear(Color.Transparent);
        using var r = new SolidBrush(Color.Red);
        using var gr = new SolidBrush(Color.Lime);
        using var b = new SolidBrush(Color.Blue);
        using var bl = new SolidBrush(Color.Black);
        g.FillRectangle(r, 0, 0, 25, 100);
        g.FillRectangle(gr, 25, 0, 25, 100);
        g.FillRectangle(b, 50, 0, 25, 100);
        g.FillRectangle(bl, 75, 0, 25, 100);
    });

    [Fact] public void Color_GrayLevels() => Render(100, 100, g => {
        g.Clear(Color.Transparent);
        for (int i = 0; i < 10; i++) {
            int gray = Math.Min(255, i * 28);
            using var brush = new SolidBrush(Color.FromArgb(255, gray, gray, gray));
            g.FillRectangle(brush, i * 10, 0, 10, 100);
        }
    });

    [Fact] public void Color_AlphaBlending() => Render(100, 100, g => {
        g.Clear(Color.White);
        for (int i = 0; i < 5; i++) {
            int alpha = 50 + i * 50;
            using var brush = new SolidBrush(Color.FromArgb(alpha, 255, 0, 0));
            g.FillRectangle(brush, i * 20, 0, 20, 100);
        }
    });
}
