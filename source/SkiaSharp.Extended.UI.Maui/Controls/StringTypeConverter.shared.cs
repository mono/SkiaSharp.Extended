using System.Globalization;

namespace SkiaSharp.Extended.UI.Controls.Converters;

public abstract class StringTypeConverter : TypeConverter
{
	internal StringTypeConverter()
	{
	}

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		ConvertFromStringCore(value?.ToString());

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		ConvertToStringCore(value);

	protected abstract object? ConvertFromStringCore(string? value);

	protected virtual string? ConvertToStringCore(object? value) => throw new NotImplementedException();
}
