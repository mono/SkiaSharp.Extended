using SkiaSharp.Drawing.Scenarios;

var outputDir = args.Length > 0 ? args[0] : "ReferenceImages";
Console.WriteLine($"Generating reference images with real System.Drawing (GDI+)...");
ScenarioRunner.RunAll(outputDir);
Console.WriteLine("Done!");
