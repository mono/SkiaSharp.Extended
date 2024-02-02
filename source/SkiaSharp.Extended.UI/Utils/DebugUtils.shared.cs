using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SkiaSharp.Extended.UI;

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

	[Conditional("DEBUG")]
	public static void LogMessage(string message, params object[] args)
	{
		Debug.WriteLine($"{message} [{string.Join(", ", args.Select(a => a?.ToString()))}]");
	}

	[Conditional("DEBUG")]
	public static void LogEvent(string eventName, params object[] args)
	{
		Debug.WriteLine($"Event: {eventName}({string.Join(", ", args.Select(a => a?.ToString()))})");
	}
}
