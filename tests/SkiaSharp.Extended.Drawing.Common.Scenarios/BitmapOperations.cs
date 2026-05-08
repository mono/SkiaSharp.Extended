using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class BitmapOperations : ScenarioBase
{
    private static Bitmap CreateColorQuadrant(int size)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        int half = size / 2;
        g.FillRectangle(Brushes.Red, 0, 0, half, half);
        g.FillRectangle(Brushes.Blue, half, 0, half, half);
        g.FillRectangle(Brushes.Green, 0, half, half, half);
        g.FillRectangle(Brushes.Yellow, half, half, half, half);
        return bmp;
    }

    // --- RotateFlipType: all 15 non-trivial values ---

    [Fact] public void Bitmap_RotateFlip_Rotate90FlipNone() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate90FlipX() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate90FlipXY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipXY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate90FlipY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate90FlipY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate180FlipNone() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate180FlipX() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate180FlipXY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate180FlipXY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate180FlipY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate270FlipNone() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate270FlipX() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate270FlipXY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate270FlipXY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_Rotate270FlipY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.Rotate270FlipY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_RotateNoneFlipX() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_RotateNoneFlipXY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);
        g.DrawImage(bmp, 30, 30);
    });

    [Fact] public void Bitmap_RotateFlip_RotateNoneFlipY() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(40, 40);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.White);
        bg.FillRectangle(Brushes.Red, 0, 0, 20, 20);
        bg.FillRectangle(Brushes.Blue, 20, 0, 20, 20);
        bg.FillRectangle(Brushes.Green, 0, 20, 20, 20);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        g.DrawImage(bmp, 30, 30);
    });

    // --- Other bitmap operations ---

    [Fact] public void Bitmap_Clone_Region() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var src = CreateColorQuadrant(60);
        using var cloned = src.Clone(new Rectangle(0, 0, 30, 30), src.PixelFormat);
        g.DrawImage(cloned, 10, 10, 80, 80);
    });

    [Fact] public void Bitmap_GetSetPixel() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 20; y++)
            for (int x = 0; x < 20; x++)
                bmp.SetPixel(x, y, ((x + y) % 2 == 0) ? Color.Black : Color.White);
        g.DrawImage(bmp, 10, 10, 80, 80);
    });

    [Fact] public void Bitmap_Transparent() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.FillRectangle(Brushes.LightGray, 0, 0, 50, 50);
        g.FillRectangle(Brushes.LightGray, 50, 50, 50, 50);
        using var bmp = new Bitmap(40, 40, PixelFormat.Format32bppArgb);
        using var bg = Graphics.FromImage(bmp);
        bg.Clear(Color.Transparent);
        bg.FillEllipse(Brushes.Red, 0, 0, 40, 40);
        g.DrawImage(bmp, 30, 30, 40, 40);
    });
}
