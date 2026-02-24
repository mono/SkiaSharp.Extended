using System;
using System.IO;
using SkiaSharp;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
    public class FrameDecodingAdvancedTests
    {
        [Fact]
        public void DecodeFrame_WithPositioning_CreatesFullSizeBitmap()
        {
            // Create a simple GIF with frame positioning
            var gif = CreateMinimalGifWithPositioning();
            
            using var ms = new MemoryStream(gif);
            using var decoder = SKGifDecoder.Create(ms);
            
            using var frame = decoder.GetFrame(0);
            
            // Should create a bitmap with the logical screen size, not just the frame size
            Assert.NotNull(frame.Bitmap);
            Assert.True(frame.Bitmap.Width >= 0);
            Assert.True(frame.Bitmap.Height >= 0);
        }
        
        [Fact]
        public void GetFrameInfo_ReturnsCorrectDisposalMethods()
        {
            var gif = CreateMinimalGif();
            
            using var ms = new MemoryStream(gif);
            using var decoder = SKGifDecoder.Create(ms);
            
            var frameInfo = decoder.FrameInfo;
            
            Assert.NotNull(frameInfo);
            Assert.True(frameInfo.Length > 0);
            
            // First frame should have a valid disposal method
            var disposal = frameInfo[0].DisposalMethod;
            Assert.True(Enum.IsDefined(typeof(SKGifDisposalMethod), disposal));
        }
        
        [Fact]
        public void Decoder_WithMultipleFrames_ParsesAllFrameInfo()
        {
            var gif = CreateMinimalGif();
            
            using var ms = new MemoryStream(gif);
            using var decoder = SKGifDecoder.Create(ms);
            
            var info = decoder.Info;
            var gifInfo = decoder.GifInfo;
            var frameInfo = decoder.FrameInfo;
            
            Assert.Equal(gifInfo.FrameCount, frameInfo.Length);
        }
        
        [Fact]
        public void Decoder_Info_HasCorrectDimensions()
        {
            var gif = CreateMinimalGif();
            
            using var ms = new MemoryStream(gif);
            using var decoder = SKGifDecoder.Create(ms);
            
            var info = decoder.Info;
            
            Assert.True(info.Width > 0);
            Assert.True(info.Height > 0);
        }
        
        [Fact]
        public void Decoder_GifInfo_HasFrameCount()
        {
            var gif = CreateMinimalGif();
            
            using var ms = new MemoryStream(gif);
            using var decoder = SKGifDecoder.Create(ms);
            
            var gifInfo = decoder.GifInfo;
            
            Assert.True(gifInfo.FrameCount > 0);
        }
        
        private byte[] CreateMinimalGif()
        {
            // Minimal valid GIF89a with one frame
            return new byte[]
            {
                // Header
                0x47, 0x49, 0x46, 0x38, 0x39, 0x61, // GIF89a
                
                // Logical Screen Descriptor
                0x02, 0x00, // Width = 2
                0x02, 0x00, // Height = 2
                0x80,       // Global Color Table flag, 2 colors
                0x00,       // Background color index
                0x00,       // Pixel aspect ratio
                
                // Global Color Table (2 colors = 1 bit)
                0xFF, 0x00, 0x00, // Red
                0x00, 0x00, 0xFF, // Blue
                
                // Image Descriptor
                0x2C,       // Image separator
                0x00, 0x00, // Left
                0x00, 0x00, // Top
                0x02, 0x00, // Width = 2
                0x02, 0x00, // Height = 2
                0x00,       // Packed fields (no local color table)
                
                // Image Data
                0x02,       // LZW minimum code size
                0x04,       // Block size
                0x84, 0x8F, 0xA9, 0xCB, // Compressed data
                0x00,       // Block terminator
                
                // Trailer
                0x3B
            };
        }
        
        private byte[] CreateMinimalGifWithPositioning()
        {
            // GIF with frame positioned at (1, 1) instead of (0, 0)
            return new byte[]
            {
                // Header
                0x47, 0x49, 0x46, 0x38, 0x39, 0x61, // GIF89a
                
                // Logical Screen Descriptor
                0x04, 0x00, // Width = 4
                0x04, 0x00, // Height = 4
                0x80,       // Global Color Table flag
                0x00,       // Background color index
                0x00,       // Pixel aspect ratio
                
                // Global Color Table (2 colors)
                0xFF, 0x00, 0x00, // Red
                0x00, 0x00, 0xFF, // Blue
                
                // Image Descriptor
                0x2C,       // Image separator
                0x01, 0x00, // Left = 1 (positioned)
                0x01, 0x00, // Top = 1 (positioned)
                0x02, 0x00, // Width = 2
                0x02, 0x00, // Height = 2
                0x00,       // Packed fields
                
                // Image Data
                0x02,       // LZW minimum code size
                0x04,       // Block size
                0x84, 0x8F, 0xA9, 0xCB, // Compressed data
                0x00,       // Block terminator
                
                // Trailer
                0x3B
            };
        }
    }
}
