using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing;

public partial class IconConverter : ExpandableObjectConverter
{
	public IconConverter() { }

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(byte[]) || base.CanConvertFrom(context, sourceType);

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		=> destinationType == typeof(byte[]) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is byte[] bytes)
		{
			using var ms = new MemoryStream(bytes);
			return new Icon(ms);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return value is Icon icon ? icon.ToString() : "(none)";
		}
		if (destinationType == typeof(byte[]) && value is Icon icon2)
		{
			using var ms = new MemoryStream();
			icon2.Save(ms);
			return ms.ToArray();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
