namespace SkiaSharp.Extended.UI;

#if NETSTANDARD
using IVisualElementRenderer = System.Object;
#endif

internal static class PlatformExtensions
{
	internal static void StartTimer(this IDispatcher dispatcher, TimeSpan interval, Func<bool> callback) =>
		Device.StartTimer(interval, callback);

	internal static IVisualElementRenderer? GetRenderer(this VisualElement element) =>
#if NETSTANDARD
		default;
#else
		Platform.GetRenderer(element);
#endif

	internal static bool IsLoadedEx(this VisualElement element) =>
		element.GetRenderer() is not null;

	internal static void RegisterLoadedUnloaded(this VisualElement element, Action? loaded, Action? unloaded)
	{
		element.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName != "Renderer")
				return;

			if (element.GetRenderer() is null)
				unloaded?.Invoke();
			else
				loaded?.Invoke();
		};
	}
}
