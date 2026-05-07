using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class LineCaps : ScenarioBase
{
    [Fact] public void LineCap_Flat() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.Flat, EndCap = LineCap.Flat };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_Square() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.Square, EndCap = LineCap.Square };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_Round() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_Triangle() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_NoAnchor() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.NoAnchor, EndCap = LineCap.NoAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_SquareAnchor() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.SquareAnchor, EndCap = LineCap.SquareAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_RoundAnchor() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.RoundAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_DiamondAnchor() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.DiamondAnchor, EndCap = LineCap.DiamondAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_ArrowAnchor() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_AnchorMask() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.AnchorMask, EndCap = LineCap.AnchorMask };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    // --- Mixed cap scenarios ---

    [Fact] public void LineCap_Mixed_RoundArrow() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });

    [Fact] public void LineCap_Mixed_SquareDiamond() => Render(150, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 6) { StartCap = LineCap.Square, EndCap = LineCap.DiamondAnchor };
        g.DrawLine(pen, 20, 25, 130, 25);
    });
}
