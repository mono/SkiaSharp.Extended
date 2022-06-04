using System.Globalization;

namespace SkiaSharp.Extended.UI.Controls.Converters;

public abstract class ExtendedTypeConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		=> false;

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Convert(value?.ToString());

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		=> throw new NotSupportedException();

	protected abstract object? Convert(string? value);
}
