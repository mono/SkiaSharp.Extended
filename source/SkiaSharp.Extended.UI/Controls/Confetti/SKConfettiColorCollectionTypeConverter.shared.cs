namespace SkiaSharp.Extended.UI.Controls.Converters;

#if XAMARIN_FORMS
[TypeConversion(typeof(SKConfettiColorCollection))]
#endif
public class SKConfettiColorCollectionTypeConverter : ExtendedTypeConverter
{
	protected override object? Convert(string? value)
	{
		if (value == null)
			return null;

		value = value?.Trim();
		var parts = value?.Split(',');

		if (parts == null)
			return null;

		var colors = new SKConfettiColorCollection();
		var colConv = new ColorTypeConverter();

		foreach (var part in parts)
		{
			var c = colConv.ConvertFromInvariantString(part);
			colors.Add((Color)c);
		}

		return colors;
	}
}
