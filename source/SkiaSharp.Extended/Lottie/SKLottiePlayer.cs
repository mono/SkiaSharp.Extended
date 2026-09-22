using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A platform-agnostic Skottie (Lottie) animation player that manages playback state
/// and rendering.
/// </summary>
/// <remarks>
/// The player is not thread-safe; all calls should occur on the same thread (typically the UI thread).
/// </remarks>
public class SKLottiePlayer
{
	private Skottie.Animation? animation;
	private bool isInForwardPhase = true;
	private int repeatsCompleted;
	private SKLottieRepeat repeat = SKLottieRepeat.Never;

	/// <summary>Gets the total duration of the loaded animation.</summary>
	public TimeSpan Duration { get; private set; } = TimeSpan.Zero;

	/// <summary>Gets the current playback position.</summary>
	public TimeSpan Progress { get; private set; }

	/// <summary>Gets whether the animation has completed all repeats.</summary>
	public bool IsComplete { get; private set; }

	/// <summary>Gets or sets how the animation repeats. Defaults to <see cref="SKLottieRepeat.Never"/>.</summary>
	/// <remarks>
	/// Changing this property resets the repeat counter and completion state but preserves the
	/// current playback direction. The direction phase is reset when <see cref="Animation"/> changes.
	/// </remarks>
	public SKLottieRepeat Repeat
	{
		get => repeat;
		set
		{
			if (repeat != value)
			{
				repeat = value;
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
	public bool HasAnimation => Animation is not null;

	/// <summary>
	/// Gets or sets the animation currently loaded into the player. The caller retains ownership of the native animation.
	/// </summary>
	public Skottie.Animation? Animation
	{
		get => animation;
		set
		{
			animation = value;
			Reset();
		}
	}

	/// <summary>Fires when the animation completes all repeats.</summary>
	public event EventHandler? AnimationCompleted;

	/// <summary>Fires after each <see cref="Seek"/> or <see cref="Update"/> call that changes playback state.</summary>
	public event EventHandler? AnimationUpdated;

	/// <summary>
	/// Seeks the animation to an absolute position and raises <see cref="AnimationUpdated"/>.
	/// </summary>
	/// <remarks>
	/// The position is clamped to [<see cref="TimeSpan.Zero"/>, <see cref="Duration"/>]. Seeking
	/// never consumes repeats, changes direction, or raises <see cref="AnimationCompleted"/>. It
	/// clears <see cref="IsComplete"/> so playback can continue from the requested frame.
	/// </remarks>
	public void Seek(TimeSpan position)
	{
		SetPosition(position);
		IsComplete = false;
		AnimationUpdated?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Advances the animation by the given time delta, applying <see cref="AnimationSpeed"/> and <see cref="Repeat"/>.
	/// </summary>
	/// <remarks>
	/// Once complete, updates are ignored until <see cref="Seek"/>, a changed <see cref="Animation"/>, or a changed
	/// <see cref="Repeat"/> value clears completion. Changing <see cref="AnimationSpeed"/> alone does not resume playback.
	/// </remarks>
	public void Update(TimeSpan deltaTime)
	{
		if (!HasAnimation || IsComplete)
			return;

		var scaledTicks = ScaleTicks(deltaTime.Ticks, AnimationSpeed);
		if (scaledTicks == 0 || Duration <= TimeSpan.Zero)
		{
			AnimationUpdated?.Invoke(this, EventArgs.Empty);
			return;
		}

		if (deltaTime < TimeSpan.Zero)
		{
			SetPosition(Progress + TimeSpan.FromTicks(scaledTicks));
			AnimationUpdated?.Invoke(this, EventArgs.Empty);
			return;
		}

		var remainingTicks = Math.Abs((double)scaledTicks);
		if (repeat.IsReverseRepeating)
			AdvanceReverse(remainingTicks);
		else if (repeat.IsRestartRepeating)
			AdvanceRestart(remainingTicks);
		else
			AdvanceOnce(remainingTicks);

		AnimationUpdated?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Renders the current animation frame to the given canvas within the specified rectangle.</summary>
	public void Render(SKCanvas canvas, SKRect rect) =>
		Animation?.Render(canvas, rect);

	private void AdvanceOnce(double remainingTicks)
	{
		var movingForward = IsMovingForward();
		var distance = movingForward
			? Duration.Ticks - Progress.Ticks
			: Progress.Ticks;

		if (remainingTicks < distance)
		{
			SetPosition(Progress + TimeSpan.FromTicks((long)(movingForward ? remainingTicks : -remainingTicks)));
			return;
		}

		SetPosition(movingForward ? Duration : TimeSpan.Zero);
		Complete();
	}

	private void AdvanceRestart(double remainingTicks)
	{
		var durationTicks = (double)Duration.Ticks;
		var baseForward = AnimationSpeed >= 0;

		// Restart normally follows the natural speed direction. If a caller switched
		// from reverse ping-pong while returning, finish that leg before restarting.
		if (IsMovingForward() != baseForward)
		{
			var movingForward = IsMovingForward();
			var distanceToBoundary = movingForward
				? Duration.Ticks - Progress.Ticks
				: Progress.Ticks;
			if (remainingTicks < distanceToBoundary)
			{
				SetPosition(Progress + TimeSpan.FromTicks((long)(movingForward ? remainingTicks : -remainingTicks)));
				return;
			}

			remainingTicks -= distanceToBoundary;
			SetPosition(movingForward ? Duration : TimeSpan.Zero);
			if (repeat.Count >= 0)
			{
				if (repeatsCompleted >= repeat.Count)
				{
					Complete();
					return;
				}

				repeatsCompleted++;
			}

			isInForwardPhase = true;
		}

		var offset = baseForward ? Progress.Ticks : Duration.Ticks - Progress.Ticks;
		var totalTicks = offset + remainingTicks;

		if (repeat.Count < 0)
		{
			SetRestartPosition(totalTicks % durationTicks, baseForward);
			return;
		}

		var runsToCompletion = repeat.Count - repeatsCompleted + 1d;
		if (totalTicks >= runsToCompletion * durationTicks)
		{
			SetPosition(baseForward ? Duration : TimeSpan.Zero);
			Complete();
			return;
		}

		var completedRuns = (int)(totalTicks / durationTicks);
		repeatsCompleted += completedRuns;
		SetRestartPosition(totalTicks % durationTicks, baseForward);
	}

	private void AdvanceReverse(double remainingTicks)
	{
		var durationTicks = (double)Duration.Ticks;
		var cycleTicks = durationTicks * 2;
		var baseForward = AnimationSpeed >= 0;
		var offset = GetReverseOffset(baseForward, durationTicks);
		var totalTicks = offset + remainingTicks;

		if (repeat.Count < 0)
		{
			SetReversePosition(totalTicks % cycleTicks, baseForward, durationTicks);
			return;
		}

		var cyclesToCompletion = repeat.Count - repeatsCompleted + 1d;
		if (totalTicks >= cyclesToCompletion * cycleTicks)
		{
			SetReversePosition(cycleTicks, baseForward, durationTicks);
			Complete();
			return;
		}

		var completedCycles = (int)(totalTicks / cycleTicks);
		repeatsCompleted += completedCycles;
		SetReversePosition(totalTicks % cycleTicks, baseForward, durationTicks);
	}

	private double GetReverseOffset(bool baseForward, double durationTicks)
	{
		var movingForward = IsMovingForward();
		if (baseForward)
			return movingForward ? Progress.Ticks : 2 * durationTicks - Progress.Ticks;

		return movingForward ? durationTicks + Progress.Ticks : durationTicks - Progress.Ticks;
	}

	private void SetRestartPosition(double offset, bool baseForward)
	{
		isInForwardPhase = true;
		SetPosition(TimeSpan.FromTicks((long)(baseForward ? offset : Duration.Ticks - offset)));
	}

	private void SetReversePosition(double offset, bool baseForward, double durationTicks)
	{
		bool movingForward;
		double progressTicks;

		if (offset < durationTicks)
		{
			movingForward = baseForward;
			progressTicks = baseForward ? offset : durationTicks - offset;
		}
		else
		{
			movingForward = !baseForward;
			progressTicks = baseForward ? 2 * durationTicks - offset : offset - durationTicks;
		}

		isInForwardPhase = baseForward ? movingForward : !movingForward;
		SetPosition(TimeSpan.FromTicks((long)progressTicks));
	}

	private bool IsMovingForward() =>
		AnimationSpeed >= 0 ? isInForwardPhase : !isInForwardPhase;

	private void SetPosition(TimeSpan position)
	{
		if (position < TimeSpan.Zero)
			position = TimeSpan.Zero;
		else if (position > Duration)
			position = Duration;

		Progress = position;
		Animation?.SeekFrameTime(position.TotalSeconds);
	}

	private void Complete()
	{
		if (IsComplete)
			return;

		IsComplete = true;
		AnimationCompleted?.Invoke(this, EventArgs.Empty);
	}

	private void Reset()
	{
		isInForwardPhase = true;
		repeatsCompleted = 0;
		IsComplete = false;
		Duration = Animation?.Duration ?? TimeSpan.Zero;
		SetPosition(AnimationSpeed < 0 ? Duration : TimeSpan.Zero);
		AnimationUpdated?.Invoke(this, EventArgs.Empty);
	}

	private static long ScaleTicks(long ticks, double speed)
	{
		var scaledTicks = ticks * speed;
		if (double.IsNaN(scaledTicks))
			return 0;
		if (scaledTicks >= long.MaxValue)
			return long.MaxValue;
		if (scaledTicks <= long.MinValue)
			return long.MinValue;
		return (long)scaledTicks;
	}
}
