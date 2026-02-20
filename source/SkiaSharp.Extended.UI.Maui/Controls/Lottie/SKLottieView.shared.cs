namespace SkiaSharp.Extended.UI.Controls;

public class SKLottieView : SKAnimatedSurfaceView
{
	public static readonly BindableProperty SourceProperty = BindableProperty.Create(
		nameof(Source),
		typeof(SKLottieImageSource),
		typeof(SKLottieView),
		null,
		propertyChanged: OnSourcePropertyChanged);

	private static readonly BindablePropertyKey DurationPropertyKey = BindableProperty.CreateReadOnly(
		nameof(Duration),
		typeof(TimeSpan),
		typeof(SKLottieView),
		TimeSpan.Zero,
		defaultBindingMode: BindingMode.OneWayToSource,
		propertyChanged: OnProgressDurationPropertyChanged);

	public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

	public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
		nameof(Progress),
		typeof(TimeSpan),
		typeof(SKLottieView),
		TimeSpan.Zero,
		BindingMode.TwoWay,
		propertyChanged: OnProgressDurationPropertyChanged);

	private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsComplete),
		typeof(bool),
		typeof(SKLottieView),
		false,
		defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

	public static readonly BindableProperty RepeatCountProperty = BindableProperty.Create(
		nameof(RepeatCount),
		typeof(int),
		typeof(SKLottieView),
		0);

	public static readonly BindableProperty RepeatModeProperty = BindableProperty.Create(
		nameof(RepeatMode),
		typeof(SKLottieRepeatMode),
		typeof(SKLottieView),
		SKLottieRepeatMode.Restart);

	public static readonly BindableProperty AnimationSpeedProperty = BindableProperty.Create(
		nameof(AnimationSpeed),
		typeof(double),
		typeof(SKLottieView),
		1.0);

	private static readonly BindablePropertyKey FpsPropertyKey = BindableProperty.CreateReadOnly(
		nameof(Fps),
		typeof(double),
		typeof(SKLottieView),
		0.0,
		defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly BindableProperty FpsProperty = FpsPropertyKey.BindableProperty;

	private static readonly BindablePropertyKey FrameCountPropertyKey = BindableProperty.CreateReadOnly(
		nameof(FrameCount),
		typeof(int),
		typeof(SKLottieView),
		0,
		defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly BindableProperty FrameCountProperty = FrameCountPropertyKey.BindableProperty;

	private static readonly BindablePropertyKey CurrentFramePropertyKey = BindableProperty.CreateReadOnly(
		nameof(CurrentFrame),
		typeof(int),
		typeof(SKLottieView),
		0,
		defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly BindableProperty CurrentFrameProperty = CurrentFramePropertyKey.BindableProperty;

	private static readonly BindablePropertyKey SegmentStartPropertyKey = BindableProperty.CreateReadOnly(
		nameof(SegmentStart),
		typeof(TimeSpan),
		typeof(SKLottieView),
		TimeSpan.Zero,
		defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly BindableProperty SegmentStartProperty = SegmentStartPropertyKey.BindableProperty;

	private static readonly BindablePropertyKey SegmentEndPropertyKey = BindableProperty.CreateReadOnly(
		nameof(SegmentEnd),
		typeof(TimeSpan),
		typeof(SKLottieView),
		TimeSpan.Zero,
		defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly BindableProperty SegmentEndProperty = SegmentEndPropertyKey.BindableProperty;

	Skottie.Animation? animation;
	bool isInForwardPhase = true;
	int repeatsCompleted = 0;
	CancellationTokenSource? loadCancellation;
	bool isResetting;
	TimeSpan? playToTarget = null;
	int playToDirection = 0; // 1 = forward, -1 = backward
	TimeSpan fullAnimationDuration = TimeSpan.Zero; // InPoint→OutPoint duration from Skottie
	TimeSpan segmentOffset = TimeSpan.Zero;         // offset into the animation where the segment starts

	public SKLottieView()
	{
		ResourceLoader<Themes.SKLottieViewResources>.EnsureRegistered(this);

		IsAnimationEnabled = true;

#if DEBUG
		AnimationCompleted += (s, e) => DebugUtils.LogEvent(nameof(AnimationCompleted));
		AnimationFailed += (s, e) => DebugUtils.LogEvent(nameof(AnimationFailed));
		AnimationLoaded += (s, e) => DebugUtils.LogEvent(nameof(AnimationLoaded));
#endif
	}

	public SKLottieImageSource? Source
	{
		get => (SKLottieImageSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	public TimeSpan Duration
	{
		get => (TimeSpan)GetValue(DurationProperty);
		private set => SetValue(DurationPropertyKey, value);
	}

	public TimeSpan Progress
	{
		get => (TimeSpan)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public bool IsComplete
	{
		get => (bool)GetValue(IsCompleteProperty);
		private set => SetValue(IsCompletePropertyKey, value);
	}

	public int RepeatCount
	{
		get => (int)GetValue(RepeatCountProperty);
		set => SetValue(RepeatCountProperty, value);
	}

	public SKLottieRepeatMode RepeatMode
	{
		get => (SKLottieRepeatMode)GetValue(RepeatModeProperty);
		set => SetValue(RepeatModeProperty, value);
	}

	/// <summary>
	/// Gets or sets the animation playback speed multiplier.
	/// Default is 1.0 (normal speed). Values greater than 1.0 speed up the animation,
	/// values between 0 and 1.0 slow it down. Use 0 to pause the animation.
	/// Negative values reverse the playback direction.
	/// </summary>
	public double AnimationSpeed
	{
		get => (double)GetValue(AnimationSpeedProperty);
		set => SetValue(AnimationSpeedProperty, value);
	}

	/// <summary>
	/// Gets the frames per second of the loaded animation.
	/// Returns 0 if no animation is loaded.
	/// </summary>
	public double Fps
	{
		get => (double)GetValue(FpsProperty);
		private set => SetValue(FpsPropertyKey, value);
	}

	/// <summary>
	/// Gets the total number of frames in the animation.
	/// Returns 0 if no animation is loaded.
	/// </summary>
	public int FrameCount
	{
		get => (int)GetValue(FrameCountProperty);
		private set => SetValue(FrameCountPropertyKey, value);
	}

	/// <summary>
	/// Gets the current frame number of the animation (zero-based).
	/// Returns 0 if no animation is loaded.
	/// </summary>
	public int CurrentFrame
	{
		get => (int)GetValue(CurrentFrameProperty);
		private set => SetValue(CurrentFramePropertyKey, value);
	}

	/// <summary>
	/// Gets the start of the active playback segment, expressed as a time offset from the
	/// animation's InPoint. Defaults to <see cref="TimeSpan.Zero"/> (= InPoint of the animation).
	/// Use <see cref="SetSegment(TimeSpan, TimeSpan)"/> to change.
	/// </summary>
	public TimeSpan SegmentStart
	{
		get => (TimeSpan)GetValue(SegmentStartProperty);
		private set => SetValue(SegmentStartPropertyKey, value);
	}

	/// <summary>
	/// Gets the end of the active playback segment, expressed as a time offset from the
	/// animation's InPoint. Defaults to the full animation duration (= OutPoint).
	/// Use <see cref="SetSegment(TimeSpan, TimeSpan)"/> to change.
	/// </summary>
	public TimeSpan SegmentEnd
	{
		get => (TimeSpan)GetValue(SegmentEndProperty);
		private set => SetValue(SegmentEndPropertyKey, value);
	}

	public event EventHandler<SKLottieAnimationFailedEventArgs>? AnimationFailed;

	public event EventHandler<SKLottieAnimationLoadedEventArgs>? AnimationLoaded;

	public event EventHandler? AnimationCompleted;

	/// <summary>
	/// Seeks to a specific frame in the animation and optionally stops playback.
	/// Frame numbers are zero-based and are clamped to the valid range [0, FrameCount - 1].
	/// </summary>
	/// <param name="frameNumber">The zero-based frame number to seek to.</param>
	/// <param name="stopPlayback">If true, stops animation playback after seeking. Default is false.</param>
	public void SeekToFrame(int frameNumber, bool stopPlayback = false)
	{
		if (animation is null || Fps <= 0)
			return;

		// Cancel any in-progress animated play-to
		playToTarget = null;
		playToDirection = 0;

		var clampedFrame = Math.Max(0, Math.Min(frameNumber, FrameCount - 1));
		Progress = TimeSpan.FromSeconds(clampedFrame / Fps);

		if (stopPlayback)
			IsAnimationEnabled = false;
	}

	/// <summary>
	/// Seeks to a specific time position in the animation and optionally stops playback.
	/// </summary>
	/// <param name="time">The time position to seek to.</param>
	/// <param name="stopPlayback">If true, stops animation playback after seeking. Default is false.</param>
	public void SeekToTime(TimeSpan time, bool stopPlayback = false)
	{
		// Cancel any in-progress animated play-to
		playToTarget = null;
		playToDirection = 0;

		Progress = time;

		if (stopPlayback)
			IsAnimationEnabled = false;
	}

	/// <summary>
	/// Seeks to a normalized progress position (0.0 to 1.0) in the animation and optionally stops playback.
	/// The progress value is clamped to the valid range [0.0, 1.0].
	/// </summary>
	/// <param name="progress">The normalized progress value between 0.0 (start) and 1.0 (end).</param>
	/// <param name="stopPlayback">If true, stops animation playback after seeking. Default is false.</param>
	public void SeekToProgress(double progress, bool stopPlayback = false)
	{
		if (animation is null)
			return;

		// Cancel any in-progress animated play-to
		playToTarget = null;
		playToDirection = 0;

		var clampedProgress = Math.Max(0.0, Math.Min(progress, 1.0));
		Progress = TimeSpan.FromSeconds(Duration.TotalSeconds * clampedProgress);

		if (stopPlayback)
			IsAnimationEnabled = false;
	}

	/// <summary>
	/// Pauses the animation playback. Equivalent to setting <c>IsAnimationEnabled</c> to false.
	/// </summary>
	public void Pause() =>
		IsAnimationEnabled = false;

	/// <summary>
	/// Resumes the animation playback. Equivalent to setting <c>IsAnimationEnabled</c> to true.
	/// </summary>
	public void Resume() =>
		IsAnimationEnabled = true;

	/// <summary>
	/// Restricts playback to the specified frame range (zero-based, relative to the animation's InPoint).
	/// While a segment is active, <see cref="Duration"/>, <see cref="FrameCount"/>, and
	/// <see cref="Progress"/> all operate within the segment. Use <see cref="ClearSegment"/> to restore
	/// the full InPoint→OutPoint range.
	/// </summary>
	/// <param name="startFrame">The first frame of the segment (zero-based, inclusive, clamped to valid range).</param>
	/// <param name="endFrame">The end frame of the segment (zero-based, clamped to valid range). Must be &gt;= startFrame.</param>
	public void SetSegment(int startFrame, int endFrame)
	{
		if (animation is null || Fps <= 0)
			return;

		SetSegment(
			TimeSpan.FromSeconds(Math.Max(0, startFrame) / Fps),
			TimeSpan.FromSeconds(Math.Max(0, endFrame) / Fps));
	}

	/// <summary>
	/// Restricts playback to the specified time range (relative to the animation's InPoint).
	/// While a segment is active, <see cref="Duration"/>, <see cref="FrameCount"/>, and
	/// <see cref="Progress"/> all operate within the segment. Use <see cref="ClearSegment"/> to restore
	/// the full InPoint→OutPoint range.
	/// </summary>
	/// <param name="start">The start of the segment (relative to InPoint, clamped to valid range).</param>
	/// <param name="end">The end of the segment (relative to InPoint, clamped to valid range). Must be &gt;= start.</param>
	public void SetSegment(TimeSpan start, TimeSpan end)
	{
		if (animation is null)
			return;

		var clampedStart = TimeSpan.FromTicks(
			Math.Max(0L, Math.Min(start.Ticks, fullAnimationDuration.Ticks)));
		var clampedEnd = TimeSpan.FromTicks(
			Math.Max(clampedStart.Ticks, Math.Min(end.Ticks, fullAnimationDuration.Ticks)));

		segmentOffset = clampedStart;
		SegmentStart = clampedStart;
		SegmentEnd = clampedEnd;
		ApplyCurrentSegment();
	}

	/// <summary>
	/// Clears the active segment and restores playback over the full InPoint→OutPoint range.
	/// </summary>
	public void ClearSegment()
	{
		if (animation is null)
			return;

		segmentOffset = TimeSpan.Zero;
		SegmentStart = TimeSpan.Zero;
		SegmentEnd = fullAnimationDuration;
		ApplyCurrentSegment();
	}

	// Applies the current SegmentStart/SegmentEnd by updating Duration, FrameCount, and resetting Progress.
	private void ApplyCurrentSegment()
	{
		// Cancel any pending play-to since the playable range has changed
		playToTarget = null;
		playToDirection = 0;

		var segmentDuration = SegmentEnd - segmentOffset;
		isResetting = true;
		try
		{
			Duration = segmentDuration;
			FrameCount = Fps > 0 ? (int)Math.Round(segmentDuration.TotalSeconds * Fps) : 0;
			Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
			CurrentFrame = 0;
		}
		finally
		{
			isResetting = false;
		}
	}

	/// <summary>
	/// Plays the animation from the current position to the specified frame, then stops.
	/// The direction (forward or backward) is determined automatically.
	/// Frame numbers are zero-based and are clamped to the valid range [0, FrameCount - 1].
	/// </summary>
	/// <param name="frameNumber">The zero-based target frame number.</param>
	public void PlayToFrame(int frameNumber)
	{
		if (animation is null || Fps <= 0)
			return;

		var clampedFrame = Math.Max(0, Math.Min(frameNumber, FrameCount - 1));
		PlayToTime(TimeSpan.FromSeconds(clampedFrame / Fps));
	}

	/// <summary>
	/// Plays the animation from the current position to the specified normalized progress, then stops.
	/// The direction (forward or backward) is determined automatically.
	/// The progress value is clamped to the valid range [0.0, 1.0].
	/// </summary>
	/// <param name="progress">The normalized target progress value between 0.0 (start) and 1.0 (end).</param>
	public void PlayToProgress(double progress)
	{
		if (animation is null)
			return;

		var clamped = Math.Max(0.0, Math.Min(progress, 1.0));
		PlayToTime(TimeSpan.FromSeconds(Duration.TotalSeconds * clamped));
	}

	/// <summary>
	/// Plays the animation from the current position to the specified time, then stops.
	/// The direction (forward or backward) is determined automatically.
	/// </summary>
	/// <param name="targetTime">The target time to animate to.</param>
	public void PlayToTime(TimeSpan targetTime)
	{
		if (animation is null)
			return;

		// Clamp target to valid range
		var clampedTicks = Math.Max(0L, Math.Min(targetTime.Ticks, Duration.Ticks));
		var target = TimeSpan.FromTicks(clampedTicks);

		if (target == Progress)
			return; // Already at the target position

		playToDirection = target > Progress ? 1 : -1;
		playToTarget = target;
		IsAnimationEnabled = true;
	}

	protected override void Update(TimeSpan deltaTime)
	{
		if (animation is null)
			return;

		// Play-to mode: animate toward the target, ignoring phase/repeat logic
		if (playToTarget.HasValue)
		{
			// Use the absolute value of AnimationSpeed for the rate, defaulting to 1x
			// if the user has speed set to 0 (since PlayToTime explicitly requests motion)
			var absSpeed = Math.Abs(AnimationSpeed);
			if (!double.IsFinite(absSpeed) || absSpeed == 0) absSpeed = 1.0;

			var scaledTicks = ClampToSafeRange(deltaTime.Ticks * absSpeed * playToDirection);
			var newProgress = Progress + TimeSpan.FromTicks(scaledTicks);

			// Clamp to the play-to target so we don't overshoot
			if (playToDirection > 0 && newProgress > playToTarget.Value)
				newProgress = playToTarget.Value;
			else if (playToDirection < 0 && newProgress < playToTarget.Value)
				newProgress = playToTarget.Value;

			// Also enforce the animation bounds
			if (newProgress > Duration)
				newProgress = Duration;
			if (newProgress < TimeSpan.Zero)
				newProgress = TimeSpan.Zero;

			Progress = newProgress;
			return;
		}

		// Apply animation speed with overflow protection
		deltaTime = TimeSpan.FromTicks(ClampToSafeRange(deltaTime.Ticks * AnimationSpeed));

		// Apply phase direction (for RepeatMode.Reverse ping-pong)
		if (!isInForwardPhase)
			deltaTime = -deltaTime;

		var newProgressNormal = Progress + deltaTime;
		if (newProgressNormal > Duration)
			newProgressNormal = Duration;
		if (newProgressNormal < TimeSpan.Zero)
			newProgressNormal = TimeSpan.Zero;

		Progress = newProgressNormal;
	}

	// Clamps a double tick count to a safe range for TimeSpan.FromTicks, handling NaN/Infinity/overflow.
	private static long ClampToSafeRange(double ticks)
	{
		const long SafeMax = long.MaxValue - 1;
		const long SafeMin = long.MinValue + 2;
		if (!double.IsFinite(ticks))
			return double.IsNaN(ticks) || ticks < 0 ? SafeMin : SafeMax;
		if (ticks > SafeMax)
			return SafeMax;
		if (ticks < SafeMin)
			return SafeMin;
		return (long)ticks;
	}

	protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
		if (animation is null)
			return;

		animation.Render(canvas, SKRect.Create(SKPoint.Empty, size));

#if DEBUG
		WriteDebugStatus($"Repeats: {repeatsCompleted}/{RepeatCount}");
		WriteDebugStatus($"Forward: {isInForwardPhase} ({RepeatMode})");
#endif
	}

	private void UpdateProgress(TimeSpan progress)
	{
		if (animation is null)
		{
			IsComplete = true;
			return;
		}

		animation.SeekFrameTime(segmentOffset.TotalSeconds + progress.TotalSeconds);

		// Keep CurrentFrame in sync with progress
		if (Fps > 0)
			CurrentFrame = (int)Math.Floor(progress.TotalSeconds * Fps);

		// Skip completion/repeat logic during Reset to avoid spurious events
		if (isResetting)
			return;

		// Play-to mode: stop when we reach the target (don't trigger repeat/completion logic)
		if (playToTarget.HasValue)
		{
			var reached = playToDirection > 0
				? progress >= playToTarget.Value
				: progress <= playToTarget.Value;

			if (reached)
			{
				playToTarget = null;
				playToDirection = 0;
				IsAnimationEnabled = false;
			}
			return;
		}

		var repeatMode = RepeatMode;
		var duration = Duration;

		// Determine effective movement direction
		// Negative AnimationSpeed inverts the movement relative to the phase
		var movingForward = AnimationSpeed >= 0 ? isInForwardPhase : !isInForwardPhase;

		// Have we reached a boundary based on our movement direction?
		var atStart = !movingForward && progress <= TimeSpan.Zero;
		var atEnd = movingForward && progress >= duration;
		
		// A run is "finished" based on RepeatMode:
		// - Restart: finished when reaching the destination (end for forward, start for backward)
		// - Reverse: finished when completing full cycle (forward + back to start, or backward + back to end)
		//   With positive speed: start -> end -> start (finish at start)
		//   With negative speed: end -> start -> end (finish at end)
		var reverseFinishPoint = AnimationSpeed >= 0 ? atStart : atEnd;
		var isFinishedRun = repeatMode == SKLottieRepeatMode.Restart 
			? (movingForward ? atEnd : atStart)
			: reverseFinishPoint;

		// For Reverse mode: flip direction when hitting a boundary (but not the finish boundary)
		// With positive speed: flip at end (start going back toward start)
		// With negative speed: flip at start (start going back toward end)
		var needsFlip = repeatMode == SKLottieRepeatMode.Reverse && 
			(AnimationSpeed >= 0 ? atEnd : atStart) && !isFinishedRun;

		if (needsFlip)
		{
			// we need to reverse to finish the run
			isInForwardPhase = !isInForwardPhase;

			IsComplete = false;
		}
		else
		{
			// make sure repeats are positive to make things easier
			var totalRepeatCount = RepeatCount;
			if (totalRepeatCount < 0)
				totalRepeatCount = int.MaxValue;

			// infinite
			var infinite = totalRepeatCount == int.MaxValue;
			if (infinite)
				repeatsCompleted = 0;

			// if we are at the end and we are repeating, then repeat
			if (isFinishedRun && repeatsCompleted < totalRepeatCount)
			{
				if (!infinite)
					repeatsCompleted++;

				isFinishedRun = false;

				if (repeatMode == SKLottieRepeatMode.Restart)
				{
					// Restart at the beginning of the movement direction:
					// - Positive speed: restart at 0, move toward Duration
					// - Negative speed: restart at Duration, move toward 0
					Progress = AnimationSpeed >= 0 ? TimeSpan.Zero : Duration;
				}
				else if (repeatMode == SKLottieRepeatMode.Reverse)
					isInForwardPhase = !isInForwardPhase;
			}

			IsComplete =
				isFinishedRun &&
				repeatsCompleted >= totalRepeatCount;

			if (IsComplete)
				AnimationCompleted?.Invoke(this, EventArgs.Empty);
		}

		if (!IsAnimationEnabled)
			Invalidate();
	}

	private async Task LoadAnimationAsync(SKLottieImageSource? imageSource)
	{
		// Cancel and dispose any in-flight load
		loadCancellation?.Cancel();
		loadCancellation?.Dispose();
		loadCancellation = new CancellationTokenSource();
		var cancellationToken = loadCancellation.Token;

		if (imageSource is null || imageSource.IsEmpty)
		{
			animation = null;
			Reset();
		}
		else
		{
			Exception? exception;
			try
			{
				var loadResult = await Task.Run(() => imageSource.LoadAnimationAsync(cancellationToken), cancellationToken);

				// Check if cancelled before applying result
				if (cancellationToken.IsCancellationRequested)
					return;

				exception = null;
				animation = loadResult.Animation;
			}
			catch (OperationCanceledException)
			{
				// Load was cancelled, don't update state
				return;
			}
			catch (Exception ex)
			{
				exception = ex;
				animation = null;
			}

			Reset();

			if (animation is null)
				AnimationFailed?.Invoke(this, new SKLottieAnimationFailedEventArgs(exception));
			else
				AnimationLoaded?.Invoke(this, SKLottieAnimationLoadedEventArgs.Create(animation));
		}

		if (!IsAnimationEnabled)
			Invalidate();

		void Reset()
		{
			isResetting = true;
			try
			{
				isInForwardPhase = true;
				repeatsCompleted = 0;
				playToTarget = null;
				playToDirection = 0;

				// Store the full InPoint→OutPoint duration so SetSegment can clamp to it
				fullAnimationDuration = animation?.Duration ?? TimeSpan.Zero;

				// Reset segment to the full animation range
				segmentOffset = TimeSpan.Zero;
				SegmentStart = TimeSpan.Zero;
				SegmentEnd = fullAnimationDuration;

				// Initialize Progress based on AnimationSpeed:
				// - Positive/zero speed: start at 0, move toward Duration
				// - Negative speed: start at Duration, move toward 0
				Duration = fullAnimationDuration;
				Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
				Fps = animation?.Fps ?? 0.0;
				FrameCount = Fps > 0 && animation is not null
					? (int)Math.Round(fullAnimationDuration.TotalSeconds * Fps)
					: 0;
				CurrentFrame = 0;
			}
			finally
			{
				isResetting = false;
			}
		}
	}

	private static async void OnSourcePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKLottieView lv)
			return;

		if (oldValue is SKLottieImageSource oldSource)
			oldSource.SourceChanged -= lv.OnSourceChanged;
		if (newValue is SKLottieImageSource newSource)
			newSource.SourceChanged += lv.OnSourceChanged;

		await lv.LoadAnimationAsync(newValue as SKLottieImageSource);
	}

	private async void OnSourceChanged(object? sender, EventArgs e)
	{
		await LoadAnimationAsync(sender as SKLottieImageSource);
	}

	private static void OnProgressDurationPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKLottieView lv)
			return;

		lv.UpdateProgress(lv.Progress);
	}
}
