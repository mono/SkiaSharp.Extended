namespace SkiaSharp.Extended.UI.Controls.Converters;

/// <summary>
/// Converts string values to <see cref="SKConfettiPhysics"/> instances.
/// </summary>
public class SKConfettiPhysicsTypeConverter : StringTypeConverter
{
	/// <inheritdoc/>
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
