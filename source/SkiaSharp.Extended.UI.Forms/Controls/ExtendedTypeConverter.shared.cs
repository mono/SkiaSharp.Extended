namespace SkiaSharp.Extended.UI.Controls.Converters;

public abstract class ExtendedTypeConverter : TypeConverter
{
	public override object? ConvertFromInvariantString(string? value) =>
		Convert(value);

	protected abstract object? Convert(string? value);
}
