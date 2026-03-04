using System.Globalization;

namespace SkiaSharp.Extended.UI.Controls.Converters;

/// <summary>
/// An abstract type converter that converts to and from string values.
/// </summary>
public abstract class StringTypeConverter : TypeConverter
{
	internal StringTypeConverter()
	{
	}

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		ConvertFromStringCore(value?.ToString());

	/// <inheritdoc/>
	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		ConvertToStringCore(value);

	/// <summary>
	/// Converts a string value to the target type.
	/// </summary>
	/// <param name="value">The string value to convert.</param>
	/// <returns>The converted object, or <see langword="null"/>.</returns>
	protected abstract object? ConvertFromStringCore(string? value);

	/// <summary>
	/// Converts a value to its string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>The string representation of the value.</returns>
	protected virtual string? ConvertToStringCore(object? value) => throw new NotImplementedException();
}
