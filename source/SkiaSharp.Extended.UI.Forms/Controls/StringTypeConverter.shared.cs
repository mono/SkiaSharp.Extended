namespace SkiaSharp.Extended.UI.Controls.Converters;

public abstract class StringTypeConverter : TypeConverter
{
	public override object? ConvertFromInvariantString(string? value) =>
		Convert(value);

	public override string? ConvertToInvariantString(object? value) =>
		ConvertTo(value);

	protected abstract object? Convert(string? value);

	protected virtual string? ConvertTo(object? value) => throw new NotImplementedException();
}
