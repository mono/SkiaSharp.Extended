using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
    public class DecoderIntegrationTests
    {
        [Fact]
        public void CanDecodeSimpleGif()
        {
            // Create a simple 2x2 GIF with 4 colors
            var gifData = CreateSimple2x2Gif();
            
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.NotNull(decoder);
            Assert.Equal(2, decoder.Info.Width);
            Assert.Equal(2, decoder.Info.Height);
            Assert.Equal(1, decoder.FrameCount);
            
            var frame = decoder.GetFrame(0);
            Assert.NotNull(frame);
            Assert.NotNull(frame.Bitmap);
            Assert.Equal(2, frame.Bitmap.Width);
            Assert.Equal(2, frame.Bitmap.Height);
            
            frame.Bitmap.Dispose();
        }
        
        private byte[] CreateSimple2x2Gif()
        {
            // Manually create a valid GIF file with a 2x2 image
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }); // "GIF89a"
            
            // Logical Screen Descriptor
            writer.Write((ushort)2); // Width
            writer.Write((ushort)2); // Height
            writer.Write((byte)0xF0); // Packed: GCT=1, ColorRes=7, Sort=0, GCTSize=0 (2 colors)
            writer.Write((byte)0); // Background color index
            writer.Write((byte)0); // Pixel aspect ratio
            
            // Global Color Table (2 colors: black and white)
            writer.Write(new byte[] { 0x00, 0x00, 0x00 }); // Black
            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF }); // White
            
            // Image Descriptor
            writer.Write((byte)0x2C); // Image separator
            writer.Write((ushort)0); // Left
            writer.Write((ushort)0); // Top
            writer.Write((ushort)2); // Width
            writer.Write((ushort)2); // Height
            writer.Write((byte)0x00); // Packed: no local color table, not interlaced
            
            // Image Data
            writer.Write((byte)2); // LZW minimum code size
            
            // Compressed data for 2x2 image with pattern: 0,1,1,0
            // LZW stream: clear(4), 0, 1, 1, 0, end(5)
            var compressedData = new byte[] {
                0x04, // Sub-block size
                0x84, 0x01, 0x01, 0x00, // Simple LZW data
                0x00, // Block terminator
            };
            writer.Write(compressedData);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
    }
}
