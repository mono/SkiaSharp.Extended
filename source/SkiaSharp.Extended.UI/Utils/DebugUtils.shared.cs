using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI
{
	internal static class DebugUtils
	{
		[Conditional("DEBUG")]
		public static void LogPropertyChanged(BindableObject bindable)
		{
			bindable.PropertyChanged += OnPropertyChanged;

			static void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				var value = sender.GetType().GetProperty(e.PropertyName)?.GetValue(sender);

				Debug.WriteLine($"PropertyChanged: {sender.GetType().Name}.{e.PropertyName} = {value}");
			}
		}
	}
}
