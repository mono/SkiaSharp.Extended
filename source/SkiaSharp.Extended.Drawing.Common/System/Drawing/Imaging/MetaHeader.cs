namespace System.Drawing.Imaging;

/// <summary>Contains information about a Windows-format (WMF) metafile.</summary>
public sealed partial class MetaHeader
{
	/// <summary>Initializes a new instance of the <see cref="MetaHeader"/> class.</summary>
	public MetaHeader() { }

	/// <summary>Gets or sets the size, in bytes, of the header.</summary>
	public short HeaderSize { get; set; }
	/// <summary>Gets or sets the size, in bytes, of the largest record in the associated Metafile object.</summary>
	public int MaxRecord { get; set; }
	/// <summary>Gets or sets the maximum number of objects that exist in the Metafile object at the same time.</summary>
	public short NoObjects { get; set; }
	/// <summary>Not used. Always returns 0.</summary>
	public short NoParameters { get; set; }
	/// <summary>Gets or sets the size, in bytes, of the associated Metafile object.</summary>
	public int Size { get; set; }
	/// <summary>Gets or sets the type of the associated Metafile object.</summary>
	public short Type { get; set; }
	/// <summary>Gets or sets the version number of the header format.</summary>
	public short Version { get; set; }
}
