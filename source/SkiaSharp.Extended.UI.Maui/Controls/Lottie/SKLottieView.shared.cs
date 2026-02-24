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

	public static readonly BindableProperty FrameStartProperty = BindableProperty.Create(
		nameof(FrameStart),
		typeof(int),
		typeof(SKLottieView),
		0,
		propertyChanged: OnFramePropertyChanged);

	public static readonly BindableProperty FrameEndProperty = BindableProperty.Create(
		nameof(FrameEnd),
		typeof(int),
		typeof(SKLottieView),
		-1,
		propertyChanged: OnFramePropertyChanged);

	Skottie.Animation? animation;
	bool isInForwardPhase = true;
	int repeatsCompleted = 0;
	CancellationTokenSource? loadCancellation;
	bool isResetting;
	int fullFrameCount = 0;             // InPoint→OutPoint frame count from Skottie
	TimeSpan segmentOffset = TimeSpan.Zero; // time offset into the animation from FrameStart

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
	/// Gets the current frame number of the animation (zero-based, relative to <see cref="FrameStart"/>).
	/// Returns 0 if no animation is loaded.
	/// </summary>
	public int CurrentFrame
	{
		get => (int)GetValue(CurrentFrameProperty);
		private set => SetValue(CurrentFramePropertyKey, value);
	}

	/// <summary>
	/// Gets or sets the first frame to play (zero-based, offset from the animation's InPoint).
	/// Default is 0 (= InPoint). Clamped to [0, total frame count].
	/// Setting this property immediately updates <see cref="Duration"/>, <see cref="FrameCount"/>,
	/// and resets <see cref="Progress"/> to the beginning of the range.
	/// </summary>
	public int FrameStart
	{
		get => (int)GetValue(FrameStartProperty);
		set => SetValue(FrameStartProperty, value);
	}

	/// <summary>
	/// Gets or sets the end frame (exclusive, zero-based, offset from the animation's InPoint).
	/// Use -1 (the default) to play through to the animation's OutPoint.
	/// Clamped to [FrameStart, total frame count].
	/// Setting this property immediately updates <see cref="Duration"/>, <see cref="FrameCount"/>,
	/// and resets <see cref="Progress"/> to the beginning of the range.
	/// </summary>
	public int FrameEnd
	{
		get => (int)GetValue(FrameEndProperty);
		set => SetValue(FrameEndProperty, value);
	}

	public event EventHandler<SKLottieAnimationFailedEventArgs>? AnimationFailed;

	public event EventHandler<SKLottieAnimationLoadedEventArgs>? AnimationLoaded;

	public event EventHandler? AnimationCompleted;

	// Recomputes segmentOffset, Duration, FrameCount from the current FrameStart/FrameEnd values.
	// Called whenever FrameStart or FrameEnd changes, or when an animation finishes loading.
	private void ApplyFrames()
	{
		if (animation is null || Fps <= 0)
			return;

		var effectiveStart = Math.Max(0, Math.Min(FrameStart, fullFrameCount));
		var effectiveEnd = FrameEnd < 0
			? fullFrameCount
			: Math.Max(effectiveStart, Math.Min(FrameEnd, fullFrameCount));

		segmentOffset = TimeSpan.FromSeconds(effectiveStart / Fps);
		var segmentFrames = effectiveEnd - effectiveStart;
		var segmentDuration = TimeSpan.FromSeconds(segmentFrames / Fps);

		isResetting = true;
		try
		{
			Duration = segmentDuration;
			FrameCount = segmentFrames;
			Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
			CurrentFrame = 0;
		}
		finally
		{
			isResetting = false;
		}
	}

	protected override void Update(TimeSpan deltaTime)
	{
		if (animation is null)
			return;

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

				Fps = animation?.Fps ?? 0.0;
				fullFrameCount = Fps > 0 && animation is not null
					? (int)Math.Round(animation.Duration.TotalSeconds * Fps)
					: 0;

				// Compute the effective playback range from FrameStart/FrameEnd (clamped to the new animation)
				var effectiveStart = Fps > 0 ? Math.Max(0, Math.Min(FrameStart, fullFrameCount)) : 0;
				var effectiveEnd = Fps > 0
					? (FrameEnd < 0 ? fullFrameCount : Math.Max(effectiveStart, Math.Min(FrameEnd, fullFrameCount)))
					: 0;

				segmentOffset = Fps > 0 ? TimeSpan.FromSeconds(effectiveStart / Fps) : TimeSpan.Zero;
				var segmentFrames = effectiveEnd - effectiveStart;
				var segmentDuration = Fps > 0 ? TimeSpan.FromSeconds(segmentFrames / Fps) : TimeSpan.Zero;

				// Initialize Progress based on AnimationSpeed:
				// - Positive/zero speed: start at 0, move toward Duration
				// - Negative speed: start at Duration, move toward 0
				Duration = segmentDuration;
				FrameCount = segmentFrames;
				Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
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

	private static void OnFramePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is SKLottieView lv)
			lv.ApplyFrames();
	}

	private static void OnProgressDurationPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKLottieView lv)
			return;

		lv.UpdateProgress(lv.Progress);
	}
}
