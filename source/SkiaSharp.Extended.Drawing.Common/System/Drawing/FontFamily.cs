using SkiaSharp;

namespace System.Drawing
{
	/// <summary>
	///  Defines a group of type faces having a similar basic design and certain variations in styles.
	///  This class cannot be inherited.
	/// </summary>
	public sealed partial class FontFamily : System.MarshalByRefObject, System.IDisposable
	{
		/// <summary>
		///  The SkiaSharp typeface that backs this font family.
		/// </summary>
		internal SKTypeface SKTypeface { get; private set; }

		private bool _disposed;

		/// <summary>
		///  Initializes a new <see cref="FontFamily"/> from the specified generic font family.
		/// </summary>
		/// <param name="genericFamily">The <see cref="Text.GenericFontFamilies"/> from which to create the new <see cref="FontFamily"/>.</param>
		public FontFamily(System.Drawing.Text.GenericFontFamilies genericFamily)
		{
			var name = genericFamily switch
			{
				Text.GenericFontFamilies.Serif => "serif",
				Text.GenericFontFamilies.SansSerif => "sans-serif",
				Text.GenericFontFamilies.Monospace => "monospace",
				_ => "sans-serif",
			};
			SKTypeface = SKTypeface.FromFamilyName(name) ?? SKTypeface.Default;
		}

		/// <summary>
		///  Initializes a new <see cref="FontFamily"/> with the specified name.
		/// </summary>
		/// <param name="name">The name of the new <see cref="FontFamily"/>.</param>
		public FontFamily(string name)
		{
			SKTypeface = SKTypeface.FromFamilyName(name) ?? SKTypeface.Default;
		}

		/// <summary>
		///  Initializes a new <see cref="FontFamily"/> in the specified <see cref="Text.FontCollection"/> with the specified name.
		/// </summary>
		/// <param name="name">The name of the new <see cref="FontFamily"/>.</param>
		/// <param name="fontCollection">The <see cref="Text.FontCollection"/> that contains this <see cref="FontFamily"/>.</param>
		public FontFamily(string name, System.Drawing.Text.FontCollection? fontCollection)
		{
			// FontCollection is not used in the SkiaSharp implementation.
			SKTypeface = SKTypeface.FromFamilyName(name) ?? SKTypeface.Default;
		}

		/// <summary>
		///  Initializes a new <see cref="FontFamily"/> wrapping an existing <see cref="SkiaSharp.SKTypeface"/>.
		/// </summary>
		/// <param name="typeface">The <see cref="SkiaSharp.SKTypeface"/> to wrap.</param>
		internal FontFamily(SKTypeface typeface)
		{
			SKTypeface = typeface ?? SKTypeface.Default;
		}

		/// <summary>
		///  Returns an array that contains all the <see cref="FontFamily"/> objects currently available in the system.
		/// </summary>
		public static System.Drawing.FontFamily[] Families
		{
			get
			{
				var manager = SKFontManager.Default;
				var count = manager.FontFamilyCount;
				var families = new FontFamily[count];
				for (int i = 0; i < count; i++)
				{
					families[i] = new FontFamily(manager.GetFamilyName(i));
				}
				return families;
			}
		}

		/// <summary>
		///  Gets a generic monospace <see cref="FontFamily"/>.
		/// </summary>
		public static System.Drawing.FontFamily GenericMonospace => new FontFamily(Text.GenericFontFamilies.Monospace);

		/// <summary>
		///  Gets a generic sans serif <see cref="FontFamily"/>.
		/// </summary>
		public static System.Drawing.FontFamily GenericSansSerif => new FontFamily(Text.GenericFontFamilies.SansSerif);

		/// <summary>
		///  Gets a generic serif <see cref="FontFamily"/>.
		/// </summary>
		public static System.Drawing.FontFamily GenericSerif => new FontFamily(Text.GenericFontFamilies.Serif);

		/// <summary>
		///  Gets the name of this <see cref="FontFamily"/>.
		/// </summary>
		public string Name => SKTypeface.FamilyName;

		/// <summary>
		///  Returns an array that contains all the <see cref="FontFamily"/> objects associated with the specified graphics.
		/// </summary>
		[System.ObsoleteAttribute("FontFamily.GetFamilies has been deprecated. Use Families instead.")]
		public static System.Drawing.FontFamily[] GetFamilies(System.Drawing.Graphics graphics)
			=> Families;

