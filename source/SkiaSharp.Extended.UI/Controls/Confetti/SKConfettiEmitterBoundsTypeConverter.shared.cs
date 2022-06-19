namespace SkiaSharp.Extended.UI.Controls.Converters;

#if XAMARIN_FORMS
[TypeConversion(typeof(SKConfettiEmitterBounds))]
#endif
public class SKConfettiEmitterBoundsTypeConverter : StringTypeConverter
{
	protected override object? ConvertFromStringCore(string? value)
	{
		if (value == null)
			return null;

		value = value?.Trim();

		if (string.Compare(value, "top", StringComparison.OrdinalIgnoreCase) == 0)
			return SKConfettiEmitterBounds.Top;

		if (string.Compare(value, "left", StringComparison.OrdinalIgnoreCase) == 0)
			return SKConfettiEmitterBounds.Left;

		if (string.Compare(value, "right", StringComparison.OrdinalIgnoreCase) == 0)
			return SKConfettiEmitterBounds.Right;

		if (string.Compare(value, "bottom", StringComparison.OrdinalIgnoreCase) == 0)
			return SKConfettiEmitterBounds.Bottom;

		if (string.Compare(value, "center", StringComparison.OrdinalIgnoreCase) == 0)
			return SKConfettiEmitterBounds.Center;

		if (value?.IndexOf(',') == value?.LastIndexOf(','))
		{
			var pointConverter = new PointTypeConverter();
			var point = (Point)pointConverter.ConvertFromInvariantString(value);
			return SKConfettiEmitterBounds.Point(point);
		}

		var rectConverter = new RectTypeConverter();
		var rect = (Rect)rectConverter.ConvertFromInvariantString(value);
		return SKConfettiEmitterBounds.Bounds(rect);
	}
}
