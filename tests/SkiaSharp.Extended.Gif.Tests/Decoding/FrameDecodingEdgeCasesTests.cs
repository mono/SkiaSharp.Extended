using SkiaSharp.Extended.Gif.Decoding;
using SkiaSharp.Extended.Gif.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
	public class FrameDecodingEdgeCasesTests
	{
		[Fact]
		public void DecodeFrame_WithNonZeroPosition_CreatesFullSizeBitmap()
		{
			// Frame positioned at (10, 10) should create full-size bitmap
			var descriptor = new ImageDescriptor
			{
				Left = 10,
				Top = 10,
				Width = 20,
				Height = 20,
				InterlaceFlag = false
			};
			
			var frame = CreateTestFrame(descriptor);
			var bitmap = FrameDecoder.DecodeFrame(frame, 100, 100);
			
			// Should create 100x100 bitmap, not 20x20
			Assert.Equal(100, bitmap.Width);
			Assert.Equal(100, bitmap.Height);
			
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeFrame_SmallerThanScreen_CreatesFullSizeBitmap()
		{
			// Frame 50x50 in 100x100 screen
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 50,
				Height = 50,
				InterlaceFlag = false
			};
			
			var frame = CreateTestFrame(descriptor);
			var bitmap = FrameDecoder.DecodeFrame(frame, 100, 100);
			
			Assert.Equal(100, bitmap.Width);
			Assert.Equal(100, bitmap.Height);
			
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeFrame_WithTransparency_HandlesCorrectly()
		{
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 10,
				Height = 10,
				InterlaceFlag = false
			};
			
			var gce = new GraphicsControlExtension
			{
				DisposalMethod = 2,
				UserInputFlag = false,
				TransparencyFlag = true,
				DelayTime = 10,
				TransparentColorIndex = 0
			};
			
			var frame = CreateTestFrame(descriptor, gce);
			var bitmap = FrameDecoder.DecodeFrame(frame, 10, 10);
			
			Assert.NotNull(bitmap);
			bitmap.Dispose();
		}
		
		private ParsedFrame CreateTestFrame(ImageDescriptor descriptor, GraphicsControlExtension? gce = null)
		{
			// Create minimal valid LZW compressed data
			byte minCodeSize = 2;
			var clearCode = 1 << minCodeSize;
			var endCode = clearCode + 1;
			
			var data = new byte[10];
			data[0] = (byte)clearCode;
			data[1] = 0;
			data[2] = (byte)endCode;
			
			return new ParsedFrame
			{
				ImageDescriptor = descriptor,
				LocalColorTable = CreateTestColorTable(),
				CompressedData = data,
				LzwMinimumCodeSize = minCodeSize,
				GraphicsControlExtension = gce
			};
		}
		
		private SKColor[] CreateTestColorTable()
		{
			var table = new SKColor[256];
			for (int i = 0; i < 256; i++)
				table[i] = new SKColor((byte)i, (byte)i, (byte)i);
			return table;
		}
	}
}
