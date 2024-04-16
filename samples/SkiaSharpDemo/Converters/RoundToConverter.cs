using System.Globalization;

namespace SkiaSharpDemo.Converters;

public class RoundToConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
		value switch
		{
			double d => d,
			_ => throw new ArgumentException("Value was not a double.", nameof(value)),
		};

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (parameter == null || !int.TryParse(parameter.ToString(), out var decimals))
			decimals = 0;

		return value switch
		{
			double d => Math.Round(d, decimals),
			_ => throw new ArgumentException("Value was not a double.", nameof(value)),
		};
	}
}
