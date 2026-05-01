using SkiaSharp.Drawing.Scenarios;
using SkiaSharp.Drawing.ReferenceGenerator;

var outputDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "ReferenceImages");
Directory.CreateDirectory(outputDir);

var scenarios = AllScenarios.GetAll();
Console.WriteLine($"Generating {scenarios.Count} reference images to: {outputDir}");

foreach (var scenario in scenarios)
{
    var categoryDir = Path.Combine(outputDir, scenario.Category);
    Directory.CreateDirectory(categoryDir);

    using var surface = new SystemDrawingSurface(scenario.Width, scenario.Height);
    scenario.Draw(surface);

    var pngData = surface.SaveAsPng();
    var path = Path.Combine(categoryDir, $"{scenario.Name}.png");
    File.WriteAllBytes(path, pngData);
    Console.WriteLine($"  ✓ {scenario.Category}/{scenario.Name}.png ({pngData.Length} bytes)");
}

Console.WriteLine($"Done! Generated {scenarios.Count} reference images.");
