using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.Gif.Encoding
{
    /// <summary>
    /// Simple color quantizer using median cut algorithm.
    /// </summary>
    internal class ColorQuantizer
    {
        public static SKColor[] QuantizeColors(SKBitmap bitmap, int maxColors)
        {
            if (maxColors < 2 || maxColors > 256)
                throw new ArgumentOutOfRangeException(nameof(maxColors), "Max colors must be 2-256");
            
            // Collect all unique colors from the bitmap
            var colors = new HashSet<SKColor>();
            
            // Use GetPixel for safer access (slower but more compatible)
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    colors.Add(bitmap.GetPixel(x, y));
                }
            }
            
            // If we already have fewer colors than the max, return them
            if (colors.Count <= maxColors)
            {
                var result = colors.ToArray();
                // Pad to power of 2
                int targetSize = 2;
                while (targetSize < maxColors)
                    targetSize *= 2;
                
                // Use the smaller of targetSize or maxColors to avoid over-allocation
                targetSize = Math.Min(targetSize, 256);
                
                if (targetSize > result.Length)
                {
                    Array.Resize(ref result, targetSize);
                    // Fill remaining slots with black
                    for (int i = colors.Count; i < targetSize; i++)
                    {
                        result[i] = SKColors.Black;
                    }
                }
                
                return result;
            }
            
            // Use median cut algorithm
            var palette = MedianCut(colors.ToList(), maxColors);
            
            // Ensure power of 2
            int paletteSize = 2;
            while (paletteSize < palette.Count)
                paletteSize *= 2;
            
            var finalPalette = new SKColor[paletteSize];
            for (int i = 0; i < palette.Count; i++)
            {
                finalPalette[i] = palette[i];
            }
            for (int i = palette.Count; i < paletteSize; i++)
            {
                finalPalette[i] = SKColors.Black;
            }
            
            return finalPalette;
        }
        
        private static List<SKColor> MedianCut(List<SKColor> colors, int maxColors)
        {
            var boxes = new List<ColorBox> { new ColorBox(colors) };
            
            while (boxes.Count < maxColors)
            {
                // Find box with largest range
                var boxToSplit = boxes.OrderByDescending(b => b.Range).FirstOrDefault();
                if (boxToSplit == null || boxToSplit.Colors.Count <= 1)
                    break;
                
                boxes.Remove(boxToSplit);
                
                var (box1, box2) = boxToSplit.Split();
                
                // Only add non-empty boxes
                if (box1.Colors.Count > 0)
                    boxes.Add(box1);
                if (box2.Colors.Count > 0)
                    boxes.Add(box2);
                
                // Safety: if we didn't make progress, stop
                if (box1.Colors.Count == 0 && box2.Colors.Count == 0)
                    break;
            }
            
            return boxes.Select(b => b.AverageColor).ToList();
        }
        
        public static byte[] MapBitmapToPalette(SKBitmap bitmap, SKColor[] palette)
        {
            int pixelCount = bitmap.Width * bitmap.Height;
            var indices = new byte[pixelCount];
            
            int index = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    indices[index++] = FindNearestColorIndex(color, palette);
                }
            }
            
            return indices;
        }
        
        private static byte FindNearestColorIndex(SKColor color, SKColor[] palette)
        {
            byte bestIndex = 0;
            int bestDistance = int.MaxValue;
            
            for (int i = 0; i < palette.Length; i++)
            {
                int distance = ColorDistance(color, palette[i]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = (byte)i;
                }
            }
            
            return bestIndex;
        }
        
        public static byte FindNearestColor(SKColor color, SKColor[] palette)
        {
            return FindNearestColorIndex(color, palette);
        }
        
        private static int ColorDistance(SKColor a, SKColor b)
        {
            int dr = a.Red - b.Red;
            int dg = a.Green - b.Green;
            int db = a.Blue - b.Blue;
            return dr * dr + dg * dg + db * db;
        }
        
        private class ColorBox
        {
            public List<SKColor> Colors { get; }
            
            public ColorBox(List<SKColor> colors)
            {
                Colors = colors;
            }
            
            public int Range
            {
                get
                {
                    if (Colors.Count == 0) return 0;
                    
                    int minR = 255, maxR = 0;
                    int minG = 255, maxG = 0;
                    int minB = 255, maxB = 0;
                    
                    foreach (var c in Colors)
                    {
                        minR = Math.Min(minR, c.Red);
                        maxR = Math.Max(maxR, c.Red);
                        minG = Math.Min(minG, c.Green);
                        maxG = Math.Max(maxG, c.Green);
                        minB = Math.Min(minB, c.Blue);
                        maxB = Math.Max(maxB, c.Blue);
                    }
                    
                    return Math.Max(Math.Max(maxR - minR, maxG - minG), maxB - minB);
                }
            }
            
            public SKColor AverageColor
            {
                get
                {
                    if (Colors.Count == 0) return SKColors.Black;
                    
                    long r = 0, g = 0, b = 0;
                    foreach (var c in Colors)
                    {
                        r += c.Red;
                        g += c.Green;
                        b += c.Blue;
                    }
                    
                    return new SKColor(
                        (byte)(r / Colors.Count),
                        (byte)(g / Colors.Count),
                        (byte)(b / Colors.Count)
                    );
                }
            }
            
            public (ColorBox, ColorBox) Split()
            {
                if (Colors.Count == 0)
                    return (new ColorBox(new List<SKColor>()), new ColorBox(new List<SKColor>()));
                
                // Find widest channel
                int minR = 255, maxR = 0;
                int minG = 255, maxG = 0;
                int minB = 255, maxB = 0;
                
                foreach (var c in Colors)
                {
                    minR = Math.Min(minR, c.Red);
                    maxR = Math.Max(maxR, c.Red);
                    minG = Math.Min(minG, c.Green);
                    maxG = Math.Max(maxG, c.Green);
                    minB = Math.Min(minB, c.Blue);
                    maxB = Math.Max(maxB, c.Blue);
                }
                
                int rangeR = maxR - minR;
                int rangeG = maxG - minG;
                int rangeB = maxB - minB;
                
                // Sort by widest channel
                List<SKColor> sorted;
                if (rangeR >= rangeG && rangeR >= rangeB)
                {
                    sorted = Colors.OrderBy(c => c.Red).ToList();
                }
                else if (rangeG >= rangeB)
                {
                    sorted = Colors.OrderBy(c => c.Green).ToList();
                }
                else
                {
                    sorted = Colors.OrderBy(c => c.Blue).ToList();
                }
                
                int median = sorted.Count / 2;
                return (
                    new ColorBox(sorted.Take(median).ToList()),
                    new ColorBox(sorted.Skip(median).ToList())
                );
            }
        }
    }
}
