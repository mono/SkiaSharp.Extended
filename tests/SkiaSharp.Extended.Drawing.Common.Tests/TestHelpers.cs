using System.Drawing;
using System.Drawing.Imaging;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

internal static class TestHelpers
{
    /// <summary>
    /// Creates a small test PNG image file and returns the path.
    /// Caller is responsible for cleanup.
    /// </summary>
    internal static string CreateTestImageFile(int width = 10, int height = 10)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        using var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Blue);
        bmp.Save(path, ImageFormat.Png);
        return path;
    }

    /// <summary>
    /// Creates a test bitmap with a known pixel pattern (checkerboard).
    /// </summary>
    internal static Bitmap CreateCheckerboard(int width, int height)
    {
        var bmp = new Bitmap(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                bmp.SetPixel(x, y, (x + y) % 2 == 0 ? Color.Black : Color.White);
        return bmp;
    }

    /// <summary>
    /// Creates a solid-color bitmap.
    /// </summary>
    internal static Bitmap CreateSolidBitmap(int width, int height, Color color)
    {
        var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.Clear(color);
        return bmp;
    }

    /// <summary>
    /// Asserts that a pixel at (x,y) matches the expected color (ignoring minor rounding in premultiplied alpha).
    /// </summary>
    internal static void AssertPixelColor(Bitmap bmp, int x, int y, Color expected, int tolerance = 2)
    {
        var actual = bmp.GetPixel(x, y);
        Assert.True(
            Math.Abs(actual.R - expected.R) <= tolerance &&
            Math.Abs(actual.G - expected.G) <= tolerance &&
            Math.Abs(actual.B - expected.B) <= tolerance &&
            Math.Abs(actual.A - expected.A) <= tolerance,
            $"Pixel ({x},{y}): expected ({expected.A},{expected.R},{expected.G},{expected.B}) " +
            $"but got ({actual.A},{actual.R},{actual.G},{actual.B})");
    }
}
