using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SkiaSharp;
using SkiaSharp.Extended.Gif.Encoding;

namespace SkiaSharp.Extended.Gif.Benchmarks
{
    /// <summary>
    /// Benchmarks for color quantization performance.
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class QuantizationBenchmarks
    {
        private SKBitmap? bitmap10x10;
        private SKBitmap? bitmap100x100;
        private SKBitmap? bitmap1000x1000;
        private SKColor[]? palette256;
        
        [GlobalSetup]
        public void Setup()
        {
            bitmap10x10 = CreateTestBitmap(10, 10);
            bitmap100x100 = CreateTestBitmap(100, 100);
            bitmap1000x1000 = CreateTestBitmap(1000, 1000);
            
            // Pre-generate a palette for mapping benchmarks
            palette256 = ColorQuantizer.QuantizeColors(bitmap100x100, 256);
        }
        
        [GlobalCleanup]
        public void Cleanup()
        {
            bitmap10x10?.Dispose();
            bitmap100x100?.Dispose();
            bitmap1000x1000?.Dispose();
        }
        
        private SKBitmap CreateTestBitmap(int width, int height)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            
            // Create gradient for varied colors
            using var paint = new SKPaint();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r = (byte)(255 * x / width);
                    byte g = (byte)(255 * y / height);
                    byte b = (byte)(128);
                    paint.Color = new SKColor(r, g, b);
                    canvas.DrawPoint(x, y, paint);
                }
            }
            
            return bitmap;
        }
        
        [Benchmark]
        public void Quantize_10x10_To256Colors()
        {
            var palette = ColorQuantizer.QuantizeColors(bitmap10x10!, 256);
        }
        
        [Benchmark]
        public void Quantize_100x100_To256Colors()
        {
            var palette = ColorQuantizer.QuantizeColors(bitmap100x100!, 256);
        }
        
        [Benchmark]
        public void Quantize_1000x1000_To256Colors()
        {
            var palette = ColorQuantizer.QuantizeColors(bitmap1000x1000!, 256);
        }
        
        [Benchmark]
        public void MapToPalette_10x10()
        {
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap10x10!, palette256!);
        }
        
        [Benchmark]
        public void MapToPalette_100x100()
        {
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap100x100!, palette256!);
        }
        
        [Benchmark]
        public void MapToPalette_1000x1000()
        {
            var indices = ColorQuantizer.MapBitmapToPalette(bitmap1000x1000!, palette256!);
        }
    }
}
