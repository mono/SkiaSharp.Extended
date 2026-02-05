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
	public async Task FrameCountReturnsCorrectValue()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test - trophy.json is 2.3666665 seconds at 30 fps
		// Frame count = duration * fps = 2.3666665 * 30 = 71 frames
		Assert.Equal(71, lottie.FrameCount);
	}

	[Fact]
	public async Task CurrentFrameReturnsCorrectValueAtZero()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test
		Assert.Equal(0, lottie.CurrentFrame);
	}

	[Fact]
	public async Task CurrentFrameUpdatesWithProgress()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// update - move to 1 second (30 frames at 30fps)
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test
		Assert.Equal(30, lottie.CurrentFrame);
	}

	[Fact]
	public async Task SeekToFrameMovesToCorrectFrame()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to frame 30
		lottie.SeekToFrame(30);

		// test
		Assert.Equal(30, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Progress);
	}

	[Fact]
	public async Task SeekToFrameAndStopStopsAnimation()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// Initial state - animation is enabled
		Assert.True(lottie.IsAnimationEnabled);

		// seek to frame 30 and stop
		lottie.SeekToFrameAndStop(30);

		// test
		Assert.Equal(30, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekToFrameAndPlayStartsAnimation()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// Stop animation first
		lottie.IsAnimationEnabled = false;
		Assert.False(lottie.IsAnimationEnabled);

		// seek to frame 30 and play
		lottie.SeekToFrameAndPlay(30);

		// test
		Assert.Equal(30, lottie.CurrentFrame);
		Assert.True(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekToFrameClampsNegativeFrames()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to negative frame
		lottie.SeekToFrame(-10);

		// test - should clamp to 0
		Assert.Equal(0, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
	}

	[Fact]
	public async Task SeekToFrameClampsFramesBeyondMax()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to frame beyond max
		lottie.SeekToFrame(1000);

		// test - should clamp to max frame (71)
		Assert.Equal(71, lottie.CurrentFrame);
		Assert.Equal(lottie.Duration, lottie.Progress);
	}

	[Fact]
	public async Task FrameCountReturnsZeroWhenNoAnimation()
	{
		// create
		var lottie = new WaitingLottieView();

		// test
		Assert.Equal(0, lottie.FrameCount);
	}

	[Fact]
	public async Task CurrentFrameReturnsZeroWhenNoAnimation()
	{
		// create
		var lottie = new WaitingLottieView();

		// test
		Assert.Equal(0, lottie.CurrentFrame);
	}
}
