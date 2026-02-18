using System;
using System.IO;
using SkiaSharp.Extended.Gif.Decoding;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
    public class GifParserTests
    {
        [Fact]
        public void GifParser_ValidatesNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => new GifParser(null!));
        }
        
        [Fact]
        public void Parse_ValidatesInvalidGif()
        {
            var invalidData = new byte[] { 0x00, 0x00, 0x00 };
            using var stream = new MemoryStream(invalidData);
            var parser = new GifParser(stream);
            
            Assert.Throws<InvalidDataException>(() => parser.Parse());
        }
        
        [Fact]
        public void Parse_HandlesSimpleGif()
        {
            var gifData = CreateMinimalGif();
            using var stream = new MemoryStream(gifData);
            var parser = new GifParser(stream);
            
            var parsed = parser.Parse();
            
            Assert.NotNull(parsed);
            Assert.True(parsed.Header.IsValid);
            Assert.Equal(2, parsed.ScreenDescriptor.Width);
            Assert.Equal(2, parsed.ScreenDescriptor.Height);
            Assert.NotNull(parsed.Frames);
            Assert.True(parsed.Frames.Length > 0);
        }
        
        private byte[] CreateMinimalGif()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }); // "GIF89a"
            
            // Logical Screen Descriptor
            writer.Write((ushort)2);
            writer.Write((ushort)2);
            writer.Write((byte)0xF0); // GCT flag + size
            writer.Write((byte)0);
            writer.Write((byte)0);
            
            // Global Color Table (2 colors)
            writer.Write(new byte[] { 0x00, 0x00, 0x00 }); // Black
            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF }); // White
            
            // Image Descriptor
            writer.Write((byte)0x2C);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)2);
            writer.Write((ushort)2);
            writer.Write((byte)0x00);
            
            // Image Data (minimal LZW)
            writer.Write((byte)2); // Min code size
            writer.Write((byte)4); // Sub-block size
            writer.Write(new byte[] { 0x84, 0x01, 0x01, 0x00 }); // Minimal LZW data
            writer.Write((byte)0x00); // Block terminator
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
    }
}
