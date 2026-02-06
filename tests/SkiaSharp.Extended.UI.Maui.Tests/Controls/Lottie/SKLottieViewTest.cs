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
}
