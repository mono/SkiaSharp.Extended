using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKDotLottieImageSourceTest : SKLottieImageSourceTest<SKDotLottieImageSource>
{
	private const string TestLottieFile = "TestAssets/Lottie/test.lottie";

	protected override SKDotLottieImageSource CreateEmptyImageSource() =>
		new SKDotLottieImageSource { };

	protected override SKDotLottieImageSource CreateCompleteImageSource() =>
		new SKDotLottieImageSource { File = TestLottieFile };

	protected override void UpdateImageSource(SKDotLottieImageSource imageSource, bool first) =>
		imageSource.File = TestLottieFile;

	protected override void ResetImageSource(SKDotLottieImageSource imageSource) =>
		imageSource.File = null;

	[Fact]
	public async Task DotLottieFileLoadsSuccessfully()
	{
		// create & set source
		var lottie = new WaitingLottieView
		{
			Source = new SKDotLottieImageSource { File = TestLottieFile }
		};

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public void FilePropertyCanBeSetAndRetrieved()
	{
		var source = new SKDotLottieImageSource();
		Assert.Null(source.File);

		source.File = TestLottieFile;
		Assert.Equal(TestLottieFile, source.File);
	}

	[Fact]
	public void IsEmptyReturnsTrueWhenFileIsNull()
	{
		var source = new SKDotLottieImageSource();
		Assert.True(source.IsEmpty);
	}

	[Fact]
	public void IsEmptyReturnsFalseWhenFileIsSet()
	{
		var source = new SKDotLottieImageSource { File = TestLottieFile };
		Assert.False(source.IsEmpty);
	}

	[Fact]
	public async Task LoadAnimationAsyncReturnsEmptyAnimationWhenFileIsNull()
	{
		var source = new SKDotLottieImageSource();
		var animation = await source.LoadAnimationAsync();
		Assert.False(animation.IsLoaded);
	}

	[Fact]
	public async Task LoadAnimationAsyncReturnsEmptyAnimationWhenFileIsEmpty()
	{
		var source = new SKDotLottieImageSource { File = string.Empty };
		var animation = await source.LoadAnimationAsync();
		Assert.False(animation.IsLoaded);
	}

	[Fact]
	public async Task LoadAnimationAsyncThrowsWhenFileNotFound()
	{
		var source = new SKDotLottieImageSource { File = "nonexistent.lottie" };
		await Assert.ThrowsAsync<FileLoadException>(() => source.LoadAnimationAsync());
	}

	[Fact]
	public async Task DotLottieSourceWithImageAssetsFolderLoadsCorrectly()
	{
		// Test that .lottie files can also set ImageAssetsFolder if needed
		var source = new SKDotLottieImageSource
		{
			File = TestLottieFile,
			ImageAssetsFolder = "TestAssets/Lottie"
		};

		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}
}
