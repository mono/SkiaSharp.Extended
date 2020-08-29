using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaSharp.Extended.Controls.Converters
{
	[TypeConversion(typeof(SKConfettiColorCollection))]
	public class SKConfettiColorCollectionTypeConverter : TypeConverter
	{
		public override object? ConvertFromInvariantString(string? value)
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
}
