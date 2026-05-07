using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class PixelOffsetModes : ScenarioBase
{
    [Fact] public void PixelOffset_Default() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.PixelOffsetMode = PixelOffsetMode.Default;
        g.FillRectangle(Brushes.Blue, 10, 10, 30, 30);
        g.FillEllipse(Brushes.Red, 50, 10, 40, 40);
        g.DrawLine(Pens.Black, 10, 70, 90, 70);
    });

    [Fact] public void PixelOffset_Half() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.FillRectangle(Brushes.Blue, 10, 10, 30, 30);
        g.FillEllipse(Brushes.Red, 50, 10, 40, 40);
        g.DrawLine(Pens.Black, 10, 70, 90, 70);
    });

    [Fact] public void PixelOffset_HighQuality() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.FillRectangle(Brushes.Blue, 10, 10, 30, 30);
        g.FillEllipse(Brushes.Red, 50, 10, 40, 40);
        g.DrawLine(Pens.Black, 10, 70, 90, 70);
    });

    [Fact] public void Compositing_SourceOver() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.CompositingMode = CompositingMode.SourceOver;
        g.FillRectangle(Brushes.Blue, 10, 10, 50, 50);
        using var semiBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        g.FillRectangle(semiBrush, 30, 30, 50, 50);
    });

    [Fact] public void Compositing_SourceCopy() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.FillRectangle(Brushes.Blue, 10, 10, 50, 50);
        g.CompositingMode = CompositingMode.SourceCopy;
        using var semiBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        g.FillRectangle(semiBrush, 30, 30, 50, 50);
    });

    [Fact] public void InterpolationMode_NearestNeighbor() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        using var src = CreateSmallImage();
        g.DrawImage(src, 5, 5, 90, 90);
    });

    [Fact] public void InterpolationMode_Bicubic() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        using var src = CreateSmallImage();
        g.DrawImage(src, 5, 5, 90, 90);
    });

    private static Bitmap CreateSmallImage()
    {
        var bmp = new Bitmap(8, 8, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.FillRectangle(Brushes.Red, 0, 0, 4, 4);
        g.FillRectangle(Brushes.Blue, 4, 0, 4, 4);
        g.FillRectangle(Brushes.Green, 0, 4, 4, 4);
        g.FillRectangle(Brushes.Yellow, 4, 4, 4, 4);
        return bmp;
    }
}
