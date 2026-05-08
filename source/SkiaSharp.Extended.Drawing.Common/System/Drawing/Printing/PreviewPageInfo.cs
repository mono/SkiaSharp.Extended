namespace System.Drawing.Printing;

/// <summary>
///  Specifies print preview information for a single page.
/// </summary>
public sealed partial class PreviewPageInfo
{
	/// <summary>Initializes a new instance of the <see cref="PreviewPageInfo"/> class.</summary>
	/// <param name="image">The image of the printed page.</param>
	/// <param name="physicalSize">The size of the printed page, in hundredths of an inch.</param>
	public PreviewPageInfo(Image image, Size physicalSize) { Image = image; PhysicalSize = physicalSize; }
	/// <summary>Gets the image of the printed page.</summary>
	public Image Image { get; }
	/// <summary>Gets the size of the printed page, in hundredths of an inch.</summary>
	public Size PhysicalSize { get; }
}
