namespace SkiaSharp.Extended.UI.Controls;

public class SKAnimatedSurfaceView : SKSurfaceView
{
	public static readonly BindableProperty IsAnimationEnabledProperty = BindableProperty.Create(
		nameof(IsAnimationEnabled),
		typeof(bool),
		typeof(SKAnimatedSurfaceView),
		false,
		BindingMode.OneWay,
		propertyChanged: OnIsAnimationEnabledPropertyChanged);

	private readonly SKFrameCounter frameCounter = new SKFrameCounter();

	public SKAnimatedSurfaceView()
	{
		Loaded += OnLoaded;
	}

	/// <summary>
	/// Gets or sets a value indicating whether this control will play the animation provided.
	/// </summary>
	public bool IsAnimationEnabled
	{
		get => (bool)GetValue(IsAnimationEnabledProperty);
		set => SetValue(IsAnimationEnabledProperty, value);
	}

	protected virtual void Update(TimeSpan deltaTime)
	{
	}

	internal override void OnPaintSurfaceCore(SKSurface surface, SKSize size)
	{
		base.OnPaintSurfaceCore(surface, size);

#if DEBUG
		WriteDebugStatus($"FPS: {frameCounter.Rate:0.0}");
#endif
	}

	internal override void InvalidateCore()
	{
		UpdateCore();

		base.InvalidateCore();
	}

	private void UpdateCore()
	{
		var deltaTime = IsAnimationEnabled
			? frameCounter.NextFrame()
			: TimeSpan.Zero;

		Update(deltaTime);
	}

	private static void OnIsAnimationEnabledPropertyChanged(BindableObject bindable, object? oldValue, object? newValue) =>
		(bindable as SKAnimatedSurfaceView)?.UpdateIsAnimationEnabled();

	private void OnLoaded(object? sender, EventArgs e)
	{
		UpdateIsAnimationEnabled();
	}

	private void UpdateIsAnimationEnabled()
	{
		if (!this.IsLoadedEx())
			return;

		frameCounter.Reset();

		if (!IsAnimationEnabled)
			return;

		var weakThis = new WeakReference<SKAnimatedSurfaceView>(this);

		Dispatcher.StartTimer(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				if (!weakThis.TryGetTarget(out var strongThis))
					return false;

				if (strongThis.Window is null)
					return false;

				if (!strongThis.IsLoadedEx())
					return false;

				strongThis.Invalidate();

				return strongThis.IsAnimationEnabled;
			});
	}
}
