using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaSharp.Extended.Controls
{
	[TypeConversion(typeof(SKConfettiSystemBounds))]
	public class SKConfettiSystemBoundsTypeConverter : TypeConverter
	{
		public override object? ConvertFromInvariantString(string? value)
		{
			if (value == null)
				return null;

			value = value?.Trim();

			if (string.Compare(value, "top", StringComparison.OrdinalIgnoreCase) == 0)
				return SKConfettiSystemBounds.Top;

			if (string.Compare(value, "left", StringComparison.OrdinalIgnoreCase) == 0)
				return SKConfettiSystemBounds.Left;

			if (string.Compare(value, "right", StringComparison.OrdinalIgnoreCase) == 0)
				return SKConfettiSystemBounds.Right;

			if (string.Compare(value, "bottom", StringComparison.OrdinalIgnoreCase) == 0)
				return SKConfettiSystemBounds.Bottom;

			if (string.Compare(value, "center", StringComparison.OrdinalIgnoreCase) == 0)
				return SKConfettiSystemBounds.Center;

			if (value?.IndexOf(',') == value?.LastIndexOf(','))
			{
				var pointConverter = new PointTypeConverter();
				var point = (Point)pointConverter.ConvertFromInvariantString(value);
				return SKConfettiSystemBounds.Location(point);
			}

			var rectConverter = new RectTypeConverter();
			var rect = (Rect)rectConverter.ConvertFromInvariantString(value);
			return SKConfettiSystemBounds.Bounds(rect);
		}
	}
}
