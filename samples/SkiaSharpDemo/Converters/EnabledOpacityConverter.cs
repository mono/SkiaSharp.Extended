using System;
using System.Globalization;
using Xamarin.Forms;

namespace SkiaSharpDemo.Converters
{
	public class EnabledOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is bool enabled && enabled ? 1.0 : 0.5;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}
}
