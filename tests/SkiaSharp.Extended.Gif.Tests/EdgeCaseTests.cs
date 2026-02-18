using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class EdgeCaseTests
    {
        [Fact]
        public void Decoder_ThrowsOnNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => SKGifDecoder.Create(null!));
        }
        
        [Fact]
        public void Decoder_ThrowsOnInvalidHeader()
        {
            var invalidData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            using var stream = new MemoryStream(invalidData);
            
            Assert.Throws<InvalidDataException>(() => SKGifDecoder.Create(stream));
        }
        
        [Fact]
        public void Decoder_ThrowsOnTruncatedFile()
        {
            var truncated = new byte[] { 0x47, 0x49, 0x46 }; // Just "GIF"
            using var stream = new MemoryStream(truncated);
            
            Assert.Throws<InvalidDataException>(() => SKGifDecoder.Create(stream));
        }
        
        [Fact]
        public void Decoder_GetFrame_ValidatesIndex()
        {
            var gifData = CreateValidMinimalGif();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Throws<ArgumentOutOfRangeException>(() => decoder.GetFrame(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => decoder.GetFrame(999));
        }
        
        [Fact]
        public void Decoder_GetFrameInfo_ValidatesIndex()
        {
            var gifData = CreateValidMinimalGif();
            using var stream = new MemoryStream(gifData);
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Throws<ArgumentOutOfRangeException>(() => decoder.GetFrameInfo(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => decoder.GetFrameInfo(decoder.FrameCount));
        }
        
        [Fact]
        public void Encoder_ThrowsOnNullStream()
        {
            Assert.Throws<ArgumentNullException>(() => new SKGifEncoder(null!));
        }
        
        [Fact]
        public void Encoder_ThrowsOnReadOnlyStream()
        {
            using var readOnlyStream = new MemoryStream(new byte[100], writable: false);
            Assert.Throws<ArgumentException>(() => new SKGifEncoder(readOnlyStream));
        }
        
        [Fact]
        public void Encoder_AddFrame_ThrowsOnNull()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            
            Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!, 100));
        }
        
        [Fact]
        public void Encoder_AddFrame_ThrowsOnNullWithFrameInfo()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            var frameInfo = new SKGifFrameInfo();
            
            Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!, frameInfo));
        }
        
        [Fact]
        public void Encoder_Encode_ThrowsWhenNoFrames()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            
            Assert.Throws<InvalidOperationException>(() => encoder.Encode());
        }
        
        [Fact]
        public void Encoder_SetLoopCount_AcceptsValidValues()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            
            encoder.SetLoopCount(0); // Loop forever
            encoder.SetLoopCount(1); // Loop once
            encoder.SetLoopCount(100); // Loop 100 times
            encoder.SetLoopCount(-1); // No loop extension
        }
        
        [Fact]
        public void Encoder_Dispose_CanBeCalledMultipleTimes()
        {
            using var stream = new MemoryStream();
            var encoder = new SKGifEncoder(stream);
            
            encoder.Dispose();
            encoder.Dispose(); // Should not throw
        }
        
        [Fact]
        public void Decoder_Dispose_CanBeCalledMultipleTimes()
        {
            var gifData = CreateValidMinimalGif();
            using var stream = new MemoryStream(gifData);
            var decoder = SKGifDecoder.Create(stream);
            
            decoder.Dispose();
            decoder.Dispose(); // Should not throw
        }
        
        private byte[] CreateValidMinimalGif()
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
