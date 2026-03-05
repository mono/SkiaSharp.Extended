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
	}
}
