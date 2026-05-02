using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// Base class for drawing scenarios. Each subclass is a category (folder name).
/// Each public method is a scenario (file name). Call Render() with a drawing action
/// and it saves to the output directory.
/// </summary>
public abstract class ScenarioBase
{
    private readonly string _outputDir;
    private readonly string _category;

    protected ScenarioBase(string outputDir)
    {
        _outputDir = outputDir;
        _category = GetType().Name;
    }

    /// <summary>
    /// Renders a scenario. The caller name becomes the PNG filename.
    /// </summary>
    protected void Render(int width, int height, Action<Graphics> draw, [CallerMemberName] string? name = null)
    {
        var categoryDir = Path.Combine(_outputDir, _category);
        Directory.CreateDirectory(categoryDir);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        draw(graphics);

        var path = Path.Combine(categoryDir, $"{name}.png");
        bitmap.Save(path, ImageFormat.Png);
    }

    /// <summary>
    /// Runs all public void methods on this instance as scenarios.
    /// </summary>
    public void RunAll()
    {
        var methods = GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            if (method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.Name != "RunAll")
            {
                method.Invoke(this, null);
            }
        }
    }
}

/// <summary>
/// Registry of all scenario classes for runners.
/// </summary>
public static class AllScenarios
{
    public static ScenarioBase[] Create(string outputDir) => new ScenarioBase[]
    {
        new Clear(outputDir),
        new Lines(outputDir),
        new LinesAA(outputDir),
        new Rectangles(outputDir),
        new Ellipses(outputDir),
        new EllipsesAA(outputDir),
        new Arcs(outputDir),
        new ArcsAA(outputDir),
        new Pies(outputDir),
        new PiesAA(outputDir),
        new Polygons(outputDir),
        new PolygonsAA(outputDir),
        new Composites(outputDir),
        new CompositesAA(outputDir),
        new Colors(outputDir),
        new Boundaries(outputDir),
    };

    public static void RunAll(string outputDir)
    {
        foreach (var scenario in Create(outputDir))
            scenario.RunAll();
    }

    /// <summary>
    /// Returns all (Category, Name) pairs for test discovery.
    /// </summary>
    public static IEnumerable<(string Category, string Name)> Enumerate()
    {
        // Use a temp dir to discover scenario names via reflection
        foreach (var scenario in Create(Path.GetTempPath()))
        {
            var category = scenario.GetType().Name;
            var methods = scenario.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                if (method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.Name != "RunAll")
                    yield return (category, method.Name);
            }
        }
    }
}
