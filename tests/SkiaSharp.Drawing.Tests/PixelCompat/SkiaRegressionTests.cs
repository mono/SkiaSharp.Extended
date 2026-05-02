using SkiaSharp;
using SkiaSharp.Drawing.Scenarios;
using SkiaSharp.Drawing.Tests.Infrastructure;

namespace SkiaSharp.Drawing.Tests.PixelCompat;

/// <summary>
/// Step 2: Skia regression.
/// Runs each scenario with SkiaSharp.Drawing and compares against checked-in .skia.png baselines.
/// Fails if our rendering changed without updating the checked-in Skia images.
/// </summary>
public class SkiaRegressionTests : PixelCompatibilityTestBase
{
    public static IEnumerable<object[]> AllScenarioData()
    {
        var refDir = ReferenceImagesPath;
        if (!Directory.Exists(refDir))
            yield break;

        foreach (var categoryDir in Directory.GetDirectories(refDir))
        {
            var category = Path.GetFileName(categoryDir);
            foreach (var png in Directory.GetFiles(categoryDir, "*.skia.png"))
            {
                var name = Path.GetFileNameWithoutExtension(png).Replace(".skia", "");
                yield return new object[] { name, category };
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllScenarioData))]
    public void SkiaOutput_MatchesCheckedInBaseline(string name, string category)
    {
        // Render with our SkiaSharp.Drawing
        var tmpDir = Path.Combine(TestArtifactsPath, "_skia_render");
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

        // Compare against checked-in .skia.png — should be near-exact match
        var referenceFile = Path.Combine(category, $"{name}.skia.png");
        AssertPixelCompatible(actual, referenceFile, 0.001, "Skia-Regression");
    }
}
