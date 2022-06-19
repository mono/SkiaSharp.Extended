using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKBlurHashSerializerTest
	{
		[Fact]
		public void NullEncodeThrows()
		{
			Assert.Throws<ArgumentNullException>(() => SKBlurHash.Serialize((SKBitmap)null, 4, 3));
		}

		[Theory]
		[InlineData("img1.jpg", 4, 3, SKBlurHashTest.Image1)]
		[InlineData("img1.jpg", 8, 6, SKBlurHashTest.Image1_8x6)]
		[InlineData("img2.jpg", 4, 3, SKBlurHashTest.Image2)]
		[InlineData("img2.jpg", 8, 6, SKBlurHashTest.Image2_8x6)]
		[InlineData("img3.jpg", 4, 3, SKBlurHashTest.Image3)]
		[InlineData("img4.jpg", 4, 3, SKBlurHashTest.Image4)]
		[InlineData("img6.png", 4, 3, SKBlurHashTest.Image6)]
		[InlineData("img7.png", 4, 3, SKBlurHashTest.Image7)]
		public void CanEncodeImage(string source, int compX, int compY, string expected)
		{
			using var img = SKImage.FromEncodedData(Path.Combine("images", source));

			var result = SKBlurHash.Serialize(img, compX, compY);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("img1.jpg", 4, 3, SKBlurHashTest.Image1)]
		[InlineData("img1.jpg", 8, 6, SKBlurHashTest.Image1_8x6)]
		[InlineData("img2.jpg", 4, 3, SKBlurHashTest.Image2)]
		[InlineData("img2.jpg", 8, 6, SKBlurHashTest.Image2_8x6)]
		[InlineData("img3.jpg", 4, 3, SKBlurHashTest.Image3)]
		[InlineData("img4.jpg", 4, 3, SKBlurHashTest.Image4)]
		[InlineData("img6.png", 4, 3, SKBlurHashTest.Image6)]
		[InlineData("img7.png", 4, 3, SKBlurHashTest.Image7)]
		public void CanEncodeBitmap(string source, int compX, int compY, string expected)
		{
			using var img = SKBitmap.Decode(Path.Combine("images", source));

			var result = SKBlurHash.Serialize(img, compX, compY);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("img1.jpg", 4, 3, SKBlurHashTest.Image1)]
		[InlineData("img1.jpg", 8, 6, SKBlurHashTest.Image1_8x6)]
		[InlineData("img2.jpg", 4, 3, SKBlurHashTest.Image2)]
		[InlineData("img2.jpg", 8, 6, SKBlurHashTest.Image2_8x6)]
		[InlineData("img3.jpg", 4, 3, SKBlurHashTest.Image3)]
		[InlineData("img4.jpg", 4, 3, SKBlurHashTest.Image4)]
		[InlineData("img6.png", 4, 3, SKBlurHashTest.Image6)]
		[InlineData("img7.png", 4, 3, SKBlurHashTest.Image7)]
		public void CanEncodePixmap(string source, int compX, int compY, string expected)
		{
			using var img = SKBitmap.Decode(Path.Combine("images", source));
			using var pixmap = img.PeekPixels();

			var result = SKBlurHash.Serialize(pixmap, compX, compY);

			Assert.Equal(expected, result);
		}
	}
}
