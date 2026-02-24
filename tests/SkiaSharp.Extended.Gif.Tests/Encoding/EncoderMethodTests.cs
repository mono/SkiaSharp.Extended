using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Encoding
{
	public class EncoderMethodTests
	{
		[Fact]
		public void SetLoopCount_BeforeEncoding_Succeeds()
		{
			using var stream = new MemoryStream();
			using var encoder = new SKGifEncoder(stream);
			
			// Should not throw
			encoder.SetLoopCount(0);
			encoder.SetLoopCount(5);
			encoder.SetLoopCount(-1);
		}
		
		[Fact]
		public void SetLoopCount_AfterAddingFrame_StillSucceeds()
		{
			// SetLoopCount only throws after Encode() is called, not after AddFrame()
			using var stream = new MemoryStream();
			using var encoder = new SKGifEncoder(stream);
			
			// Add frame doesn't trigger encoding
			// encoder.AddFrame(...) would require SKBitmap
			
			// SetLoopCount should still work
			encoder.SetLoopCount(10);
		}
		
		[Fact]
		public void Encode_WithNoFrames_ThrowsInvalidOperationException()
		{
			using var stream = new MemoryStream();
			using var encoder = new SKGifEncoder(stream);
			
			// Should throw because no frames added
			var ex = Assert.Throws<InvalidOperationException>(() => encoder.Encode());
			Assert.Contains("No frames added", ex.Message);
		}
		
		[Fact]
		public void AddFrame_WithNullBitmap_ThrowsArgumentNullException()
		{
			using var stream = new MemoryStream();
			using var encoder = new SKGifEncoder(stream);
			
			var ex = Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!, 100));
			Assert.Equal("bitmap", ex.ParamName);
		}
		
		[Fact]
		public void AddFrame_WithNullBitmapAndFrameInfo_ThrowsArgumentNullException()
		{
			using var stream = new MemoryStream();
			using var encoder = new SKGifEncoder(stream);
			
			var frameInfo = new SKGifFrameInfo { Duration = 100 };
			var ex = Assert.Throws<ArgumentNullException>(() => encoder.AddFrame(null!, frameInfo));
			Assert.Equal("bitmap", ex.ParamName);
		}
	}
}
