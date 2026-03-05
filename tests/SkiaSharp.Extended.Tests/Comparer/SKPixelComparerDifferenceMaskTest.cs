#nullable enable
using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKPixelComparerDifferenceMaskTest : SKPixelComparerTestBase
	{
		[Fact]
		public void GenerateDifferenceMaskProducesValidImage()
		{
			using var first = CreateTestImage(0xFF000000);
			using var second = CreateTestImage(0xFF050505);

			using var mask = SKPixelComparer.GenerateDifferenceMask(first, second);

			Assert.NotNull(mask);
			Assert.Equal(5, mask.Width);
			Assert.Equal(5, mask.Height);
		}

		[Fact]
		public void GenerateDifferenceMaskDifferentSizesFails()
		{
			using var first = CreateTestImage(SKColors.Black, 5, 5);
			using var second = CreateTestImage(SKColors.Black, 10, 10);

			Assert.Throws<InvalidOperationException>(() => SKPixelComparer.GenerateDifferenceMask(first, second));
		}

		[Fact]
		public void NullImagesThrowForDifferenceMask()
		{
			using var image = CreateTestImage(SKColors.Black);

			Assert.Throws<ArgumentNullException>(() => SKPixelComparer.GenerateDifferenceMask(null!, image));
			Assert.Throws<ArgumentNullException>(() => SKPixelComparer.GenerateDifferenceMask(image, null!));
		}
	}
}
