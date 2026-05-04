namespace System.Drawing.Printing;

/// <summary>
///  Provides data for the <see cref="PrintDocument.PrintPage"/> event.
/// </summary>
public partial class PrintPageEventArgs : System.EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="PrintPageEventArgs"/> class.</summary>
	/// <param name="graphics">The <see cref="System.Drawing.Graphics"/> used to paint the page.</param>
	/// <param name="marginBounds">The area between the margins.</param>
	/// <param name="pageBounds">The total area of the paper.</param>
	/// <param name="pageSettings">The page settings for the current page.</param>
	public PrintPageEventArgs(System.Drawing.Graphics? graphics, System.Drawing.Rectangle marginBounds, System.Drawing.Rectangle pageBounds, System.Drawing.Printing.PageSettings pageSettings)
	{
		Graphics = graphics;
		MarginBounds = marginBounds;
		PageBounds = pageBounds;
		PageSettings = pageSettings;
	}

	/// <summary>Gets or sets a value indicating whether the print job should be canceled.</summary>
	public bool Cancel { get; set; }
	/// <summary>Gets the <see cref="System.Drawing.Graphics"/> used to paint the page.</summary>
	public System.Drawing.Graphics? Graphics { get; internal set; }
	/// <summary>Gets or sets a value indicating whether an additional page should be printed.</summary>
	public bool HasMorePages { get; set; }
	/// <summary>Gets the page area between the margins.</summary>
	public System.Drawing.Rectangle MarginBounds { get; }
	/// <summary>Gets the page area of the paper.</summary>
	public System.Drawing.Rectangle PageBounds { get; }
	/// <summary>Gets the page settings for the current page.</summary>
	public System.Drawing.Printing.PageSettings PageSettings { get; }
}
