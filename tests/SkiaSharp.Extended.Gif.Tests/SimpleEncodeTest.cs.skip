using System;
using System.IO;
using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class SimpleEncodeTest
    {
        [Fact]
        public void DebugEncodeStep()
        {
            Console.WriteLine("Creating bitmap...");
            using var bitmap = new SKBitmap(10, 10, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Red);
            }
            Console.WriteLine("Bitmap created");
            
            Console.WriteLine("Creating encoder...");
            using var stream = new MemoryStream();
            using var encoder = new SKGifEncoder(stream);
            Console.WriteLine("Encoder created");
            
            Console.WriteLine("Adding frame...");
            encoder.AddFrame(bitmap, 100);
            Console.WriteLine("Frame added");
            
            Console.WriteLine("Starting encode...");
            try
            {
                encoder.Encode();
                Console.WriteLine("Encode completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
            
            Console.WriteLine($"Stream length: {stream.Length}");
            Assert.True(stream.Length > 0);
        }
    }
}
