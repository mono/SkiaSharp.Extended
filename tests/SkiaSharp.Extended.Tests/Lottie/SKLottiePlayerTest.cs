using System;
using SkiaSharp.Skottie;
using Xunit;

namespace SkiaSharp.Extended.Tests;

public class SKLottiePlayerTest
{
	// Minimal valid Lottie JSON: 60 frames at 60 fps → 1-second duration.
	private static readonly string MinimalLottieJson =
		"""{"v":"5.7.4","fr":60,"ip":0,"op":60,"w":100,"h":100,"nm":"test","ddd":0,"assets":[],"layers":[{"ddd":0,"ind":1,"ty":4,"nm":"layer","sr":1,"ks":{"o":{"a":0,"k":100,"ix":11},"r":{"a":0,"k":0,"ix":10},"p":{"a":0,"k":[50,50,0],"ix":2},"a":{"a":0,"k":[0,0,0],"ix":1},"s":{"a":0,"k":[100,100,100],"ix":6}},"ao":0,"shapes":[],"ip":0,"op":60,"st":0,"bm":0}]}""";

	private static Animation CreateAnimation() =>
		Animation.Parse(MinimalLottieJson)
		?? throw new InvalidOperationException("Failed to parse test animation.");

	// ── Initial state ────────────────────────────────────────────────────────

	[Fact]
	public void InitialState_IsEmpty()
	{
		var player = new SKLottiePlayer();

		Assert.Equal(TimeSpan.Zero, player.Progress);
		Assert.Equal(TimeSpan.Zero, player.Duration);
		Assert.False(player.IsComplete);
		Assert.False(player.HasAnimation);
	}

	// ── SetAnimation ─────────────────────────────────────────────────────────

	[Fact]
	public void SetAnimation_Null_ClearsState()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.SetAnimation(null);

