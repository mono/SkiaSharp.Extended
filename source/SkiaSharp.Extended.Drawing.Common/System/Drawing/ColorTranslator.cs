using System.Globalization;

namespace System.Drawing
{
	/// <summary>
	///  Translates colors to and from GDI+ <see cref="Color"/> structures.
	/// </summary>
	public static partial class ColorTranslator
	{
		/// <summary>
		///  Translates an HTML color representation to a GDI+ <see cref="Color"/> structure.
		/// </summary>
		/// <param name="htmlColor">The string representation of the HTML color to translate.</param>
		/// <returns>The <see cref="Color"/> structure that represents the translated HTML color.</returns>
		public static System.Drawing.Color FromHtml(string htmlColor)
		{
			if (string.IsNullOrEmpty(htmlColor))
				return Color.Empty;

			// Handle named colors (case-insensitive)
			if (htmlColor[0] != '#')
			{
				// Try known color names
				switch (htmlColor.ToLowerInvariant())
				{
					case "transparent": return Color.Transparent;
					case "activeborder": case "activecaption": case "appworkspace":
					case "background": case "buttonface": case "buttonhighlight":
					case "buttonshadow": case "buttontext": case "captiontext":
					case "graytext": case "highlight": case "highlighttext":
					case "inactiveborder": case "inactivecaption": case "inactivecaptiontext":
					case "infobackground": case "infotext": case "menu": case "menutext":
					case "scrollbar": case "threeddarkshadow": case "threedface":
					case "threedhighlight": case "threedlightshadow": case "threedshadow":
					case "window": case "windowframe": case "windowtext":
						// System colors not supported in SkiaSharp; return black as fallback
						return Color.Black;
					default:
						// Try Color.FromName for named colors like "Red", "Blue", etc.
						var namedColor = Color.FromName(htmlColor);
						if (namedColor.IsNamedColor && namedColor.A != 0)
							return namedColor;
						break;
				}
			}

			// Handle hex colors: #RGB, #RRGGBB
			if (htmlColor[0] == '#')
			{
				string hex = htmlColor.Substring(1);
				if (hex.Length == 3)
				{
					// Expand shorthand #RGB to #RRGGBB
					hex = new string(new[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
				}
				if (hex.Length == 6)
				{
					int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					return Color.FromArgb(r, g, b);
				}
				if (hex.Length == 8)
				{
					int a = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					int r = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					int g = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					int b = int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					return Color.FromArgb(a, r, g, b);
				}
			}

			// Fallback: try Color.FromName
			return Color.FromName(htmlColor);
		}

		/// <summary>
		///  Translates an OLE color value to a GDI+ <see cref="Color"/> structure.
		/// </summary>
		/// <param name="oleColor">The OLE color to translate.</param>
		/// <returns>The <see cref="Color"/> structure that represents the translated OLE color.</returns>
		public static System.Drawing.Color FromOle(int oleColor)
		{
			return FromWin32(oleColor);
		}

		/// <summary>
		///  Translates a Windows color value to a GDI+ <see cref="Color"/> structure.
		/// </summary>
		/// <param name="win32Color">The Windows color to translate.</param>
		/// <returns>The <see cref="Color"/> structure that represents the translated Windows color.</returns>
		public static System.Drawing.Color FromWin32(int win32Color)
		{
			// Win32 COLORREF format: 0x00BBGGRR
			int r = win32Color & 0xFF;
			int g = (win32Color >> 8) & 0xFF;
			int b = (win32Color >> 16) & 0xFF;
			return Color.FromArgb(r, g, b);
		}

		/// <summary>
		///  Translates the specified <see cref="Color"/> structure to an HTML string color representation.
		/// </summary>
		/// <param name="c">The <see cref="Color"/> structure to translate.</param>
		/// <returns>The string that represents the HTML color.</returns>
		public static string ToHtml(System.Drawing.Color c)
		{
			if (c.IsEmpty)
				return string.Empty;

			if (c == Color.Transparent)
				return "Transparent";

			if (c.IsNamedColor)
				return c.Name;

			return "#" + c.R.ToString("X2", CultureInfo.InvariantCulture) +
			       c.G.ToString("X2", CultureInfo.InvariantCulture) +
			       c.B.ToString("X2", CultureInfo.InvariantCulture);
		}

		/// <summary>
		///  Translates the specified <see cref="Color"/> structure to an OLE color.
		/// </summary>
		/// <param name="c">The <see cref="Color"/> structure to translate.</param>
		/// <returns>The OLE color value.</returns>
		public static int ToOle(System.Drawing.Color c)
		{
			return ToWin32(c);
		}

		/// <summary>
		///  Translates the specified <see cref="Color"/> structure to a Windows color.
		/// </summary>
		/// <param name="c">The <see cref="Color"/> structure to translate.</param>
		/// <returns>The Windows color value.</returns>
		public static int ToWin32(System.Drawing.Color c)
		{
			// Win32 COLORREF format: 0x00BBGGRR
			return c.R | (c.G << 8) | (c.B << 16);
		}
	}
}
