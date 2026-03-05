using System;
using Xunit;
using SkiaSharp;

namespace SkiaSharp.Extended.Tests
{
    /// <summary>
    /// Tests verifying that GetPixelSpan&lt;SKColor&gt;() is a raw memory blit with
    /// no color-type swizzle, and that our comparer code handles this correctly.
    /// SKColor's uint is ARGB-packed (0xFFAABBCC for R=AA,G=BB,B=CC,A=FF).
    /// On little-endian, that's bytes [CC, BB, AA, FF] which matches BGRA8888
    /// layout [B, G, R, A]. On RGBA8888 platforms, raw span writes produce
    /// swapped R/B channels in the pixel buffer.
    /// </summary>
    public class SKColorLayoutTest
    {
        [Fact]
        public void SKColorUintIsArgbPacked()
        {
            var color = new SKColor(0xAA, 0xBB, 0xCC, 0xFF);
            Assert.Equal((uint)0xFFAABBCC, (uint)color);
        }

        [Fact]
        public void SpanWriteToBgra8888RoundtripsCorrectly()
        {
            var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888));
            using var px = bitmap.PeekPixels();
            px.GetPixelSpan<SKColor>()[0] = new SKColor(0xAA, 0xBB, 0xCC, 0xFF);

            // BGRA8888 raw bytes match SKColor LE layout
            var raw = px.GetPixelSpan<byte>();
            Assert.Equal(0xCC, raw[0]); // B
            Assert.Equal(0xBB, raw[1]); // G
            Assert.Equal(0xAA, raw[2]); // R
            Assert.Equal(0xFF, raw[3]); // A

            var c = px.GetPixelSpan<SKColor>()[0];
            Assert.Equal(0xAA, c.Red);
            Assert.Equal(0xBB, c.Green);
            Assert.Equal(0xCC, c.Blue);
        }

        [Fact]
        public void DiffImageEncodesWithCorrectChannels()
        {
            // Verify the full pipeline: generate diff → encode to PNG → decode → check channels
            using var first = CreateTestImage(0xFF100000); // R=16
            using var second = CreateTestImage(0xFF200000); // R=32

            using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

            // Encode and decode to verify the image has correct channel data
            using var encoded = diff.Encode(SKEncodedImageFormat.Png, 100);
            using var decoded = SKImage.FromEncodedData(encoded);

            // Normalize to BGRA8888 for reliable SKColor span reading
            var normalized = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888));
            using (var canvas = new SKCanvas(normalized))
                canvas.DrawImage(decoded, 0, 0);

            using var px = normalized.PeekPixels();
            var c = px.GetPixelSpan<SKColor>()[0];

            // Diff should be R=16, G=0, B=0 (only red channel differs)
            Assert.Equal(16, c.Red);
            Assert.Equal(0, c.Green);
            Assert.Equal(0, c.Blue);
        }

        [Fact]
        public void DiffMaskEncodesWithCorrectChannels()
        {
            using var first = CreateTestImage(0xFF000000);
            using var second = CreateTestImage(0xFF010101);

            using var mask = SKPixelComparer.GenerateDifferenceMask(first, second);
            using var encoded = mask.Encode(SKEncodedImageFormat.Png, 100);
            using var decoded = SKImage.FromEncodedData(encoded);

            var normalized = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888));
            using (var canvas = new SKCanvas(normalized))
                canvas.DrawImage(decoded, 0, 0);

            using var px = normalized.PeekPixels();
            var c = px.GetPixelSpan<SKColor>()[0];

            // Different pixels should be white
            Assert.Equal(255, c.Red);
            Assert.Equal(255, c.Green);
            Assert.Equal(255, c.Blue);
        }

        private static SKImage CreateTestImage(uint color)
        {
            var bitmap = new SKBitmap(5, 5);
            bitmap.Erase(new SKColor(color));
            return SKImage.FromBitmap(bitmap);
        }
    }
}
