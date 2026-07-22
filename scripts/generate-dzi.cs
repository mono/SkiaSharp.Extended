#!/usr/bin/env dotnet-script
// Deep Zoom Image (DZI) tile generator
// Usage: dotnet run scripts/generate-dzi.cs -- <input-image> <output-dir> [options]
//
// Options:
//   --tile-size <n>   Tile size in pixels (default: 256)
//   --overlap <n>     Tile overlap in pixels (default: 1)
//   --format <ext>    Output tile format: png or jpg (default: png)
//   --quality <n>     JPEG quality 1-100, only used for jpg (default: 90)
//
// Example:
//   dotnet run scripts/generate-dzi.cs -- myimage.png output/ --tile-size 256 --overlap 1

#:package SkiaSharp@3.119.1

using System;
using System.IO;
using SkiaSharp;

// ── Parse arguments ──────────────────────────────────────────────────────────

string? inputPath = null;
string? outputDir = null;
int tileSize = 256;
int overlap = 1;
string format = "png";
int jpegQuality = 90;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--tile-size": tileSize   = int.Parse(args[++i]); break;
        case "--overlap":   overlap    = int.Parse(args[++i]); break;
        case "--format":    format     = args[++i].ToLowerInvariant(); break;
        case "--quality":   jpegQuality = int.Parse(args[++i]); break;
        default:
            if (inputPath is null)  inputPath  = args[i];
            else if (outputDir is null) outputDir = args[i];
            else { Console.Error.WriteLine($"Unknown argument: {args[i]}"); Environment.Exit(1); }
            break;
    }
}

if (inputPath is null || outputDir is null)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/generate-dzi.cs -- <input-image> <output-dir> [--tile-size 256] [--overlap 1] [--format png|jpg] [--quality 90]");
    Environment.Exit(1);
}

if (format != "png" && format != "jpg")
{
    Console.Error.WriteLine("--format must be 'png' or 'jpg'");
    Environment.Exit(1);
}

// ── Load source image ─────────────────────────────────────────────────────────

Console.WriteLine($"Loading {inputPath}…");
using var srcStream = File.OpenRead(inputPath);
using var srcBitmap = SKBitmap.Decode(srcStream);
if (srcBitmap is null)
{
    Console.Error.WriteLine($"Failed to decode image: {inputPath}");
    Environment.Exit(1);
}

int imageWidth  = srcBitmap.Width;
int imageHeight = srcBitmap.Height;
int maxLevel    = (int)Math.Ceiling(Math.Log2(Math.Max(imageWidth, imageHeight)));

Console.WriteLine($"Image: {imageWidth}×{imageHeight}, max level: {maxLevel}, tile size: {tileSize}, overlap: {overlap}, format: {format}");

// ── Prepare output directories ────────────────────────────────────────────────

string baseName  = Path.GetFileNameWithoutExtension(outputDir.TrimEnd('/', '\\'));
// If outputDir itself ends with the image name, use it directly; otherwise create a subdir.
string dziPath   = outputDir.EndsWith(".dzi", StringComparison.OrdinalIgnoreCase)
    ? outputDir
    : Path.Combine(outputDir, baseName + ".dzi");

// Derive tile directory from DZI path: same name with _files suffix
string tileRoot  = Path.Combine(Path.GetDirectoryName(dziPath)!, Path.GetFileNameWithoutExtension(dziPath) + "_files");

Directory.CreateDirectory(Path.GetDirectoryName(dziPath)!);

// ── Generate tiles level by level ─────────────────────────────────────────────

var encodeFormat  = format == "jpg" ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png;
var fileExtension = format == "jpg" ? "jpg" : "png";

for (int level = 0; level <= maxLevel; level++)
{
    int scale      = maxLevel - level;
    int levelWidth  = Math.Max(1, (int)Math.Ceiling(imageWidth  / Math.Pow(2, scale)));
    int levelHeight = Math.Max(1, (int)Math.Ceiling(imageHeight / Math.Pow(2, scale)));

    string levelDir = Path.Combine(tileRoot, level.ToString());
    Directory.CreateDirectory(levelDir);

    // Scale source image to level dimensions
    using var levelBitmap = new SKBitmap(levelWidth, levelHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
    using (var canvas = new SKCanvas(levelBitmap))
    {
        canvas.Clear(SKColors.Transparent);
        var dest = new SKRect(0, 0, levelWidth, levelHeight);
        var src  = new SKRect(0, 0, imageWidth, imageHeight);
        using var paint = new SKPaint { IsAntialias = true };
        canvas.DrawBitmap(srcBitmap, src, dest, paint);
    }

    int cols = (int)Math.Ceiling((double)levelWidth  / tileSize);
    int rows = (int)Math.Ceiling((double)levelHeight / tileSize);
    int tileCount = cols * rows;

    Console.Write($"  Level {level,2}: {levelWidth,6}×{levelHeight,-6}  {cols}×{rows} tiles… ");

    for (int col = 0; col < cols; col++)
    {
        for (int row = 0; row < rows; row++)
        {
            // Tile origin in the level image (without overlap)
            int tileX = col * tileSize;
            int tileY = row * tileSize;

            // Extend by overlap on each side (clamped to image bounds)
            int srcX = Math.Max(0, tileX - overlap);
            int srcY = Math.Max(0, tileY - overlap);
            int srcRight  = Math.Min(levelWidth,  tileX + tileSize + overlap);
            int srcBottom = Math.Min(levelHeight, tileY + tileSize + overlap);
            int srcW = srcRight  - srcX;
            int srcH = srcBottom - srcY;

            // Extract the tile region
            using var tileBitmap = new SKBitmap(srcW, srcH, SKColorType.Rgba8888, SKAlphaType.Premul);
            levelBitmap.ExtractSubset(tileBitmap, new SKRectI(srcX, srcY, srcRight, srcBottom));

            string tilePath = Path.Combine(levelDir, $"{col}_{row}.{fileExtension}");
            using var fs = File.Create(tilePath);
            tileBitmap.Encode(fs, encodeFormat, jpegQuality);
        }
    }

    Console.WriteLine("done");
}

// ── Write DZI descriptor ──────────────────────────────────────────────────────

var dziContent = $"""
    <?xml version="1.0" encoding="UTF-8"?>
    <Image xmlns="http://schemas.microsoft.com/deepzoom/2008"
           Format="{fileExtension}" Overlap="{overlap}" TileSize="{tileSize}">
      <Size Width="{imageWidth}" Height="{imageHeight}"/>
    </Image>
    """;

File.WriteAllText(dziPath, dziContent, System.Text.Encoding.UTF8);

Console.WriteLine($"\nWrote DZI: {dziPath}");
Console.WriteLine($"Tiles:     {tileRoot}");
Console.WriteLine("Done.");
