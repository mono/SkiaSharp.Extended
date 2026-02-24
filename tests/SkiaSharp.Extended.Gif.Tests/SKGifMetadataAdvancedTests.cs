using System;
using SkiaSharp;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class SKGifMetadataAdvancedTests
    {
        [Fact]
        public void SKGifInfo_IsAnimated_SingleFrame()
        {
            var gifInfo = new SKGifInfo
            {
                FrameCount = 1
            };
            
            Assert.False(gifInfo.IsAnimated); // Single frame is not animated
        }
        
        [Fact]
        public void SKGifInfo_IsAnimated_MultipleFrames()
        {
            var gifInfo = new SKGifInfo
            {
                FrameCount = 2
            };
            
            Assert.True(gifInfo.IsAnimated); // 2+ frames is animated
        }
        
        [Fact]
        public void SKGifInfo_ImageInfo_HasSize()
        {
            var gifInfo = new SKGifInfo
            {
                ImageInfo = new SKImageInfo(100, 200)
            };
            
            Assert.Equal(100, gifInfo.Width);
            Assert.Equal(200, gifInfo.Height);
        }
        
        [Fact]
        public void SKGifInfo_Width_ReflectsImageInfo()
        {
            var gifInfo = new SKGifInfo
            {
                ImageInfo = new SKImageInfo(640, 480)
            };
            
            Assert.Equal(640, gifInfo.Width);
        }
        
        [Fact]
        public void SKGifInfo_Height_ReflectsImageInfo()
        {
            var gifInfo = new SKGifInfo
            {
                ImageInfo = new SKImageInfo(640, 480)
            };
            
            Assert.Equal(480, gifInfo.Height);
        }
        
        [Fact]
        public void SKGifInfo_BackgroundColor_CanBeSet()
        {
            var gifInfo = new SKGifInfo
            {
                BackgroundColor = SKColors.Yellow
            };
            
            Assert.Equal(SKColors.Yellow, gifInfo.BackgroundColor);
        }
        
        [Fact]
        public void SKGifInfo_Comment_CanBeSet()
        {
            var gifInfo = new SKGifInfo
            {
                Comment = "Test comment"
            };
            
            Assert.Equal("Test comment", gifInfo.Comment);
        }
        
        [Fact]
        public void SKGifInfo_Comment_CanBeNull()
        {
            var gifInfo = new SKGifInfo
            {
                Comment = null
            };
            
            Assert.Null(gifInfo.Comment);
        }
        
        [Fact]
        public void SKGifInfo_ApplicationData_CanBeSet()
        {
            var data = new byte[] { 0x01, 0x02, 0x03 };
            var gifInfo = new SKGifInfo
            {
                ApplicationData = data
            };
            
            Assert.Equal(data, gifInfo.ApplicationData);
        }
        
        [Fact]
        public void SKGifInfo_ApplicationIdentifier_CanBeSet()
        {
            var gifInfo = new SKGifInfo
            {
                ApplicationIdentifier = "MYAPP"
            };
            
            Assert.Equal("MYAPP", gifInfo.ApplicationIdentifier);
        }
        
        [Fact]
        public void SKGifInfo_AllProperties_CanBeSet()
        {
            var gifInfo = new SKGifInfo
            {
                ImageInfo = new SKImageInfo(320, 240),
                FrameCount = 10,
                LoopCount = 5,
                BackgroundColor = SKColors.Black,
                Comment = "Animation",
                ApplicationIdentifier = "APP",
                ApplicationData = new byte[] { 0xFF }
            };
            
            Assert.Equal(320, gifInfo.Width);
            Assert.Equal(240, gifInfo.Height);
            Assert.Equal(10, gifInfo.FrameCount);
            Assert.Equal(5, gifInfo.LoopCount);
            Assert.Equal(SKColors.Black, gifInfo.BackgroundColor);
            Assert.Equal("Animation", gifInfo.Comment);
            Assert.Equal("APP", gifInfo.ApplicationIdentifier);
            Assert.Single(gifInfo.ApplicationData);
        }
    }
}
