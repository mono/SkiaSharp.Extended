namespace System.Drawing.Printing;

/// <summary>
///  Represents the resolution supported by a printer.
/// </summary>
public partial class PrinterResolution
{
	/// <summary>Initializes a new instance of the <see cref="PrinterResolution"/> class.</summary>
	public PrinterResolution() { }
	/// <summary>Gets or sets the printer resolution kind.</summary>
	public PrinterResolutionKind Kind { get; set; }
	/// <summary>Gets or sets the horizontal printer resolution, in dots per inch.</summary>
	public int X { get; set; }
	/// <summary>Gets or sets the vertical printer resolution, in dots per inch.</summary>
	public int Y { get; set; }
	/// <summary>Returns a string representation of this <see cref="PrinterResolution"/>.</summary>
	public override string ToString() => Kind == PrinterResolutionKind.Custom ? $"[PrinterResolution X={X} Y={Y}]" : $"[PrinterResolution {Kind}]";
}
