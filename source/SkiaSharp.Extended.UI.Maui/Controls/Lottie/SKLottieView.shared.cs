namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A view that plays Lottie animations using the Skottie library.
/// </summary>
public class SKLottieView : SKAnimatedSurfaceView, IDisposable
{
	/// <summary>Identifies the <see cref="Source"/> bindable property.</summary>
	public static readonly BindableProperty SourceProperty = BindableProperty.Create(
		nameof(Source), typeof(SKLottieImageSource), typeof(SKLottieView), null, propertyChanged: OnSourcePropertyChanged);

	private static readonly BindablePropertyKey DurationPropertyKey = BindableProperty.CreateReadOnly(
		nameof(Duration), typeof(TimeSpan), typeof(SKLottieView), TimeSpan.Zero, defaultBindingMode: BindingMode.OneWayToSource);

	/// <summary>Identifies the <see cref="Duration"/> bindable property.</summary>
	public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

	/// <summary>Identifies the <see cref="Progress"/> bindable property.</summary>
	public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
		nameof(Progress), typeof(TimeSpan), typeof(SKLottieView), TimeSpan.Zero, BindingMode.TwoWay,
		propertyChanged: OnProgressPropertyChanged);

	private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsComplete), typeof(bool), typeof(SKLottieView), false, defaultBindingMode: BindingMode.OneWayToSource);

	/// <summary>Identifies the <see cref="IsComplete"/> bindable property.</summary>
	public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

	/// <summary>Identifies the <see cref="RepeatCount"/> bindable property.</summary>
	public static readonly BindableProperty RepeatCountProperty = BindableProperty.Create(
		nameof(RepeatCount), typeof(int), typeof(SKLottieView), 0, propertyChanged: OnRepeatPropertyChanged);

	/// <summary>Identifies the <see cref="RepeatMode"/> bindable property.</summary>
	public static readonly BindableProperty RepeatModeProperty = BindableProperty.Create(
		nameof(RepeatMode), typeof(SKLottieRepeatMode), typeof(SKLottieView), SKLottieRepeatMode.Restart,
		propertyChanged: OnRepeatPropertyChanged);

	/// <summary>Identifies the <see cref="AnimationSpeed"/> bindable property.</summary>
	public static readonly BindableProperty AnimationSpeedProperty = BindableProperty.Create(
		nameof(AnimationSpeed), typeof(double), typeof(SKLottieView), 1.0, propertyChanged: OnAnimationSpeedPropertyChanged);

	private readonly SKLottiePlayer player = new();
	private CancellationTokenSource? loadCancellation;
	private bool disposed;
	private bool handlerWasAttached;
	private bool isHandlerAttached;

	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieView"/> class.
	/// </summary>
	public SKLottieView()
	{
		ResourceLoader<Themes.SKLottieViewResources>.EnsureRegistered(this);

		IsAnimationEnabled = true;
		player.Repeat = GetRepeat();
		player.AnimationSpeed = AnimationSpeed;
		player.AnimationUpdated += OnPlayerAnimationUpdated;
		player.AnimationCompleted += OnPlayerAnimationCompleted;
		HandlerChanging += OnHandlerChanging;
		HandlerChanged += OnHandlerChanged;

#if DEBUG
		AnimationCompleted += (s, e) => DebugUtils.LogEvent(nameof(AnimationCompleted));
		AnimationFailed += (s, e) => DebugUtils.LogEvent(nameof(AnimationFailed));
		AnimationLoaded += (s, e) => DebugUtils.LogEvent(nameof(AnimationLoaded));
#endif
	}

	/// <summary>Gets or sets the Lottie animation image source.</summary>
	public SKLottieImageSource? Source
	{
		get => (SKLottieImageSource?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>Gets the total duration of the animation.</summary>
	public TimeSpan Duration
	{
		get => (TimeSpan)GetValue(DurationProperty);
		private set => SetValue(DurationPropertyKey, value);
	}

	/// <summary>Gets or sets the current playback progress of the animation.</summary>
	public TimeSpan Progress
	{
		get => (TimeSpan)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	/// <summary>Gets a value indicating whether the animation has completed all repeats.</summary>
	public bool IsComplete
	{
		get => (bool)GetValue(IsCompleteProperty);
		private set => SetValue(IsCompletePropertyKey, value);
	}

	/// <summary>Gets or sets the number of times to repeat the animation. Use -1 for infinite.</summary>
	public int RepeatCount
	{
		get => (int)GetValue(RepeatCountProperty);
		set => SetValue(RepeatCountProperty, value);
	}

	/// <summary>Gets or sets the repeat mode for the animation.</summary>
	public SKLottieRepeatMode RepeatMode
	{
		get => (SKLottieRepeatMode)GetValue(RepeatModeProperty);
		set => SetValue(RepeatModeProperty, value);
	}

	/// <summary>Gets or sets the animation playback speed multiplier.</summary>
	public double AnimationSpeed
	{
		get => (double)GetValue(AnimationSpeedProperty);
		set => SetValue(AnimationSpeedProperty, value);
	}

	/// <summary>Occurs when the animation fails to load.</summary>
	public event EventHandler<SKLottieAnimationFailedEventArgs>? AnimationFailed;

	/// <summary>Occurs when the animation has been successfully loaded.</summary>
	public event EventHandler<SKLottieAnimationLoadedEventArgs>? AnimationLoaded;

	/// <summary>Occurs when the animation has completed playback.</summary>
	public event EventHandler? AnimationCompleted;

	/// <inheritdoc/>
	protected override void Update(TimeSpan deltaTime) => player.Update(deltaTime);

	/// <inheritdoc/>
	protected override void OnPaintSurface(SKCanvas canvas, SKSize size) =>
		player.Render(canvas, SKRect.Create(SKPoint.Empty, size));

	private void OnPlayerAnimationUpdated(object? sender, EventArgs e)
	{
		Duration = player.Duration;
		Progress = player.Progress;
		IsComplete = player.IsComplete;
	}

	private void OnPlayerAnimationCompleted(object? sender, EventArgs e)
	{
		OnPlayerAnimationUpdated(sender, e);
		AnimationCompleted?.Invoke(this, EventArgs.Empty);
	}

	private async Task LoadAnimationAsync(SKLottieImageSource? imageSource)
	{
		CancelLoad();
		var currentCancellation = new CancellationTokenSource();
		loadCancellation = currentCancellation;
		var cancellationToken = currentCancellation.Token;
		SKLottieAnimation? loadResult = null;

		try
		{
			if (imageSource is null || imageSource.IsEmpty)
			{
				if (IsCurrentLoad(currentCancellation))
					ReplaceAnimation(null);
				return;
			}

			loadResult = await Task.Run(() => imageSource.LoadAnimationAsync(cancellationToken), cancellationToken);
			if (!IsCurrentLoad(currentCancellation))
				return;

			if (loadResult?.IsLoaded == true)
			{
				var animation = loadResult.Animation!;
				ReplaceAnimation(animation);
				loadResult = null;
				AnimationLoaded?.Invoke(this, SKLottieAnimationLoadedEventArgs.Create(animation));
			}
			else
			{
				ReplaceAnimation(null);
				AnimationFailed?.Invoke(this, new SKLottieAnimationFailedEventArgs(
					new InvalidOperationException("The Lottie animation source could not be parsed.")));
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
		}
		catch (Exception ex)
		{
			if (IsCurrentLoad(currentCancellation))
			{
				ReplaceAnimation(null);
				AnimationFailed?.Invoke(this, new SKLottieAnimationFailedEventArgs(ex));
			}
		}
		finally
		{
			loadResult?.Animation?.Dispose();
			if (IsCurrentLoad(currentCancellation) && !IsAnimationEnabled)
				Invalidate();
			if (ReferenceEquals(loadCancellation, currentCancellation))
				loadCancellation = null;
			currentCancellation.Dispose();
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
		if (!lv.ShouldLoadForCurrentHandler())
			return;

		await lv.LoadAnimationAsync(newValue as SKLottieImageSource);
	}

	private async void OnSourceChanged(object? sender, EventArgs e)
	{
		if (!ReferenceEquals(sender, Source) || !ShouldLoadForCurrentHandler())
			return;

		await LoadAnimationAsync(sender as SKLottieImageSource);
	}

	private static void OnProgressPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKLottieView lv)
			return;

		var newProgress = (TimeSpan)newValue!;
		if (lv.player.Progress == newProgress)
			return;

		lv.player.Seek(newProgress);
		if (!lv.IsAnimationEnabled)
			lv.Invalidate();
	}

	private static void OnRepeatPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is SKLottieView lv)
		{
			lv.player.Repeat = lv.GetRepeat();
			lv.OnPlayerAnimationUpdated(lv, EventArgs.Empty);
			if (!lv.IsAnimationEnabled)
				lv.Invalidate();
		}
	}

	private static void OnAnimationSpeedPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is SKLottieView lv)
			lv.player.AnimationSpeed = (double)newValue!;
	}

	private SKLottieRepeat GetRepeat() =>
		RepeatMode == SKLottieRepeatMode.Reverse
			? SKLottieRepeat.Reverse(RepeatCount)
			: RepeatCount == 0 ? SKLottieRepeat.Never : SKLottieRepeat.Restart(RepeatCount);

	private void CancelLoad()
	{
		var cancellation = loadCancellation;
		loadCancellation = null;
		cancellation?.Cancel();
	}

	private bool IsCurrentLoad(CancellationTokenSource cancellation) =>
		!disposed && ReferenceEquals(loadCancellation, cancellation) && !cancellation.IsCancellationRequested;

	private void ReplaceAnimation(Skottie.Animation? newAnimation)
	{
		var previousAnimation = player.Animation;
		player.Animation = newAnimation;
		if (!ReferenceEquals(previousAnimation, newAnimation))
			previousAnimation?.Dispose();
	}

	private bool ShouldLoadForCurrentHandler() => !disposed && (!handlerWasAttached || isHandlerAttached);

	private void OnHandlerChanging(object? sender, HandlerChangingEventArgs e)
	{
		if (e.OldHandler is null)
			return;

		handlerWasAttached = true;
		isHandlerAttached = false;
		CancelLoad();
		ReplaceAnimation(null);
	}

	private void OnHandlerChanged(object? sender, EventArgs e)
	{
		if (disposed || Handler is null)
			return;

		var shouldReload = handlerWasAttached && !isHandlerAttached;
		handlerWasAttached = true;
		isHandlerAttached = true;
		if (shouldReload && Source is { IsEmpty: false } source)
			_ = LoadAnimationAsync(source);
	}

	/// <summary>Releases the loaded animation and cancels any in-progress source load.</summary>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;
		CancelLoad();
		if (Source is SKLottieImageSource source)
			source.SourceChanged -= OnSourceChanged;

		ReplaceAnimation(null);
		player.AnimationUpdated -= OnPlayerAnimationUpdated;
		player.AnimationCompleted -= OnPlayerAnimationCompleted;
		HandlerChanging -= OnHandlerChanging;
		HandlerChanged -= OnHandlerChanged;
		GC.SuppressFinalize(this);
	}
}
