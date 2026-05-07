using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class SaveRestore : ScenarioBase
{
    [Fact] public void SaveRestore_Transform() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var state = g.Save();
        g.TranslateTransform(50, 50);
        g.RotateTransform(45);
        g.FillRectangle(Brushes.Red, -15, -15, 30, 30);
        g.Restore(state);
        // After restore, transform is reset
        g.FillRectangle(Brushes.Blue, 5, 5, 30, 30);
    });

    [Fact] public void SaveRestore_Clip() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        var state = g.Save();
        g.SetClip(new Rectangle(20, 20, 30, 30));
        g.FillRectangle(Brushes.Red, 0, 0, 100, 100);
        g.Restore(state);
        // After restore, clip is removed — this should fill the full area
        g.FillRectangle(Brushes.Blue, 60, 60, 30, 30);
    });

    [Fact] public void SaveRestore_Nested() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        // Level 0: draw at origin
        g.FillRectangle(Brushes.Red, 5, 5, 20, 20);
        var state1 = g.Save();
        g.TranslateTransform(25, 0);
        g.FillRectangle(Brushes.Green, 5, 5, 20, 20);
        var state2 = g.Save();
        g.TranslateTransform(25, 0);
        g.FillRectangle(Brushes.Blue, 5, 5, 20, 20);
        g.Restore(state2);
        // Back to state1 translate
        g.FillRectangle(Brushes.Yellow, 5, 35, 20, 20);
        g.Restore(state1);
        // Back to original
        g.FillRectangle(Brushes.Purple, 5, 65, 20, 20);
    });

    [Fact] public void SaveRestore_SmoothingMode() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        // Draw a circle without AA
        g.FillEllipse(Brushes.Red, 5, 5, 40, 40);
        var state = g.Save();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.FillEllipse(Brushes.Blue, 50, 5, 40, 40);
        g.Restore(state);
        // Should be back to None
        g.FillEllipse(Brushes.Green, 25, 50, 40, 40);
    });

    [Fact] public void SaveRestore_Pen() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var thinPen = new Pen(Color.Black, 1);
        using var thickPen = new Pen(Color.Red, 5);
        g.DrawRectangle(thinPen, 10, 10, 30, 30);
        var state = g.Save();
        g.TranslateTransform(40, 0);
        g.DrawRectangle(thickPen, 10, 10, 30, 30);
        g.Restore(state);
        g.DrawRectangle(thinPen, 10, 55, 80, 30);
    });
}
