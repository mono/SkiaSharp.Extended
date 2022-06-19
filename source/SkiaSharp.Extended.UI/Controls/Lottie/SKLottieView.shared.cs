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
		TimeSpan.Zero);

	public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

	public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
		nameof(Progress),
		typeof(TimeSpan),
		typeof(SKLottieView),
		TimeSpan.Zero,
		BindingMode.TwoWay,
		propertyChanged: OnProgressPropertyChanged);

	private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsComplete),
		typeof(bool),
		typeof(SKLottieView),
		false);

	public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

	Skottie.Animation? animation;

	public SKLottieView()
	{
		Themes.SKLottieViewResources.EnsureRegistered();

		IsRunning = true;
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

	protected override void Update(TimeSpan deltaTime)
	{
		if (animation is null)
			return;

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
	}

	private static void OnProgressPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKLottieView lv || newValue is not TimeSpan progress)
			return;

		lv.UpdateProgress(progress);
	}

	private void UpdateProgress(TimeSpan progress)
	{
		if (animation is null)
			return;

		animation.SeekFrameTime(progress.TotalSeconds);

		UpdateIsComplete();

		if (!IsRunning)
			Invalidate();
	}

	private static async void OnSourcePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKLottieView lv)
			return;

		await lv.LoadAnimationAsync(newValue as SKLottieImageSource);
	}

	private async Task LoadAnimationAsync(SKLottieImageSource? imageSource, CancellationToken cancellationToken = default)
	{
		var newAnimation = imageSource is not null
			? await imageSource.LoadAnimationAsync(cancellationToken)
			: null;

		animation = newAnimation;

		Progress = TimeSpan.Zero;
		Duration = TimeSpan.FromSeconds(animation?.Duration ?? 0);

		if (!IsRunning)
			Invalidate();
	}

	private void UpdateIsComplete()
	{
		if (animation is null)
		{
			IsComplete = true;
			return;
		}

		IsComplete = Progress >= Duration;
	}
}
