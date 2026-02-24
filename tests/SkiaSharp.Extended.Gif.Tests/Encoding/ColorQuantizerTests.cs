using System;
using SkiaSharp.Extended.Gif.Encoding;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Encoding
{
    public class ColorQuantizerTests
    {
        [Fact]
        public void QuantizeColors_ValidatesMaxColors()
        {
            using var bitmap = new SKBitmap(10, 10);
            
            Assert.Throws<ArgumentOutOfRangeException>(() => ColorQuantizer.QuantizeColors(bitmap, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ColorQuantizer.QuantizeColors(bitmap, 257));
        }
        
        [Fact]
        public void QuantizeColors_ReturnsPowerOf2()
        {
            using var bitmap = new SKBitmap(10, 10);
            FillBitmapWithColors(bitmap, 5); // 5 unique colors
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 256);
            
            // Should round up to power of 2
            Assert.True(IsPowerOfTwo(palette.Length));
            Assert.True(palette.Length >= 5); // At least the number of unique colors
        }
        
        [Fact]
        public void QuantizeColors_HandlesMonochrome()
        {
            using var bitmap = new SKBitmap(10, 10);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Black);
            }
            
            var palette = ColorQuantizer.QuantizeColors(bitmap, 256);
            
            Assert.True(palette.Length >= 1);
            Assert.Contains(SKColors.Black, palette);
        }
        
        [Fact]
        public void MapBitmapToPalette_ProducesCorrectSize()
        {
            using var bitmap = new SKBitmap(10, 15);
            var palette = new[] { SKColors.Black, SKColors.White };
            
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap, palette);
            
            Assert.Equal(10 * 15, indices.Length);
        }
        
        [Fact]
        public void MapBitmapToPalette_MapsToNearestColor()
        {
            using var bitmap = new SKBitmap(2, 2);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                using var paint = new SKPaint { Color = SKColors.Black };
                canvas.DrawPoint(0, 0, paint);
            }
            
            var palette = new[] { SKColors.Black, SKColors.White };
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap, palette);
            
            // First pixel should be black (index 0)
            Assert.Equal(0, indices[0]);
            
            // Other pixels should be white (index 1)
            Assert.Equal(1, indices[1]);
            Assert.Equal(1, indices[2]);
            Assert.Equal(1, indices[3]);
        }
        
        [Fact]
        public void FindNearestColor_FindsExactMatch()
        {
            var palette = new[] { SKColors.Red, SKColors.Green, SKColors.Blue };
            
            var index = ColorQuantizer.FindNearestColor(SKColors.Red, palette);
            Assert.Equal(0, index);
            
            index = ColorQuantizer.FindNearestColor(SKColors.Green, palette);
            Assert.Equal(1, index);
            
            index = ColorQuantizer.FindNearestColor(SKColors.Blue, palette);
            Assert.Equal(2, index);
        }
        
        private void FillBitmapWithColors(SKBitmap bitmap, int colorCount)
        {
            using var canvas = new SKCanvas(bitmap);
            var colors = new[] { SKColors.Red, SKColors.Green, SKColors.Blue, SKColors.Yellow, SKColors.Cyan };
            
            for (int i = 0; i < Math.Min(colorCount, colors.Length); i++)
            {
                using var paint = new SKPaint { Color = colors[i] };
                canvas.DrawRect(i * 2, 0, 2, 10, paint);
            }
        }
        
        private bool IsPowerOfTwo(int n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }
    }
}
