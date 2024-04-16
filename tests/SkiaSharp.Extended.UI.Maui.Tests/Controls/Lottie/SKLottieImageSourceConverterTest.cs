using SkiaSharp.Extended.UI.Controls.Converters;
using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKLottieImageSourceConverterTest
{
	[Theory]
	[InlineData("http://example.org/lottie.json")]
	[InlineData("https://example.org/lottie.json")]
	public void CanConvertFromUriString(string uri)
	{
		var converter = new SKLottieImageSourceConverter();

		var result = converter.ConvertFromInvariantString(uri);

		var source = Assert.IsType<SKUriLottieImageSource>(result);
		Assert.Equal(new Uri(uri), source.Uri);
	}

	[Theory]
	[InlineData("/Projects/lottie.json")]
	[InlineData("file://Projects/lottie.json")]
	[InlineData("file:///Projects/lottie.json")]
	[InlineData("C:/Projects/lottie.json")]
	[InlineData("C:\\Projects\\lottie.json")]
	[InlineData("lottie.json")]
	[InlineData("file://lottie.json")]
	public void CanConvertFromFileString(string file)
	{
		var converter = new SKLottieImageSourceConverter();

		var result = converter.ConvertFromInvariantString(file);

		var source = Assert.IsType<SKFileLottieImageSource>(result);
		Assert.Equal(file, source.File);
	}
}
