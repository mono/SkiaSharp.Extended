using System;
using System.Globalization;
using Xamarin.Forms;

namespace SkiaSharpDemo.Converters
{
	public class EnabledDisabledConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is bool enabled && enabled ? "Disable" : "Enable";

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}
}
