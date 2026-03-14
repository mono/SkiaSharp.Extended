#!/usr/bin/env dotnet-script
// Test-grid DZI generator
//
// Generates a 32768×32768 Deep Zoom Image with 16 levels (0-15) directly from
// a procedural grid pattern — no source image needed, so memory stays tiny.
//
// Each color block spans 4 tiles × 4 tiles at the highest (full-resolution) level.
// Colors are light pastels. No borders. Each tile shows its DZI level in the top-left corner.
//
// Usage:
//   dotnet run scripts/generate-testgrid.cs
//   dotnet run scripts/generate-testgrid.cs -- <output-dir>    (default: resources/collections/testgrid)
//
// The output directory will contain:
//   <name>.dzi              — DZI manifest
//   <name>_files/<level>/   — tile images (PNG, 256×256 + 1px overlap)

#:package SkiaSharp@3.119.1

using SkiaSharp;

// ── Configuration ─────────────────────────────────────────────────────────────

const int ImageSize    = 32768;   // 2^15  → maxLevel = 15, 16 levels total (0-15)
const int TileSize     = 256;
const int Overlap      = 1;
// 4×4 tiles per color block at the full-res level (level 15).
// Block size in image pixels = 4 × TileSize = 1024px
const int BlockTiles   = 4;
const int BlockPx      = BlockTiles * TileSize;   // 1024 px at full resolution
// Number of color blocks across the full image
const int GridDivs     = ImageSize / BlockPx;     // 32768/1024 = 32 blocks across

string outputDir = args.Length > 0 ? args[0] : "resources/collections/testgrid";

// ── DZI manifest ──────────────────────────────────────────────────────────────

int maxLevel = (int)Math.Ceiling(Math.Log2(ImageSize));   // = 15

string baseName  = Path.GetFileName(outputDir.TrimEnd('/', '\\'));
string dziPath   = Path.Combine(outputDir, baseName + ".dzi");
string tileRoot  = Path.Combine(outputDir, baseName + "_files");

Directory.CreateDirectory(outputDir);

File.WriteAllText(dziPath, $"""
<?xml version="1.0" encoding="UTF-8"?>
<Image xmlns="http://schemas.microsoft.com/deepzoom/2008"
       Format="png" Overlap="{Overlap}" TileSize="{TileSize}">
  <Size Width="{ImageSize}" Height="{ImageSize}"/>
</Image>
""");
Console.WriteLine($"Written: {dziPath}");
Console.WriteLine($"Image: {ImageSize}×{ImageSize}, maxLevel: {maxLevel}, tileSize: {TileSize}, overlap: {Overlap}");
Console.WriteLine($"Color blocks: {GridDivs}×{GridDivs}  ({BlockPx}px each, {BlockTiles} tiles per block at max zoom)");

// ── Pastel colour palette ──────────────────────────────────────────────────────
// Light pastels — low saturation, high lightness, easy on the eye

SKColor[] palette =
[
    new(0xFF, 0xC1, 0xC1),   // soft rose
    new(0xFF, 0xD9, 0xB3),   // peach
    new(0xFF, 0xF0, 0xB3),   // butter yellow
    new(0xC1, 0xE8, 0xC1),   // mint green
    new(0xB3, 0xD9, 0xFF),   // sky blue
    new(0xD4, 0xB3, 0xFF),   // lavender
    new(0xFF, 0xC1, 0xE8),   // pink lilac
    new(0xB3, 0xF0, 0xE8),   // aqua mint
    new(0xE8, 0xFF, 0xB3),   // lime cream
    new(0xFF, 0xD4, 0xB3),   // apricot
    new(0xC1, 0xD4, 0xFF),   // periwinkle
    new(0xF0, 0xC1, 0xFF),   // mauve
];

// ── Tile generation ────────────────────────────────────────────────────────────

for (int level = 0; level <= maxLevel; level++)
{
    int scalePow    = maxLevel - level;
    int levelSize   = Math.Max(1, ImageSize >> scalePow);   // pixels at this level

    string levelDir = Path.Combine(tileRoot, level.ToString());
    Directory.CreateDirectory(levelDir);

    int cols = (int)Math.Ceiling((double)levelSize / TileSize);
    int rows = (int)Math.Ceiling((double)levelSize / TileSize);

    Console.Write($"  Level {level,2}: {levelSize,6}×{levelSize,-6}  {cols}×{rows} tiles … ");

    int count = 0;

    // Label font — sized relative to tile pixels at this level (min 8, max 22)
    float fontSize = Math.Clamp((float)levelSize / ImageSize * 256f, 8f, 22f);

    for (int col = 0; col < cols; col++)
    for (int row = 0; row < rows; row++)
    {
        // Tile pixel bounds in level space (including overlap)
        int x0 = col * TileSize - (col > 0 ? Overlap : 0);
        int y0 = row * TileSize - (row > 0 ? Overlap : 0);
        int x1 = Math.Min(levelSize, (col + 1) * TileSize + (col < cols - 1 ? Overlap : 0));
        int y1 = Math.Min(levelSize, (row + 1) * TileSize + (row < rows - 1 ? Overlap : 0));
        int tw = x1 - x0;
        int th = y1 - y0;

        using var bmp    = new SKBitmap(tw, th, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.White);

        // Block size at this level (in pixels)
        // BlockPx is at full resolution; scale down proportionally
        double blockSz = (double)BlockPx / ImageSize * levelSize;

        // Fill pastel color blocks (no borders)
        int bxMin = (int)Math.Floor(x0 / blockSz);
        int byMin = (int)Math.Floor(y0 / blockSz);
        int bxMax = (int)Math.Ceiling((double)x1 / blockSz);
        int byMax = (int)Math.Ceiling((double)y1 / blockSz);

        for (int by = byMin; by < byMax; by++)
        for (int bx = bxMin; bx < bxMax; bx++)
        {
            int gbx = Math.Clamp(bx, 0, GridDivs - 1);
            int gby = Math.Clamp(by, 0, GridDivs - 1);

            float left   = (float)(gbx       * blockSz - x0);
            float top    = (float)(gby        * blockSz - y0);
            float right  = (float)((gbx + 1) * blockSz - x0);
            float bottom = (float)((gby + 1) * blockSz - y0);

            var color = palette[(gbx + gby * 3) % palette.Length];
            using var fill = new SKPaint { Color = color };
            canvas.DrawRect(left, top, right - left, bottom - top, fill);
        }

        // Draw tile-level label in top-left corner (only when tile is ≥ 16px)
        if (tw >= 16 && th >= 16 && fontSize >= 8f)
        {
            using var font   = new SKFont(SKTypeface.Default, fontSize);
            string text = $"L{level}";
            float textW = font.MeasureText(text);
            float px = 3f;
            float py = fontSize + 2f;

            // Shadow/background so label is readable over any pastel
            using var bg = new SKPaint { Color = new SKColor(255, 255, 255, 160), IsAntialias = false };
            canvas.DrawRect(px - 1, 1, textW + 4, fontSize + 3, bg);

            using var lbl = new SKPaint { Color = new SKColor(90, 90, 90, 200), IsAntialias = true };
            canvas.DrawText(text, px, py, font, lbl);
        }

        // Save tile
        string tilePath = Path.Combine(levelDir, $"{col}_{row}.png");
        using var img  = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var fs   = File.Create(tilePath);
        data.SaveTo(fs);
        count++;
    }

    Console.WriteLine($"{count} tiles");
}

Console.WriteLine("Done!");