		/// <summary>
		///  Releases all resources used by this <see cref="FontFamily"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					SKTypeface?.Dispose();
					SKTypeface = null!;
				}
				_disposed = true;
			}
		}

		/// <summary>
		///  Indicates whether the specified object is a <see cref="FontFamily"/> and is identical to this <see cref="FontFamily"/>.
		/// </summary>
		public override bool Equals(object? obj)
			=> obj is FontFamily other &&
			   string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

		/// <summary>
		///  Gets the height, in font design units, of the em square for the specified style.
		/// </summary>
		/// <param name="style">The <see cref="FontStyle"/> for which to get the em height.</param>
		/// <returns>The height of the em square.</returns>
		public int GetEmHeight(System.Drawing.FontStyle style)
			=> SKTypeface.UnitsPerEm;

		/// <summary>
		///  Returns the cell ascent, in design units, of the <see cref="FontFamily"/> of the specified style.
		/// </summary>
		/// <param name="style">A <see cref="FontStyle"/> that contains style information for the font family.</param>
		/// <returns>The cell ascent for this <see cref="FontFamily"/> that uses the specified <see cref="FontStyle"/>.</returns>
		public int GetCellAscent(System.Drawing.FontStyle style)
		{
			using var typeface = CreateTypefaceForStyle(style);
			using var font = new SKFont(typeface, (float)typeface.UnitsPerEm);
			return (int)Math.Round(Math.Abs(font.Metrics.Ascent));
		}

		/// <summary>
		///  Returns the cell descent, in design units, of the <see cref="FontFamily"/> of the specified style.
		/// </summary>
		/// <param name="style">A <see cref="FontStyle"/> that contains style information for the font family.</param>
		/// <returns>The cell descent metric for this <see cref="FontFamily"/> that uses the specified <see cref="FontStyle"/>.</returns>
		public int GetCellDescent(System.Drawing.FontStyle style)
		{
			using var typeface = CreateTypefaceForStyle(style);
			using var font = new SKFont(typeface, (float)typeface.UnitsPerEm);
			return (int)Math.Round(Math.Abs(font.Metrics.Descent));
		}

		/// <summary>
		///  Returns a hash code for this <see cref="FontFamily"/>.
		/// </summary>
		public override int GetHashCode()
			=> StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

		/// <summary>
		///  Returns the line spacing, in design units, of the <see cref="FontFamily"/> of the specified style.
		///  The line spacing is the vertical distance between the base lines of two consecutive lines of text.
		/// </summary>
		/// <param name="style">The <see cref="FontStyle"/> to apply.</param>
		/// <returns>The distance between two consecutive lines of text.</returns>
		public int GetLineSpacing(System.Drawing.FontStyle style)
		{
			using var typeface = CreateTypefaceForStyle(style);
			using var font = new SKFont(typeface, (float)typeface.UnitsPerEm);
			var m = font.Metrics;
			return (int)Math.Round(Math.Abs(m.Ascent) + Math.Abs(m.Descent) + Math.Abs(m.Leading));
		}

		/// <summary>
		///  Returns the name, in the specified language, of this <see cref="FontFamily"/>.
		/// </summary>
		/// <param name="language">The language in which the name is returned.</param>
		/// <returns>A <see cref="string"/> that represents the name, in the specified language, of this <see cref="FontFamily"/>.</returns>
		public string GetName(int language) => Name;

		/// <summary>
		///  Indicates whether the specified <see cref="FontStyle"/> enumeration is available.
		/// </summary>
		/// <param name="style">The <see cref="FontStyle"/> to test.</param>
		/// <returns><see langword="true"/> if the specified <see cref="FontStyle"/> is available; otherwise, <see langword="false"/>.</returns>
		public bool IsStyleAvailable(System.Drawing.FontStyle style)
		{
			// SkiaSharp will always resolve a typeface (possibly via fallback),
			// so all styles are considered available.
			return true;
		}

		/// <summary>
		///  Converts this <see cref="FontFamily"/> to a human-readable string representation.
		/// </summary>
		public override string ToString() => $"[FontFamily: Name={Name}]";

		/// <summary>
		///  Allows a <see cref="FontFamily"/> to attempt to free resources before it is reclaimed by garbage collection.
		/// </summary>
		~FontFamily()
		{
			Dispose(false);
		}

		/// <summary>
		///  Maps a <see cref="FontStyle"/> to SkiaSharp font style weight and slant.
		/// </summary>
		internal static (SKFontStyleWeight weight, SKFontStyleSlant slant) ToSkiaStyle(FontStyle style)
		{
			var weight = (style & FontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
			var slant = (style & FontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
			return (weight, slant);
		}

		/// <summary>
		///  Creates a new <see cref="SkiaSharp.SKTypeface"/> that matches this family and the specified style.
		///  The caller is responsible for disposing the returned typeface.
		/// </summary>
		internal SKTypeface CreateTypefaceForStyle(FontStyle style)
		{
			var (weight, slant) = ToSkiaStyle(style);
			return SKTypeface.FromFamilyName(SKTypeface.FamilyName, weight, SKFontStyleWidth.Normal, slant)
			       ?? SKTypeface.Default;
		}
	}
}
