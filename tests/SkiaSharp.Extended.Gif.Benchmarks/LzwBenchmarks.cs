using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SkiaSharp.Extended.Gif.Codec;
using System.IO;

namespace SkiaSharp.Extended.Gif.Benchmarks
{
    /// <summary>
    /// Benchmarks for LZW compression and decompression performance.
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net90)]
    public class LzwBenchmarks
    {
        private byte[]? smallData;
        private byte[]? mediumData;
        private byte[]? largeData;
        
        private byte[]? compressedSmall;
        private byte[]? compressedMedium;
        private byte[]? compressedLarge;
        
        [GlobalSetup]
        public void Setup()
        {
            // Create test data of different sizes
            smallData = new byte[100];  // 100 bytes
            mediumData = new byte[10000]; // 10 KB
            largeData = new byte[1000000]; // 1 MB
            
            // Fill with patterns
            for (int i = 0; i < smallData.Length; i++)
                smallData[i] = (byte)(i % 10);
            for (int i = 0; i < mediumData.Length; i++)
                mediumData[i] = (byte)(i % 20);
            for (int i = 0; i < largeData.Length; i++)
                largeData[i] = (byte)(i % 50);
            
            // Pre-compress for decompression benchmarks
            compressedSmall = Compress(smallData, 3);
            compressedMedium = Compress(mediumData, 4);
            compressedLarge = Compress(largeData, 5);
        }
        
        private byte[] Compress(byte[] data, byte minCodeSize)
        {
            using var ms = new MemoryStream();
            using var encoder = new LzwEncoder(minCodeSize);
            encoder.Compress(data, ms);
            return ms.ToArray();
        }
        
        [Benchmark]
        public void Compress_Small_100B()
        {
            using var ms = new MemoryStream();
            using var encoder = new LzwEncoder(3);
            encoder.Compress(smallData!, ms);
        }
        
        [Benchmark]
        public void Compress_Medium_10KB()
        {
            using var ms = new MemoryStream();
            using var encoder = new LzwEncoder(4);
            encoder.Compress(mediumData!, ms);
        }
        
        [Benchmark]
        public void Compress_Large_1MB()
        {
            using var ms = new MemoryStream();
            using var encoder = new LzwEncoder(5);
            encoder.Compress(largeData!, ms);
        }
        
        [Benchmark]
        public void Decompress_Small_100B()
        {
            using var ms = new MemoryStream(compressedSmall!);
            using var decoder = new LzwDecoder(ms, 3);
            var output = new byte[smallData!.Length];
            decoder.Decompress(output, 0, output.Length);
        }
        
        [Benchmark]
        public void Decompress_Medium_10KB()
        {
            using var ms = new MemoryStream(compressedMedium!);
            using var decoder = new LzwDecoder(ms, 4);
            var output = new byte[mediumData!.Length];
            decoder.Decompress(output, 0, output.Length);
        }
        
        [Benchmark]
        public void Decompress_Large_1MB()
        {
            using var ms = new MemoryStream(compressedLarge!);
            using var decoder = new LzwDecoder(ms, 5);
            var output = new byte[largeData!.Length];
            decoder.Decompress(output, 0, output.Length);
        }
    }
}
