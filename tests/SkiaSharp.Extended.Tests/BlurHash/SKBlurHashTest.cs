using System.IO;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKBlurHashTest
	{
		public const string Image1 = "LLHV6nae2ek8lAo0aeR*%fkCMxn%";
		public const string Image1_8x6 = "qLHV6nae2ek8w^xbjYR*lAo0aeR*-6s:NeWB%fkCMxn%xuS2NHs.ShRj$%xaI;R%n$a$W?oeoJR+W:aes:xaofaxj[bHoLo0R*WW";
		public const string Image2 = "LGFFaXYk^6#M@-5c,1Ex@@or[j6o";
		public const string Image2_8x6 = "qHFFaXYk^6#M9vF~W@j=@-5b,1J5PBV=R:s;@[or[k6oO[TLtJrq};Fxi^OZE3NgM}spjMFxS#OtcXnzRjxZj]OYNeWGJCs9xunh";
		public const string Image3 = "L6Pj0^nh.AyE?vt7t7R**0o#DgR4";
		public const string Image4 = "LKO2?V%2Tw=^]~RBVZRi};RPxuwH";
		public const string Image6 = "LlM~Oi00%#MwS|WDWEIoR*X8R*bH";
		public const string Image6_cs= "LlMF%n00%#MwS|WCWEM{R*bbWBbH";
		public const string Image7 = "LjIY5?00?bIUofWBWBM{WBofWBj[";

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
