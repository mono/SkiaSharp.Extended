using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// Base class for drawing scenario tests. Each subclass is a test class and a category.
/// Each [Fact] method renders a scenario and saves the PNG.
/// The output directory is controlled by the SCENARIO_OUTPUT_PATH environment variable.
/// </summary>
public abstract class ScenarioBase
{
    private static string OutputDir =>
        Environment.GetEnvironmentVariable("SCENARIO_OUTPUT_PATH")
        ?? Path.Combine(Path.GetDirectoryName(typeof(ScenarioBase).Assembly.Location)!, "ScenarioOutput");

    /// <summary>
    /// Suffix for generated images. Set SCENARIO_SUFFIX env var to "gdi" or "skia".
    /// Defaults to "skia".
    /// </summary>
    private static string Suffix =>
        Environment.GetEnvironmentVariable("SCENARIO_SUFFIX") ?? "skia";

    private string Category => GetType().Name;

    /// <summary>
    /// Renders a drawing scenario and saves the result as a PNG.
    /// The filename is derived from the calling method name.
    /// </summary>
    protected void Render(int width, int height, Action<Graphics> draw, [CallerMemberName] string? name = null)
    {
        var categoryDir = Path.Combine(OutputDir, Category);
        Directory.CreateDirectory(categoryDir);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        draw(graphics);

        var path = Path.Combine(categoryDir, $"{name}.{Suffix}.png");
        bitmap.Save(path, ImageFormat.Png);
    }
}
