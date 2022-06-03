using System;
using System.Threading.Tasks;
using SkiaSharp.Extended.UI.Forms.WPF.Tests;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xunit;

namespace SkiaSharp.Extended.UI.Forms.Extensions.Tests
{
	public class SKImageSourceExtensionsTest
	{
		[Fact]
		public async Task NullThrows()
		{
			ImageSource? source = null;

			await Assert.ThrowsAsync<ArgumentNullException>(() => source!.ToSKImageAsync());
		}

		[Fact]
		public async Task ImageReturnsSameImage()
		{
			using var image = SKImage.Create(new SKImageInfo(100, 100));

			var source = new SKImageImageSource { Image = image };

			var result = await source.ToSKImageAsync();
			Assert.NotNull(result);

			Assert.Equal(image, result);
		}

		[Fact]
		public async Task BitmapReturnsSimilarImage()
		{
			using var bitmap = new SKBitmap(new SKImageInfo(100, 100));

			var source = new SKBitmapImageSource { Bitmap = bitmap };

			var result = await source.ToSKImageAsync();
			Assert.NotNull(result);

			var resultInfo = new SKImageInfo(result.Width, result.Height, result.ColorType, result.AlphaType, result.ColorSpace);

			Assert.Equal(bitmap.Info, resultInfo);
		}

		[Fact]
		public async Task PixmapReturnsSimilarImage()
		{
			using var bitmap = new SKBitmap(new SKImageInfo(100, 100));
			using var pixmap = bitmap.PeekPixels();

			var source = new SKPixmapImageSource { Pixmap = pixmap };

			var result = await source.ToSKImageAsync();
			Assert.NotNull(result);

			Assert.Equal(bitmap.Info, pixmap.Info);
		}

		[Fact]
		public async Task StreamReturnsSimilarImage()
		{
			var info = new SKImageInfo(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
			using var image = SKImage.Create(info);
			using var data = image.Encode();
			using var stream = data.AsStream();

			var source = new StreamImageSource { Stream = token => Task.FromResult(stream) };

			var result = await source.ToSKImageAsync();
			Assert.NotNull(result);

			var resultInfo = new SKImageInfo(result.Width, result.Height, result.ColorType, result.AlphaType, result.ColorSpace);

			Assert.Equal(info, resultInfo);
		}

		[Fact]
		public async Task UriReturnsSimilarImage()
		{
			TestUtils.EnsureFormsInit();

			using var image = SKImage.FromEncodedData("images/logo.png");
			var info = new SKImageInfo(image.Width, image.Height, image.ColorType, image.AlphaType, image.ColorSpace);

			var source = new UriImageSource { Uri = new Uri("https://raw.githubusercontent.com/mono/SkiaSharp.Extended/main/images/logo.png") };

			var result = await source.ToSKImageAsync();
			Assert.NotNull(result);

			var resultInfo = new SKImageInfo(result.Width, result.Height, result.ColorType, result.AlphaType, result.ColorSpace);

			Assert.Equal(info, resultInfo);
		}

		[Fact]
		public async Task FileReturnsSimilarImage()
		{
			using var image = SKImage.FromEncodedData("images/logo.png");
			var info = new SKImageInfo(image.Width, image.Height, image.ColorType, image.AlphaType, image.ColorSpace);

			var source = new FileImageSource { File = "images/logo.png" };

			var result = await source.ToSKImageAsync();
			Assert.NotNull(result);

			var resultInfo = new SKImageInfo(result.Width, result.Height, result.ColorType, result.AlphaType, result.ColorSpace);

			Assert.Equal(info, resultInfo);
		}
	}
}
