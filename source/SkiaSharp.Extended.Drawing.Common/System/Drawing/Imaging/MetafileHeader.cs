namespace System.Drawing.Imaging;

/// <summary>Contains attributes of an associated <see cref="Metafile"/>.</summary>
public sealed partial class MetafileHeader
{
	private Rectangle _bounds;
	private float _dpiX = 96f;
	private float _dpiY = 96f;
	private int _emfPlusHeaderSize;
	private int _logicalDpiX = 96;
	private int _logicalDpiY = 96;
	private int _metafileSize;
	private MetafileType _type = MetafileType.Wmf;
	private int _version;
	private MetaHeader _wmfHeader = new MetaHeader();

	internal MetafileHeader() {}

	/// <summary>Gets a <see cref="Rectangle"/> that bounds the associated <see cref="Metafile"/>.</summary>
	public Rectangle Bounds => _bounds;
	/// <summary>Gets the horizontal resolution, in dots per inch, of the associated <see cref="Metafile"/>.</summary>
	public float DpiX => _dpiX;
	/// <summary>Gets the vertical resolution, in dots per inch, of the associated <see cref="Metafile"/>.</summary>
	public float DpiY => _dpiY;
	/// <summary>Gets the size, in bytes, of the enhanced metafile plus header file.</summary>
	public int EmfPlusHeaderSize => _emfPlusHeaderSize;
	/// <summary>Gets the logical horizontal resolution, in dots per inch.</summary>
	public int LogicalDpiX => _logicalDpiX;
	/// <summary>Gets the logical vertical resolution, in dots per inch.</summary>
	public int LogicalDpiY => _logicalDpiY;
	/// <summary>Gets the size, in bytes, of the associated <see cref="Metafile"/>.</summary>
	public int MetafileSize => _metafileSize;
	/// <summary>Gets the type of the associated <see cref="Metafile"/>.</summary>
	public MetafileType Type => _type;
	/// <summary>Gets the version number of the associated <see cref="Metafile"/>.</summary>
	public int Version => _version;
	/// <summary>Gets the WMF header file for the associated <see cref="Metafile"/>.</summary>
	public MetaHeader WmfHeader => _wmfHeader;

	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Windows enhanced metafile format and can be displayed in a device context.</summary>
	public bool IsDisplay() => _type == MetafileType.EmfPlusDual;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Windows enhanced metafile format.</summary>
	public bool IsEmf() => _type == MetafileType.Emf;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Windows enhanced metafile format or the Windows enhanced metafile plus format.</summary>
	public bool IsEmfOrEmfPlus() => _type == MetafileType.Emf || _type == MetafileType.EmfPlusOnly || _type == MetafileType.EmfPlusDual;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Windows enhanced metafile plus format.</summary>
	public bool IsEmfPlus() => _type == MetafileType.EmfPlusOnly || _type == MetafileType.EmfPlusDual;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Dual enhanced metafile plus format.</summary>
	public bool IsEmfPlusDual() => _type == MetafileType.EmfPlusDual;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the EMF+ only format.</summary>
	public bool IsEmfPlusOnly() => _type == MetafileType.EmfPlusOnly;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Windows metafile format.</summary>
	public bool IsWmf() => _type == MetafileType.Wmf || _type == MetafileType.WmfPlaceable;
	/// <summary>Returns a value that indicates whether the associated <see cref="Metafile"/> is in the Windows placeable metafile format.</summary>
	public bool IsWmfPlaceable() => _type == MetafileType.WmfPlaceable;
}
