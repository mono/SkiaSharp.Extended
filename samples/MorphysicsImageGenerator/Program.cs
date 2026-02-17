using SkiaSharp;

namespace MorphysicsImageGenerator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Morphysics Image Generator");
        Console.WriteLine("Generating sample images...");
        
        var outputDir = Path.Combine(Environment.CurrentDirectory, "output");
        Directory.CreateDirectory(outputDir);
        
        GenerateImages(outputDir);
        
        Console.WriteLine($"Images generated in: {outputDir}");
    }
    
    static void GenerateImages(string outputDir)
    {
        // Generate morphing progression
        using var surface = SKSurface.Create(new SKImageInfo(800, 300));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        
        using var paint = new SKPaint
        {
            Color = SKColors.DeepPink,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        
        // Draw squares showing morphing concept
        canvas.DrawRect(50, 100, 100, 100, paint);
        canvas.DrawOval(400 - 50, 100, 100, 100, paint);
        
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        using var stream = File.OpenWrite(Path.Combine(outputDir, "morphing-demo.png"));
        data.SaveTo(stream);
        
        Console.WriteLine("✓ Created: morphing-demo.png");
    }
}
