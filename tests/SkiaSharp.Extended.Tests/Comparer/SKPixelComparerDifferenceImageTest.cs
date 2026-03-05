#nullable enable
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerDifferenceImageTest : SKPixelComparerTestBase
	{
		[Fact]
		public void GenerateDifferenceImageShowsPerChannelDifferences()
		{
			using var first = CreateTestImage(0xFF102030);
			using var second = CreateTestImage(0xFF132639);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			Assert.Equal(5, diff.Width);
			Assert.Equal(5, diff.Height);

			using var bitmap = GetNormalizedBitmap(diff);
			using var pixmap = bitmap.PeekPixels();
			var pixel = pixmap.GetPixelSpan<SKColor>()[0];

			Assert.Equal(3, pixel.Red);
			Assert.Equal(6, pixel.Green);
			Assert.Equal(9, pixel.Blue);
		}

		[Fact]
		public void GenerateDifferenceImageIdenticalIsBlack()
		{
			using var first = CreateTestImage(0xFFAABBCC);
			using var second = CreateTestImage(0xFFAABBCC);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			using var bitmap = GetNormalizedBitmap(diff);
			using var pixmap = bitmap.PeekPixels();
			var pixel = pixmap.GetPixelSpan<SKColor>()[0];

			Assert.Equal(0, pixel.Red);
			Assert.Equal(0, pixel.Green);
			Assert.Equal(0, pixel.Blue);
		}

		[Fact]
		public void GenerateDifferenceImageWithBitmapsWorks()
		{
			using var first = CreateTestBitmap(0xFF000000);
			using var second = CreateTestBitmap(0xFF050505);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			Assert.NotNull(diff);
			Assert.Equal(5, diff.Width);
			Assert.Equal(5, diff.Height);
		}

		[Fact]
		public void GenerateDifferenceImageWithDifferentSizesFails()
		{
			using var first = CreateTestImage(SKColors.Black, 5, 5);
			using var second = CreateTestImage(SKColors.Black, 10, 10);

			Assert.Throws<System.InvalidOperationException>(() => SKPixelComparer.GenerateDifferenceImage(first, second));
		}

		[Fact]
		public void GenerateDifferenceImageProducesValidImage()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF050505);

			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second);

			Assert.NotNull(diff);
			Assert.Equal(5, diff.Width);
			Assert.Equal(5, diff.Height);
		}

		[Fact]
		public void NullImagesThrowForDifferenceImage()
		{
			using var image = CreateTestImage(SKColors.Black);

			Assert.Throws<ArgumentNullException>(() => SKPixelComparer.GenerateDifferenceImage(null!, image));
			Assert.Throws<ArgumentNullException>(() => SKPixelComparer.GenerateDifferenceImage(image, null!));
		}
	}
}
