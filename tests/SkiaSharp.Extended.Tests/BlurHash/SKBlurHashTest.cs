using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKBlurHashTest
	{
		[Fact]
		public void CanEncodeAndDecode()
		{
			using var img = SKBitmap.Decode(Path.Combine("images", "img1.jpg"));

			var encoded = SKBlurHash.Encode(img, 4, 3);

			Assert.NotNull(encoded);
			Assert.True(encoded.Length > 6);

			var decoded = SKBlurHash.DecodeBitmap(encoded, 12, 10);

			Assert.NotNull(decoded);
		}
	}
}
