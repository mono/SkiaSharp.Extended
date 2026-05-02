using SkiaSharp;
using SkiaSharp.Drawing.Tests.Infrastructure;

namespace SkiaSharp.Drawing.Tests.PixelCompat;

/// <summary>
/// Step 1: GDI reference stability.
/// Compares freshly generated GDI+ images against checked-in .gdi.png baselines.
/// Fails if a scenario was changed without updating the checked-in GDI images.
/// </summary>
public class GdiReferenceStabilityTests : PixelCompatibilityTestBase
{
    /// <summary>
    /// Path to freshly generated GDI+ images (set by CI via env var).
    /// </summary>
    private static string? FreshGdiPath =>
        Environment.GetEnvironmentVariable("FRESH_GDI_IMAGES_PATH");

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
    public void GdiOutput_MatchesCheckedInBaseline(string name, string category)
    {
        if (name == "__no_data__")
            Assert.Skip("No .gdi.png baselines checked in yet.");

        if (FreshGdiPath == null)
            Assert.Skip("FRESH_GDI_IMAGES_PATH not set — this test runs on CI only.");

        var freshPath = Path.Combine(FreshGdiPath, category, $"{name}.gdi.png");
        if (!File.Exists(freshPath))
            Assert.Skip($"Fresh GDI image not found: {freshPath}");

        using var fresh = SKBitmap.Decode(freshPath);
        Assert.NotNull(fresh);

        // Compare fresh GDI output against checked-in .gdi.png baseline.
        // Should be near-exact match (same platform rendering the same code).
        var referenceFile = Path.Combine(category, $"{name}.gdi.png");
        AssertPixelCompatible(fresh, referenceFile, 0.001, "GDI-Stability");
    }
}
