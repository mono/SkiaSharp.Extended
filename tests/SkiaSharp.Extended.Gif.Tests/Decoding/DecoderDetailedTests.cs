using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
    public class DecoderDetailedTests
    {
        [Fact]
        public void Decoder_ParsesFrameCount()
        {
            var gifData = CreateTwoFrameGif();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(2, decoder.FrameCount);
        }
        
        [Fact]
        public void Decoder_ParsesLoopCount()
        {
            var gifData = CreateGifWithLoop(5);
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(5, decoder.GifInfo.LoopCount);
        }
        
        [Fact]
        public void Decoder_ParsesFrameInfo()
        {
            var gifData = CreateGifWithFrameInfo();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.NotNull(decoder.FrameInfo);
            Assert.True(decoder.FrameInfo.Length > 0);
            
            var firstFrame = decoder.FrameInfo[0];
            Assert.True(firstFrame.Duration >= 0);
        }
        
        [Fact]
        public void Decoder_HandlesNoLoopExtension()
        {
            var gifData = CreateGifWithoutLoop();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(-1, decoder.GifInfo.LoopCount);
        }
        
        private byte[] CreateTwoFrameGif()
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
            
            // Frame 1
            WriteImageBlock(writer);
            
            // Frame 2
            WriteImageBlock(writer);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
        
        private byte[] CreateGifWithLoop(int loopCount)
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
            
            // NETSCAPE loop extension
            writer.Write((byte)0x21); // Extension
            writer.Write((byte)0xFF); // Application
            writer.Write((byte)0x0B); // Block size
            writer.Write(new byte[] { 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30 });
            writer.Write((byte)0x03); // Sub-block size
            writer.Write((byte)0x01);
            writer.Write((ushort)loopCount);
            writer.Write((byte)0x00); // Terminator
            
            // Image
            WriteImageBlock(writer);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
        
        private byte[] CreateGifWithFrameInfo()
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
            
            // Graphics Control Extension
            writer.Write((byte)0x21);
            writer.Write((byte)0xF9);
            writer.Write((byte)0x04);
            writer.Write((byte)0x00); // Packed
            writer.Write((ushort)10); // 100ms delay
            writer.Write((byte)0);
            writer.Write((byte)0x00);
            
            // Image
            WriteImageBlock(writer);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
        
        private byte[] CreateGifWithoutLoop()
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
            
            // Image
            WriteImageBlock(writer);
            
            // Trailer
            writer.Write((byte)0x3B);
            
            return ms.ToArray();
        }
        
        private void WriteImageBlock(BinaryWriter writer)
        {
            // Image Descriptor
            writer.Write((byte)0x2C);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write((byte)0x00);
            
            // Image Data
            writer.Write((byte)2); // Min code size
            writer.Write((byte)2); // Sub-block size
            writer.Write(new byte[] { 0x84, 0x01 }); // Minimal LZW
            writer.Write((byte)0x00); // Terminator
        }
    }
}
