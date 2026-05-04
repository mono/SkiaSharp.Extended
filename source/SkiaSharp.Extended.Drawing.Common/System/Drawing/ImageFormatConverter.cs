using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;

namespace System.Drawing;

public partial class ImageFormatConverter : TypeConverter
{
	public ImageFormatConverter() { }

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
		=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType!);

	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		=> destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is string text)
		{
			text = text.Trim();
			// Try well-known format names
			if (text.Equals("Bmp", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Bmp;
			if (text.Equals("Emf", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Emf;
			if (text.Equals("Exif", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Exif;
			if (text.Equals("Gif", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Gif;
			if (text.Equals("Icon", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Icon;
			if (text.Equals("Jpeg", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Jpeg;
			if (text.Equals("MemoryBmp", StringComparison.OrdinalIgnoreCase)) return ImageFormat.MemoryBmp;
			if (text.Equals("Png", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Png;
			if (text.Equals("Tiff", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Tiff;
			if (text.Equals("Wmf", StringComparison.OrdinalIgnoreCase)) return ImageFormat.Wmf;
			// Try parsing as Guid
			if (Guid.TryParse(text, out var guid)) return new ImageFormat(guid);
			throw new FormatException($"'{text}' is not a valid ImageFormat.");
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is ImageFormat format)
		{
			return format.ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
	{
		return new StandardValuesCollection(new[]
		{
			ImageFormat.Bmp, ImageFormat.Emf, ImageFormat.Exif, ImageFormat.Gif,
			ImageFormat.Icon, ImageFormat.Jpeg, ImageFormat.MemoryBmp, ImageFormat.Png,
			ImageFormat.Tiff, ImageFormat.Wmf
		});
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
}
