using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public class ImageOperations : ScenarioBase
{
    /// <summary>Creates a small test image with a colored pattern.</summary>
    private static Bitmap CreateTestImage(int width, int height)
    {
        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var redBrush = new SolidBrush(Color.Red);
        g.FillRectangle(redBrush, 0, 0, width / 2, height / 2);
        using var blueBrush = new SolidBrush(Color.Blue);
        g.FillRectangle(blueBrush, width / 2, 0, width / 2, height / 2);
        using var greenBrush = new SolidBrush(Color.Green);
        g.FillRectangle(greenBrush, 0, height / 2, width / 2, height / 2);
        using var yellowBrush = new SolidBrush(Color.Yellow);
        g.FillRectangle(yellowBrush, width / 2, height / 2, width / 2, height / 2);
        return bmp;
    }

    [Fact] public void DrawImage_Scaled() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(20, 20);
        g.DrawImage(src, 10, 10, 80, 80);
    });

    [Fact] public void DrawImage_Stretched() => Render(150, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(30, 30);
        g.DrawImage(src, 5, 5, 140, 90);
    });

    [Fact] public void DrawImage_InRect() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(40, 40);
        g.DrawImage(src, new Rectangle(20, 20, 60, 60));
    });

    [Fact] public void DrawImage_SrcRect_To_DestRect() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(40, 40);
        // Draw only the top-left quadrant (red) into the full canvas
        g.DrawImage(src, new Rectangle(10, 10, 80, 80), new Rectangle(0, 0, 20, 20), GraphicsUnit.Pixel);
    });

    [Fact] public void DrawImage_Multiple() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(20, 20);
        g.DrawImage(src, 5, 5, 40, 40);
        g.DrawImage(src, 55, 5, 40, 40);
        g.DrawImage(src, 5, 55, 40, 40);
        g.DrawImage(src, 55, 55, 40, 40);
    });

    [Fact] public void DrawImage_OnGradient() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var gradBrush = new LinearGradientBrush(new Point(0, 0), new Point(99, 99), Color.LightGray, Color.DarkGray);
        g.FillRectangle(gradBrush, 0, 0, 100, 100);
        using var src = CreateTestImage(30, 30);
        g.DrawImage(src, 35, 35, 30, 30);
    });

    [Fact] public void DrawImage_SmallToLarge() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(10, 10);
        g.DrawImage(src, 0, 0, 100, 100);
    });

    [Fact] public void DrawImage_LargeToSmall() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var src = CreateTestImage(80, 80);
        g.DrawImage(src, 25, 25, 50, 50);
    });
}
