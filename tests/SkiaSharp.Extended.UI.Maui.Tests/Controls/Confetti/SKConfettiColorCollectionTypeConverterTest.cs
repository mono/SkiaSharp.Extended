using SkiaSharp.Extended.UI.Controls.Converters;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests
{
	public class SKConfettiColorCollectionTypeConverterTest
	{
		[Theory]
		[InlineData("red")]
		[InlineData(" red ")]
		[InlineData("#ff0000")]
		[InlineData("#ffff0000")]
		[InlineData(" #ffff0000 ")]
		public void CanParseSingleColor(string value)
		{
			var converter = new SKConfettiColorCollectionTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var collection = Assert.IsType<SKConfettiColorCollection>(result);

			Assert.NotNull(collection);
			var color = Assert.Single(collection);
			Assert.Equal(Colors.Red, color);
		}

		[Theory]
		[InlineData("red,red,blue")]
		[InlineData(" red, red, blue ")]
		[InlineData("#ff0000,#ff0000,#0000ff")]
		[InlineData("#ffff0000,#ffff0000,#ff0000ff")]
		[InlineData(" #ffff0000, #ffff0000, #ff0000ff ")]
		public void CanParseMultipleColors(string value)
		{
			var converter = new SKConfettiColorCollectionTypeConverter();

			var result = converter.ConvertFromInvariantString(value);

			var collection = Assert.IsType<SKConfettiColorCollection>(result);

			Assert.NotNull(collection);
			Assert.Equal(new[] { Colors.Red, Colors.Red, Colors.Blue }, collection);
		}
	}
}
