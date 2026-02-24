using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SkiaSharp;
using System.IO;

namespace SkiaSharp.Extended.Gif.Benchmarks
{
    /// <summary>
    /// Benchmarks for GIF decoder performance.
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class DecoderBenchmarks
    {
        private byte[]? gifData10x10;
        private byte[]? gifData100x100;
        private byte[]? gifData1000x1000;
        
        [GlobalSetup]
        public void Setup()
        {
            // Create test GIF files of different sizes
            gifData10x10 = CreateTestGif(10, 10);
            gifData100x100 = CreateTestGif(100, 100);
            gifData1000x1000 = CreateTestGif(1000, 1000);
        }
        
        private byte[] CreateTestGif(int width, int height)
        {
            using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Red);
            
            // For now, return minimal valid GIF (will enhance when encoder is fixed)
            using var ms = new MemoryStream();
            // Write minimal GIF structure
            ms.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a"));
            ms.WriteByte((byte)(width & 0xFF));
            ms.WriteByte((byte)((width >> 8) & 0xFF));
            ms.WriteByte((byte)(height & 0xFF));
            ms.WriteByte((byte)((height >> 8) & 0xFF));
            ms.WriteByte(0xF7); // Global color table
            ms.WriteByte(0); // BG color
            ms.WriteByte(0); // Aspect ratio
            
            // Global color table (256 colors)
            for (int i = 0; i < 256; i++)
            {
                ms.WriteByte((byte)i);
                ms.WriteByte(0);
                ms.WriteByte(0);
            }
            
            // Image descriptor
            ms.WriteByte(0x2C); // Image separator
            ms.WriteByte(0); // Left
            ms.WriteByte(0);
            ms.WriteByte(0); // Top
            ms.WriteByte(0);
            ms.WriteByte((byte)(width & 0xFF)); // Width
            ms.WriteByte((byte)((width >> 8) & 0xFF));
            ms.WriteByte((byte)(height & 0xFF)); // Height
            ms.WriteByte((byte)((height >> 8) & 0xFF));
            ms.WriteByte(0); // Packed

            // Image data (minimal LZW)
            ms.WriteByte(8); // Min code size
            ms.WriteByte(2); // Block size
            ms.WriteByte(0x8C); // Clear code + data
            ms.WriteByte(0x2D);
            ms.WriteByte(0); // Block terminator
            
            ms.WriteByte(0x3B); // Trailer
            
            return ms.ToArray();
        }
        
        [Benchmark]
        public void Decode_10x10()
        {
            using var stream = new MemoryStream(gifData10x10!);
            using var decoder = SKGifDecoder.Create(stream);
            var info = decoder.Info;
        }
        
        [Benchmark]
        public void Decode_100x100()
        {
            using var stream = new MemoryStream(gifData100x100!);
            using var decoder = SKGifDecoder.Create(stream);
            var info = decoder.Info;
        }
        
        [Benchmark]
        public void Decode_1000x1000()
        {
            using var stream = new MemoryStream(gifData1000x1000!);
            using var decoder = SKGifDecoder.Create(stream);
            var info = decoder.Info;
        }
    }
}
