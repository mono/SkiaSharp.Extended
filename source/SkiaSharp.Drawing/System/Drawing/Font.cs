using SkiaSharp;

namespace System.Drawing
{
	/// <summary>
	///  Defines a particular format for text, including font face, size, and style attributes.
	///  This class cannot be inherited.
	/// </summary>
	[System.ComponentModel.EditorAttribute("System.Drawing.Design.FontEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.FontConverter))]
	public sealed partial class Font : System.MarshalByRefObject, System.ICloneable, System.IDisposable, System.Runtime.Serialization.ISerializable
	{
		private const float DefaultDpi = 96f;

		/// <summary>
		///  The SkiaSharp font used for text measurement and rendering.
		/// </summary>
		internal SKFont SKFont { get; private set; }

		/// <summary>
		///  The SkiaSharp typeface backing this font.
		/// </summary>
		internal SKTypeface SKTypeface { get; private set; }

		private readonly float _emSize;
		private readonly FontStyle _style;
		private readonly GraphicsUnit _unit;
		private readonly byte _gdiCharSet;
		private readonly bool _gdiVerticalFont;
		private readonly string _originalFontName;
		private bool _disposed;

		/// <summary>
		///  Initializes a new <see cref="Font"/> that uses the specified existing <see cref="Font"/> and <see cref="FontStyle"/> enumeration.
		/// </summary>
		public Font(System.Drawing.Font prototype, System.Drawing.FontStyle newStyle)
			: this(prototype.FontFamily, prototype._emSize, newStyle, prototype._unit, prototype._gdiCharSet, prototype._gdiVerticalFont)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified <see cref="FontFamily"/> and size.
		/// </summary>
		public Font(System.Drawing.FontFamily family, float emSize)
			: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified <see cref="FontFamily"/>, size, and style.
		/// </summary>
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style)
			: this(family, emSize, style, GraphicsUnit.Point, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified <see cref="FontFamily"/>, size, style, and unit.
		/// </summary>
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit)
			: this(family, emSize, style, unit, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified <see cref="FontFamily"/>, size, style, unit, and character set.
		/// </summary>
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet)
			: this(family, emSize, style, unit, gdiCharSet, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified <see cref="FontFamily"/>, size, style, unit, character set, and vertical font flag.
		/// </summary>
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
		{
			if (family is null) throw new ArgumentNullException(nameof(family));
			if (emSize <= 0) throw new ArgumentException("emSize must be greater than 0.", nameof(emSize));

			_emSize = emSize;
			_style = style;
			_unit = unit;
			_gdiCharSet = gdiCharSet;
			_gdiVerticalFont = gdiVerticalFont;
			_originalFontName = family.Name;

			SKTypeface = family.CreateTypefaceForStyle(style);
			SKFont = new SKFont(SKTypeface, ConvertToPixels(emSize, unit));
			SKFont.Subpixel = true;
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified <see cref="FontFamily"/>, size, and unit.
		/// </summary>
		public Font(System.Drawing.FontFamily family, float emSize, System.Drawing.GraphicsUnit unit)
			: this(family, emSize, FontStyle.Regular, unit, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified family name and size.
		/// </summary>
		public Font(string familyName, float emSize)
			: this(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified family name, size, and style.
		/// </summary>
		public Font(string familyName, float emSize, System.Drawing.FontStyle style)
			: this(familyName, emSize, style, GraphicsUnit.Point, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified family name, size, style, and unit.
		/// </summary>
		public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit)
			: this(familyName, emSize, style, unit, 1, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified family name, size, style, unit, and character set.
		/// </summary>
		public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet)
			: this(familyName, emSize, style, unit, gdiCharSet, false)
		{
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified family name, size, style, unit, character set, and vertical font flag.
		/// </summary>
		public Font(string familyName, float emSize, System.Drawing.FontStyle style, System.Drawing.GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
		{
			if (familyName is null) throw new ArgumentNullException(nameof(familyName));
			if (emSize <= 0) throw new ArgumentException("emSize must be greater than 0.", nameof(emSize));

			_emSize = emSize;
			_style = style;
			_unit = unit;
			_gdiCharSet = gdiCharSet;
			_gdiVerticalFont = gdiVerticalFont;
			_originalFontName = familyName;

			var (weight, slant) = FontFamily.ToSkiaStyle(style);
			SKTypeface = SKTypeface.FromFamilyName(familyName, weight, SKFontStyleWidth.Normal, slant)
			             ?? SKTypeface.Default;
			SKFont = new SKFont(SKTypeface, ConvertToPixels(emSize, unit));
			SKFont.Subpixel = true;
		}

		/// <summary>
		///  Initializes a new <see cref="Font"/> using a specified family name, size, and unit.
		/// </summary>
		public Font(string familyName, float emSize, System.Drawing.GraphicsUnit unit)
			: this(familyName, emSize, FontStyle.Regular, unit, 1, false)
		{
		}

		/// <summary>
		///  Gets a value that indicates whether this <see cref="Font"/> is bold.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Bold => (_style & FontStyle.Bold) != 0;

		/// <summary>
		///  Gets the <see cref="System.Drawing.FontFamily"/> associated with this <see cref="Font"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.FontFamily FontFamily => new FontFamily(SKTypeface);

		/// <summary>
		///  Gets a byte value that specifies the GDI character set that this <see cref="Font"/> uses.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public byte GdiCharSet => _gdiCharSet;

		/// <summary>
		///  Gets a value that indicates whether this <see cref="Font"/> is derived from a GDI vertical font.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool GdiVerticalFont => _gdiVerticalFont;

		/// <summary>
		///  Gets the line spacing of this font, in pixels.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public int Height
		{
			get
			{
				ThrowIfDisposed();
				var m = SKFont.Metrics;
				return (int)Math.Ceiling(Math.Abs(m.Ascent) + Math.Abs(m.Descent) + Math.Abs(m.Leading));
			}
		}

		/// <summary>
		///  Gets a value indicating whether this <see cref="Font"/> is a member of <see cref="SystemFonts"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public bool IsSystemFont => false;

		/// <summary>
		///  Gets a value that indicates whether this <see cref="Font"/> has the italic style applied.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Italic => (_style & FontStyle.Italic) != 0;

		/// <summary>
		///  Gets the face name of this <see cref="Font"/>.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		[System.ComponentModel.EditorAttribute("System.Drawing.Design.FontNameEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.FontConverter.FontNameConverter))]
		public string Name
		{
			get
			{
				ThrowIfDisposed();
				return SKTypeface.FamilyName;
			}
		}

		/// <summary>
		///  Gets the font family name originally specified when this <see cref="Font"/> was created.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public string? OriginalFontName => _originalFontName;

		/// <summary>
		///  Gets the em-size of this <see cref="Font"/> measured in the units specified by the <see cref="Unit"/> property.
		/// </summary>
		public float Size => _emSize;

		/// <summary>
		///  Gets the em-size, in points, of this <see cref="Font"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public float SizeInPoints => ConvertToPoints(_emSize, _unit);

		/// <summary>
		///  Gets a value that indicates whether this <see cref="Font"/> specifies a horizontal line through the font.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Strikeout => (_style & FontStyle.Strikeout) != 0;

		/// <summary>
		///  Gets style information for this <see cref="Font"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public System.Drawing.FontStyle Style => _style;

		/// <summary>
		///  Gets the name of the system font if the <see cref="IsSystemFont"/> property returns <see langword="true"/>.
		/// </summary>
		[System.ComponentModel.BrowsableAttribute(false)]
		public string SystemFontName => string.Empty;

		/// <summary>
		///  Gets a value that indicates whether this <see cref="Font"/> is underlined.
		/// </summary>
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Underline => (_style & FontStyle.Underline) != 0;

		/// <summary>
		///  Gets the unit of measure for this <see cref="Font"/>.
		/// </summary>
		[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.FontConverter.FontUnitConverter))]
		public System.Drawing.GraphicsUnit Unit => _unit;

		/// <summary>
		///  Creates a <see cref="Font"/> from the specified Windows handle to a device context.
		/// </summary>
		public static System.Drawing.Font FromHdc(nint hdc) { throw new System.PlatformNotSupportedException("FromHdc is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Creates a <see cref="Font"/> from the specified Windows handle to a font.
		/// </summary>
		public static System.Drawing.Font FromHfont(nint hfont) { throw new System.PlatformNotSupportedException("FromHfont is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Creates a <see cref="Font"/> from the specified GDI logical font (LOGFONT) structure.
		/// </summary>
		public static System.Drawing.Font FromLogFont(object lf) { throw new System.PlatformNotSupportedException("FromLogFont is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Creates a <see cref="Font"/> from the specified GDI logical font (LOGFONT) structure.
		/// </summary>
		public static System.Drawing.Font FromLogFont(object lf, nint hdc) { throw new System.PlatformNotSupportedException("FromLogFont is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Creates an exact copy of this <see cref="Font"/>.
		/// </summary>
		public object Clone()
		{
			ThrowIfDisposed();
			return new Font(_originalFontName, _emSize, _style, _unit, _gdiCharSet, _gdiVerticalFont);
		}

		/// <summary>
		///  Releases all resources used by this <see cref="Font"/>.
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
					SKFont?.Dispose();
					SKFont = null!;
					SKTypeface?.Dispose();
					SKTypeface = null!;
				}
				_disposed = true;
			}
		}

		/// <summary>
		///  Indicates whether the specified object is a <see cref="Font"/> and has the same properties as this <see cref="Font"/>.
		/// </summary>
		public override bool Equals(object? obj)
		{
			if (obj is not Font other) return false;
			return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
			       && Math.Abs(_emSize - other._emSize) < 0.001f
			       && _style == other._style
			       && _unit == other._unit;
		}

		/// <summary>
		///  Gets the hash code for this <see cref="Font"/>.
		/// </summary>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
				hash = (hash * 397) ^ _emSize.GetHashCode();
				hash = (hash * 397) ^ (int)_style;
				hash = (hash * 397) ^ (int)_unit;
				return hash;
			}
		}

		/// <summary>
		///  Returns the line spacing, in pixels, of this font.
		/// </summary>
		/// <returns>The line spacing, in pixels, of this font.</returns>
		public float GetHeight()
		{
			ThrowIfDisposed();
			return GetHeight(DefaultDpi);
		}

		/// <summary>
		///  Returns the line spacing, in the current unit of a specified <see cref="Graphics"/>, of this font.
		/// </summary>
		public float GetHeight(System.Drawing.Graphics graphics)
		{
			ThrowIfDisposed();
			if (graphics is null) throw new ArgumentNullException(nameof(graphics));
			return GetHeight(graphics.DpiY);
		}

		/// <summary>
		///  Returns the height, in pixels, of this <see cref="Font"/> when drawn to a device with the specified vertical resolution.
		/// </summary>
		/// <param name="dpi">The vertical resolution, in dots per inch, used to calculate the height of the font.</param>
		/// <returns>The height, in pixels, of this <see cref="Font"/>.</returns>
		public float GetHeight(float dpi)
		{
			ThrowIfDisposed();
			// Compute line spacing in pixels at the target DPI
			float sizeInPoints = SizeInPoints;
			float sizeInPixelsAtDpi = sizeInPoints * dpi / 72f;

			// Scale the font metrics proportionally
			using var tempFont = new SKFont(SKTypeface, sizeInPixelsAtDpi);
			var m = tempFont.Metrics;
			return Math.Abs(m.Ascent) + Math.Abs(m.Descent) + Math.Abs(m.Leading);
		}

		/// <summary>
		///  Returns a handle to this <see cref="Font"/>.
		/// </summary>
		public nint ToHfont() { throw new System.PlatformNotSupportedException("ToHfont is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Creates a GDI logical font (LOGFONT) structure from this <see cref="Font"/>.
		/// </summary>
		public void ToLogFont(object logFont) { throw new System.PlatformNotSupportedException("ToLogFont is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Creates a GDI logical font (LOGFONT) structure from this <see cref="Font"/>.
		/// </summary>
		public void ToLogFont(object logFont, System.Drawing.Graphics graphics) { throw new System.PlatformNotSupportedException("ToLogFont is not supported in SkiaSharp.Drawing."); }

		/// <summary>
		///  Returns a human-readable string representation of this <see cref="Font"/>.
		/// </summary>
		public override string ToString()
		{
			ThrowIfDisposed();
			return $"[Font: Name={Name}, Size={_emSize}, Units={_unit}, GdiCharSet={_gdiCharSet}, GdiVerticalFont={_gdiVerticalFont}]";
		}

		/// <summary>
		///  Allows a <see cref="Font"/> to attempt to free resources before it is reclaimed by garbage collection.
		/// </summary>
		~Font()
		{
			Dispose(false);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(Font));
		}

		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			throw new System.PlatformNotSupportedException("Serialization is not supported in SkiaSharp.Drawing.");
		}

		/// <summary>
		///  Converts an em size in the specified <see cref="GraphicsUnit"/> to pixels, assuming 96 DPI.
		/// </summary>
		private static float ConvertToPixels(float emSize, GraphicsUnit unit)
		{
			return unit switch
			{
				GraphicsUnit.Pixel => emSize,
				GraphicsUnit.Point => emSize * DefaultDpi / 72f,
				GraphicsUnit.Inch => emSize * DefaultDpi,
				GraphicsUnit.Document => emSize * DefaultDpi / 300f,
				GraphicsUnit.Millimeter => emSize * DefaultDpi / 25.4f,
				GraphicsUnit.Display => emSize, // 1 display unit = 1 pixel at 96 DPI
				GraphicsUnit.World => emSize,
				_ => emSize,
			};
		}

		/// <summary>
		///  Converts an em size in the specified <see cref="GraphicsUnit"/> to points.
		/// </summary>
		private static float ConvertToPoints(float emSize, GraphicsUnit unit)
		{
			return unit switch
			{
				GraphicsUnit.Point => emSize,
				GraphicsUnit.Pixel => emSize * 72f / DefaultDpi,
				GraphicsUnit.Inch => emSize * 72f,
				GraphicsUnit.Document => emSize * 72f / 300f,
				GraphicsUnit.Millimeter => emSize * 72f / 25.4f,
				GraphicsUnit.Display => emSize * 72f / DefaultDpi,
				GraphicsUnit.World => emSize * 72f / DefaultDpi,
				_ => emSize,
			};
		}
	}
}
