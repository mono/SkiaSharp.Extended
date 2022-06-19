using System.Globalization;

namespace SkiaSharp.Extended.UI.Controls.Converters;

public abstract class StringTypeConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Convert(value?.ToString());

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		ConvertTo(value);

	protected abstract object? Convert(string? value);

	protected virtual string? ConvertTo(object? value) => throw new NotImplementedException();
}
