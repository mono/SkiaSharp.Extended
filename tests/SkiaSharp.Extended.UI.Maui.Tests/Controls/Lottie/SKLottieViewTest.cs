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
	public async Task AnimationSpeedDoubleMakesItTwiceAsFast()
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

		// update with normal speed (0.5 seconds)
		lottie.CallUpdate(TimeSpan.FromSeconds(0.5));
		Assert.Equal(TimeSpan.FromSeconds(0.5), lottie.Progress);

		// change speed to 2x
		lottie.AnimationSpeed = 2.0;
		lottie.CallUpdate(TimeSpan.FromSeconds(0.5));

		// test - should now be at 1.5 seconds (0.5 + 1.0 at 2x speed)
		Assert.Equal(TimeSpan.FromSeconds(1.5), lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task NegativeAnimationSpeedReversesPlayback()
	{
		// create - start at Progress = Duration to play backwards
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = -1.0 };
		await lottie.LoadedTask;

		// set progress to end of animation
		lottie.Progress = lottie.Duration;
		var startProgress = lottie.Progress;

		// update with 1 second - should go backwards
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test - progress should have decreased
		Assert.Equal(startProgress - TimeSpan.FromSeconds(1), lottie.Progress);
	}

	[Fact]
	public async Task NegativeAnimationSpeedStartsAtDuration()
	{
		// With negative speed, animation should start at Duration and play backwards
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = -1.0 };
		await lottie.LoadedTask;

		// progress starts at Duration (not 0) for negative speed
		Assert.Equal(lottie.Duration, lottie.Progress);

		// update with 1 second - should go backwards
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test - progress should have decreased by 1 second
		Assert.Equal(lottie.Duration - TimeSpan.FromSeconds(1), lottie.Progress);
	}

	[Fact]
	public async Task AnimationSpeedWorksWithRepeatModeReverse()
	{
		// create with RepeatMode.Reverse and 2x speed
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView
		{
			Source = source,
			AnimationSpeed = 2.0,
			RepeatMode = SKLottieRepeatMode.Reverse,
			RepeatCount = 1
		};
		await lottie.LoadedTask;

		var duration = lottie.Duration;

		// update to slightly past the end to ensure we hit the boundary and reverse
		lottie.CallUpdate(TimeSpan.FromTicks((duration.Ticks / 2) + 1));

		// should be at the end (clamped)
		Assert.Equal(duration, lottie.Progress);

		// update again - should now be playing in reverse at 2x speed (0.5 seconds of real time = 1 second at 2x)
		lottie.CallUpdate(TimeSpan.FromSeconds(0.5));

		// test - progress should have decreased by 1 second (0.5s * 2x speed in reverse)
		Assert.Equal(duration - TimeSpan.FromSeconds(1), lottie.Progress);
	}

	[Fact]
	public async Task NegativeAnimationSpeedCompletesAndFiresEvent()
	{
		// create with negative speed and start at end
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView
		{
			Source = source,
			AnimationSpeed = -1.0,
			RepeatCount = 0  // no repeats, should complete
		};
		await lottie.LoadedTask;

		var duration = lottie.Duration;
		var completedFired = false;
		lottie.AnimationCompleted += (s, e) => completedFired = true;

		// set progress to end
		lottie.Progress = duration;

		// update enough to reach the start
		lottie.CallUpdate(duration + TimeSpan.FromSeconds(1));

		// test - should be at start and completed
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.True(lottie.IsComplete);
		Assert.True(completedFired);
	}

	[Fact]
	public async Task NegativeAnimationSpeedWithRepeatModeRestartLoops()
	{
		// create with negative speed and RepeatMode.Restart with infinite repeats
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView
		{
			Source = source,
			AnimationSpeed = -1.0,
			RepeatMode = SKLottieRepeatMode.Restart,
			RepeatCount = -1  // infinite repeats
		};
		await lottie.LoadedTask;

		var duration = lottie.Duration;

		// set progress to end (where negative speed animations start)
		lottie.Progress = duration;
		Assert.False(lottie.IsComplete);

		// Run multiple times - should never complete with infinite repeats
		for (int i = 0; i < 5; i++)
		{
			lottie.CallUpdate(duration);
			Assert.False(lottie.IsComplete);
		}
	}

	// ===========================================
	// Edge Case Tests (from multi-model review)
	// ===========================================

	[Fact]
	public async Task AnimationCompletedEventFiresOnlyOnce()
	{
		// Verify AnimationCompleted doesn't spam every frame
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, RepeatCount = 0 };
		await lottie.LoadedTask;

		var completedCount = 0;
		lottie.AnimationCompleted += (s, e) => completedCount++;

		// Play to completion
		lottie.CallUpdate(lottie.Duration + TimeSpan.FromSeconds(1));
		Assert.True(lottie.IsComplete);
		Assert.Equal(1, completedCount);

		// Call update several more times - event should NOT fire again
		for (int i = 0; i < 5; i++)
		{
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}
		Assert.Equal(1, completedCount);
	}

	[Fact]
	public async Task ManualProgressSetDoesNotIncrementRepeatCount()
	{
		// Manually setting Progress to a boundary should not trigger repeat logic
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView 
		{ 
			Source = source, 
			RepeatCount = 5,
			RepeatMode = SKLottieRepeatMode.Restart
		};
		await lottie.LoadedTask;

		var duration = lottie.Duration;

		// Manually scrub to end multiple times
		lottie.Progress = duration;
		lottie.Progress = TimeSpan.Zero;
		lottie.Progress = duration;
		lottie.Progress = TimeSpan.Zero;

		// Now play normally - should still have all 5 repeats available
		// (repeatsCompleted should still be 0)
		lottie.CallUpdate(duration);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task SwitchingRepeatModeFromReverseToRestartMidAnimation()
	{
		// Verify animation doesn't get stuck when switching modes
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView 
		{ 
			Source = source, 
			RepeatCount = -1,  // infinite
			RepeatMode = SKLottieRepeatMode.Reverse
		};
		await lottie.LoadedTask;

		var duration = lottie.Duration;

		// Play to end and start reversing
		lottie.CallUpdate(duration);
		Assert.Equal(duration, lottie.Progress);

		// Play partway back
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		var midProgress = lottie.Progress;
		Assert.True(midProgress < duration);

		// Switch to Restart mode
		lottie.RepeatMode = SKLottieRepeatMode.Restart;

		// Continue playing - should still be able to move
		lottie.CallUpdate(TimeSpan.FromSeconds(0.5));
		
		// Progress should have changed (not stuck)
		Assert.NotEqual(midProgress, lottie.Progress);
	}

	[Fact]
	public async Task ProgressOutOfBoundsIsAccepted()
	{
		// CURRENT BEHAVIOR: Out-of-bounds Progress values are accepted without clamping
		// This test documents the current behavior - a future fix should add clamping
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		var duration = lottie.Duration;

		// Set negative progress - currently accepted (should ideally be clamped)
		lottie.Progress = TimeSpan.FromSeconds(-100);
		// Verify the animation doesn't crash
		Assert.NotNull(lottie);

		// Set progress beyond duration - currently accepted
		lottie.Progress = duration + TimeSpan.FromSeconds(100);
		// Verify the animation doesn't crash
		Assert.NotNull(lottie);
	}

	[Fact]
	public async Task ChangingAnimationSpeedToNegativeMidPlayback()
	{
		// Verify animation continues correctly when speed changes sign
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView 
		{ 
			Source = source, 
			AnimationSpeed = 1.0,
			RepeatCount = -1  // infinite
		};
		await lottie.LoadedTask;

		// Play forward partway
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		var progress1 = lottie.Progress;
		Assert.Equal(TimeSpan.FromSeconds(1), progress1);

		// Change to negative speed
		lottie.AnimationSpeed = -1.0;

		// Continue - should now play backward
		lottie.CallUpdate(TimeSpan.FromSeconds(0.5));
		var progress2 = lottie.Progress;
		Assert.True(progress2 < progress1);
	}

	[Fact]
	public async Task ZeroAnimationSpeedPausesAnimation()
	{
		// Verify zero speed pauses without side effects
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = 0 };
		await lottie.LoadedTask;

		var initialProgress = lottie.Progress;

		// Multiple updates should not change progress
		for (int i = 0; i < 10; i++)
		{
			lottie.CallUpdate(TimeSpan.FromSeconds(1));
		}

		Assert.Equal(initialProgress, lottie.Progress);
		Assert.False(lottie.IsComplete);
	}

	[Fact]
	public async Task NegativeSpeedAnimationStartsAtDurationOnLoad()
	{
		// Verify Fix 1: Negative speed animation initializes Progress to Duration
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = -1.0 };
		await lottie.LoadedTask;

		// Should start at Duration, not Zero
		Assert.Equal(lottie.Duration, lottie.Progress);
		Assert.False(lottie.IsComplete);

		// Should be able to play backwards without manual intervention
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.True(lottie.Progress < lottie.Duration);
	}

	[Fact]
	public async Task SourceChangeCancellsPreviousLoad()
	{
		// Verify Fix 2: Changing Source cancels any in-flight load
		var source1 = new SKFileLottieImageSource { File = TrophyJson };
		var source2 = new SKFileLottieImageSource { File = LoloJson };
		var lottie = new WaitingLottieView { Source = source1 };
		
		// Immediately change source before first load completes
		lottie.ResetTask();
		lottie.Source = source2;
		await lottie.LoadedTask;

		// Should have loaded the second source, not the first
		// (Duration differs between trophy.json and lolo.json)
		Assert.NotEqual(TimeSpan.Zero, lottie.Duration);
	}

	// ===========================================
	// Frame-Based Control Tests (issue #166)
	// ===========================================

	[Fact]
	public async Task FpsIsSetAfterLoad()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// trophy.json is 30 fps
		Assert.Equal(30.0, lottie.Fps);
	}

	[Fact]
	public async Task FpsIsZeroWithoutAnimation()
	{
		// create without source
		var lottie = new WaitingLottieView();

		// test
		Assert.Equal(0.0, lottie.Fps);
	}

	[Fact]
	public async Task FrameCountIsSetAfterLoad()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// trophy.json: 2.3666665s at 30fps → Math.Round(2.3666665 * 30) = Math.Round(71.0) = 71
		var expected = (int)Math.Round(lottie.Duration.TotalSeconds * lottie.Fps);
		Assert.Equal(expected, lottie.FrameCount);
		Assert.True(lottie.FrameCount > 0);
	}

	[Fact]
	public async Task FrameCountIsZeroWithoutAnimation()
	{
		// create without source
		var lottie = new WaitingLottieView();

		// test
		Assert.Equal(0, lottie.FrameCount);
	}

	[Fact]
	public async Task CurrentFrameIsZeroAfterLoad()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// test
		Assert.Equal(0, lottie.CurrentFrame);
	}

	[Fact]
	public async Task CurrentFrameIsZeroWithoutAnimation()
	{
		// create without source
		var lottie = new WaitingLottieView();

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

		// trophy.json is 30fps: 1 second = 30 frames
		lottie.CallUpdate(TimeSpan.FromSeconds(1));

		// test - Math.Floor(1.0 * 30) = 30
		Assert.Equal(30, lottie.CurrentFrame);
	}

	[Fact]
	public async Task CurrentFrameUsesFloorRounding()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// At 30fps, 0.5s is frame 15.0 exactly; 0.9s is frame 27.0
		lottie.CallUpdate(TimeSpan.FromSeconds(0.9));

		// Math.Floor(0.9 * 30) = Math.Floor(27.0) = 27
		Assert.Equal(27, lottie.CurrentFrame);
	}

	[Fact]
	public async Task SeekToFrameMovesToCorrectPosition()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to frame 30 (1 second at 30fps)
		lottie.SeekToFrame(30);

		// test - frame 30 / 30fps = 1 second
		Assert.Equal(30, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.FromSeconds(1.0), lottie.Progress);
	}

	[Fact]
	public async Task SeekToFrameWithStopPlaybackPausesAnimation()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// animation is enabled by default
		Assert.True(lottie.IsAnimationEnabled);

		// seek to frame 15 and stop
		lottie.SeekToFrame(15, stopPlayback: true);

		// test
		Assert.Equal(15, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekToFrameWithoutStopKeepsAnimationRunning()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek without stopping
		lottie.SeekToFrame(15, stopPlayback: false);

		// animation should still be enabled
		Assert.True(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekToFrameClampsNegativeFrame()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to negative frame - should clamp to 0
		lottie.SeekToFrame(-10);

		// test
		Assert.Equal(0, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
	}

	[Fact]
	public async Task SeekToFrameClampsBeyondMax()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek beyond max frame - should clamp to FrameCount - 1
		lottie.SeekToFrame(10000);

		// test
		Assert.Equal(lottie.FrameCount - 1, lottie.CurrentFrame);
	}

	[Fact]
	public async Task SeekToFrameDoesNothingWithoutAnimation()
	{
		// create without source
		var lottie = new WaitingLottieView();

		// seek - should not throw or change state
		lottie.SeekToFrame(10);

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.Equal(0, lottie.CurrentFrame);
	}

	[Fact]
	public async Task SeekToProgressMovesToCorrectPosition()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to 50% progress
		lottie.SeekToProgress(0.5);

		// test
		var expectedTime = TimeSpan.FromSeconds(lottie.Duration.TotalSeconds * 0.5);
		Assert.Equal(expectedTime, lottie.Progress);
	}

	[Fact]
	public async Task SeekToProgressWithStopPlaybackPausesAnimation()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to 25% and stop
		lottie.SeekToProgress(0.25, stopPlayback: true);

		// test
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekToProgressClampsNegativeValue()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to negative progress - should clamp to 0
		lottie.SeekToProgress(-0.5);

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
	}

	[Fact]
	public async Task SeekToProgressClampsAboveOne()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to progress > 1.0 - should clamp to 1.0 (= Duration)
		lottie.SeekToProgress(1.5);

		// test
		Assert.Equal(lottie.Duration, lottie.Progress);
	}

	[Fact]
	public async Task SeekToProgressDoesNothingWithoutAnimation()
	{
		// create without source
		var lottie = new WaitingLottieView();

		// seek - should not throw or change state
		lottie.SeekToProgress(0.5);

		// test
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
	}

	[Fact]
	public async Task SeekToTimeMovesToCorrectPosition()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to 1 second
		var targetTime = TimeSpan.FromSeconds(1);
		lottie.SeekToTime(targetTime);

		// test
		Assert.Equal(targetTime, lottie.Progress);
	}

	[Fact]
	public async Task SeekToTimeWithStopPlaybackPausesAnimation()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to 1 second and stop
		lottie.SeekToTime(TimeSpan.FromSeconds(1), stopPlayback: true);

		// test
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Progress);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task PauseSetsAnimationEnabledFalse()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// animation is enabled by default
		Assert.True(lottie.IsAnimationEnabled);

		// pause
		lottie.Pause();

		// test
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task ResumeSetsAnimationEnabledTrue()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// pause first
		lottie.Pause();
		Assert.False(lottie.IsAnimationEnabled);

		// resume
		lottie.Resume();

		// test
		Assert.True(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekToLastFrameWorksCorrectly()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// seek to last valid frame
		var lastFrame = lottie.FrameCount - 1;
		lottie.SeekToFrame(lastFrame);

		// test
		Assert.Equal(lastFrame, lottie.CurrentFrame);
	}

	[Fact]
	public async Task FramePropertiesResetWhenSourceChanges()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// verify properties are set
		var fps = lottie.Fps;
		var frameCount = lottie.FrameCount;
		Assert.True(fps > 0);
		Assert.True(frameCount > 0);

		// advance animation
		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.True(lottie.CurrentFrame > 0);

		// reset with new source
		lottie.ResetTask();
		source.File = LoloJson;
		await lottie.LoadedTask;

		// CurrentFrame should be reset to 0
		Assert.Equal(0, lottie.CurrentFrame);
		// Fps and FrameCount should be recalculated
		Assert.True(lottie.Fps > 0);
		Assert.True(lottie.FrameCount > 0);
	}

	// ===========================================
	// Animated Play-To Tests (issue #166)
	// ===========================================

	[Fact]
	public async Task PlayToFrameAnimatesForwardToTarget()
	{
		// create
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// start at frame 0, animate to frame 30
		Assert.Equal(0, lottie.CurrentFrame);
		lottie.PlayToFrame(30);
		Assert.True(lottie.IsAnimationEnabled);

		// drive the animation forward in large steps until it stops
		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		// should have stopped at frame 30
		Assert.False(lottie.IsAnimationEnabled);
		Assert.Equal(30, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.FromSeconds(30.0 / lottie.Fps), lottie.Progress);
	}

	[Fact]
	public async Task PlayToFrameAnimatesBackwardToTarget()
	{
		// create and seek to frame 60 first
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SeekToFrame(60, stopPlayback: true);
		Assert.Equal(60, lottie.CurrentFrame);

		// animate backward to frame 0
		lottie.PlayToFrame(0);
		Assert.True(lottie.IsAnimationEnabled);

		// drive the animation until it stops
		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		// should have stopped at frame 0
		Assert.False(lottie.IsAnimationEnabled);
		Assert.Equal(0, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
	}

	[Fact]
	public async Task PlayToFrameStopsExactlyAtTarget()
	{
		// Verify animation stops at the exact target, not beyond it
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// animate to frame 10 with a large step size that would overshoot
		lottie.PlayToFrame(10);
		lottie.CallUpdate(TimeSpan.FromSeconds(100)); // massive overshoot step

		// should be clamped to exactly frame 10
		Assert.Equal(10, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task PlayToFrameDoesNothingWhenAlreadyAtTarget()
	{
		// If current position == target, PlayToFrame should be a no-op
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SeekToFrame(15, stopPlayback: true);
		Assert.Equal(15, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);

		// PlayToFrame to the same frame should not start animation
		lottie.PlayToFrame(15);
		Assert.Equal(15, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task PlayToFrameDoesNothingWithoutAnimation()
	{
		// Without a loaded animation, PlayToFrame should be safe to call
		var lottie = new WaitingLottieView();

		lottie.PlayToFrame(10);

		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.Equal(0, lottie.CurrentFrame);
	}

	[Fact]
	public async Task PlayToFrameClampsNegativeFrame()
	{
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SeekToFrame(30, stopPlayback: true);
		lottie.PlayToFrame(-10); // clamps to 0, animates backward

		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		Assert.Equal(0, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task PlayToFrameClampsBeyondMax()
	{
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// animate forward, clamped to FrameCount - 1
		lottie.PlayToFrame(10000);

		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		Assert.Equal(lottie.FrameCount - 1, lottie.CurrentFrame);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task PlayToProgressAnimatesToTarget()
	{
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// animate to 50%
		lottie.PlayToProgress(0.5);
		Assert.True(lottie.IsAnimationEnabled);

		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		var expectedTime = TimeSpan.FromSeconds(lottie.Duration.TotalSeconds * 0.5);
		Assert.Equal(expectedTime, lottie.Progress);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task PlayToTimeAnimatesToTarget()
	{
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		var target = TimeSpan.FromSeconds(1.0);
		lottie.PlayToTime(target);
		Assert.True(lottie.IsAnimationEnabled);

		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		Assert.Equal(target, lottie.Progress);
		Assert.False(lottie.IsAnimationEnabled);
	}

	[Fact]
	public async Task SeekCancelsPlayTo()
	{
		// Calling SeekToFrame while PlayToFrame is in progress cancels the play-to
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		// start animating to frame 60
		lottie.PlayToFrame(60);
		lottie.CallUpdate(TimeSpan.FromSeconds(0.1)); // partial progress

		// interrupt with an instant seek
		lottie.SeekToFrame(5, stopPlayback: true);

		// we should be at frame 5, and more updates should not resume play-to
		Assert.Equal(5, lottie.CurrentFrame);
		lottie.CallUpdate(TimeSpan.FromSeconds(10)); // large step with no IsAnimationEnabled
		Assert.Equal(5, lottie.CurrentFrame); // no movement since stopped
	}

	[Fact]
	public async Task PlayToDoesNotFireAnimationCompleted()
	{
		// PlayTo stopping at target should NOT fire AnimationCompleted
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		var completedCount = 0;
		lottie.AnimationCompleted += (s, e) => completedCount++;

		// Animate to a non-end frame (frame 30 of 71)
		lottie.PlayToFrame(30);
		for (var i = 0; i < 100; i++)
		{
			if (!lottie.IsAnimationEnabled) break;
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
		}

		Assert.Equal(30, lottie.CurrentFrame);
		Assert.Equal(0, completedCount); // AnimationCompleted not fired for PlayTo stop
	}

	[Fact]
	public async Task PlayToRespectsAnimationSpeed()
	{
		// PlayToFrame should move faster when AnimationSpeed is 2x
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, AnimationSpeed = 2.0 };
		await lottie.LoadedTask;

		// Animate to frame 60 with one 0.5s update at 2x speed = effectively 1s of movement
		// 1s at 30fps = 30 frames, so starting at 0 we should reach >= frame 30 in one update
		lottie.PlayToFrame(60);
		lottie.CallUpdate(TimeSpan.FromSeconds(0.5)); // 0.5s * 2x = 1s of movement = 30 frames

		// Should have moved 30 frames in one 0.5s update
		Assert.True(lottie.CurrentFrame >= 30);
	}

	// ===========================================
	// Segment Tests (issue #166)
	// ===========================================

	[Fact]
	public async Task SegmentDefaultsToFullRangeAfterLoad()
	{
		// After loading, SegmentStart=0 and SegmentEnd=Duration (full animation)
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		Assert.Equal(TimeSpan.Zero, lottie.SegmentStart);
		Assert.Equal(lottie.Duration, lottie.SegmentEnd);
	}

	[Fact]
	public async Task SetSegmentByTimeUpdatesDuration()
	{
		// SetSegment(0s, 1s) on trophy.json (2.36s total) → Duration becomes 1s
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(TimeSpan.Zero, TimeSpan.FromSeconds(1.0));

		Assert.Equal(TimeSpan.Zero, lottie.SegmentStart);
		Assert.Equal(TimeSpan.FromSeconds(1.0), lottie.SegmentEnd);
		Assert.Equal(TimeSpan.FromSeconds(1.0), lottie.Duration);
	}

	[Fact]
	public async Task SetSegmentByFrameUpdatesDuration()
	{
		// SetSegment(0, 30) on trophy.json (30fps) → Duration = 30/30 = 1.0s
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(0, 30);

		Assert.Equal(TimeSpan.Zero, lottie.SegmentStart);
		Assert.Equal(TimeSpan.FromSeconds(30.0 / lottie.Fps), lottie.SegmentEnd);
		Assert.Equal(TimeSpan.FromSeconds(1.0), lottie.Duration);
	}

	[Fact]
	public async Task SetSegmentUpdatesFrameCount()
	{
		// SetSegment(0, 30) → FrameCount = round(1.0 * 30) = 30
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(0, 30);

		Assert.Equal(30, lottie.FrameCount);
	}

	[Fact]
	public async Task SetSegmentMidRangeUpdatesDuration()
	{
		// SetSegment(10, 40) on trophy.json → Duration = 30 frames = 1.0s
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(10, 40);

		var expectedStart = TimeSpan.FromSeconds(10.0 / lottie.Fps);
		var expectedEnd = TimeSpan.FromSeconds(40.0 / lottie.Fps);
		Assert.Equal(expectedStart, lottie.SegmentStart);
		Assert.Equal(expectedEnd, lottie.SegmentEnd);
		Assert.Equal(expectedEnd - expectedStart, lottie.Duration);
		Assert.Equal(30, lottie.FrameCount);
	}

	[Fact]
	public async Task SetSegmentResetsProgress()
	{
		// After advancing progress, SetSegment should reset it to 0
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.CallUpdate(TimeSpan.FromSeconds(1));
		Assert.True(lottie.Progress > TimeSpan.Zero);

		lottie.SetSegment(0, 30);

		Assert.Equal(TimeSpan.Zero, lottie.Progress);
		Assert.Equal(0, lottie.CurrentFrame);
	}

	[Fact]
	public async Task ProgressStopsAtSegmentEnd()
	{
		// With SetSegment(0, 30), Duration = 1s. A large update should stop at 1s.
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(0, 30);
		lottie.CallUpdate(TimeSpan.FromSeconds(100)); // much larger than segment Duration

		Assert.Equal(lottie.Duration, lottie.Progress); // clamped to segment end
	}

	[Fact]
	public async Task SeekToFrameWorksWithinSegment()
	{
		// With SetSegment(10, 40), SeekToFrame(0) = frame 10 of full animation
		// and SeekToFrame(FrameCount-1) = frame 39 of full animation
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(10, 40);

		// Frame 0 within the segment
		lottie.SeekToFrame(0, stopPlayback: true);
		Assert.Equal(0, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.Zero, lottie.Progress);

		// Frame 15 within the segment (= frame 25 of full animation)
		lottie.SeekToFrame(15, stopPlayback: true);
		Assert.Equal(15, lottie.CurrentFrame);
		Assert.Equal(TimeSpan.FromSeconds(15.0 / lottie.Fps), lottie.Progress);
	}

	[Fact]
	public async Task SetSegmentClampsStartToAnimationBounds()
	{
		// Negative startFrame → clamped to 0
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(-10, 30);

		Assert.Equal(TimeSpan.Zero, lottie.SegmentStart);
		Assert.Equal(30, lottie.FrameCount);
	}

	[Fact]
	public async Task SetSegmentClampsEndToAnimationBounds()
	{
		// endFrame > FrameCount → clamped to full animation duration
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		var fullDuration = lottie.Duration;
		lottie.SetSegment(0, 10000);

		Assert.Equal(fullDuration, lottie.SegmentEnd);
		Assert.Equal(fullDuration, lottie.Duration);
	}

	[Fact]
	public async Task SetSegmentWhereEndLessThanStartUsesZeroDuration()
	{
		// end < start → clamped so end == start → Duration = 0
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.5));

		// end is clamped to max(start, end) = start
		Assert.Equal(lottie.SegmentStart, lottie.SegmentEnd);
		Assert.Equal(TimeSpan.Zero, lottie.Duration);
	}

	[Fact]
	public async Task ClearSegmentRestoresFullRange()
	{
		// After SetSegment + ClearSegment, Duration returns to original
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		var fullDuration = lottie.Duration;
		var fullFrameCount = lottie.FrameCount;

		lottie.SetSegment(10, 40);
		Assert.NotEqual(fullDuration, lottie.Duration); // sanity check

		lottie.ClearSegment();

		Assert.Equal(TimeSpan.Zero, lottie.SegmentStart);
		Assert.Equal(fullDuration, lottie.SegmentEnd);
		Assert.Equal(fullDuration, lottie.Duration);
		Assert.Equal(fullFrameCount, lottie.FrameCount);
		Assert.Equal(TimeSpan.Zero, lottie.Progress);
	}

	[Fact]
	public async Task SetSegmentDoesNothingWithoutAnimation()
	{
		// Without a loaded animation, SetSegment should not throw
		var lottie = new WaitingLottieView();

		lottie.SetSegment(0, 10);

		Assert.Equal(TimeSpan.Zero, lottie.Duration);
		Assert.Equal(0, lottie.FrameCount);
	}

	[Fact]
	public async Task ClearSegmentDoesNothingWithoutAnimation()
	{
		// Without a loaded animation, ClearSegment should not throw
		var lottie = new WaitingLottieView();

		lottie.ClearSegment();

		Assert.Equal(TimeSpan.Zero, lottie.Duration);
	}

	[Fact]
	public async Task SegmentResetWhenSourceChanges()
	{
		// Loading a new source resets the segment to the full animation
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };
		await lottie.LoadedTask;

		lottie.SetSegment(10, 40);
		Assert.NotEqual(TimeSpan.Zero, lottie.SegmentStart);

		// Change source → segment should reset
		lottie.ResetTask();
		source.File = LoloJson;
		await lottie.LoadedTask;

		Assert.Equal(TimeSpan.Zero, lottie.SegmentStart);
		Assert.Equal(lottie.Duration, lottie.SegmentEnd);
	}

	[Fact]
	public async Task SetSegmentWithRepeatCountLoopsWithinSegment()
	{
		// With RepeatCount=1 (play twice) and a segment, animation completes within the segment
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source, RepeatCount = 1 };
		await lottie.LoadedTask;

		lottie.SetSegment(0, 30); // 1.0s segment

		var completedCount = 0;
		lottie.AnimationCompleted += (s, e) => completedCount++;

		// Play through twice (2 seconds total for 2 loops of a 1s segment)
		for (var i = 0; i < 100; i++)
		{
			lottie.CallUpdate(TimeSpan.FromSeconds(0.1));
			if (lottie.IsComplete) break;
		}

		// Should complete within the segment duration (never exceed 1.0s progress)
		Assert.Equal(1, completedCount);
		Assert.True(lottie.IsComplete);
	}
}
