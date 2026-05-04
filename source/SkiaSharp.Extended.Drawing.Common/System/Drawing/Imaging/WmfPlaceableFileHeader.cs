namespace System.Drawing.Imaging;

/// <summary>Defines a placeable metafile header.</summary>
public sealed partial class WmfPlaceableFileHeader
{
	/// <summary>Initializes a new instance of the <see cref="WmfPlaceableFileHeader"/> class.</summary>
	public WmfPlaceableFileHeader() { }

	/// <summary>Gets or sets the y-coordinate of the lower-right corner of the bounding rectangle.</summary>
	public short BboxBottom { get; set; }
	/// <summary>Gets or sets the x-coordinate of the upper-left corner of the bounding rectangle.</summary>
	public short BboxLeft { get; set; }
	/// <summary>Gets or sets the x-coordinate of the lower-right corner of the bounding rectangle.</summary>
	public short BboxRight { get; set; }
	/// <summary>Gets or sets the y-coordinate of the upper-left corner of the bounding rectangle.</summary>
	public short BboxTop { get; set; }
	/// <summary>Gets or sets the checksum value for the previous ten WORDs in the header.</summary>
	public short Checksum { get; set; }
	/// <summary>Gets or sets the handle of the metafile in memory.</summary>
	public short Hmf { get; set; }
	/// <summary>Gets or sets the number of twips per inch.</summary>
	public short Inch { get; set; }
	/// <summary>Gets or sets a value indicating the presence of a placeable metafile header.</summary>
	public int Key { get; set; }
	/// <summary>Reserved. Do not use.</summary>
	public int Reserved { get; set; }
}
