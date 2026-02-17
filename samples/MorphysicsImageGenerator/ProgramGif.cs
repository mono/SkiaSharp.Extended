namespace MorphysicsImageGenerator;

class ProgramGif
{
    static void Main(string[] args)
    {
        Console.WriteLine("Morphysics Animated GIF Generator");
        Console.WriteLine("==================================\n");
        
        var outputDir = Path.Combine(Environment.CurrentDirectory, "output", "gifs");
        Directory.CreateDirectory(outputDir);
        
        try
        {
            // Generate animated GIFs
            GifGeneratorImageSharp.GenerateParticlesGravityGif(Path.Combine(outputDir, "particles-gravity.gif"));
            GifGeneratorImageSharp.GenerateMorphingGif(Path.Combine(outputDir, "morphing-square-circle.gif"));
            GifGeneratorImageSharp.GenerateAttractorGif(Path.Combine(outputDir, "attractor-demo.gif"));
            
            Console.WriteLine($"\n✅ All animated GIFs generated successfully!");
            Console.WriteLine($"📁 Output directory: {outputDir}");
            
            // List generated files
            var files = Directory.GetFiles(outputDir, "*.gif");
            Console.WriteLine($"\n📊 Generated {files.Length} GIF(s):");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                Console.WriteLine($"  • {Path.GetFileName(file)} ({info.Length / 1024}KB)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
