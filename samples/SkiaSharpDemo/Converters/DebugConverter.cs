using System;
using System.Globalization;
using Xamarin.Forms;

namespace SkiaSharpDemo.Converters
{
	public class DebugConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			value;
	}
}
