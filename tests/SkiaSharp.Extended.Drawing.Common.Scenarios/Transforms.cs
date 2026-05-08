using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class Transforms : ScenarioBase
{
    [Fact] public void Transform_Translate() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.TranslateTransform(30, 30);
        g.FillRectangle(Brushes.Red, 0, 0, 40, 40);
    });

    [Fact] public void Transform_Scale() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(2, 2);
        g.FillRectangle(Brushes.Blue, 5, 5, 20, 20);
    });

    [Fact] public void Transform_Rotate() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.RotateTransform(45);
        g.FillRectangle(Brushes.Green, -20, -20, 40, 40);
    });

    [Fact] public void Transform_Combined() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.TranslateTransform(50, 50);
        g.ScaleTransform(1.5f, 1.5f);
        g.RotateTransform(30);
        g.FillRectangle(Brushes.Purple, -15, -15, 30, 30);
    });
}
