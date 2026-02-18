using System;
using SkiaSharp;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class SKGifMetadataTests
    {
        [Fact]
        public void SKGifInfo_DefaultConstructor()
        {
            var gifInfo = new SKGifInfo();
            
            Assert.Equal(-1, gifInfo.LoopCount); // Defaults to -1
            Assert.Equal(0, gifInfo.FrameCount);
        }
        
        [Fact]
        public void SKGifInfo_SetLoopCount()
        {
            var gifInfo = new SKGifInfo
            {
                LoopCount = 5
            };
            
            Assert.Equal(5, gifInfo.LoopCount);
        }
        
        [Fact]
        public void SKGifInfo_SetFrameCount()
        {
            var gifInfo = new SKGifInfo
            {
                FrameCount = 10
            };
            
            Assert.Equal(10, gifInfo.FrameCount);
        }
        
        [Fact]
        public void SKGifInfo_InfiniteLoop()
        {
            var gifInfo = new SKGifInfo
            {
                LoopCount = 0 // 0 means loop forever
            };
            
            Assert.Equal(0, gifInfo.LoopCount);
        }
        
        [Fact]
        public void SKGifInfo_NoLoopExtension()
        {
            var gifInfo = new SKGifInfo
            {
                LoopCount = -1 // -1 means no loop extension
            };
            
            Assert.Equal(-1, gifInfo.LoopCount);
        }
        
        [Fact]
        public void SKGifInfo_LargeFrameCount()
        {
            var gifInfo = new SKGifInfo
            {
                FrameCount = 1000
            };
            
            Assert.Equal(1000, gifInfo.FrameCount);
        }
        
        [Fact]
        public void SKGifInfo_AllPropertiesSet()
        {
            var gifInfo = new SKGifInfo
            {
                LoopCount = 3,
                FrameCount = 25
            };
            
            Assert.Equal(3, gifInfo.LoopCount);
            Assert.Equal(25, gifInfo.FrameCount);
        }
        
        [Fact]
        public void SKGifInfo_CanModifyAfterCreation()
        {
            var gifInfo = new SKGifInfo { LoopCount = 1 };
            Assert.Equal(1, gifInfo.LoopCount);
            
            gifInfo.LoopCount = 10;
            Assert.Equal(10, gifInfo.LoopCount);
            
            gifInfo.FrameCount = 50;
            Assert.Equal(50, gifInfo.FrameCount);
        }
    }
}
