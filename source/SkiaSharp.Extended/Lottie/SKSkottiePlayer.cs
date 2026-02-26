using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A platform-agnostic Skottie (Lottie) animation player that manages playback state
/// and rendering. Can be shared between MAUI, Blazor, and other platforms.
/// </summary>
public class SKSkottiePlayer
{
	private Skottie.Animation? animation;
	private bool isInForwardPhase = true;
	private int repeatsCompleted = 0;
	private bool isResetting;

	private TimeSpan _progress;

	/// <summary>Gets the total duration of the loaded animation.</summary>
	public TimeSpan Duration { get; private set; } = TimeSpan.Zero;

	/// <summary>
	/// Gets or sets the current playback position. Setting this seeks the animation
	/// and may trigger completion logic (repeats, AnimationCompleted event).
	/// </summary>
	public TimeSpan Progress
	{
		get => _progress;
		set
		{
			_progress = value;
			UpdateProgress(_progress);
		}
	}

	/// <summary>Gets whether the animation has completed all repeats.</summary>
	public bool IsComplete { get; private set; } = false;

	/// <summary>Gets or sets the number of additional times the animation repeats after the first play. Use -1 for infinite.</summary>
	public int RepeatCount { get; set; } = 0;

	/// <summary>Gets or sets the repeat mode (Restart or Reverse ping-pong).</summary>
	public SKSkottieRepeatMode RepeatMode { get; set; } = SKSkottieRepeatMode.Restart;

	/// <summary>
	/// Gets or sets the playback speed multiplier.
	/// 1.0 = normal speed, 2.0 = double speed, 0.5 = half speed, negative = reverse.
	/// </summary>
	public double AnimationSpeed { get; set; } = 1.0;

	/// <summary>Gets whether an animation is currently loaded.</summary>
	public bool HasAnimation => animation is not null;

	/// <summary>Fires when the animation completes all repeats.</summary>
	public event EventHandler? AnimationCompleted;

	/// <summary>
	/// Sets the animation to play. Pass null to clear the current animation.
	/// Resets playback state (Progress, IsComplete, repeat counters).
	/// </summary>
	public void SetAnimation(Skottie.Animation? newAnimation)
	{
		animation = newAnimation;
		Reset();
	}

	/// <summary>
	/// Advances the animation by the given time delta, applying AnimationSpeed and RepeatMode.
	/// Call this on each frame tick.
	/// </summary>
	public void Update(TimeSpan deltaTime)
	{
		if (animation is null)
			return;

		// Apply animation speed with overflow protection
		var scaledTicks = deltaTime.Ticks * AnimationSpeed;
		const long SafeMax = long.MaxValue - 1;
		const long SafeMin = long.MinValue + 2;
		if (double.IsNaN(scaledTicks) || double.IsInfinity(scaledTicks))
			scaledTicks = double.IsNaN(scaledTicks) || scaledTicks < 0 ? SafeMin : SafeMax;
		else if (scaledTicks > SafeMax)
			scaledTicks = SafeMax;
		else if (scaledTicks < SafeMin)
			scaledTicks = SafeMin;
		deltaTime = TimeSpan.FromTicks((long)scaledTicks);

		// Apply phase direction (for RepeatMode.Reverse ping-pong)
		if (!isInForwardPhase)
			deltaTime = -deltaTime;

		var newProgress = Progress + deltaTime;
		if (newProgress > Duration)
			newProgress = Duration;
		if (newProgress < TimeSpan.Zero)
			newProgress = TimeSpan.Zero;

		Progress = newProgress;
	}

	/// <summary>Renders the current animation frame to the given canvas within the specified rectangle.</summary>
	public void Render(SKCanvas canvas, SKRect rect)
	{
		animation?.Render(canvas, rect);
	}

	private void UpdateProgress(TimeSpan progress)
	{
		if (animation is null)
		{
			IsComplete = true;
			return;
		}

		animation.SeekFrameTime(progress.TotalSeconds);

		// Skip completion/repeat logic during Reset to avoid spurious events
		if (isResetting)
			return;

		var repeatMode = RepeatMode;
		var duration = Duration;

		// Determine effective movement direction
		var movingForward = AnimationSpeed >= 0 ? isInForwardPhase : !isInForwardPhase;

		// Have we reached a boundary based on our movement direction?
		var atStart = !movingForward && progress <= TimeSpan.Zero;
		var atEnd = movingForward && progress >= duration;

		// A run is "finished" based on RepeatMode
		var reverseFinishPoint = AnimationSpeed >= 0 ? atStart : atEnd;
		var isFinishedRun = repeatMode == SKSkottieRepeatMode.Restart
			? (movingForward ? atEnd : atStart)
			: reverseFinishPoint;

		// For Reverse mode: flip direction when hitting a boundary (but not the finish boundary)
		var needsFlip = repeatMode == SKSkottieRepeatMode.Reverse &&
			(AnimationSpeed >= 0 ? atEnd : atStart) && !isFinishedRun;

		if (needsFlip)
		{
			isInForwardPhase = !isInForwardPhase;
			IsComplete = false;
		}
		else
		{
			var totalRepeatCount = RepeatCount;
			if (totalRepeatCount < 0)
				totalRepeatCount = int.MaxValue;

			var infinite = totalRepeatCount == int.MaxValue;
			if (infinite)
				repeatsCompleted = 0;

			if (isFinishedRun && repeatsCompleted < totalRepeatCount)
			{
				if (!infinite)
					repeatsCompleted++;

				isFinishedRun = false;

				if (repeatMode == SKSkottieRepeatMode.Restart)
				{
					Progress = AnimationSpeed >= 0 ? TimeSpan.Zero : Duration;
				}
				else if (repeatMode == SKSkottieRepeatMode.Reverse)
					isInForwardPhase = !isInForwardPhase;
			}

			IsComplete =
				isFinishedRun &&
				repeatsCompleted >= totalRepeatCount;

			if (IsComplete)
				AnimationCompleted?.Invoke(this, EventArgs.Empty);
		}
	}

	private void Reset()
	{
		isResetting = true;
		try
		{
			isInForwardPhase = true;
			repeatsCompleted = 0;

			Duration = animation?.Duration ?? TimeSpan.Zero;
			Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
		}
		finally
		{
			isResetting = false;
		}
	}
}
