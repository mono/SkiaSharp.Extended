namespace System.Drawing.Printing
{
	/// <summary>
	///  Specifies a series of conversion methods that are useful when interoperating with the Win32 printing API.
	/// </summary>
	public sealed partial class PrinterUnitConvert
	{
		internal PrinterUnitConvert() {}

		/// <summary>Converts a double-precision floating-point number from one <see cref="PrinterUnit"/> type to another.</summary>
		public static double Convert(double value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit)
		{
			double inThousandths = ConvertToThousandthsOfAnInch(value, fromUnit);
			return ConvertFromThousandthsOfAnInch(inThousandths, toUnit);
		}

		/// <summary>Converts a <see cref="Point"/> from one <see cref="PrinterUnit"/> type to another.</summary>
		public static System.Drawing.Point Convert(System.Drawing.Point value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit)
		{
			return new Point(
				(int)Convert((double)value.X, fromUnit, toUnit),
				(int)Convert((double)value.Y, fromUnit, toUnit));
		}

		/// <summary>Converts a <see cref="Margins"/> from one <see cref="PrinterUnit"/> type to another.</summary>
		public static System.Drawing.Printing.Margins Convert(System.Drawing.Printing.Margins value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit)
		{
			return new Margins(
				(int)Convert((double)value.Left, fromUnit, toUnit),
				(int)Convert((double)value.Right, fromUnit, toUnit),
				(int)Convert((double)value.Top, fromUnit, toUnit),
				(int)Convert((double)value.Bottom, fromUnit, toUnit));
		}

		/// <summary>Converts a <see cref="Rectangle"/> from one <see cref="PrinterUnit"/> type to another.</summary>
		public static System.Drawing.Rectangle Convert(System.Drawing.Rectangle value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit)
		{
			return new Rectangle(
				(int)Convert((double)value.X, fromUnit, toUnit),
				(int)Convert((double)value.Y, fromUnit, toUnit),
				(int)Convert((double)value.Width, fromUnit, toUnit),
				(int)Convert((double)value.Height, fromUnit, toUnit));
		}

		/// <summary>Converts a <see cref="Size"/> from one <see cref="PrinterUnit"/> type to another.</summary>
		public static System.Drawing.Size Convert(System.Drawing.Size value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit)
		{
			return new Size(
				(int)Convert((double)value.Width, fromUnit, toUnit),
				(int)Convert((double)value.Height, fromUnit, toUnit));
		}

		/// <summary>Converts an integer from one <see cref="PrinterUnit"/> type to another.</summary>
		public static int Convert(int value, System.Drawing.Printing.PrinterUnit fromUnit, System.Drawing.Printing.PrinterUnit toUnit)
		{
			return (int)Convert((double)value, fromUnit, toUnit);
		}

		private static double ConvertToThousandthsOfAnInch(double value, PrinterUnit unit)
		{
			switch (unit)
			{
				case PrinterUnit.Display:
					// Display units are hundredths of an inch
					return value * 10.0;
				case PrinterUnit.ThousandthsOfAnInch:
					return value;
				case PrinterUnit.HundredthsOfAMillimeter:
					// 1 inch = 2540 hundredths of a millimeter
					return value * 1000.0 / 2540.0;
				case PrinterUnit.TenthsOfAMillimeter:
					// 1 inch = 254 tenths of a millimeter
					return value * 1000.0 / 254.0;
				default:
					return value;
			}
		}

		private static double ConvertFromThousandthsOfAnInch(double value, PrinterUnit unit)
		{
			switch (unit)
			{
				case PrinterUnit.Display:
					return value / 10.0;
				case PrinterUnit.ThousandthsOfAnInch:
					return value;
				case PrinterUnit.HundredthsOfAMillimeter:
					return value * 2540.0 / 1000.0;
				case PrinterUnit.TenthsOfAMillimeter:
					return value * 254.0 / 1000.0;
				default:
					return value;
			}
		}
	}
}
