using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
    public class FrameDecodingTests
    {
        [Fact]
        public void GetFrame_DecodesNonInterlacedFrame()
        {
            var gifData = CreateNonInterlacedGif();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            var frame = decoder.GetFrame(0);
            
            Assert.NotNull(frame);
            Assert.NotNull(frame.Bitmap);
            Assert.Equal(2, frame.Bitmap.Width);
            Assert.Equal(2, frame.Bitmap.Height);
            
            frame.Bitmap.Dispose();
        }
        
        [Fact]
        public void GetFrame_ReturnsCorrectFrameInfo()
        {
            var gifData = CreateGifWithDelay();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            var frame = decoder.GetFrame(0);
            
            Assert.NotNull(frame);
            Assert.Equal(10, frame.FrameInfo.Duration); // 10 centiseconds = 100ms // 100ms
            
            frame.Bitmap.Dispose();
        }
        
        [Fact]
        public void GetFrame_HandlesTransparency()
        {
            var gifData = CreateGifWithTransparency();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            var frame = decoder.GetFrame(0);
            
            Assert.NotNull(frame);
            Assert.True(frame.FrameInfo.HasTransparency);
            
            frame.Bitmap.Dispose();
        }
        
        private byte[] CreateNonInterlacedGif()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });
            
            // Logical Screen Descriptor
            writer.Write((ushort)2);
            writer.Write((ushort)2);
            writer.Write((byte)0xF0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            
            // Global Color Table
            writer.Write(new byte[] { 0x00, 0x00, 0x00 });
            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF });
            
            // Image Descriptor (NOT interlaced)
            writer.Write((byte)0x2C);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)2);
            writer.Write((ushort)2);
            writer.Write((byte)0x00); // No interlace
            
            // Image Data
            writer.Write((byte)2);
            writer.Write((byte)4);
            writer.Write(new byte[] { 0x84, 0x01, 0x01, 0x00 });
            writer.Write((byte)0x00);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
        
        private byte[] CreateGifWithDelay()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });
            
            // Logical Screen Descriptor
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write((byte)0xF0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            
            // Global Color Table
            writer.Write(new byte[] { 0x00, 0x00, 0x00 });
            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF });
            
            // Graphics Control Extension with delay
            writer.Write((byte)0x21);
            writer.Write((byte)0xF9);
            writer.Write((byte)0x04);
            writer.Write((byte)0x00);
            writer.Write((ushort)10); // 10 centiseconds = 100ms
            writer.Write((byte)0);
            writer.Write((byte)0x00);
            
            // Image Descriptor
            writer.Write((byte)0x2C);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write((byte)0x00);
            
            // Image Data
            writer.Write((byte)2);
            writer.Write((byte)2);
            writer.Write(new byte[] { 0x84, 0x01 });
            writer.Write((byte)0x00);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
        
        private byte[] CreateGifWithTransparency()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });
            
            // Logical Screen Descriptor
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write((byte)0xF0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            
            // Global Color Table
            writer.Write(new byte[] { 0x00, 0x00, 0x00 });
            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF });
            
            // Graphics Control Extension with transparency
            writer.Write((byte)0x21);
            writer.Write((byte)0xF9);
            writer.Write((byte)0x04);
            writer.Write((byte)0x01); // Transparency flag set
            writer.Write((ushort)0);
            writer.Write((byte)0); // Transparent color index
            writer.Write((byte)0x00);
            
            // Image Descriptor
            writer.Write((byte)0x2C);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write((byte)0x00);
            
            // Image Data
            writer.Write((byte)2);
            writer.Write((byte)2);
            writer.Write(new byte[] { 0x84, 0x01 });
            writer.Write((byte)0x00);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
    }
}
