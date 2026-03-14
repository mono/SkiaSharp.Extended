#!/usr/bin/env dotnet-script
// Test-grid DZI generator
//
// Generates a 32768×32768 Deep Zoom Image with 16 levels (0-15) directly from
// a procedural grid pattern — no source image needed, so memory stays tiny.
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
const int GridDivs     = 64;      // 64×64 labelled grid cells across the full image
                                  // → each cell = 512×512 px at full resolution

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

// ── Colour palette ─────────────────────────────────────────────────────────────

SKColor[] palette =
[
    new(0xFF, 0x6B, 0x6B), new(0xFF, 0xA5, 0x00), new(0xFF, 0xD7, 0x00),
    new(0x6B, 0xD9, 0x6B), new(0x00, 0xB8, 0xFF), new(0xAF, 0x6B, 0xFF),
    new(0xFF, 0x6B, 0xB5), new(0x6B, 0xFF, 0xE4), new(0xFF, 0xA5, 0x6B),
    new(0x8B, 0xD4, 0x6B),
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

        // Cell size at this level (may be < 1 px at very low levels — handled via clamp)
        double cellSz = (double)levelSize / GridDivs;

        // Identify which grid cells overlap this tile
        int cxMin = (int)Math.Floor(x0 / cellSz);
        int cyMin = (int)Math.Floor(y0 / cellSz);
        int cxMax = (int)Math.Ceiling((double)x1 / cellSz);
        int cyMax = (int)Math.Ceiling((double)y1 / cellSz);

        // Fill grid cell backgrounds
        for (int cy = cyMin; cy < cyMax; cy++)
        for (int cx = cxMin; cx < cxMax; cx++)
        {
            int gcx = Math.Clamp(cx, 0, GridDivs - 1);
            int gcy = Math.Clamp(cy, 0, GridDivs - 1);

            float left   = (float)(gcx       * cellSz - x0);
            float top    = (float)(gcy        * cellSz - y0);
            float right  = (float)((gcx + 1) * cellSz - x0);
            float bottom = (float)((gcy + 1) * cellSz - y0);

            var color = palette[(gcx + gcy * 3) % palette.Length];
            using var fill = new SKPaint { Color = color };
            canvas.DrawRect(left, top, right - left, bottom - top, fill);
        }

        // Grid lines
        if (cellSz >= 2)
        {
            using var line = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 100),
                StrokeWidth = Math.Max(1f, (float)(cellSz / 64)),
                IsAntialias = false,
                Style = SKPaintStyle.Stroke,
            };
            for (int cy = cyMin; cy <= cyMax; cy++)
            {
                float ly = (float)(cy * cellSz - y0);
                canvas.DrawLine(0, ly, tw, ly, line);
            }
            for (int cx = cxMin; cx <= cxMax; cx++)
            {
                float lx = (float)(cx * cellSz - x0);
                canvas.DrawLine(lx, 0, lx, th, line);
            }
        }

        // Cell labels (only when cell is ≥ 20 px — visible at mid/high levels)
        if (cellSz >= 20)
        {
            float fontSize = Math.Max(8f, (float)(cellSz * 0.15));
            using var font  = new SKFont(SKTypeface.Default, fontSize);
            using var label = new SKPaint { Color = new SKColor(0, 0, 0, 180), IsAntialias = true };

            for (int cy = cyMin; cy < cyMax; cy++)
            for (int cx = cxMin; cx < cxMax; cx++)
            {
                int gcx = Math.Clamp(cx, 0, GridDivs - 1);
                int gcy = Math.Clamp(cy, 0, GridDivs - 1);

                float cx_ = (float)((gcx + 0.5) * cellSz - x0);
                float cy_ = (float)((gcy + 0.5) * cellSz - y0 + fontSize / 2);
                string text = $"{gcx},{gcy}";
                float textW = font.MeasureText(text);
                canvas.DrawText(text, cx_ - textW / 2, cy_, font, label);
            }
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
