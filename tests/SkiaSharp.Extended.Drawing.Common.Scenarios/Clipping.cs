using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class Clipping : ScenarioBase
{
    [Fact] public void Clip_Rectangle() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.SetClip(new Rectangle(20, 20, 60, 60));
        g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
    });

    [Fact] public void Clip_Ellipse_Path() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddEllipse(10, 10, 80, 80);
        g.SetClip(path);
        g.FillRectangle(Brushes.Blue, 0, 0, 100, 100);
    });

    [Fact] public void Clip_Exclude() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.SetClip(new Rectangle(0, 0, 100, 100));
        g.ExcludeClip(new Rectangle(30, 30, 40, 40));
        g.FillRectangle(Brushes.Green, 0, 0, 100, 100);
    });
}
