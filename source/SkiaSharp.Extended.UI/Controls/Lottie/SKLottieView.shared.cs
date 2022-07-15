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

	Skottie.Animation? animation;
	bool playForwards = true;
	int repeatsCompleted = 0;

	public SKLottieView()
	{
		Themes.SKLottieViewResources.EnsureRegistered();

		IsAnimationEnabled = true;
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

	public event EventHandler? AnimationFailed;

	public event EventHandler? AnimationLoaded;

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
			try
			{
				animation = await Task.Run(() => imageSource.LoadAnimationAsync(cancellationToken));
			}
			catch
			{
				animation = null;
			}

			Reset();

			if (animation is null)
				AnimationFailed?.Invoke(this, EventArgs.Empty);
			else
				AnimationLoaded?.Invoke(this, EventArgs.Empty);
		}

		if (!IsAnimationEnabled)
			Invalidate();

		void Reset()
		{
			playForwards = true;
			repeatsCompleted = 0;

			Progress = TimeSpan.Zero;
			Duration = animation?.Duration ?? TimeSpan.Zero;
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
