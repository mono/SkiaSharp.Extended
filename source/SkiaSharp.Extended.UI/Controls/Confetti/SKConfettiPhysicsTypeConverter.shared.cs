namespace SkiaSharp.Extended.UI.Controls.Converters;

#if XAMARIN_FORMS
[TypeConversion(typeof(SKConfettiPhysics))]
#endif
public class SKConfettiPhysicsTypeConverter : StringTypeConverter
{
	protected override object? ConvertFromStringCore(string? value)
	{
		if (value == null)
			return null;

		value = value?.Trim();

		var pointConverter = new PointTypeConverter();
		var point = (Point)pointConverter.ConvertFromInvariantString(value);
		return new SKConfettiPhysics(point.X, point.Y);
	}
}
