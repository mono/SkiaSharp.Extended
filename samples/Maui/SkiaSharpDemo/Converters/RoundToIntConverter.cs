using System.Globalization;

namespace SkiaSharpDemo.Converters;

public class RoundToIntConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
		value switch
		{
			double d => d,
			int i => (double)i,
			_ => throw new ArgumentException("Value was not an integer or double.", nameof(value)),
		};

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
		value switch
		{
			double d => (int)Math.Round(d),
			int i => i,
			_ => throw new ArgumentException("Value was not an integer or double.", nameof(value)),
		};
}
