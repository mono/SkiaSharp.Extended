using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

/// <summary>
/// Tests for ImageAssetsFolder builder parameter passing and .lottie integration.
/// </summary>
public class SKLottieImageAssetsFolderBuilderTest
{
	private const string WithImagesJson = "TestAssets/Lottie/with-images.json";
	private const string ImageAssetsFolder = "TestAssets/Lottie";
	private const string TestLottieFile = "TestAssets/Lottie/test.lottie";

	[Fact]
	public async Task LottieFileWithImagesInSubdirectoryLoadsCorrectly()
	{
		// .lottie files should auto-detect 'i/' subdirectory for images
		var source = new SKFileLottieImageSource { File = TestLottieFile };
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task ImageAssetsFolderWorksWithJsonAnimations()
	{
		var source = new SKFileLottieImageSource
		{
			File = WithImagesJson,
			ImageAssetsFolder = ImageAssetsFolder
		};
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task ImageAssetsFolderCanBeSetAfterCreation()
	{
		var source = new SKFileLottieImageSource { File = WithImagesJson };
		var lottie = new WaitingLottieView { Source = source };

		// Set ImageAssetsFolder after source is attached to view
		lottie.ResetTask();
		source.ImageAssetsFolder = ImageAssetsFolder;

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task ImageAssetsFolderCanBeClearedAfterSet()
	{
		var source = new SKFileLottieImageSource
		{
			File = WithImagesJson,
			ImageAssetsFolder = ImageAssetsFolder
		};

		Assert.Equal(ImageAssetsFolder, source.ImageAssetsFolder);

		source.ImageAssetsFolder = null;
		Assert.Null(source.ImageAssetsFolder);
	}

	[Fact]
	public void ImageAssetsFolderPropertyTriggersSourceChangedEvent()
	{
		var source = new SKFileLottieImageSource { File = WithImagesJson };
		var eventFired = false;

		source.SourceChanged += (s, e) => eventFired = true;
		source.ImageAssetsFolder = ImageAssetsFolder;

		Assert.True(eventFired);
	}

	[Fact]
	public async Task UriSourceSupportsImageAssetsFolder()
	{
		// Even though URI sources download from network,
		// ImageAssetsFolder property should be available
		var source = new SKUriLottieImageSource();
		source.ImageAssetsFolder = "test/path";

		Assert.Equal("test/path", source.ImageAssetsFolder);
	}

	[Fact]
	public async Task StreamSourceSupportsImageAssetsFolder()
	{
		var source = new SKStreamLottieImageSource
		{
			Stream = async ct =>
			{
				var root = AppContext.BaseDirectory;
				var path = Path.Combine(root, WithImagesJson);
				return File.OpenRead(path);
			},
			ImageAssetsFolder = ImageAssetsFolder
		};
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}
}
