using System.ComponentModel;
using System.Globalization;

namespace System.Drawing
{
	public partial class FontConverter : TypeConverter
	{
		public sealed partial class FontNameConverter : TypeConverter, IDisposable
		{
			public FontNameConverter() { }
			public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
			public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
			{
				if (value is string s) return s;
				return base.ConvertFrom(context, culture, value);
			}
			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
			{
				var families = FontFamily.Families;
				var names = new string[families.Length];
				for (int i = 0; i < families.Length; i++)
					names[i] = families[i].Name;
				return new StandardValuesCollection(names);
			}
			public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;
			public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
			void IDisposable.Dispose() { }
		}

		public partial class FontUnitConverter : EnumConverter
		{
			public FontUnitConverter() : base(typeof(GraphicsUnit)) { }
			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
			{
				// Exclude World and Display which are not typically used for fonts
				return new StandardValuesCollection(new[] { GraphicsUnit.Point, GraphicsUnit.Pixel, GraphicsUnit.Inch, GraphicsUnit.Millimeter, GraphicsUnit.Document });
			}
		}

		public FontConverter() { }

		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			if (value is string text)
			{
				text = text.Trim();
				if (text.Length == 0) return null;

				// Parse format: "Name, Size[pt|px][ style]"
				var sep = culture?.TextInfo.ListSeparator ?? ",";
				var parts = text.Split(new[] { sep }, StringSplitOptions.None);
				string familyName = parts[0].Trim();
				float fontSize = 12f;
				FontStyle fontStyle = FontStyle.Regular;
				GraphicsUnit unit = GraphicsUnit.Point;

				if (parts.Length > 1)
				{
					var sizePart = parts[1].Trim();
					// Remove unit suffix
					if (sizePart.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
					{
						sizePart = sizePart.Substring(0, sizePart.Length - 2).Trim();
						unit = GraphicsUnit.Point;
					}
					else if (sizePart.EndsWith("px", StringComparison.OrdinalIgnoreCase))
					{
						sizePart = sizePart.Substring(0, sizePart.Length - 2).Trim();
						unit = GraphicsUnit.Pixel;
					}
					if (float.TryParse(sizePart, NumberStyles.Float, culture ?? CultureInfo.InvariantCulture, out float parsed))
						fontSize = parsed;
				}

				for (int i = 2; i < parts.Length; i++)
				{
					var stylePart = parts[i].Trim();
					if (stylePart.Equals("Bold", StringComparison.OrdinalIgnoreCase)) fontStyle |= FontStyle.Bold;
					else if (stylePart.Equals("Italic", StringComparison.OrdinalIgnoreCase)) fontStyle |= FontStyle.Italic;
					else if (stylePart.Equals("Strikeout", StringComparison.OrdinalIgnoreCase)) fontStyle |= FontStyle.Strikeout;
					else if (stylePart.Equals("Underline", StringComparison.OrdinalIgnoreCase)) fontStyle |= FontStyle.Underline;
					else if (stylePart.StartsWith("style=", StringComparison.OrdinalIgnoreCase))
					{
						var styleStr = stylePart.Substring(6).Trim();
						if (Enum.TryParse<FontStyle>(styleStr, true, out var parsed2))
							fontStyle = parsed2;
					}
				}

				return new Font(familyName, fontSize, fontStyle, unit);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is Font font)
			{
				var sep = culture?.TextInfo.ListSeparator ?? ",";
				var unitSuffix = font.Unit == GraphicsUnit.Point ? "pt" : "px";
				var result = $"{font.Name}{sep} {font.Size}{unitSuffix}";
				if (font.Style != FontStyle.Regular)
					result += $"{sep} style={font.Style}";
				return result;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(ITypeDescriptorContext? context, System.Collections.IDictionary propertyValues)
		{
			if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
			var name = propertyValues["Name"] as string ?? "Arial";
			var size = propertyValues["Size"] is float f ? f : 12f;
			var style = propertyValues["Style"] is FontStyle fs ? fs : FontStyle.Regular;
			var unit = propertyValues["Unit"] is GraphicsUnit gu ? gu : GraphicsUnit.Point;
			return new Font(name, size, style, unit);
		}

		public override bool GetCreateInstanceSupported(ITypeDescriptorContext? context) => true;

		public override PropertyDescriptorCollection? GetProperties(ITypeDescriptorContext? context, object? value, Attribute[]? attributes)
			=> TypeDescriptor.GetProperties(typeof(Font), attributes);

		public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;
	}
}
