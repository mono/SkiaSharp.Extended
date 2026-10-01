using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

/// <summary>
/// Tests for the ImageAssetsFolder property on SKLottieImageSource.
/// 
/// NOTE: These tests use file system paths (TestAssets/Lottie) which work because
/// test assets are copied to the output directory. In a real MAUI app, external
/// image assets currently need to be extracted from app package resources to the
/// file system before they can be loaded by FileResourceProvider.
/// </summary>
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

	[Fact]
	public void ImageAssetsFolderPropertyWorksOnAllImageSourceTypes()
	{
		// Test on SKFileLottieImageSource
		var fileSource = new SKFileLottieImageSource();
		fileSource.ImageAssetsFolder = "test/path";
		Assert.Equal("test/path", fileSource.ImageAssetsFolder);

		// Test on SKUriLottieImageSource
		var uriSource = new SKUriLottieImageSource();
		uriSource.ImageAssetsFolder = "another/path";
		Assert.Equal("another/path", uriSource.ImageAssetsFolder);

		// Test on SKStreamLottieImageSource
		var streamSource = new SKStreamLottieImageSource();
		streamSource.ImageAssetsFolder = "stream/path";
		Assert.Equal("stream/path", streamSource.ImageAssetsFolder);
	}

	[Fact]
	public void ImageAssetsFolderCanBeSetToNull()
	{
		var source = new SKFileLottieImageSource
		{
			ImageAssetsFolder = "test/path"
		};
		Assert.Equal("test/path", source.ImageAssetsFolder);

		source.ImageAssetsFolder = null;
		Assert.Null(source.ImageAssetsFolder);
	}

	[Fact]
	public void ImageAssetsFolderCanBeSetToEmptyString()
	{
		var source = new SKFileLottieImageSource
		{
			ImageAssetsFolder = "test/path"
		};
		Assert.Equal("test/path", source.ImageAssetsFolder);

		source.ImageAssetsFolder = string.Empty;
		Assert.Equal(string.Empty, source.ImageAssetsFolder);
	}
}
