using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaSharp.Extended.UI.Controls.Converters
{
	[TypeConversion(typeof(SKConfettiPhysics))]
	public class SKConfettiPhysicsTypeConverter : TypeConverter
	{
		public override object? ConvertFromInvariantString(string? value)
		{
			if (value == null)
				return null;

			value = value?.Trim();

			var pointConverter = new PointTypeConverter();
			var point = (Point)pointConverter.ConvertFromInvariantString(value);
			return new SKConfettiPhysics(point.X, point.Y);
		}
	}
}
