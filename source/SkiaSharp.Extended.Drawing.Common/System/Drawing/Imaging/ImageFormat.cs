namespace System.Drawing.Imaging;

/// <summary>
///  Specifies the file format of the image.
/// </summary>
[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.ImageFormatConverter))]
public sealed partial class ImageFormat
{
	private readonly Guid _guid;

	private static readonly ImageFormat s_bmp = new ImageFormat(new Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_emf = new ImageFormat(new Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_exif = new ImageFormat(new Guid("{b96b3cb2-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_gif = new ImageFormat(new Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_icon = new ImageFormat(new Guid("{b96b3cb5-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_jpeg = new ImageFormat(new Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_memoryBmp = new ImageFormat(new Guid("{b96b3caa-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_png = new ImageFormat(new Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_tiff = new ImageFormat(new Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}"));
	private static readonly ImageFormat s_wmf = new ImageFormat(new Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}"));

	/// <summary>
	///  Initializes a new instance of the <see cref="ImageFormat"/> class by using the specified <see cref="System.Guid"/> structure.
	/// </summary>
	public ImageFormat(System.Guid guid) { _guid = guid; }

	/// <summary>
	///  Gets the BMP image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Bmp { get { return s_bmp; } }

	/// <summary>
	///  Gets the enhanced metafile (EMF) image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Emf { get { return s_emf; } }

	/// <summary>
	///  Gets the Exchangeable Image File (Exif) format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Exif { get { return s_exif; } }

	/// <summary>
	///  Gets the Graphics Interchange Format (GIF) image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Gif { get { return s_gif; } }

	/// <summary>
	///  Gets a <see cref="System.Guid"/> structure that represents this <see cref="ImageFormat"/> object.
	/// </summary>
	public System.Guid Guid { get { return _guid; } }

	/// <summary>
	///  Gets the Windows icon image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Icon { get { return s_icon; } }

	/// <summary>
	///  Gets the Joint Photographic Experts Group (JPEG) image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Jpeg { get { return s_jpeg; } }

	/// <summary>
	///  Gets the format of a bitmap in memory.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat MemoryBmp { get { return s_memoryBmp; } }

	/// <summary>
	///  Gets the W3C Portable Network Graphics (PNG) image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Png { get { return s_png; } }

	/// <summary>
	///  Gets the Tagged Image File Format (TIFF) image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Tiff { get { return s_tiff; } }

	/// <summary>
	///  Gets the Windows metafile (WMF) image format.
	/// </summary>
	public static System.Drawing.Imaging.ImageFormat Wmf { get { return s_wmf; } }

	/// <summary>
	///  Returns a value that indicates whether the specified object is an <see cref="ImageFormat"/> object that is equivalent to this <see cref="ImageFormat"/> object.
	/// </summary>
	public override bool Equals(object? o) { return o is ImageFormat other && _guid == other._guid; }

	/// <summary>
	///  Returns a hash code value that represents this object.
	/// </summary>
	public override int GetHashCode() { return _guid.GetHashCode(); }

	/// <summary>
	///  Converts this <see cref="ImageFormat"/> object to a human-readable string.
	/// </summary>
	public override string ToString()
	{
		if (_guid == Bmp.Guid) return "[ImageFormat: Bmp]";
		if (_guid == Emf.Guid) return "[ImageFormat: Emf]";
		if (_guid == Exif.Guid) return "[ImageFormat: Exif]";
		if (_guid == Gif.Guid) return "[ImageFormat: Gif]";
		if (_guid == Icon.Guid) return "[ImageFormat: Icon]";
		if (_guid == Jpeg.Guid) return "[ImageFormat: Jpeg]";
		if (_guid == MemoryBmp.Guid) return "[ImageFormat: MemoryBmp]";
		if (_guid == Png.Guid) return "[ImageFormat: Png]";
		if (_guid == Tiff.Guid) return "[ImageFormat: Tiff]";
		if (_guid == Wmf.Guid) return "[ImageFormat: Wmf]";
		return "[ImageFormat: " + _guid.ToString() + "]";
	}
}
