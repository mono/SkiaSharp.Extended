using System;
using System.IO;
using SkiaSharp.Extended.Gif.Codec;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Codec
{
    public class LzwEncoderTests
    {
        [Fact]
        public void Constructor_ValidatesMinCodeSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LzwEncoder(1)); // Too small
            Assert.Throws<ArgumentOutOfRangeException>(() => new LzwEncoder(9)); // Too large
        }
        
        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        public void Constructor_AcceptsValidMinCodeSize(int minCodeSize)
        {
            using var encoder = new LzwEncoder(minCodeSize);
            Assert.Equal(minCodeSize, encoder.MinimumCodeSize);
        }
        
        [Fact]
        public void Compress_ValidatesNullInput()
        {
            using var encoder = new LzwEncoder(2);
            using var output = new MemoryStream();
            
            Assert.Throws<ArgumentNullException>(() => encoder.Compress(null!, output));
        }
        
        [Fact]
        public void Compress_ValidatesNullOutput()
        {
            using var encoder = new LzwEncoder(2);
            var input = new byte[] { 0, 1, 2, 3 };
            
            Assert.Throws<ArgumentNullException>(() => encoder.Compress(input, null!));
        }
        
        [Fact]
        public void Compress_ProducesOutput()
        {
            using var encoder = new LzwEncoder(2);
            using var output = new MemoryStream();
            var input = new byte[] { 0, 1, 1, 0, 0, 1 };
            
            encoder.Compress(input, output);
            
            Assert.True(output.Length > 0, "Compression should produce output");
        }
        
        [Fact]
        public void Compress_EmptyData()
        {
            using var encoder = new LzwEncoder(2);
            using var output = new MemoryStream();
            var input = new byte[] { };
            
            encoder.Compress(input, output);
            
            // Should write at least clear code and end code
            Assert.True(output.Length > 0);
        }
        
        [Fact]
        public void Compress_SingleByte()
        {
            using var encoder = new LzwEncoder(2);
            using var output = new MemoryStream();
            var input = new byte[] { 0 };
            
            encoder.Compress(input, output);
            
            Assert.True(output.Length > 0);
        }
        
        [Fact]
        public void Compress_CanBeCalledMultipleTimes()
        {
            using var encoder = new LzwEncoder(2);
            var input = new byte[] { 0, 1, 2 };
            
            using var output1 = new MemoryStream();
            encoder.Compress(input, output1);
            
            using var output2 = new MemoryStream();
            encoder.Compress(input, output2);
            
            // Both should succeed
            Assert.True(output1.Length > 0);
            Assert.True(output2.Length > 0);
        }
        
        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var encoder = new LzwEncoder(2);
            encoder.Dispose();
            encoder.Dispose(); // Should not throw
        }
    }
}
