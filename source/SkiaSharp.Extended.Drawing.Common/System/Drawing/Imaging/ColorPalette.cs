namespace System.Drawing.Imaging;

/// <summary>
///  Defines an array of colors that make up a color palette. The colors are 32-bit ARGB colors.
/// </summary>
public sealed partial class ColorPalette
{
	private Color[] _entries;
	private int _flags;

	/// <summary>
	///  Initializes a new instance of the <see cref="ColorPalette"/> class.
	/// </summary>
	internal ColorPalette()
	{
		_entries = Array.Empty<Color>();
		_flags = 0;
	}

	/// <summary>
	///  Gets an array of <see cref="Color"/> structures.
	/// </summary>
	public System.Drawing.Color[] Entries { get { return _entries; } }

	/// <summary>
	///  Gets a value that specifies how to interpret the color information in the array of colors.
	/// </summary>
	public int Flags { get { return _flags; } }
}
