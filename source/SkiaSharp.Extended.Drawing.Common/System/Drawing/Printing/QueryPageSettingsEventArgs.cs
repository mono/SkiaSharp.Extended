namespace System.Drawing.Printing;

/// <summary>
///  Provides data for the <see cref="PrintDocument.QueryPageSettings"/> event.
/// </summary>
public partial class QueryPageSettingsEventArgs : System.Drawing.Printing.PrintEventArgs
{
	/// <summary>Initializes a new instance of the <see cref="QueryPageSettingsEventArgs"/> class.</summary>
	/// <param name="pageSettings">The page settings for the page to be printed.</param>
	public QueryPageSettingsEventArgs(System.Drawing.Printing.PageSettings pageSettings) { PageSettings = pageSettings; }
	/// <summary>Gets or sets the page settings for the page to be printed.</summary>
	public System.Drawing.Printing.PageSettings PageSettings { get; set; }
}
