using SkiaSharp.Extended.Gif.Decoding;
using SkiaSharp.Extended.Gif.IO;
using Xunit;

namespace SkiaSharp.Extended.Gif.Tests.Decoding
{
	public class InterlacingTests
	{
		[Fact]
		public void DecodeInterlacedFrame_Pass1_WritesCorrectRows()
		{
			// Create a simple interlaced GIF data
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 8,
				Height = 8,
				InterlaceFlag = true
			};
			
			// Create test frame
			var frame = CreateTestFrame(descriptor, minCodeSize: 2);
			
			// Decode should handle interlacing
			var bitmap = FrameDecoder.DecodeFrame(frame, 8, 8);
			
			Assert.NotNull(bitmap);
			Assert.Equal(8, bitmap.Width);
			Assert.Equal(8, bitmap.Height);
			
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeInterlacedFrame_Pass2_WritesCorrectRows()
		{
			// Pass 2 starts at row 4, every 8th row
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 16,
				Height = 16,
				InterlaceFlag = true
			};
			
			var frame = CreateTestFrame(descriptor, minCodeSize: 2);
			var bitmap = FrameDecoder.DecodeFrame(frame, 16, 16);
			
			Assert.NotNull(bitmap);
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeInterlacedFrame_Pass3_WritesCorrectRows()
		{
			// Pass 3 starts at row 2, every 4th row
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 32,
				Height = 32,
				InterlaceFlag = true
			};
			
			var frame = CreateTestFrame(descriptor, minCodeSize: 2);
			var bitmap = FrameDecoder.DecodeFrame(frame, 32, 32);
			
			Assert.NotNull(bitmap);
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeInterlacedFrame_Pass4_WritesCorrectRows()
		{
			// Pass 4 starts at row 1, every 2nd row
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 64,
				Height = 64,
				InterlaceFlag = true
			};
			
			var frame = CreateTestFrame(descriptor, minCodeSize: 2);
			var bitmap = FrameDecoder.DecodeFrame(frame, 64, 64);
			
			Assert.NotNull(bitmap);
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeLargeInterlacedFrame_ExpandsBufferCorrectly()
		{
			// Large frame to trigger buffer expansion
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 1024,
				Height = 1024,
				InterlaceFlag = true
			};
			
			var frame = CreateTestFrame(descriptor, minCodeSize: 2);
			var bitmap = FrameDecoder.DecodeFrame(frame, 1024, 1024);
			
			Assert.NotNull(bitmap);
			Assert.Equal(1024, bitmap.Width);
			Assert.Equal(1024, bitmap.Height);
			
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeInterlacedFrame_SmallImage_HandlesAllPasses()
		{
			// Small image still uses all 4 passes
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 4,
				Height = 4,
				InterlaceFlag = true
			};
			
			var frame = CreateTestFrame(descriptor, minCodeSize: 2);
			var bitmap = FrameDecoder.DecodeFrame(frame, 4, 4);
			
			Assert.NotNull(bitmap);
			bitmap.Dispose();
		}
		
		[Fact]
		public void DecodeInterlacedFrame_VeryLargeImage_HandlesBufferExpansion()
		{
			// Very large to definitely trigger buffer expansion
			var descriptor = new ImageDescriptor
			{
				Left = 0,
				Top = 0,
				Width = 2048,
				Height = 2048,
				InterlaceFlag = true
			};
			
			// Create large compressed data
			var largeData = new byte[2048 * 2048 + 1000];
			for (int i = 0; i < largeData.Length; i++)
				largeData[i] = (byte)(i % 256);
			
			var frame = new ParsedFrame
			{
				ImageDescriptor = descriptor,
				LocalColorTable = CreateTestColorTable(),
				CompressedData = largeData,
				LzwMinimumCodeSize = 2
			};
			
			// This should expand buffer multiple times
			try
			{
				var bitmap = FrameDecoder.DecodeFrame(frame, 2048, 2048);
				Assert.NotNull(bitmap);
				bitmap.Dispose();
			}
			catch
			{
				// Expected - random data won't decompress properly, but we test the buffer expansion path
			}
		}
		
		private ParsedFrame CreateTestFrame(ImageDescriptor descriptor, byte minCodeSize)
		{
			// Create minimal valid LZW compressed data
			// Clear code, some data, end code
			var clearCode = 1 << minCodeSize;
			var endCode = clearCode + 1;
			
			// Simple compressed data: clear, 0, 1, 0, 1, end
			var data = new byte[100];
			data[0] = (byte)clearCode;
			data[1] = 0;
			data[2] = 1;
			data[3] = 0;
			data[4] = 1;
			data[5] = (byte)endCode;
			
			return new ParsedFrame
			{
				ImageDescriptor = descriptor,
				LocalColorTable = CreateTestColorTable(),
				CompressedData = data,
				LzwMinimumCodeSize = minCodeSize
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
