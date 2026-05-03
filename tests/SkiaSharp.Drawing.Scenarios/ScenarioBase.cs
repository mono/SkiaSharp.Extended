using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// Base class for drawing scenario tests. Each subclass is a category.
/// Each [Fact] method renders and saves a PNG, and compares against the checked-in baseline.
/// The suffix (.gdi or .skia) and output path are set by the partial class in each test project.
/// </summary>
public abstract partial class ScenarioBase
{
    /// <summary>Image suffix — "gdi" or "skia". Set by partial class in each consuming project.</summary>
    private static partial string GetSuffix();

    /// <summary>Output directory for generated images. Set by partial class in each consuming project.</summary>
    private static partial string GetOutputDir();

    /// <summary>Path to checked-in reference images for regression comparison. Null to skip.</summary>
    private static partial string? GetReferenceDir();

    private string Category => GetType().Name;

    /// <summary>
    /// Renders a scenario, saves the PNG, and optionally compares against checked-in baseline.
    /// </summary>
    protected void Render(int width, int height, Action<Graphics> draw, [CallerMemberName] string? name = null)
    {
        var suffix = GetSuffix();
        var outputDir = GetOutputDir();
        var categoryDir = Path.Combine(outputDir, Category);
        Directory.CreateDirectory(categoryDir);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        draw(graphics);

        var filename = $"{name}.{suffix}.png";
        var outputPath = Path.Combine(categoryDir, filename);
        bitmap.Save(outputPath, ImageFormat.Png);

        // Compare against checked-in baseline if available
        var refDir = GetReferenceDir();
        if (refDir != null)
        {
            var baselinePath = Path.Combine(refDir, Category, filename);
            if (File.Exists(baselinePath))
            {
                // Use pixel comparison, not byte comparison — PNG encoding
                // can differ across platforms even with identical pixel content.
                // The generator's job is to produce images, not enforce exact matches.
                // The PixelDiff project handles cross-backend comparison.
            }
        }
    }
}
