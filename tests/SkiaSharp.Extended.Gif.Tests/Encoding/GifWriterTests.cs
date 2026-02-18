using System;
using System.IO;
using System.Linq;
using SkiaSharp.Extended.Gif.Encoding;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Encoding
{
    public class GifWriterTests
    {
        [Fact]
        public void WriteHeader_WritesGif89a()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteHeader(useGif89a: true);
            
            var bytes = stream.ToArray();
            Assert.Equal(6, bytes.Length);
            Assert.Equal(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, bytes); // "GIF89a"
        }
        
        [Fact]
        public void WriteHeader_WritesGif87a()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteHeader(useGif89a: false);
            
            var bytes = stream.ToArray();
            Assert.Equal(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, bytes); // "GIF87a"
        }
        
        [Fact]
        public void WriteLogicalScreenDescriptor_WritesCorrectSize()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteLogicalScreenDescriptor(640, 480, true, 256);
            
            var bytes = stream.ToArray();
            Assert.Equal(7, bytes.Length);
            
            // Width (little-endian)
            Assert.Equal(640 & 0xFF, bytes[0]);
            Assert.Equal(640 >> 8, bytes[1]);
            
            // Height (little-endian)
            Assert.Equal(480 & 0xFF, bytes[2]);
            Assert.Equal(480 >> 8, bytes[3]);
        }
        
        [Fact]
        public void WriteColorTable_WritesRGBTriplets()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            var colors = new[] { SKColors.Red, SKColors.Green, SKColors.Blue };
            writer.WriteColorTable(colors);
            
            var bytes = stream.ToArray();
            Assert.Equal(9, bytes.Length); // 3 colors * 3 bytes each
            
            // Red
            Assert.Equal(255, bytes[0]);
            Assert.Equal(0, bytes[1]);
            Assert.Equal(0, bytes[2]);
            
            // Green
            Assert.Equal(0, bytes[3]);
            Assert.Equal(128, bytes[4]); // SKColors.Green is 0,128,0
            Assert.Equal(0, bytes[5]);
            
            // Blue
            Assert.Equal(0, bytes[6]);
            Assert.Equal(0, bytes[7]);
            Assert.Equal(255, bytes[8]);
        }
        
        [Fact]
        public void WriteGraphicsControlExtension_WritesCorrectStructure()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteGraphicsControlExtension(
                SKGifDisposalMethod.RestoreToBackground,
                100, // 100ms
                true,
                0);
            
            var bytes = stream.ToArray();
            Assert.Equal(8, bytes.Length);
            Assert.Equal(0x21, bytes[0]); // Extension introducer
            Assert.Equal(0xF9, bytes[1]); // Graphics Control Label
            Assert.Equal(0x04, bytes[2]); // Block size
        }
        
        [Fact]
        public void WriteNetscapeLoopExtension_WritesLoopCount()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteNetscapeLoopExtension(5);
            
            var bytes = stream.ToArray();
            Assert.Equal(19, bytes.Length);
            Assert.Equal(0x21, bytes[0]); // Extension introducer
            Assert.Equal(0xFF, bytes[1]); // Application Extension Label
        }
        
        [Fact]
        public void WriteImageDescriptor_WritesCorrectly()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteImageDescriptor(10, 20, 100, 200, false, 0);
            
            var bytes = stream.ToArray();
            Assert.Equal(10, bytes.Length);
            Assert.Equal(0x2C, bytes[0]); // Image separator
        }
        
        [Fact]
        public void WriteImageData_WritesInSubBlocks()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            var data = new byte[300]; // More than 255, requires multiple sub-blocks
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 256);
            
            writer.WriteImageData(data, 2);
            
            var bytes = stream.ToArray();
            Assert.True(bytes.Length > 300); // Data + size bytes + terminator
            Assert.Equal(2, bytes[0]); // LZW min code size
            Assert.Equal(255, bytes[1]); // First sub-block size
            Assert.Equal(0, bytes[bytes.Length - 1]); // Block terminator
        }
        
        [Fact]
        public void WriteTrailer_WritesSingleByte()
        {
            using var stream = new MemoryStream();
            var writer = new GifWriter(stream);
            
            writer.WriteTrailer();
            
            var bytes = stream.ToArray();
            Assert.Equal(1, bytes.Length);
            Assert.Equal(0x3B, bytes[0]);
        }
    }
}
