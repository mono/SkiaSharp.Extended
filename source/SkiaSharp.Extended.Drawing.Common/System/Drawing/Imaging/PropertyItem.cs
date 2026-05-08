namespace System.Drawing.Imaging;

/// <summary>
///  Encapsulates a metadata property to be included in an image file.
/// </summary>
public sealed partial class PropertyItem
{
	internal PropertyItem() {}
	/// <summary>Gets or sets the ID of the property.</summary>
	public int Id { get; set; }
	/// <summary>Gets or sets the length (in bytes) of the <see cref="Value"/> property.</summary>
	public int Len { get; set; }
	/// <summary>Gets or sets an integer that defines the type of data contained in the <see cref="Value"/> property.</summary>
	public short Type { get; set; }
	/// <summary>Gets or sets the value of the property item.</summary>
	public byte[]? Value { get; set; }
}
