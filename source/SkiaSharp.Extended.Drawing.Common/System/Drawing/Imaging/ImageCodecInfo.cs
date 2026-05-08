namespace System.Drawing.Imaging;

/// <summary>
///  The <see cref="ImageCodecInfo"/> class provides the necessary storage members and methods to retrieve all pertinent information about the installed image encoders and decoders (called codecs).
/// </summary>
public sealed partial class ImageCodecInfo
{
	private Guid _clsid;
	private string? _codecName;
	private string? _dllName;
	private string? _filenameExtension;
	private ImageCodecFlags _flags;
	private string? _formatDescription;
	private Guid _formatID;
	private string? _mimeType;
	private byte[][]? _signatureMasks;
	private byte[][]? _signaturePatterns;
	private int _version;

	/// <summary>
	///  Initializes a new instance of the <see cref="ImageCodecInfo"/> class.
	/// </summary>
	internal ImageCodecInfo() {}

	/// <summary>
	///  Gets or sets a <see cref="Guid"/> structure that contains a GUID that identifies the codec's class.
	/// </summary>
	public Guid Clsid { get { return _clsid; } set { _clsid = value; } }

	/// <summary>
	///  Gets or sets a string that contains the name of the codec.
	/// </summary>
	public string? CodecName { get { return _codecName; } set { _codecName = value; } }

	/// <summary>
	///  Gets or sets a string that contains the path name of the DLL that holds the codec.
	/// </summary>
	public string? DllName { get { return _dllName; } set { _dllName = value; } }

	/// <summary>
	///  Gets or sets a string that contains the file name extension(s) used in the codec.
	/// </summary>
	public string? FilenameExtension { get { return _filenameExtension; } set { _filenameExtension = value; } }

	/// <summary>
	///  Gets or sets flags of the <see cref="ImageCodecInfo"/> object.
	/// </summary>
	public ImageCodecFlags Flags { get { return _flags; } set { _flags = value; } }

	/// <summary>
	///  Gets or sets a string that describes the codec's file format.
	/// </summary>
	public string? FormatDescription { get { return _formatDescription; } set { _formatDescription = value; } }

	/// <summary>
	///  Gets or sets a <see cref="Guid"/> structure that contains a GUID that identifies the codec's format.
	/// </summary>
	public Guid FormatID { get { return _formatID; } set { _formatID = value; } }

	/// <summary>
	///  Gets or sets a string that contains the codec's Multipurpose Internet Mail Extensions (MIME) type.
	/// </summary>
	public string? MimeType { get { return _mimeType; } set { _mimeType = value; } }

	/// <summary>
	///  Gets or sets a two-dimensional array of bytes that can be used as a filter.
	/// </summary>
	public byte[][]? SignatureMasks { get { return _signatureMasks; } set { _signatureMasks = value; } }

	/// <summary>
	///  Gets or sets a two-dimensional array of bytes that represents the signature of the codec.
	/// </summary>
	public byte[][]? SignaturePatterns { get { return _signaturePatterns; } set { _signaturePatterns = value; } }

	/// <summary>
	///  Gets or sets the version number of the codec.
	/// </summary>
	public int Version { get { return _version; } set { _version = value; } }

	/// <summary>
	///  Returns an array of <see cref="ImageCodecInfo"/> objects that contain information about the image decoders built into GDI+.
	/// </summary>
	public static ImageCodecInfo[] GetImageDecoders()
	{
		return Array.Empty<ImageCodecInfo>();
	}

	/// <summary>
	///  Returns an array of <see cref="ImageCodecInfo"/> objects that contain information about the image encoders built into GDI+.
	/// </summary>
	public static ImageCodecInfo[] GetImageEncoders()
	{
		return Array.Empty<ImageCodecInfo>();
	}
}
