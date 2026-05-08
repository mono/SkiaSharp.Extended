namespace System.Drawing.Imaging;

/// <summary>
///  Defines a map for converting colors. Several methods of the <see cref="ImageAttributes"/> class adjust image colors by using a color-remap table, which is an array of <see cref="ColorMap"/> structures.
/// </summary>
public sealed partial class ColorMap
{
	/// <summary>
	///  Initializes a new instance of the <see cref="ColorMap"/> class.
	/// </summary>
	public ColorMap() { }

	/// <summary>
	///  Gets or sets the new <see cref="Color"/> structure to which to convert.
	/// </summary>
	public Color NewColor { get; set; }

	/// <summary>
	///  Gets or sets the existing <see cref="Color"/> structure to be converted.
	/// </summary>
	public Color OldColor { get; set; }
}
