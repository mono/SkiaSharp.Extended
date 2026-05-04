using System.Globalization;

namespace System.Drawing.Printing;

/// <summary>
///  Provides a <see cref="System.ComponentModel.TypeConverter"/> to convert <see cref="Margins"/> to and from other representations.
/// </summary>
public partial class MarginsConverter : System.ComponentModel.ExpandableObjectConverter
{
	/// <summary>Initializes a new instance of the <see cref="MarginsConverter"/> class.</summary>
	public MarginsConverter() { }

	/// <summary>Returns whether this converter can convert an object of one type to the type of this converter.</summary>
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns whether this converter can convert the object to the specified type.</summary>
	public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext? context, System.Type? destinationType)
	{
		return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given value to the type of this converter.</summary>
	public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
	{
		if (value is string s)
		{
			var sep = culture?.TextInfo?.ListSeparator ?? ",";
			var parts = s.Split(new[] { sep }, StringSplitOptions.None);
			if (parts.Length == 4)
			{
				return new Margins(
					int.Parse(parts[0].Trim(), culture),
					int.Parse(parts[1].Trim(), culture),
					int.Parse(parts[2].Trim(), culture),
					int.Parse(parts[3].Trim(), culture));
			}
			throw new ArgumentException($"Cannot convert '{s}' to Margins.");
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the given value object to the specified type.</summary>
	public override object? ConvertTo(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, System.Type destinationType)
	{
		if (destinationType == typeof(string) && value is Margins m)
		{
			var sep = culture?.TextInfo?.ListSeparator ?? ",";
			return $"{m.Left}{sep} {m.Right}{sep} {m.Top}{sep} {m.Bottom}";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Creates an instance of the type that this converter is associated with.</summary>
	public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext? context, System.Collections.IDictionary propertyValues)
	{
		if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
		return new Margins(
			(int)(propertyValues["Left"] ?? 0),
			(int)(propertyValues["Right"] ?? 0),
			(int)(propertyValues["Top"] ?? 0),
			(int)(propertyValues["Bottom"] ?? 0));
	}

	/// <summary>Returns whether changing a value on this object requires a call to <see cref="CreateInstance"/> to create a new value.</summary>
	public override bool GetCreateInstanceSupported(System.ComponentModel.ITypeDescriptorContext? context) => true;
}
