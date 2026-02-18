using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class DebugEncodeTest
    {
        [Fact]
        public void TestEncodeWithLogging()
        {
            Console.WriteLine("Creating bitmap");
            using var bitmap = new SKBitmap(2, 2, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Red);
            }
            
            Console.WriteLine("Creating stream");
            using var stream = new MemoryStream();
            
            Console.WriteLine("Creating encoder");
            using var encoder = new SKGifEncoder(stream);
            
            Console.WriteLine("Adding frame");
            encoder.AddFrame(bitmap, 100);
            
            Console.WriteLine("Calling Encode() - THIS IS WHERE IT HANGS");
            encoder.Encode();
            
            Console.WriteLine("Encode completed!");
            Assert.True(stream.Length > 0);
        }
    }
}
