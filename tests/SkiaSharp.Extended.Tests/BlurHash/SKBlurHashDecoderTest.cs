using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKBlurHashDecoderTest
	{
		[Fact]
		public void NullDecodeThrows()
		{
			var blur = new SKBlurHashDecoder();

			Assert.Throws<ArgumentNullException>(() => blur.DecodeBitmap(null, 20, 10));
		}

		[Theory]
		[InlineData("")]
		[InlineData("1234")]
		[InlineData("123456")]
		[InlineData("1234567890")]
		public void InvalidDataThrows(string encoded)
		{
			var blur = new SKBlurHashDecoder();

			Assert.Throws<ArgumentException>(() => blur.DecodeBitmap(encoded, 20, 10));
		}

		[Theory]
		[InlineData(SKBlurHashTest.Image1)]
		[InlineData(SKBlurHashTest.Image1_8x6)]
		[InlineData(SKBlurHashTest.Image2)]
		[InlineData(SKBlurHashTest.Image2_8x6)]
		[InlineData(SKBlurHashTest.Image3)]
		[InlineData(SKBlurHashTest.Image4)]
		[InlineData(SKBlurHashTest.Image6)]
		[InlineData(SKBlurHashTest.Image7)]
		public void DecodeBitmap(string encoded)
		{
			var blur = new SKBlurHashDecoder();

			var bmp = blur.DecodeBitmap(encoded, 20, 10);

			Assert.NotNull(bmp);
		}

		[Theory]
		[InlineData(SKBlurHashTest.Image1)]
		[InlineData(SKBlurHashTest.Image1_8x6)]
		[InlineData(SKBlurHashTest.Image2)]
		[InlineData(SKBlurHashTest.Image2_8x6)]
		[InlineData(SKBlurHashTest.Image3)]
		[InlineData(SKBlurHashTest.Image4)]
		[InlineData(SKBlurHashTest.Image6)]
		[InlineData(SKBlurHashTest.Image7)]
		public void DecodeImage(string encoded)
		{
			var blur = new SKBlurHashDecoder();

			var img = blur.DecodeImage(encoded, 20, 10);

			Assert.NotNull(img);
		}

		[Theory]
		[InlineData(SKBlurHashTest.Image1)]
		[InlineData(SKBlurHashTest.Image1_8x6)]
		[InlineData(SKBlurHashTest.Image2)]
		[InlineData(SKBlurHashTest.Image2_8x6)]
		[InlineData(SKBlurHashTest.Image3)]
		[InlineData(SKBlurHashTest.Image4)]
		[InlineData(SKBlurHashTest.Image6)]
		[InlineData(SKBlurHashTest.Image7)]
		public void DecodeImageEqualsBitmap(string encoded)
		{
			var blur = new SKBlurHashDecoder();

			var bmp = blur.DecodeBitmap(encoded, 20, 10);
			var img = blur.DecodeImage(encoded, 20, 10);

			Assert.Equal(
				bmp.PeekPixels().GetPixelSpan().ToArray(),
				img.PeekPixels().GetPixelSpan().ToArray());
		}
	}
}
