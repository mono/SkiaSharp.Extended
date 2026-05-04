using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing;

public partial class ImageConverter : TypeConverter
{
	public ImageConverter() { }

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(byte[]) || base.CanConvertFrom(context, sourceType);

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		=> destinationType == typeof(byte[]) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is byte[] bytes)
		{
			using var ms = new MemoryStream(bytes);
			return Image.FromStream(ms);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return value is Image ? value.ToString()! : "(none)";
		}
		if (destinationType == typeof(byte[]) && value is Image img)
		{
			using var ms = new MemoryStream();
			img.Save(ms, Imaging.ImageFormat.Png);
			return ms.ToArray();
		}
		return base.ConvertTo(context, culture, value, destinationType)!;
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object? value, Attribute[]? attributes)
		=> TypeDescriptor.GetProperties(typeof(Image), attributes);

	public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;
}
