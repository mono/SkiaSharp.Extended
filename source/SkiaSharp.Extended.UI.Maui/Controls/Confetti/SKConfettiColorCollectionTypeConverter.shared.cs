namespace SkiaSharp.Extended.UI.Controls.Converters;

public class SKConfettiColorCollectionTypeConverter : StringTypeConverter
{
	protected override object? ConvertFromStringCore(string? value)
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
			var c = colConv.ConvertFromInvariantString(part.Trim());
			colors.Add((Color)c);
		}

		return colors;
	}
}
