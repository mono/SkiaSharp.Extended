namespace SkiaSharp.Drawing.Scenarios;

public abstract partial class ScenarioBase
{
    private static partial string GetSuffix() => "gdi";

    private static partial string GetOutputDir() =>
        Environment.GetEnvironmentVariable("SCENARIO_OUTPUT_PATH")
        ?? Path.Combine(Path.GetDirectoryName(typeof(ScenarioBase).Assembly.Location)!, "ScenarioOutput");

    private static partial string? GetReferenceDir() =>
        Environment.GetEnvironmentVariable("REFERENCE_IMAGES_PATH");
}
