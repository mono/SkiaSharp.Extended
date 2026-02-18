using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void CanEncodeAndDecodeSingleFrame()
        {
            // Create a simple bitmap
            using var bitmap = new SKBitmap(10, 10, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Red);
            }
            
            // Encode to GIF
            using var stream = new MemoryStream();
            using (var encoder = new SKGifEncoder(stream))
            {
                encoder.AddFrame(bitmap, 100);
                encoder.Encode();
            }
            
            // Verify data was written
            Assert.True(stream.Length > 0);
            
            // Decode
            stream.Position = 0;
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(1, decoder.FrameCount);
            Assert.Equal(10, decoder.Info.Width);
            Assert.Equal(10, decoder.Info.Height);
            
            var frame = decoder.GetFrame(0);
            Assert.NotNull(frame);
            Assert.NotNull(frame.Bitmap);
            Assert.Equal(10, frame.Bitmap.Width);
            Assert.Equal(10, frame.Bitmap.Height);
            
            frame.Bitmap.Dispose();
        }
        
        [Fact]
        public void CanEncodeAndDecodeMultipleFrames()
        {
            using var stream = new MemoryStream();
            
            // Encode 3 frames
            using (var encoder = new SKGifEncoder(stream))
            {
                encoder.SetLoopCount(0); // Loop forever
                
                for (int i = 0; i < 3; i++)
                {
                    using var bitmap = new SKBitmap(20, 20, SKColorType.Rgba8888, SKAlphaType.Premul);
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        var color = i == 0 ? SKColors.Red : (i == 1 ? SKColors.Green : SKColors.Blue);
                        canvas.Clear(color);
                    }
                    
                    encoder.AddFrame(bitmap, 100);
                }
                
                encoder.Encode();
            }
            
            // Decode
            stream.Position = 0;
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(3, decoder.FrameCount);
            Assert.Equal(20, decoder.Info.Width);
            Assert.Equal(20, decoder.Info.Height);
            Assert.Equal(0, decoder.GifInfo.LoopCount);
            
            for (int i = 0; i < 3; i++)
            {
                var frame = decoder.GetFrame(i);
                Assert.NotNull(frame.Bitmap);
                Assert.Equal(100, frame.FrameInfo.Duration);
                frame.Bitmap.Dispose();
            }
        }
        
        [Fact]
        public void EncodedGif_HasValidHeader()
        {
            using var bitmap = new SKBitmap(5, 5);
            using var stream = new MemoryStream();
            
            using (var encoder = new SKGifEncoder(stream))
            {
                encoder.AddFrame(bitmap);
                encoder.Encode();
            }
            
            stream.Position = 0;
            var header = new byte[6];
            stream.Read(header, 0, 6);
            
            // Should be "GIF89a"
            Assert.Equal(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, header);
        }
        
        [Fact]
        public void EncodedGif_HasValidTrailer()
        {
            using var bitmap = new SKBitmap(5, 5);
            using var stream = new MemoryStream();
            
            using (var encoder = new SKGifEncoder(stream))
            {
                encoder.AddFrame(bitmap);
                encoder.Encode();
            }
            
            stream.Position = stream.Length - 1;
            var trailer = stream.ReadByte();
            
            Assert.Equal(0x3B, trailer); // GIF trailer
        }
        
        [Fact]
        public void Decoder_HandlesSmallGif()
        {
            using var bitmap = new SKBitmap(1, 1);
            using var stream = new MemoryStream();
            
            using (var encoder = new SKGifEncoder(stream))
            {
                encoder.AddFrame(bitmap);
                encoder.Encode();
            }
            
            stream.Position = 0;
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(1, decoder.FrameCount);
            Assert.Equal(1, decoder.Info.Width);
            Assert.Equal(1, decoder.Info.Height);
        }
        
        [Fact]
        public void Decoder_HandlesLargeGif()
        {
            using var bitmap = new SKBitmap(500, 300);
            using var stream = new MemoryStream();
            
            using (var encoder = new SKGifEncoder(stream))
            {
                encoder.AddFrame(bitmap);
                encoder.Encode();
            }
            
            stream.Position = 0;
            using var decoder = SKGifDecoder.Create(stream);
            
            Assert.Equal(1, decoder.FrameCount);
            Assert.Equal(500, decoder.Info.Width);
            Assert.Equal(300, decoder.Info.Height);
        }
    }
}
