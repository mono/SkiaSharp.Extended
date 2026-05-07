using SkiaSharp;
using System.Collections.Generic;

namespace System.Drawing.Text;

/// <summary>Provides a collection of font families built from font files that are provided by the client application.</summary>
public sealed partial class PrivateFontCollection : FontCollection
{
	private readonly List<SKTypeface> _typefaces = new();

	/// <summary>Initializes a new instance of the <see cref="PrivateFontCollection"/> class.</summary>
	public PrivateFontCollection() { }

	/// <summary>Adds a font from the specified file to this <see cref="PrivateFontCollection"/>.</summary>
	public void AddFontFile(string filename)
	{
		if (filename == null) throw new ArgumentNullException(nameof(filename));
		var typeface = SKTypeface.FromFile(filename);
		if (typeface != null)
			_typefaces.Add(typeface);
	}

	/// <summary>Adds a font contained in system memory to this <see cref="PrivateFontCollection"/>.</summary>
	public void AddMemoryFont(nint memory, int length)
	{
		if (memory == IntPtr.Zero) throw new ArgumentNullException(nameof(memory));
		var data = new byte[length];
		System.Runtime.InteropServices.Marshal.Copy(memory, data, 0, length);
		using var stream = new SKMemoryStream(data);
		var typeface = SKTypeface.FromStream(stream);
		if (typeface != null)
			_typefaces.Add(typeface);
	}

	internal override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var tf in _typefaces)
				tf.Dispose();
			_typefaces.Clear();
		}
		base.Dispose(disposing);
	}
}
