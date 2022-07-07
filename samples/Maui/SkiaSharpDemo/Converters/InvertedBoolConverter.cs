using System.Globalization;

namespace SkiaSharpDemo.Converters
{
	public class InvertedBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value switch
			{
				bool b => !b,
				_ => throw new ArgumentException("Value was not a bool.", nameof(value)),
			};

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
	}
}
