namespace SkiaSharp.Extended.UI;

internal static class ResourceLoader<T>
	where T : ResourceDictionary, new()
{
	private static bool registered;

	internal static void EnsureRegistered(VisualElement? element = null)
	{
		if (registered)
			return;

		// try register with the current app
		var merged = Application.Current?.Resources?.MergedDictionaries;
		if (merged != null)
		{
			foreach (var dic in merged)
			{
				if (dic.GetType() == typeof(T))
				{
					registered = true;
					break;
				}
			}

			if (!registered)
			{
				merged.Add(new T());
				registered = true;
			}
		}

#if !XAMARIN_FORMS
		// the app may not be ready yet - if this page is part of the app's constructor
		// so we will wait until this view gets a window, then we will try again
		if (!registered && element != null)
		{
			element.PropertyChanged += OnPropertyChanged;

			static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName != VisualElement.WindowProperty.PropertyName)
					return;

				// the window changed, so try one more time
				EnsureRegistered();

				if (sender is VisualElement ve)
					ve.PropertyChanged -= OnPropertyChanged;
			}
		}
#endif
	}
}
