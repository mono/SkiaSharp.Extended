using System;
using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class Base83Test
	{
		[Fact]
		public void NullDecodeThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Base83.Decode(null));
		}

		[Theory]
		[InlineData("", 0)]
		[InlineData("0", 0)]
		[InlineData("1", 1)]
		[InlineData("01", 1)]
		[InlineData("10", 83)]
		[InlineData("010", 83)]
		[InlineData("A", 10)]
		public void DecodeIsCorrect(string encoded, int expected)
		{
			var decoded = Base83.Decode(encoded);

			Assert.Equal(expected, decoded);
		}
	}
}
