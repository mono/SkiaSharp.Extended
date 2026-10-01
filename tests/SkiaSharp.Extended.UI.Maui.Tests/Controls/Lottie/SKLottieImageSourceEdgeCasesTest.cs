using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

/// <summary>
/// Additional tests for edge cases and error handling in Lottie image sources.
/// </summary>
public class SKLottieImageSourceEdgeCasesTest
{
	[Fact]
	public void FromFileCreatesCorrectSourceType()
	{
		var source = SKLottieImageSource.FromFile("test.json");
		Assert.IsType<SKFileLottieImageSource>(source);
	}

	[Fact]
	public void FromUriCreatesCorrectSourceType()
	{
		var uri = new Uri("https://example.com/animation.json");
		var source = SKLottieImageSource.FromUri(uri);
		Assert.IsType<SKUriLottieImageSource>(source);
	}

	[Fact]
	public void FromStreamWithFuncCreatesCorrectSourceType()
	{
		var source = SKLottieImageSource.FromStream(ct => Task.FromResult<Stream?>(Stream.Null));
		Assert.IsType<SKStreamLottieImageSource>(source);
	}

	[Fact]
	public void FromStreamWithStreamCreatesCorrectSourceType()
	{
		var stream = new MemoryStream();
		var source = SKLottieImageSource.FromStream(stream);
		Assert.IsType<SKStreamLottieImageSource>(source);
	}

	[Fact]
	public async Task EmptySourceReturnsEmptyAnimation()
	{
		var source = new SKFileLottieImageSource();
		var animation = await source.LoadAnimationAsync();
		Assert.False(animation.IsLoaded);
	}

	[Fact]
	public async Task SourceChangedEventFiresWhenPropertyChanges()
	{
		var source = new SKFileLottieImageSource();
		var eventFired = false;
		source.SourceChanged += (s, e) => eventFired = true;

		source.File = "test.json";
		await Task.Delay(100); // Give time for event to fire

		Assert.True(eventFired);
	}

	[Fact]
	public async Task SourceChangedEventFiresWhenImageAssetsFolderChanges()
	{
		var source = new SKFileLottieImageSource { File = "test.json" };
		var eventFired = false;
		source.SourceChanged += (s, e) => eventFired = true;

		source.ImageAssetsFolder = "images/";
		await Task.Delay(100); // Give time for event to fire

		Assert.True(eventFired);
	}

	[Fact]
	public void RepeatModeEnumHasCorrectValues()
	{
		Assert.Equal(0, (int)SKLottieRepeatMode.Restart);
		Assert.Equal(1, (int)SKLottieRepeatMode.Reverse);
	}

	[Fact]
	public async Task AnimationFailedEventFiresOnInvalidFile()
	{
		var source = new SKFileLottieImageSource { File = "nonexistent.json" };
		var lottie = new WaitingLottieView { Source = source };
		var failedEventFired = false;
		Exception? capturedException = null;

		lottie.AnimationFailed += (s, e) =>
		{
			failedEventFired = true;
			capturedException = e.Exception;
		};

		await Assert.ThrowsAsync<TaskCanceledException>(() => lottie.LoadedTask);
		Assert.True(failedEventFired);
		Assert.NotNull(capturedException);
	}

	[Fact]
	public async Task AnimationLoadedEventFiresOnSuccess()
	{
		var source = new SKFileLottieImageSource { File = "TestAssets/Lottie/trophy.json" };
		var lottie = new WaitingLottieView { Source = source };
		var loadedEventFired = false;
		SKLottieAnimationLoadedEventArgs? loadedArgs = null;

		lottie.AnimationLoaded += (s, e) =>
		{
			loadedEventFired = true;
			loadedArgs = e;
		};

		await lottie.LoadedTask;
		Assert.True(loadedEventFired);
		Assert.NotNull(loadedArgs);
		Assert.True(loadedArgs.Duration > TimeSpan.Zero);
	}

	[Fact]
	public void SKLottieAnimationCanBeCreatedWithNull()
	{
		var animation = new SKLottieAnimation(null);
		Assert.False(animation.IsLoaded);
		Assert.Null(animation.Animation);
	}

	[Fact]
	public void SKLottieAnimationCanBeCreatedEmpty()
	{
		var animation = new SKLottieAnimation();
		Assert.False(animation.IsLoaded);
		Assert.Null(animation.Animation);
	}
}
