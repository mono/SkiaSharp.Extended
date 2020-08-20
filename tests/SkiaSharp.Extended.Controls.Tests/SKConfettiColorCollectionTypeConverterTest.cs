using Xamarin.Forms;
using Xunit;

namespace SkiaSharp.Extended.Controls.Tests
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
			Assert.Equal(Color.Red, color);
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
			Assert.Equal(new[] { Color.Red, Color.Red, Color.Blue }, collection);
		}
	}
}
