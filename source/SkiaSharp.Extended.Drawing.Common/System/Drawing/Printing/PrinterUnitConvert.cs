namespace System.Drawing.Printing;

/// <summary>
///  Specifies a series of conversion methods that are useful when interoperating with the Win32 printing API.
/// </summary>
public sealed partial class PrinterUnitConvert
{
	/// <summary>Hundredths of a millimeter per inch.</summary>
	private const double HundredthsMmPerInch = 2540.0;

	/// <summary>Tenths of a millimeter per inch.</summary>
	private const double TenthsMmPerInch = 254.0;

	/// <summary>Display units (hundredths of an inch) per thousandth of an inch.</summary>
	private const double DisplayUnitsPerThousandth = 10.0;

	/// <summary>Thousandths of an inch per inch.</summary>
	private const double ThousandthsPerInch = 1000.0;

	internal PrinterUnitConvert() {}

	/// <summary>Converts a double-precision floating-point number from one <see cref="PrinterUnit"/> type to another.</summary>
	public static double Convert(double value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		double inThousandths = ConvertToThousandthsOfAnInch(value, fromUnit);
		return ConvertFromThousandthsOfAnInch(inThousandths, toUnit);
	}

	/// <summary>Converts a <see cref="Point"/> from one <see cref="PrinterUnit"/> type to another.</summary>
	public static Point Convert(Point value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Point(
			(int)Convert((double)value.X, fromUnit, toUnit),
			(int)Convert((double)value.Y, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="Margins"/> from one <see cref="PrinterUnit"/> type to another.</summary>
	public static Margins Convert(Margins value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Margins(
			(int)Convert((double)value.Left, fromUnit, toUnit),
			(int)Convert((double)value.Right, fromUnit, toUnit),
			(int)Convert((double)value.Top, fromUnit, toUnit),
			(int)Convert((double)value.Bottom, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="Rectangle"/> from one <see cref="PrinterUnit"/> type to another.</summary>
	public static Rectangle Convert(Rectangle value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Rectangle(
			(int)Convert((double)value.X, fromUnit, toUnit),
			(int)Convert((double)value.Y, fromUnit, toUnit),
			(int)Convert((double)value.Width, fromUnit, toUnit),
			(int)Convert((double)value.Height, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="Size"/> from one <see cref="PrinterUnit"/> type to another.</summary>
	public static Size Convert(Size value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Size(
			(int)Convert((double)value.Width, fromUnit, toUnit),
			(int)Convert((double)value.Height, fromUnit, toUnit));
	}

	/// <summary>Converts an integer from one <see cref="PrinterUnit"/> type to another.</summary>
	public static int Convert(int value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return (int)Convert((double)value, fromUnit, toUnit);
	}

	private static double ConvertToThousandthsOfAnInch(double value, PrinterUnit unit)
	{
		switch (unit)
		{
			case PrinterUnit.Display:
				// Display units are hundredths of an inch
				return value * DisplayUnitsPerThousandth;
			case PrinterUnit.ThousandthsOfAnInch:
				return value;
			case PrinterUnit.HundredthsOfAMillimeter:
				// 1 inch = 2540 hundredths of a millimeter
				return value * ThousandthsPerInch / HundredthsMmPerInch;
			case PrinterUnit.TenthsOfAMillimeter:
				// 1 inch = 254 tenths of a millimeter
				return value * ThousandthsPerInch / TenthsMmPerInch;
			default:
				return value;
		}
	}

	private static double ConvertFromThousandthsOfAnInch(double value, PrinterUnit unit)
	{
		switch (unit)
		{
			case PrinterUnit.Display:
				return value / DisplayUnitsPerThousandth;
			case PrinterUnit.ThousandthsOfAnInch:
				return value;
			case PrinterUnit.HundredthsOfAMillimeter:
				return value * HundredthsMmPerInch / ThousandthsPerInch;
			case PrinterUnit.TenthsOfAMillimeter:
				return value * TenthsMmPerInch / ThousandthsPerInch;
			default:
				return value;
		}
	}
}
