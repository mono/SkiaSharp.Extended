#if !XAMARIN_FORMS
using Microsoft.Maui.Dispatching;
#endif

namespace SkiaSharp.Extended.UI.Controls;

public class SKAnimatedSurfaceView : SKSurfaceView
{
	public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
		nameof(IsRunning),
		typeof(bool),
		typeof(SKAnimatedSurfaceView),
		false,
		propertyChanged: OnIsRunningPropertyChanged);

	internal SKAnimatedSurfaceView()
	{
	}

	public bool IsRunning
	{
		get => (bool)GetValue(IsRunningProperty);
		set => SetValue(IsRunningProperty, value);
	}

	private static void OnIsRunningPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKAnimatedSurfaceView view || newValue is not bool isRunning)
			return;

		view.frameCounter.Reset();

		if (!isRunning)
			return;

#if XAMARIN_FORMS
		Device.StartTimer(
#else
		view.Dispatcher.StartTimer(
#endif
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				view.Invalidate();

				return view.IsRunning;
			});
	}
}
