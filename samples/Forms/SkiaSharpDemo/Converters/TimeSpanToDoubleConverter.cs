using System;
using System.Globalization;
using Xamarin.Forms;

namespace SkiaSharpDemo.Converters
{
	public class TimeSpanToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value switch
			{
				TimeSpan ts => ts.TotalMilliseconds == 0 && parameter is not null
					? double.Parse(parameter.ToString())
					: ts.TotalMilliseconds,
				_ => throw new ArgumentException("Value was not a TimeSpan.", nameof(value)),
			};

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			value switch
			{
				double d => TimeSpan.FromMilliseconds(d),
				_ => throw new ArgumentException("Value was not a double.", nameof(value)),
			};
	}
}
