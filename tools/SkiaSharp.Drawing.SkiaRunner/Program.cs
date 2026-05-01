using SkiaSharp.Drawing.Scenarios;

var outputDir = args.Length > 0 ? args[0] : "SkiaImages";
Console.WriteLine($"Generating images with SkiaSharp.Drawing...");
ScenarioRunner.RunAll(outputDir);
Console.WriteLine("Done!");
