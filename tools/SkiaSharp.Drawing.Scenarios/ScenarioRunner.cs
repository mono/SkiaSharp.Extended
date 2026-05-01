using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// Runs all drawing scenarios and saves the output PNGs.
/// This code compiles against either real System.Drawing.Common or SkiaSharp.Drawing.
/// </summary>
public static class ScenarioRunner
{
    public static void RunAll(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var scenarios = DrawingScenarios.GetAll();

        foreach (var (name, category, width, height, draw) in scenarios)
        {
            var categoryDir = Path.Combine(outputDir, category);
            Directory.CreateDirectory(categoryDir);

            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            draw(graphics);

            var path = Path.Combine(categoryDir, $"{name}.png");
            bitmap.Save(path, ImageFormat.Png);
        }
    }
}
