using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public abstract class SKLottieImageSourceTest<T>
	where T : SKLottieImageSource
{
	protected abstract T CreateCompleteImageSource();

	protected abstract T CreateEmptyImageSource();

	protected abstract void UpdateImageSource(T imageSource, bool first);

	protected abstract void ResetImageSource(T imageSource);

	[Fact]
	public async Task SettingSourceDuringConstructionLoadsAnimation()
	{
		// create & set source
		var lottie = new WaitingLottieView
		{
			Source = CreateCompleteImageSource()
		};

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}

	[Fact]
	public async Task SettingSourcePropertyAfterConstructionLoadsAnimation()
	{
		// create
		var lottie = new WaitingLottieView();

		// set source
		lottie.Source = CreateCompleteImageSource();

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}

	[Fact]
	public async Task SettingSourceValueAfterConstructionLoadsAnimation()
	{
		// create & set empty source
		var source = CreateEmptyImageSource();
		var lottie = new WaitingLottieView { Source = source };

		// set source value
		UpdateImageSource(source, true);

		// test
		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}

	[Fact]
	public async Task ViewDoesNotHoldOnToSource()
	{
		// create
		var source = CreateCompleteImageSource();
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// clear out
		lottie.ResetTask();
		lottie.Source = null;

		// update
		UpdateImageSource(source, false);

		// test
		await Assert.ThrowsAsync<TaskCanceledException>(() => lottie.LoadedTask);
	}

	[Fact]
	public async Task SettingSourceNullUnloadsAnimation()
	{
		// create
		var lottie = new WaitingLottieView
		{
			Source = CreateCompleteImageSource()
		};
		await lottie.LoadedTask;

		// clear out
		lottie.ResetTask();
		lottie.Source = null;

		// test
		await Assert.ThrowsAsync<TaskCanceledException>(() => lottie.LoadedTask);
		Assert.Equal(TimeSpan.Zero, lottie.Duration);
	}

	[Fact]
	public async Task SettingSourceValueNullUnloadsAnimation()
	{
		// create
		var source = CreateCompleteImageSource();
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// clear out
		lottie.ResetTask();
		ResetImageSource(source);

		// test
		await Assert.ThrowsAsync<TaskCanceledException>(() => lottie.LoadedTask);
		Assert.Equal(TimeSpan.Zero, lottie.Duration);
	}
}
