using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A platform-agnostic Skottie (Lottie) animation player that manages playback state
/// and rendering. Can be used directly from any .NET host including .NET MAUI, Blazor,
/// console apps, or custom renderers.
/// </summary>
/// <remarks>
/// <para>
/// Typical usage:
/// <list type="number">
///   <item><description>Create a player and set <see cref="Repeat"/> and <see cref="AnimationSpeed"/>.</description></item>
///   <item><description>Call <see cref="SetAnimation"/> with a loaded <see cref="Skottie.Animation"/>.</description></item>
///   <item><description>On each frame tick, call <see cref="Update"/> with the elapsed time.</description></item>
///   <item><description>Call <see cref="Render"/> inside your paint/draw callback.</description></item>
/// </list>
/// </para>
/// <para>
/// The player is not thread-safe; all calls should occur on the same thread (typically the UI thread).
/// </para>
/// </remarks>
public class SKLottiePlayer
{
	private Skottie.Animation? animation;
	private bool isInForwardPhase = true;
	private int repeatsCompleted = 0;

	/// <summary>Gets the total duration of the loaded animation.</summary>
	public TimeSpan Duration { get; private set; } = TimeSpan.Zero;

	/// <summary>Gets the current playback position.</summary>
	public TimeSpan Progress { get; private set; }

	/// <summary>Gets whether the animation has completed all repeats.</summary>
	public bool IsComplete { get; private set; } = false;

	private SKLottieRepeat repeat = SKLottieRepeat.Never;

	/// <summary>Gets or sets how the animation repeats. Defaults to <see cref="SKLottieRepeat.Never"/>.</summary>
	/// <remarks>
	/// Changing this property resets the internal direction phase, repeat counter, and completion
	/// state so the animation starts the new mode cleanly from its current position.
	/// </remarks>
	public SKLottieRepeat Repeat
	{
		get => repeat;
		set
		{
			if (repeat != value)
			{
				repeat = value;
				isInForwardPhase = true;
				repeatsCompleted = 0;
				IsComplete = false;
			}
		}
	}

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
	/// Fires after each <see cref="Seek"/> call, notifying subscribers of
	/// updated state (Progress, Duration, IsComplete).
	/// </summary>
	public event EventHandler? AnimationUpdated;

	/// <summary>
	/// Sets the animation to play. Pass <see langword="null"/> to clear the current animation.
	/// Resets playback state (Progress, IsComplete, repeat counters).
	/// </summary>
	/// <param name="newAnimation">
	/// The <see cref="Skottie.Animation"/> to play, or <see langword="null"/> to clear.
	/// The player does not take ownership of the animation; the caller is responsible for disposing it.
	/// </param>
	/// <remarks>
	/// Calling this method always resets <see cref="Progress"/> to <see cref="TimeSpan.Zero"/>
	/// (or <see cref="Duration"/> when <see cref="AnimationSpeed"/> is negative) and clears
	/// <see cref="IsComplete"/>. It also raises <see cref="AnimationUpdated"/>.
	/// </remarks>
	public void SetAnimation(Skottie.Animation? newAnimation)
	{
		animation = newAnimation;
		Reset();
	}

