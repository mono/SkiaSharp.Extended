namespace SkiaSharp.Extended.UI.Controls;

public class SKAnimatedSurfaceView : SKSurfaceView
{
	public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
		nameof(IsRunning),
		typeof(bool),
		typeof(SKAnimatedSurfaceView),
		false,
		BindingMode.TwoWay,
		propertyChanged: OnIsRunningPropertyChanged);

	private readonly SKFrameCounter frameCounter = new SKFrameCounter();

	internal SKAnimatedSurfaceView()
	{
		Loaded += OnLoaded;
	}

	public bool IsRunning
	{
		get => (bool)GetValue(IsRunningProperty);
		set => SetValue(IsRunningProperty, value);
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
		var deltaTime = IsRunning
			? frameCounter.NextFrame()
			: TimeSpan.Zero;

		Update(deltaTime);
	}

	private static void OnIsRunningPropertyChanged(BindableObject bindable, object? oldValue, object? newValue) =>
		(bindable as SKAnimatedSurfaceView)?.UpdateIsRunning();

	private void OnLoaded(object? sender, EventArgs e)
	{
		UpdateIsRunning();
	}

	private void UpdateIsRunning()
	{
		if (!this.IsLoadedEx())
			return;

		frameCounter.Reset();

		if (!IsRunning)
			return;

		Dispatcher.StartTimer(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				Invalidate();

				return IsRunning;
			});
	}
}
