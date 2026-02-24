using System;
using SkiaSharp;
using SkiaSharp.Extended.Gif.Encoding;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Encoding
{
    public class ColorQuantizerAdvancedTests
    {
        [Fact]
        public void QuantizeColors_SolidColor_ReturnsSingleColor()
        {
            using var bitmap = new SKBitmap(10, 10);
            bitmap.Erase(SKColors.Red);
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 256);
            
            // Should have at least one color (red)
            Assert.True(palette.Length > 0);
            Assert.True(palette.Length <= 256);
            
            // Should be a power of 2
            Assert.True(IsPowerOfTwo(palette.Length));
        }
        
        [Fact]
        public void QuantizeColors_TwoColors_ReturnsTwo()
        {
            using var bitmap = new SKBitmap(10, 10);
            
            // Fill half with red, half with blue
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    bitmap.SetPixel(x, y, x < 5 ? SKColors.Red : SKColors.Blue);
                }
            }
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 256);
            
            // Should have at least 2 colors
            Assert.True(palette.Length >= 2);
            Assert.True(palette.Length <= 256);
            Assert.True(IsPowerOfTwo(palette.Length));
        }
        
        [Fact]
        public void QuantizeColors_ManyColors_ReducesToTarget()
        {
            using var bitmap = new SKBitmap(16, 16);
            
            // Create a gradient with many colors
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    byte r = (byte)(x * 16);
                    byte g = (byte)(y * 16);
                    byte b = 128;
                    bitmap.SetPixel(x, y, new SKColor(r, g, b));
                }
            }
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 64);
            
            Assert.True(palette.Length <= 64);
            Assert.True(IsPowerOfTwo(palette.Length));
        }
        
        [Fact]
        public void FindNearestColor_FindsExactMatch()
        {
            var palette = new SKColor[]
            {
                SKColors.Red,
                SKColors.Green,
                SKColors.Blue
            };
            
            var index = ColorQuantizer.FindNearestColor(SKColors.Red, palette);
            
            Assert.Equal(0, index);
        }
        
        [Fact]
        public void FindNearestColor_FindsClosestMatch()
        {
            var palette = new SKColor[]
            {
                SKColors.Black,
                SKColors.White
            };
            
            // Light gray should be closer to white
            var lightGray = new SKColor(200, 200, 200);
            var index = ColorQuantizer.FindNearestColor(lightGray, palette);
            
            Assert.Equal(1, index); // White
        }
        
        [Fact]
        public void FindNearestColor_DarkColorCloserToBlack()
        {
            var palette = new SKColor[]
            {
                SKColors.Black,
                SKColors.White
            };
            
            // Dark gray should be closer to black
            var darkGray = new SKColor(50, 50, 50);
            var index = ColorQuantizer.FindNearestColor(darkGray, palette);
            
            Assert.Equal(0, index); // Black
        }
        
        [Fact]
        public void MapBitmapToPalette_MapsCorrectly()
        {
            using var bitmap = new SKBitmap(4, 4);
            bitmap.Erase(SKColors.Red);
            
            var palette = new SKColor[] { SKColors.Red, SKColors.Blue };
            
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap, palette);
            
            Assert.Equal(16, indices.Length); // 4x4
            
            // All pixels should map to index 0 (red)
            foreach (var index in indices)
            {
                Assert.Equal(0, index);
            }
        }
        
        [Fact]
        public void MapBitmapToPalette_DifferentColors()
        {
            using var bitmap = new SKBitmap(2, 2);
            bitmap.SetPixel(0, 0, SKColors.Red);
            bitmap.SetPixel(1, 0, SKColors.Blue);
            bitmap.SetPixel(0, 1, SKColors.Green);
            bitmap.SetPixel(1, 1, SKColors.Yellow);
            
            var palette = new SKColor[] 
            { 
                SKColors.Red, 
                SKColors.Blue, 
                SKColors.Green, 
                SKColors.Yellow 
            };
            
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap, palette);
            
            Assert.Equal(4, indices.Length);
            Assert.Equal(0, indices[0]); // Red
            Assert.Equal(1, indices[1]); // Blue
            Assert.Equal(2, indices[2]); // Green
            Assert.Equal(3, indices[3]); // Yellow
        }
        
        [Fact]
        public void QuantizeColors_SmallPalette_Works()
        {
            using var bitmap = new SKBitmap(4, 4);
            bitmap.Erase(SKColors.Magenta);
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 4);
            
            Assert.True(palette.Length <= 4);
            Assert.True(IsPowerOfTwo(palette.Length));
        }
        
        [Fact]
        public void QuantizeColors_LargePalette_Works()
        {
            using var bitmap = new SKBitmap(8, 8);
            
            // Create some variety
            for (int i = 0; i < 64; i++)
            {
                int x = i % 8;
                int y = i / 8;
                bitmap.SetPixel(x, y, new SKColor((byte)(i * 4), 128, 200));
            }
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 256);
            
            Assert.True(palette.Length <= 256);
            Assert.True(IsPowerOfTwo(palette.Length));
        }
        
        [Fact]
        public void MapBitmapToPalette_LargerBitmap()
        {
            using var bitmap = new SKBitmap(8, 8);
            
            // Create a checkerboard pattern
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    bitmap.SetPixel(x, y, (x + y) % 2 == 0 ? SKColors.Black : SKColors.White);
                }
            }
            
            var palette = new SKColor[] { SKColors.Black, SKColors.White };
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap, palette);
            
            Assert.Equal(64, indices.Length);
            
            // Verify pattern
            for (int i = 0; i < 64; i++)
            {
                int x = i % 8;
                int y = i / 8;
                byte expected = (byte)((x + y) % 2 == 0 ? 0 : 1);
                Assert.Equal(expected, indices[i]);
            }
        }
        
        private bool IsPowerOfTwo(int n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }
    }
}