	/// <summary>
	/// Seeks the animation to the specified position and raises <see cref="AnimationUpdated"/>.
	/// Completion and repeat logic is applied as part of the seek.
	/// </summary>
	/// <param name="position">The absolute playback position to seek to.</param>
	/// <remarks>
	/// Unlike <see cref="Update"/>, <c>Seek</c> sets an absolute position rather than advancing
	/// by a delta. The position is clamped to [<see cref="TimeSpan.Zero"/>, <see cref="Duration"/>]
	/// and repeat/completion state is evaluated immediately.
	/// Setting <see cref="Progress"/> to a boundary via <c>Seek</c> does <em>not</em> increment
	/// the internal repeat counter; use <see cref="Update"/> for frame-by-frame playback.
	/// </remarks>
	public void Seek(TimeSpan position)
	{
		if (position < TimeSpan.Zero) position = TimeSpan.Zero;
		if (position > Duration) position = Duration;
		Progress = position;
		UpdateProgress(Progress);
		AnimationUpdated?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Advances the animation by the given time delta, applying <see cref="AnimationSpeed"/> and <see cref="Repeat"/>.
	/// Call this on each frame tick.
	/// </summary>
	/// <param name="deltaTime">
	/// The time elapsed since the last call. A positive value advances forward; a negative value
	/// moves the position backward (subject to clamping at the boundaries).
	/// </param>
	/// <remarks>
	/// <para>
	/// The effective delta is scaled by <see cref="AnimationSpeed"/> before being applied:
	/// a speed of 2.0 doubles the rate, 0.5 halves it, and -1.0 plays in reverse.
	/// </para>
	/// <para>
	/// When <see cref="Repeat"/> is <see cref="SKLottieRepeat.Reverse"/>, the internal direction
	/// is flipped automatically when the animation reaches a boundary, producing a ping-pong effect.
	/// </para>
	/// <para>
	/// Has no effect when no animation is loaded (<see cref="HasAnimation"/> is <see langword="false"/>).
	/// </para>
	/// </remarks>
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

		// Apply phase direction (for Reverse ping-pong)
		if (!isInForwardPhase)
			deltaTime = -deltaTime;

		var newProgress = Progress + deltaTime;
		if (newProgress > Duration)
			newProgress = Duration;
		if (newProgress < TimeSpan.Zero)
			newProgress = TimeSpan.Zero;

		Seek(newProgress);
	}

	/// <summary>Renders the current animation frame to the given canvas within the specified rectangle.</summary>
	/// <param name="canvas">The <see cref="SKCanvas"/> to draw onto.</param>
	/// <param name="rect">The destination rectangle within the canvas.</param>
	/// <remarks>
	/// Has no effect when no animation is loaded (<see cref="HasAnimation"/> is <see langword="false"/>).
	/// Call this inside your paint/draw callback after <see cref="Update"/> has been called for the current frame.
	/// </remarks>
	public void Render(SKCanvas canvas, SKRect rect)
	{
		animation?.Render(canvas, rect);
	}

	private void UpdateProgress(TimeSpan progress)
	{
		if (animation is null)
			return;

		animation.SeekFrameTime(progress.TotalSeconds);

		var repeat = Repeat;
		var duration = Duration;

		// Determine effective movement direction
		var movingForward = AnimationSpeed >= 0 ? isInForwardPhase : !isInForwardPhase;

		// Have we reached a boundary based on our movement direction?
		var atStart = !movingForward && progress <= TimeSpan.Zero;
		var atEnd = movingForward && progress >= duration;

		// A run is "finished" based on repeat kind.
		// For Reverse, the finish point is the start of the return trip (atStart for +speed, atEnd for -speed).
		// For Never and Restart, the finish point is simply the end of the movement direction.
		var reverseFinishPoint = AnimationSpeed >= 0 ? atStart : atEnd;
		var isFinishedRun = repeat.IsReverseRepeating
			? reverseFinishPoint
			: (movingForward ? atEnd : atStart);

		// For Reverse mode: flip direction when hitting a boundary (but not the finish boundary)
		var needsFlip = repeat.IsReverseRepeating &&
			(AnimationSpeed >= 0 ? atEnd : atStart) && !isFinishedRun;

		if (needsFlip)
		{
			isInForwardPhase = !isInForwardPhase;
			IsComplete = false;
		}
		else
		{
			var totalRepeatCount = repeat.Count;
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

				if (repeat.IsRestartRepeating)
				{
					// Reset position directly without going through Seek(), to avoid
					// firing AnimationUpdated twice (once here, once in the outer Seek).
					Progress = AnimationSpeed >= 0 ? TimeSpan.Zero : Duration;
					animation.SeekFrameTime(Progress.TotalSeconds);
				}
				else if (repeat.IsReverseRepeating)
					isInForwardPhase = !isInForwardPhase;
			}

			var prevIsComplete = IsComplete;
			IsComplete =
				isFinishedRun &&
				repeatsCompleted >= totalRepeatCount;

			if (IsComplete && !prevIsComplete)
				AnimationCompleted?.Invoke(this, EventArgs.Empty);
		}
	}

	private void Reset()
	{
		isInForwardPhase = true;
		repeatsCompleted = 0;
		IsComplete = false;

		Duration = animation?.Duration ?? TimeSpan.Zero;

		// Directly set the initial position without triggering completion logic.
		Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
		animation?.SeekFrameTime(Progress.TotalSeconds);
		AnimationUpdated?.Invoke(this, EventArgs.Empty);
	}
}
