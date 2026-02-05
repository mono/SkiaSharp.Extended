using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKImageAssetsFolderTest
{
	private const string WithImagesJson = "TestAssets/Lottie/with-images.json";
	private const string ImageAssetsFolder = "TestAssets/Lottie";

	[Fact]
	public async Task LottieWithExternalImagesLoadsWhenImageAssetsFolderIsSet()
	{
		// create & set source with ImageAssetsFolder
		var source = new SKFileLottieImageSource
		{
			File = WithImagesJson,
			ImageAssetsFolder = ImageAssetsFolder
		};
		var lottie = new WaitingLottieView
		{
			Source = source
		};

		// test - should load successfully
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public void ImageAssetsFolderPropertyCanBeSetOnSource()
	{
		// create source
		var source = new SKFileLottieImageSource
		{
			File = WithImagesJson
		};

		// verify property can be set and retrieved
		Assert.Null(source.ImageAssetsFolder);
		
		source.ImageAssetsFolder = ImageAssetsFolder;
		Assert.Equal(ImageAssetsFolder, source.ImageAssetsFolder);
	}

	[Fact]
	public async Task LottieLoadsAfterUpdatingImageAssetsFolder()
	{
		// create source
		var source = new SKFileLottieImageSource
		{
			File = WithImagesJson
		};
		var lottie = new WaitingLottieView { Source = source };

		// set ImageAssetsFolder after creation
		lottie.ResetTask();
		source.ImageAssetsFolder = ImageAssetsFolder;

		// test - should load successfully
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}
}
