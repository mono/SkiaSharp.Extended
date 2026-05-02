using SkiaSharp;
using SkiaSharp.Drawing.Scenarios;
using SkiaSharp.Drawing.Tests.Infrastructure;
using SkiaSharp.Extended;

namespace SkiaSharp.Drawing.Tests.PixelCompat;

/// <summary>
/// Step 3: Skia == GDI comparison.
/// Compares SkiaSharp.Drawing output against GDI+ reference images.
/// This is the actual pixel compatibility check with tolerance tiers.
/// </summary>
public class PixelCompatibilityComparisonTests : PixelCompatibilityTestBase
{
    public static IEnumerable<object[]> AllScenarioData()
    {
        var refDir = ReferenceImagesPath;
        if (!Directory.Exists(refDir))
        {
            yield return new object[] { "__no_data__", "__skip__" };
            yield break;
        }

        var hasData = false;
        foreach (var categoryDir in Directory.GetDirectories(refDir))
        {
            var category = Path.GetFileName(categoryDir);
            foreach (var png in Directory.GetFiles(categoryDir, "*.gdi.png"))
            {
                hasData = true;
                var name = Path.GetFileNameWithoutExtension(png).Replace(".gdi", "");
                yield return new object[] { name, category };
            }
        }

        if (!hasData)
            yield return new object[] { "__no_data__", "__skip__" };
    }

    [Theory]
    [MemberData(nameof(AllScenarioData))]
    public void SkiaOutput_MatchesGdiReference(string name, string category)
    {
        if (name == "__no_data__")
            Assert.Skip("No .gdi.png baselines checked in yet.");

        var gdiReferencePath = Path.Combine(ReferenceImagesPath, category, $"{name}.gdi.png");
        if (!File.Exists(gdiReferencePath))
            Assert.Skip($"No GDI reference: {gdiReferencePath}. Run ReferenceGenerator on Windows CI first.");

        // Render with our SkiaSharp.Drawing
        var tmpDir = Path.Combine(TestArtifactsPath, "_compat_render");
        Environment.SetEnvironmentVariable("SCENARIO_OUTPUT_PATH", tmpDir);
        Environment.SetEnvironmentVariable("SCENARIO_SUFFIX", "skia");

        var scenarioType = typeof(ScenarioBase).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name == category && t.IsSubclassOf(typeof(ScenarioBase)));
        Assert.NotNull(scenarioType);

        var instance = Activator.CreateInstance(scenarioType!)!;
        var method = scenarioType!.GetMethod(name);
        Assert.NotNull(method);
        method!.Invoke(instance, null);

        var actualPath = Path.Combine(tmpDir, category, $"{name}.skia.png");
        Assert.True(File.Exists(actualPath), $"Scenario did not produce output: {actualPath}");

        using var actual = SKBitmap.Decode(actualPath);
        Assert.NotNull(actual);

        double tolerance = category switch
        {
            "Clear" or "Colors" => Tolerance_SolidFill,
            "Lines" or "Rectangles" or "Boundaries" => Tolerance_Stroke,
            "Ellipses" or "Arcs" or "Pies" or "Polygons" or "Composites" => Tolerance_Stroke,
            "LinesAA" or "EllipsesAA" or "ArcsAA" or "PiesAA" or "PolygonsAA" or "CompositesAA" => Tolerance_AntiAliased,
            _ => Tolerance_Stroke,
        };

        // Load GDI reference and compare
        using var gdiRef = SKBitmap.Decode(gdiReferencePath);
        Assert.NotNull(gdiRef);

        var result = SKPixelComparer.Compare(actual, gdiRef);

        // Save artifacts
        var artifactDir = Path.Combine(TestArtifactsPath, category);
        Directory.CreateDirectory(artifactDir);
        SaveBitmap(actual, Path.Combine(artifactDir, $"{name}_skia.png"));
        SaveBitmap(gdiRef, Path.Combine(artifactDir, $"{name}_gdi.png"));
        SaveDiffImage(actual, gdiRef, Path.Combine(artifactDir, $"{name}_diff.png"));

        if (result.ErrorPixelPercentage > tolerance)
        {
            Assert.Fail(
                $"Pixel compatibility failed for '{name}' [{category}]" +
                $": ErrorPixelPercentage={result.ErrorPixelPercentage:P4}" +
                $" (max={tolerance:P4})," +
                $" MAE={result.MeanAbsoluteError:F4}," +
                $" PSNR={result.PeakSignalToNoiseRatio:F2}dB" +
                $" — artifacts in {artifactDir}");
        }
    }

    private static void SaveBitmap(SKBitmap bitmap, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    private static void SaveDiffImage(SKBitmap actual, SKBitmap reference, string path)
    {
        var width = Math.Max(actual.Width, reference.Width);
        var height = Math.Max(actual.Height, reference.Height);

        using var diff = new SKBitmap(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var a = (x < actual.Width && y < actual.Height) ? actual.GetPixel(x, y) : SKColors.Transparent;
                var r = (x < reference.Width && y < reference.Height) ? reference.GetPixel(x, y) : SKColors.Transparent;

                if (a == r)
                {
                    diff.SetPixel(x, y, new SKColor(r.Red, r.Green, r.Blue, 64));
                }
                else
                {
                    var errorMagnitude = (byte)Math.Min(255,
                        Math.Abs(a.Red - r.Red) + Math.Abs(a.Green - r.Green) + Math.Abs(a.Blue - r.Blue));
                    diff.SetPixel(x, y, new SKColor(255, 0, 0, errorMagnitude));
                }
            }
        }
        SaveBitmap(diff, path);
    }
}
