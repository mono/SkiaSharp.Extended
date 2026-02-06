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

	Skottie.Animation? animation;
	bool isInForwardPhase = true;
	int repeatsCompleted = 0;
	CancellationTokenSource? loadCancellation;
	bool isResetting;

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

	public event EventHandler<SKLottieAnimationFailedEventArgs>? AnimationFailed;

	public event EventHandler<SKLottieAnimationLoadedEventArgs>? AnimationLoaded;

	public event EventHandler? AnimationCompleted;

	protected override void Update(TimeSpan deltaTime)
	{
		if (animation is null)
			return;

		// Apply animation speed with overflow protection
		// Handle NaN and Infinity explicitly, and use safe bounds for long cast
		var scaledTicks = deltaTime.Ticks * AnimationSpeed;
		const long SafeMax = long.MaxValue - 1;  // Avoid overflow when casting from double
		const long SafeMin = long.MinValue + 2;  // Avoid overflow when negating TimeSpan
		if (!double.IsFinite(scaledTicks))
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

		animation.SeekFrameTime(progress.TotalSeconds);

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

				// Initialize Progress based on AnimationSpeed:
				// - Positive/zero speed: start at 0, move toward Duration
				// - Negative speed: start at Duration, move toward 0
				Duration = animation?.Duration ?? TimeSpan.Zero;
				Progress = AnimationSpeed < 0 ? Duration : TimeSpan.Zero;
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
