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

	Skottie.Animation? animation;
	bool playForwards = true;
	int repeatsCompleted = 0;

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
	/// Gets the frames per second of the animation.
	/// </summary>
	public double Fps
	{
		get => (double)GetValue(FpsProperty);
		private set => SetValue(FpsPropertyKey, value);
	}

	/// <summary>
	/// Gets the total number of frames in the animation.
	/// </summary>
	public int FrameCount
	{
		get => (int)GetValue(FrameCountProperty);
		private set => SetValue(FrameCountPropertyKey, value);
	}

	/// <summary>
	/// Gets the current frame number of the animation (zero-based).
	/// </summary>
	public int CurrentFrame
	{
		get => (int)GetValue(CurrentFrameProperty);
		private set => SetValue(CurrentFramePropertyKey, value);
	}

	public event EventHandler<SKLottieAnimationFailedEventArgs>? AnimationFailed;

	public event EventHandler<SKLottieAnimationLoadedEventArgs>? AnimationLoaded;

	public event EventHandler? AnimationCompleted;

	/// <summary>
	/// Seeks to a specific frame in the animation and optionally stops playback.
	/// </summary>
	/// <param name="frameNumber">The zero-based frame number to seek to.</param>
	/// <param name="stopPlayback">If true, stops the animation playback after seeking. Default is false.</param>
	public void SeekToFrame(int frameNumber, bool stopPlayback = false)
	{
		if (animation is null || Fps <= 0)
			return;

		// Clamp frame number to valid range
		var frame = Math.Clamp(frameNumber, 0, Math.Max(0, FrameCount - 1));
		
		// Convert frame to time
		var timeInSeconds = frame / Fps;
		Progress = TimeSpan.FromSeconds(timeInSeconds);

		if (stopPlayback)
			IsAnimationEnabled = false;
	}

	/// <summary>
	/// Seeks to a specific time in the animation and optionally stops playback.
	/// </summary>
	/// <param name="time">The time to seek to.</param>
	/// <param name="stopPlayback">If true, stops the animation playback after seeking. Default is false.</param>
	public void SeekToTime(TimeSpan time, bool stopPlayback = false)
	{
		Progress = time;

		if (stopPlayback)
			IsAnimationEnabled = false;
	}

	/// <summary>
	/// Seeks to a specific progress position in the animation (0.0 to 1.0) and optionally stops playback.
	/// </summary>
	/// <param name="progress">The progress position as a value between 0.0 (start) and 1.0 (end).</param>
	/// <param name="stopPlayback">If true, stops the animation playback after seeking. Default is false.</param>
	public void SeekToProgress(double progress, bool stopPlayback = false)
	{
		if (animation is null)
			return;

		// Clamp progress to 0.0-1.0 range
		var normalizedProgress = Math.Clamp(progress, 0.0, 1.0);
		
		// Convert progress to time
		Progress = TimeSpan.FromSeconds(Duration.TotalSeconds * normalizedProgress);

		if (stopPlayback)
			IsAnimationEnabled = false;
	}

	/// <summary>
	/// Pauses the animation playback.
	/// </summary>
	public void Pause()
	{
		IsAnimationEnabled = false;
	}

	/// <summary>
	/// Resumes the animation playback.
	/// </summary>
	public void Resume()
	{
		IsAnimationEnabled = true;
	}

	protected override void Update(TimeSpan deltaTime)
	{
		if (animation is null)
			return;

		// TODO: handle case where a repeat or revers cases the progress
		//       to either wrap or start the next round

		if (!playForwards)
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
		WriteDebugStatus($"Forward: {playForwards} ({RepeatMode})");
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

		// Update CurrentFrame based on progress
		if (Fps > 0)
		{
			CurrentFrame = (int)Math.Floor(progress.TotalSeconds * Fps);
		}

		var repeatMode = RepeatMode;
		var duration = Duration;

		// have we reached the end of this run
		var atStart = !playForwards && progress <= TimeSpan.Zero;
		var atEnd = playForwards && progress >= duration;
		var isFinishedRun = repeatMode == SKLottieRepeatMode.Restart ? atEnd : atStart;

		// maybe the direction changed
		var needsFlip =
			(atEnd && repeatMode == SKLottieRepeatMode.Reverse) ||
			(atStart && repeatMode == SKLottieRepeatMode.Restart);

		if (needsFlip)
		{
			// we need to reverse to finish the run
			playForwards = !playForwards;

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
					Progress = TimeSpan.Zero;
				else if (repeatMode == SKLottieRepeatMode.Reverse)
					playForwards = !playForwards;
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

	private async Task LoadAnimationAsync(SKLottieImageSource? imageSource, CancellationToken cancellationToken = default)
	{
		// TODO: better error messaging/handling

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
				var loadResult = await Task.Run(() => imageSource.LoadAnimationAsync(cancellationToken));

				exception = null;
				animation = loadResult.Animation;
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
			playForwards = true;
			repeatsCompleted = 0;

			Progress = TimeSpan.Zero;
			Duration = animation?.Duration ?? TimeSpan.Zero;
			Fps = animation?.Fps ?? 0.0;
			FrameCount = Fps > 0 && animation is not null 
				? (int)Math.Ceiling(animation.Duration.TotalSeconds * Fps) 
				: 0;
			CurrentFrame = 0;
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
