using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKLottieViewTest
{
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";
	private const string LoloJson = "TestAssets/Lottie/lolo.json";

	[Fact]
	public async Task EnsureAnimationIsLoaded()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task EnsureNewAnimationResetsProgress()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// update
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		lottie.ResetTask();
		source.File = LoloJson;
		await lottie.LoadedTask;

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task UpdatesMoveProgress()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Progress);
		Assert.False(lottie.IsComplete);
		Assert.Equal(0, animationCompleted);
	}

	[Fact]
	public async Task MultipleUpdatesMoveProgressUntilDurationMax()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update & test
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Progress);
		Assert.False(lottie.IsComplete);
		Assert.Equal(0, animationCompleted);

		// update & test
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.Equal(TimeSpan.FromSeconds(2), lottie.Progress);
		Assert.False(lottie.IsComplete);
		Assert.Equal(0, animationCompleted);

		// update & test
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Progress);
		Assert.True(lottie.IsComplete);
		Assert.Equal(1, animationCompleted);
	}

	[Fact]
	public async Task UpdatesLargerThanDurationHasMax()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update
		lottie.CallUpdate(TimeSpan.FromSeconds(5));

		// test
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Progress);
		Assert.True(lottie.IsComplete);
		Assert.Equal(1, animationCompleted);
	}

	[Fact]
	public async Task NegativeUpdatesDoesNothing()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update
		lottie.CallUpdate(TimeSpan.FromSeconds(-1));

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.False(lottie.IsComplete);
		Assert.Equal(0, animationCompleted);
	}

	[Fact]
	public async Task NegativeUpdatesAfterPositiveGoesBack()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		lottie.CallUpdate(TimeSpan.FromSeconds(-1));

		// test
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Progress);
		Assert.False(lottie.IsComplete);
		Assert.Equal(0, animationCompleted);
	}

	[Fact]
	public async Task AnimationFailedContainsException()
	{
		// create - use non-existent file to trigger actual load failure with exception
		var source = new SKFileLottieImageSource { File = "nonexistent.json" };
		var lottie = new SKLottieView { Source = source };
		
		SKLottieAnimationFailedEventArgs? failedEventArgs = null;
		var tcs = new TaskCompletionSource<bool>();
		
		lottie.AnimationFailed += (s, e) =>
		{
			failedEventArgs = e;
			tcs.SetResult(true);
		};

		// wait for animation to fail (using consistent 3000ms timeout)
		var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(3000));
		
		// test - verify the failure event was triggered with proper event args
		Assert.Equal(tcs.Task, completedTask);
		Assert.NotNull(failedEventArgs);
		Assert.IsType<SKLottieAnimationFailedEventArgs>(failedEventArgs);
		// Verify exception is populated when file doesn't exist
		Assert.NotNull(failedEventArgs.Exception);
	}

	[Fact]
	public async Task AnimationFailedNotTriggeredForEmptySource()
	{
		// create - empty source should NOT trigger AnimationFailed (early return)
		var source = new SKFileLottieImageSource();
		var lottie = new SKLottieView { Source = source };
		
		var failedTriggered = false;
		var loadedTriggered = false;
		
		lottie.AnimationFailed += (s, e) => failedTriggered = true;
		lottie.AnimationLoaded += (s, e) => loadedTriggered = true;

		// wait to ensure events have time to fire
		await Task.Delay(1000);
		
		// test - verify neither event was triggered for empty source
		Assert.False(failedTriggered);
		Assert.False(loadedTriggered);
	}

	[Fact]
	public async Task AnimationLoadedNotTriggeredOnFailure()
	{
		// create - non-existent file to trigger failure
		var source = new SKFileLottieImageSource { File = "nonexistent.json" };
		var lottie = new SKLottieView { Source = source };
		
		var loadedTriggered = false;
		var tcs = new TaskCompletionSource<bool>();
		
		lottie.AnimationLoaded += (s, e) => loadedTriggered = true;
		lottie.AnimationFailed += (s, e) => tcs.SetResult(true);

		// wait for failure
		var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(3000));
		
		// test - verify AnimationLoaded was NOT triggered on failure
		Assert.Equal(tcs.Task, completedTask);
		Assert.False(loadedTriggered);
	}

	[Theory]
	[InlineData(SKLottieRepeatMode.Restart, 1, 1)]
	[InlineData(SKLottieRepeatMode.Restart, 2, 2)]
	[InlineData(SKLottieRepeatMode.Restart, 3, 0)]
	[InlineData(SKLottieRepeatMode.Restart, 4, 1)]
	[InlineData(SKLottieRepeatMode.Restart, 5, 2)]
	[InlineData(SKLottieRepeatMode.Restart, 6, 0)]
	[InlineData(SKLottieRepeatMode.Restart, 7, 1)]
	[InlineData(SKLottieRepeatMode.Reverse, 1, 1)]
	[InlineData(SKLottieRepeatMode.Reverse, 2, 2)]
	[InlineData(SKLottieRepeatMode.Reverse, 3, 2.3666665)]
	[InlineData(SKLottieRepeatMode.Reverse, 4, 1.3666665)]
	[InlineData(SKLottieRepeatMode.Reverse, 5, 0.3666665)]
	[InlineData(SKLottieRepeatMode.Reverse, 6, 0)]
	[InlineData(SKLottieRepeatMode.Reverse, 7, 1)]
	public async Task ReachingTheEndAndThenMoreWithRepeat(SKLottieRepeatMode repeatMode, int steps, double progress)
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, RepeatMode = repeatMode, RepeatCount = -1 };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update
		for (var i = 0; i < steps; i++)
			lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(TimeSpan.FromSeconds(progress), lottie.Progress);
		Assert.False(lottie.IsComplete);
		Assert.Equal(0, animationCompleted);
	}

	[Theory]
	[InlineData(SKLottieRepeatMode.Restart, 1, 1, false)]
	[InlineData(SKLottieRepeatMode.Restart, 2, 2, false)]
	[InlineData(SKLottieRepeatMode.Restart, 3, 2.3666665, true)]
	[InlineData(SKLottieRepeatMode.Restart, 4, 2.3666665, true)]
	[InlineData(SKLottieRepeatMode.Restart, 5, 2.3666665, true)]
	[InlineData(SKLottieRepeatMode.Reverse, 1, 1, false)]
	[InlineData(SKLottieRepeatMode.Reverse, 2, 2, false)]
	[InlineData(SKLottieRepeatMode.Reverse, 3, 2.3666665, false)]
	[InlineData(SKLottieRepeatMode.Reverse, 4, 1.3666665, false)]
	[InlineData(SKLottieRepeatMode.Reverse, 5, 0.3666665, false)]
	[InlineData(SKLottieRepeatMode.Reverse, 6, 0, true)]
	[InlineData(SKLottieRepeatMode.Reverse, 7, 0, true)]
	[InlineData(SKLottieRepeatMode.Reverse, 8, 0, true)]
	public async Task ReachingTheEndAndThenMoreWithRepeatModeButZeroCount(SKLottieRepeatMode repeatMode, int steps, double progress, bool isComplete)
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, RepeatMode = repeatMode, RepeatCount = 0 };
		var animationCompleted = 0;
		lottie.AnimationCompleted += (s, e) => animationCompleted++;
		await lottie.LoadedTask;

		// update
		for (var i = 0; i < steps; i++)
			lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(TimeSpan.FromSeconds(progress), lottie.Progress);
		Assert.Equal(isComplete, lottie.IsComplete);
		if (isComplete)
			Assert.Equal(1, animationCompleted);
		else
			Assert.Equal(0, animationCompleted);
	}
}
