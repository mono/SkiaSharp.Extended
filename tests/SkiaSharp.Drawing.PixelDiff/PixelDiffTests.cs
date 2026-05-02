using SkiaSharp;
using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Drawing.PixelDiff;

/// <summary>
/// Compares .skia.png vs .gdi.png images from the reference images directory.
/// Discovers test cases by scanning the file system for matching pairs.
/// 
/// In CI, the reference directory is populated with fresh artifacts from
/// the gdi_generate and skia_generate jobs. Locally, uses checked-in images.
/// </summary>
public class PixelDiffTests
{
    private const double Tolerance_SolidFill = 0.001;
    private const double Tolerance_Stroke = 0.005;
    private const double Tolerance_AntiAliased = 0.05;

    private static string ReferenceDir =>
        Environment.GetEnvironmentVariable("REFERENCE_IMAGES_PATH")
        ?? FindReferenceImagesDir();

    private static string FindReferenceImagesDir()
    {
        // Walk up from assembly location to find the repo root
        var dir = Path.GetDirectoryName(typeof(PixelDiffTests).Assembly.Location)!;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "tests", "SkiaSharp.Drawing.Scenarios.ReferenceImages");
            if (Directory.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        return Path.Combine(AppContext.BaseDirectory, "ReferenceImages");
    }

    private static string ArtifactsDir
    {
        get
        {
            var path = Environment.GetEnvironmentVariable("TEST_ARTIFACTS_PATH")
                ?? Path.Combine(AppContext.BaseDirectory, "DiffArtifacts");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public static IEnumerable<object[]> AllPairs()
    {
        var refDir = ReferenceDir;
        if (!Directory.Exists(refDir))
        {
            yield return new object[] { "__NO_DATA__", "" };
            yield break;
        }

        bool found = false;
        foreach (var categoryDir in Directory.GetDirectories(refDir))
        {
            var category = Path.GetFileName(categoryDir);
            foreach (var gdiFile in Directory.GetFiles(categoryDir, "*.gdi.png"))
            {
                var name = Path.GetFileNameWithoutExtension(gdiFile).Replace(".gdi", "");
                var skiaFile = Path.Combine(categoryDir, $"{name}.skia.png");
                if (File.Exists(skiaFile))
                {
                    found = true;
                    yield return new object[] { name, category };
                }
            }
        }

        if (!found)
            yield return new object[] { "__NO_DATA__", "" };
    }

    [Theory]
    [MemberData(nameof(AllPairs))]
    public void SkiaMatchesGdi(string name, string category)
    {
        if (name == "__NO_DATA__")
        {
            Assert.Skip("No .gdi.png + .skia.png pairs found. Run generators first or download from CI.");
            return;
        }
        var gdiPath = Path.Combine(ReferenceDir, category, $"{name}.gdi.png");
        var skiaPath = Path.Combine(ReferenceDir, category, $"{name}.skia.png");

        using var gdi = SKBitmap.Decode(gdiPath);
        using var skia = SKBitmap.Decode(skiaPath);
        Assert.NotNull(gdi);
        Assert.NotNull(skia);

        var result = SKPixelComparer.Compare(skia, gdi);

        double tolerance = category switch
        {
            "Clear" or "Colors" => Tolerance_SolidFill,
            "Lines" or "Rectangles" or "Boundaries" => Tolerance_Stroke,
            "Ellipses" or "Arcs" or "Pies" or "Polygons" or "Composites" => Tolerance_Stroke,
            string c when c.EndsWith("AA") => Tolerance_AntiAliased,
            _ => Tolerance_Stroke,
        };

        // Save diff artifact
        var artifactDir = Path.Combine(ArtifactsDir, category);
        Directory.CreateDirectory(artifactDir);
        SaveDiff(skia, gdi, Path.Combine(artifactDir, $"{name}_diff.png"));

        if (result.ErrorPixelPercentage > tolerance)
        {
            // Also save the two inputs for easy inspection
            SavePng(skia, Path.Combine(artifactDir, $"{name}_skia.png"));
            SavePng(gdi, Path.Combine(artifactDir, $"{name}_gdi.png"));

            Assert.Fail(
                $"Pixel mismatch: {category}/{name} " +
                $"Error={result.ErrorPixelPercentage:P4} (max={tolerance:P4}) " +
                $"MAE={result.MeanAbsoluteError:F4} PSNR={result.PeakSignalToNoiseRatio:F2}dB");
        }
    }

    private static void SavePng(SKBitmap bmp, string path)
    {
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    private static void SaveDiff(SKBitmap a, SKBitmap b, string path)
    {
        int w = Math.Max(a.Width, b.Width), h = Math.Max(a.Height, b.Height);
        using var diff = new SKBitmap(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var pa = (x < a.Width && y < a.Height) ? a.GetPixel(x, y) : SKColors.Transparent;
                var pb = (x < b.Width && y < b.Height) ? b.GetPixel(x, y) : SKColors.Transparent;
                diff.SetPixel(x, y, pa == pb
                    ? new SKColor(pb.Red, pb.Green, pb.Blue, 64)
                    : new SKColor(255, 0, 0, (byte)Math.Min(255,
                        Math.Abs(pa.Red - pb.Red) + Math.Abs(pa.Green - pb.Green) + Math.Abs(pa.Blue - pb.Blue))));
            }
        SavePng(diff, path);
    }
}