		Assert.Equal(TimeSpan.Zero, player.Duration);
		Assert.Equal(TimeSpan.Zero, player.Progress);
		Assert.False(player.HasAnimation);
	}

	[Fact]
	public void SetAnimation_SetsHasAnimation()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);

		Assert.True(player.HasAnimation);
	}

	[Fact]
	public void SetAnimation_SetsDuration()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);

		Assert.Equal(TimeSpan.FromSeconds(1), player.Duration);
	}

	[Fact]
	public void SetAnimation_ResetsProgress()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Seek(TimeSpan.FromSeconds(0.5));

		player.SetAnimation(anim);

		Assert.Equal(TimeSpan.Zero, player.Progress);
		Assert.False(player.IsComplete);
	}

	[Fact]
	public void SetAnimation_RaisesAnimationUpdated()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		var raised = 0;
		player.AnimationUpdated += (_, _) => raised++;

		player.SetAnimation(anim);

		Assert.Equal(1, raised);
	}

	// ── Seek ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Seek_UpdatesProgress()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);

		player.Seek(TimeSpan.FromSeconds(0.5));

		Assert.Equal(TimeSpan.FromSeconds(0.5), player.Progress);
	}

	[Fact]
	public void Seek_RaisesAnimationUpdated()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		var raised = 0;
		player.AnimationUpdated += (_, _) => raised++;

		player.Seek(TimeSpan.FromSeconds(0.5));

		Assert.Equal(1, raised);
	}

	// ── Update / playback ────────────────────────────────────────────────────

	[Fact]
	public void Update_AdvancesProgress()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(0.5));

		Assert.Equal(TimeSpan.FromSeconds(0.5), player.Progress);
		Assert.False(player.IsComplete);
	}

	[Fact]
	public void Update_DoesNothing_WhenNoAnimation()
	{
		var player = new SKLottiePlayer();
		player.Update(TimeSpan.FromSeconds(1));

		Assert.Equal(TimeSpan.Zero, player.Progress);
	}

	[Fact]
	public void Update_CapsProgressAtDuration_WithRepeatNever()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Never;

		player.Update(TimeSpan.FromSeconds(10));

		Assert.Equal(player.Duration, player.Progress);
	}

	// ── Repeat.Never ─────────────────────────────────────────────────────────

	[Fact]
	public void RepeatNever_CompletesAfterOnePlay()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Never;
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		player.Update(TimeSpan.FromSeconds(10));

		Assert.True(player.IsComplete);
		Assert.Equal(1, completed);
	}

	[Fact]
	public void RepeatNever_DoesNotFireCompletedTwice()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Never;
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		player.Update(TimeSpan.FromSeconds(10));
		player.Update(TimeSpan.FromSeconds(10));

		Assert.Equal(1, completed);
	}

	// ── Repeat.Restart ───────────────────────────────────────────────────────

	[Fact]
	public void RepeatRestart_InfiniteLoop_NeverCompletes()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Restart();
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		// Advance well past end multiple times
		for (var i = 0; i < 5; i++)
			player.Update(TimeSpan.FromSeconds(10));

		Assert.False(player.IsComplete);
		Assert.Equal(0, completed);
	}

	[Fact]
	public void RepeatRestart_FiniteCount_CompletesAfterNPlus1Plays()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Restart(count: 2); // 3 plays total
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		// Play 1 — not done
		player.Update(TimeSpan.FromSeconds(10));
		Assert.False(player.IsComplete);

		// Play 2 — not done
		player.Update(TimeSpan.FromSeconds(10));
		Assert.False(player.IsComplete);

		// Play 3 — done
		player.Update(TimeSpan.FromSeconds(10));
		Assert.True(player.IsComplete);
		Assert.Equal(1, completed);
	}

	[Fact]
	public void RepeatRestart_ResetsProgressToZero()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Restart(count: 1);

		// Seek exactly to the end to trigger restart
		player.Seek(player.Duration);

		Assert.Equal(TimeSpan.Zero, player.Progress);
		Assert.False(player.IsComplete);
	}

	// ── Repeat.Reverse (ping-pong) ───────────────────────────────────────────

	[Fact]
	public void RepeatReverse_InfiniteLoop_NeverCompletes()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Reverse();
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		for (var i = 0; i < 10; i++)
			player.Update(TimeSpan.FromSeconds(10));

		Assert.False(player.IsComplete);
		Assert.Equal(0, completed);
	}

	[Fact]
	public void RepeatReverse_BouncesDirectionAtEnd()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Reverse();

		// Play forward to end
		player.Update(player.Duration + TimeSpan.FromTicks(1));
		var progressAtBounce = player.Progress;

		// After bounce, next update should move backward
		player.Update(TimeSpan.FromSeconds(0.3));

		Assert.True(player.Progress < progressAtBounce,
			$"Expected progress to decrease after bounce, but got {player.Progress} >= {progressAtBounce}");
	}

	// ── AnimationSpeed ────────────────────────────────────────────────────────

	[Fact]
	public void NegativeSpeed_StartsAtDuration()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = -1.0;
		player.SetAnimation(anim);

		Assert.Equal(player.Duration, player.Progress);
	}

	[Fact]
	public void NegativeSpeed_MovesProgressBackward()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = -1.0;
		player.Repeat = SKLottieRepeat.Never;
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(0.5));

		Assert.Equal(player.Duration - TimeSpan.FromSeconds(0.5), player.Progress);
		Assert.False(player.IsComplete);
	}

	[Fact]
	public void NegativeSpeed_CompletesAtZero()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = -1.0;
		player.Repeat = SKLottieRepeat.Never;
		player.SetAnimation(anim);
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		player.Update(TimeSpan.FromSeconds(10));

		Assert.True(player.IsComplete);
		Assert.Equal(1, completed);
	}

	// ── AnimationUpdated event ────────────────────────────────────────────────

	[Fact]
	public void AnimationUpdated_FiredOnUpdate()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		var raised = 0;
		player.AnimationUpdated += (_, _) => raised++;

		player.Update(TimeSpan.FromSeconds(0.1));

		Assert.True(raised >= 1);
	}

	[Fact]
	public void RepeatRestart_AnimationUpdated_FiresExactlyOncePerUpdate()
	{
		// Regression: Seek() called inside UpdateProgress for restart fired the
		// event once internally, then the outer Seek() fired it again — 2x per cycle.
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Restart(2);

		// Advance to well past the first cycle boundary so a restart is triggered.
		var raised = 0;
		player.AnimationUpdated += (_, _) => raised++;

		player.Update(TimeSpan.FromSeconds(1.5));

		Assert.Equal(1, raised);
	}

	[Fact]
	public void Seek_BeforeSetAnimation_DoesNotSetIsComplete()
	{
		// Regression: UpdateProgress with null animation was setting IsComplete=true.
		var player = new SKLottiePlayer();

		player.Seek(TimeSpan.FromSeconds(1));

		Assert.False(player.IsComplete);
	}

	// ── Negative deltaTime ────────────────────────────────────────────────────

	[Fact]
	public void Update_NegativeDelta_ClampsToZero()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(-1));

		Assert.Equal(TimeSpan.Zero, player.Progress);
		Assert.False(player.IsComplete);
	}

	[Fact]
	public void Update_NegativeDeltaAfterPositive_MovesBack()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Update(TimeSpan.FromSeconds(0.5));

		player.Update(TimeSpan.FromSeconds(-0.3));

		Assert.Equal(TimeSpan.FromSeconds(0.2), player.Progress);
		Assert.False(player.IsComplete);
	}

	// ── AnimationSpeed variants ───────────────────────────────────────────────

	[Fact]
	public void AnimationSpeed_Double_AdvancesAtDoubleRate()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = 2.0;
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(0.3));

		Assert.Equal(TimeSpan.FromSeconds(0.6), player.Progress);
	}

	[Fact]
	public void AnimationSpeed_Half_AdvancesAtHalfRate()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = 0.5;
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(0.5));

		Assert.Equal(TimeSpan.FromSeconds(0.25), player.Progress);
	}

	[Fact]
	public void AnimationSpeed_Zero_DoesNotAdvance()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = 0;
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(1));

		Assert.Equal(TimeSpan.Zero, player.Progress);
		Assert.False(player.IsComplete);
	}

	[Fact]
	public void AnimationSpeed_CanBeChangedDynamically()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = 1.0;
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(0.3));
		player.AnimationSpeed = 2.0;
		player.Update(TimeSpan.FromSeconds(0.2));

		// 0.3 + (0.2 × 2.0) = 0.7
		Assert.Equal(TimeSpan.FromSeconds(0.7), player.Progress);
	}

	[Fact]
	public void AnimationSpeed_NegativeMidPlayback_MovesBackward()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = 1.0;
		player.Repeat = SKLottieRepeat.Never;
		player.SetAnimation(anim);

		player.Update(TimeSpan.FromSeconds(0.5));
		var progressBefore = player.Progress;

		player.AnimationSpeed = -1.0;
		player.Update(TimeSpan.FromSeconds(0.2));

		Assert.True(player.Progress < progressBefore);
	}

	// ── Repeat.Reverse finite (count 0) ──────────────────────────────────────

	[Fact]
	public void RepeatReverse_FiniteCount_ZeroCount_CompletesAfterOneCycle()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.SetAnimation(anim);
		player.Repeat = SKLottieRepeat.Reverse(count: 0);
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		// Forward to end — triggers direction flip, not completion
		player.Update(TimeSpan.FromSeconds(10));
		Assert.False(player.IsComplete);

		// Backward to start — completes
		player.Update(TimeSpan.FromSeconds(10));
		Assert.True(player.IsComplete);
		Assert.Equal(1, completed);
	}

	// ── Negative speed + Restart infinite ────────────────────────────────────

	[Fact]
	public void NegativeSpeed_RepeatRestart_Infinite_NeverCompletes()
	{
		using var anim = CreateAnimation();
		var player = new SKLottiePlayer();
		player.AnimationSpeed = -1.0;
		player.Repeat = SKLottieRepeat.Restart();
		player.SetAnimation(anim);
		var completed = 0;
		player.AnimationCompleted += (_, _) => completed++;

		for (var i = 0; i < 5; i++)
			player.Update(TimeSpan.FromSeconds(10));

		Assert.False(player.IsComplete);
		Assert.Equal(0, completed);
	}
}
