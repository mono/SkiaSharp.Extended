namespace SkiaSharp.Extended.UI.Controls.Converters;

public abstract class StringTypeConverter : TypeConverter
{
	internal StringTypeConverter()
	{
	}

	public override object? ConvertFromInvariantString(string? value) =>
		ConvertFromStringCore(value);

	public override string? ConvertToInvariantString(object? value) =>
		ConvertToStringCore(value);

	protected abstract object? ConvertFromStringCore(string? value);

	protected virtual string? ConvertToStringCore(object? value) => throw new NotImplementedException();
}
