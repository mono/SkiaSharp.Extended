namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A view that plays Lottie animations using the Skottie library.
/// </summary>
public class SKLottieView : SKAnimatedSurfaceView
{
	/// <summary>
	/// Identifies the <see cref="Source"/> bindable property.
	/// </summary>
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

	/// <summary>
	/// Identifies the <see cref="Duration"/> bindable property.
	/// </summary>
	public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

	/// <summary>
	/// Identifies the <see cref="Progress"/> bindable property.
	/// </summary>
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

	/// <summary>
	/// Identifies the <see cref="IsComplete"/> bindable property.
	/// </summary>
	public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

	/// <summary>
	/// Identifies the <see cref="RepeatCount"/> bindable property.
	/// </summary>
	public static readonly BindableProperty RepeatCountProperty = BindableProperty.Create(
		nameof(RepeatCount),
		typeof(int),
		typeof(SKLottieView),
		0);

	/// <summary>
	/// Identifies the <see cref="RepeatMode"/> bindable property.
	/// </summary>
	public static readonly BindableProperty RepeatModeProperty = BindableProperty.Create(
		nameof(RepeatMode),
		typeof(SKLottieRepeatMode),
		typeof(SKLottieView),
		SKLottieRepeatMode.Restart);

	/// <summary>
	/// Identifies the <see cref="AnimationSpeed"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AnimationSpeedProperty = BindableProperty.Create(
		nameof(AnimationSpeed),
		typeof(double),
		typeof(SKLottieView),
		1.0);

	private readonly SKLottiePlayer player = new();
	private CancellationTokenSource? loadCancellation;
	private bool isSyncingFromPlayer;

	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieView"/> class.
	/// </summary>
	public SKLottieView()
	{
		ResourceLoader<Themes.SKLottieViewResources>.EnsureRegistered(this);

		IsAnimationEnabled = true;

		player.AnimationCompleted += OnPlayerAnimationCompleted;

#if DEBUG
		AnimationCompleted += (s, e) => DebugUtils.LogEvent(nameof(AnimationCompleted));
		AnimationFailed += (s, e) => DebugUtils.LogEvent(nameof(AnimationFailed));
		AnimationLoaded += (s, e) => DebugUtils.LogEvent(nameof(AnimationLoaded));
#endif
	}

	/// <summary>
	/// Gets or sets the Lottie animation image source.
	/// </summary>
	public SKLottieImageSource? Source
	{
		get => (SKLottieImageSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>
	/// Gets the total duration of the animation.
	/// </summary>
	public TimeSpan Duration
	{
		get => (TimeSpan)GetValue(DurationProperty);
		private set => SetValue(DurationPropertyKey, value);
	}

	/// <summary>
	/// Gets or sets the current playback progress of the animation.
	/// </summary>
	public TimeSpan Progress
	{
		get => (TimeSpan)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	/// <summary>
	/// Gets a value indicating whether the animation has completed all repeats.
	/// </summary>
	public bool IsComplete
	{
		get => (bool)GetValue(IsCompleteProperty);
		private set => SetValue(IsCompletePropertyKey, value);
	}

	/// <summary>
	/// Gets or sets the number of times to repeat the animation. Use -1 for infinite.
	/// </summary>
	public int RepeatCount
	{
		get => (int)GetValue(RepeatCountProperty);
		set => SetValue(RepeatCountProperty, value);
	}

	/// <summary>
	/// Gets or sets the repeat mode for the animation.
	/// </summary>
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
	/// Occurs when the animation fails to load.
	/// </summary>
	public event EventHandler<SKLottieAnimationFailedEventArgs>? AnimationFailed;

	/// <summary>
	/// Occurs when the animation has been successfully loaded.
	/// </summary>
	public event EventHandler<SKLottieAnimationLoadedEventArgs>? AnimationLoaded;

	/// <summary>
	/// Occurs when the animation has completed playback.
	/// </summary>
	public event EventHandler? AnimationCompleted;

	/// <inheritdoc/>
	protected override void Update(TimeSpan deltaTime)
	{
		player.Repeat = RepeatMode == SKLottieRepeatMode.Reverse
			? SKLottieRepeat.Reverse(RepeatCount)
			: SKLottieRepeat.Restart(RepeatCount);
		player.AnimationSpeed = AnimationSpeed;

		player.Update(deltaTime);

		SyncFromPlayer();
	}

	/// <inheritdoc/>
	protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
		player.Render(canvas, SKRect.Create(SKPoint.Empty, size));
	}

	private void SyncFromPlayer()
	{
		isSyncingFromPlayer = true;
		try
		{
			Progress = player.Progress;
			IsComplete = player.IsComplete;
		}
		finally
		{
			isSyncingFromPlayer = false;
		}
	}

	private void SyncFromLoad()
	{
		isSyncingFromPlayer = true;
		try
		{
			Duration = player.Duration;
			Progress = player.Progress;
			IsComplete = player.IsComplete;
		}
		finally
		{
			isSyncingFromPlayer = false;
		}
	}

	private void OnPlayerAnimationCompleted(object? sender, EventArgs e)
	{
		AnimationCompleted?.Invoke(this, EventArgs.Empty);
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
			player.SetAnimation(null);
			SyncFromLoad();
		}
		else
		{
			Exception? exception;
			SKLottieAnimation? loadResult = null;
			try
			{
				loadResult = await Task.Run(() => imageSource.LoadAnimationAsync(cancellationToken), cancellationToken);

				// Check if cancelled before applying result
				if (cancellationToken.IsCancellationRequested)
					return;

				exception = null;
			}
			catch (OperationCanceledException)
			{
				// Load was cancelled, don't update state
				return;
			}
			catch (Exception ex)
			{
				exception = ex;
				loadResult = null;
			}

			player.SetAnimation(loadResult?.Animation);
			SyncFromLoad();

			if (player.HasAnimation)
				AnimationLoaded?.Invoke(this, SKLottieAnimationLoadedEventArgs.Create(loadResult!.Animation!));
			else
				AnimationFailed?.Invoke(this, new SKLottieAnimationFailedEventArgs(exception));
		}

		if (!IsAnimationEnabled)
			Invalidate();
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

		// Skip if we are syncing from the player to avoid re-entrant seeks
		if (lv.isSyncingFromPlayer)
			return;

		// User-driven change (e.g. scrubbing): propagate to player
		lv.player.Progress = lv.Progress;

		// Trigger repaint if animation is disabled (e.g. user is scrubbing a paused animation)
		if (!lv.IsAnimationEnabled)
			lv.Invalidate();
	}

}
