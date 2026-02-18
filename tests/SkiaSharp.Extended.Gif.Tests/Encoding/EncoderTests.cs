using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Encoding
{
    public class EncoderTests
    {
        [Fact]
        public void Encoder_RequiresWritableStream()
        {
            using var readOnlyStream = new MemoryStream(new byte[100], writable: false);
            Assert.Throws<ArgumentException>(() => new SKGifEncoder(readOnlyStream));
        }
        
        [Fact]
        public void Encoder_CanSetLoopCount()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            
            encoder.SetLoopCount(0); // Loop forever
            encoder.SetLoopCount(5); // Loop 5 times
            encoder.SetLoopCount(-1); // No loop
        }
        
        [Fact]
        public void Encoder_RequiresAtLeastOneFrame()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            
            Assert.Throws<InvalidOperationException>(() => encoder.Encode());
        }
        
        [Fact]
        public void AddFrame_WithFrameInfo_ValidatesNull()
        {
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            
            var frameInfo = new SKGifFrameInfo { Duration = 100 };
            Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!, frameInfo));
        }
    }
}
