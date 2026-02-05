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

	[Fact]
	public async Task DefaultAnimationSpeedIsOne()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test
		Assert.Equal(1.0, lottie.AnimationSpeed);
	}

	[Fact]
	public async Task AnimationSpeedDoublesMakesItTwiceAsFast()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = 2.0 };
		await lottie.LoadedTask;

		// update with 1 second, but should progress 2 seconds
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(TimeSpan.FromSeconds(2), lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task AnimationSpeedHalfMakesItTwiceAsSlow()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = 0.5 };
		await lottie.LoadedTask;

		// update with 1 second, but should progress 0.5 seconds
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(TimeSpan.FromSeconds(0.5), lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task AnimationSpeedZeroStopsAnimation()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = 0 };
		await lottie.LoadedTask;

		// update with 1 second, but should not progress
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task AnimationSpeedCanBeChangedDynamically()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = 1.0 };
		await lottie.LoadedTask;

		// update with normal speed
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Progress);

		// change speed to 2x
		lottie.AnimationSpeed = 2.0;
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test - should now be at 3 seconds (1 + 2)
		Assert.Equal(TimeSpan.FromSeconds(3), lottie.Progress);
		Assert.False(lottie.IsComplete);
	}
}
