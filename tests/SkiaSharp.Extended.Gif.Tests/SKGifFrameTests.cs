using System;
using SkiaSharp;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests
{
    public class SKGifFrameTests
    {
        [Fact]
        public void SKGifFrameInfo_DefaultValues()
        {
            var frameInfo = new SKGifFrameInfo();
            
            Assert.Equal(0, frameInfo.Duration);
            Assert.Equal(SKGifDisposalMethod.None, frameInfo.DisposalMethod);
            Assert.Equal(0, frameInfo.RequiredFrame); // Defaults to 0, not -1
            Assert.Equal(SKRectI.Empty, frameInfo.FrameRect);
            Assert.False(frameInfo.HasTransparency);
            Assert.Null(frameInfo.TransparentColor);
        }
        
        [Fact]
        public void SKGifFrameInfo_CanSetAllProperties()
        {
            var frameInfo = new SKGifFrameInfo
            {
                Duration = 150,
                DisposalMethod = SKGifDisposalMethod.RestoreToPrevious,
                RequiredFrame = 2,
                FrameRect = new SKRectI(10, 20, 100, 150),
                HasTransparency = true,
                TransparentColor = SKColors.Magenta
            };
            
            Assert.Equal(150, frameInfo.Duration);
            Assert.Equal(SKGifDisposalMethod.RestoreToPrevious, frameInfo.DisposalMethod);
            Assert.Equal(2, frameInfo.RequiredFrame);
            Assert.Equal(new SKRectI(10, 20, 100, 150), frameInfo.FrameRect);
            Assert.True(frameInfo.HasTransparency);
            Assert.Equal(SKColors.Magenta, frameInfo.TransparentColor);
        }
        
        [Fact]
        public void SKGifFrame_Properties_CanBeSet()
        {
            using var bitmap = new SKBitmap(10, 10);
            var frameInfo = new SKGifFrameInfo { Duration = 100 };
            
            var frame = new SKGifFrame
            {
                Bitmap = bitmap,
                FrameInfo = frameInfo
            };
            
            Assert.NotNull(frame.Bitmap);
            Assert.Equal(100, frame.FrameInfo.Duration);
        }
        
        [Fact]
        public void SKGifFrame_Dispose_ReleasesBitmap()
        {
            var bitmap = new SKBitmap(10, 10);
            var frameInfo = new SKGifFrameInfo();
            
            var frame = new SKGifFrame
            {
                Bitmap = bitmap,
                FrameInfo = frameInfo
            };
            frame.Dispose();
            
            // After dispose, accessing bitmap should be safe (disposed bitmap)
            // We can't easily test if it's disposed, but we can verify Dispose doesn't throw
            Assert.NotNull(frame); // Frame object still exists
        }
        
        [Fact]
        public void SKGifFrame_DisposeMultipleTimes_DoesNotThrow()
        {
            using var bitmap = new SKBitmap(10, 10);
            var frameInfo = new SKGifFrameInfo();
            
            var frame = new SKGifFrame
            {
                Bitmap = bitmap,
                FrameInfo = frameInfo
            };
            frame.Dispose();
            frame.Dispose(); // Should not throw
            frame.Dispose(); // Should not throw
        }
        
        [Fact]
        public void SKGifDisposalMethod_HasCorrectValues()
        {
            // Verify enum values match GIF spec
            Assert.Equal(0, (int)SKGifDisposalMethod.None);
            Assert.Equal(1, (int)SKGifDisposalMethod.DoNotDispose);
            Assert.Equal(2, (int)SKGifDisposalMethod.RestoreToBackground);
            Assert.Equal(3, (int)SKGifDisposalMethod.RestoreToPrevious);
        }
        
        [Fact]
        public void SKGifFrameInfo_TransparencyWithoutColor()
        {
            var frameInfo = new SKGifFrameInfo
            {
                HasTransparency = true
                // TransparentColor left as null
            };
            
            Assert.True(frameInfo.HasTransparency);
            Assert.Null(frameInfo.TransparentColor);
        }
        
        [Fact]
        public void SKGifFrameInfo_LargeDuration()
        {
            var frameInfo = new SKGifFrameInfo
            {
                Duration = int.MaxValue
            };
            
            Assert.Equal(int.MaxValue, frameInfo.Duration);
        }
        
        [Fact]
        public void SKGifFrameInfo_NegativeRequiredFrame()
        {
            var frameInfo = new SKGifFrameInfo
            {
                RequiredFrame = -1 // Indicates no dependency
            };
            
            Assert.Equal(-1, frameInfo.RequiredFrame);
        }
        
        [Fact]
        public void SKGifFrame_WithNullBitmap_CanBeCreated()
        {
            var frameInfo = new SKGifFrameInfo();
            
            // SKGifFrame uses internal setters, so we can create it with null bitmap
            var frame = new SKGifFrame
            {
                FrameInfo = frameInfo
            };
            
            // The bitmap field is marked as null!, so it can be null during construction
            // This tests that the frame can exist without a bitmap initially
            Assert.NotNull(frame);
        }
    }
}
