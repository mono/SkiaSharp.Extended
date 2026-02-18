using System;
using SkiaSharp.Extended.Gif.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.IO
{
    public class GifStructuresTests
    {
        [Fact]
        public void GifHeader_ValidatesGif89a()
        {
            var header = new GifHeader
            {
                Signature = "GIF",
                Version = "89a"
            };
            
            Assert.True(header.IsValid);
            Assert.True(header.IsGif89a);
        }
        
        [Fact]
        public void GifHeader_ValidatesGif87a()
        {
            var header = new GifHeader
            {
                Signature = "GIF",
                Version = "87a"
            };
            
            Assert.True(header.IsValid);
            Assert.False(header.IsGif89a);
        }
        
        [Fact]
        public void GifHeader_InvalidSignature()
        {
            var header = new GifHeader
            {
                Signature = "XXX",
                Version = "89a"
            };
            
            Assert.False(header.IsValid);
        }
        
        [Fact]
        public void LogicalScreenDescriptor_CalculatesColorTableLength()
        {
            var descriptor = new LogicalScreenDescriptor
            {
                HasGlobalColorTable = true,
                GlobalColorTableSize = 2
            };
            
            Assert.Equal(8, descriptor.GlobalColorTableLength); // 2^(2+1) = 8
        }
        
        [Fact]
        public void LogicalScreenDescriptor_NoColorTableReturnsZero()
        {
            var descriptor = new LogicalScreenDescriptor
            {
                HasGlobalColorTable = false
            };
            
            Assert.Equal(0, descriptor.GlobalColorTableLength);
        }
        
        [Fact]
        public void ImageDescriptor_CalculatesLocalColorTableLength()
        {
            var descriptor = new ImageDescriptor
            {
                HasLocalColorTable = true,
                LocalColorTableSize = 3
            };
            
            Assert.Equal(16, descriptor.LocalColorTableLength); // 2^(3+1) = 16
        }
        
        [Fact]
        public void GraphicsControlExtension_ConvertsDelayToMilliseconds()
        {
            var gce = new GraphicsControlExtension
            {
                DelayTime = 10 // centiseconds
            };
            
            Assert.Equal(100, gce.DelayMs);
        }
        
        [Fact]
        public void ApplicationExtension_RecognizesNetscape()
        {
            var appExt = new ApplicationExtension
            {
                ApplicationIdentifier = "NETSCAPE",
                AuthenticationCode = new byte[] { 0x32, 0x2E, 0x30 }, // "2.0"
                Data = new byte[] { 0x01, 0x00, 0x00 } // Loop count = 0
            };
            
            Assert.True(appExt.IsNetscapeExtension);
            Assert.True(appExt.LoopCount.HasValue);
            Assert.Equal(0, appExt.LoopCount.Value);
        }
        
        [Fact]
        public void ApplicationExtension_NonNetscape()
        {
            var appExt = new ApplicationExtension
            {
                ApplicationIdentifier = "OTHER",
                AuthenticationCode = new byte[] { 0x00, 0x00, 0x00 },
                Data = new byte[] { }
            };
            
            Assert.False(appExt.IsNetscapeExtension);
            Assert.False(appExt.LoopCount.HasValue);
        }
    }
}
