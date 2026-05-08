namespace System.Drawing.Printing;

/// <summary>
///  Provides data for the <see cref="PrintDocument.QueryPageSettings"/> event.
/// </summary>
public partial class QueryPageSettingsEventArgs : PrintEventArgs
{
	/// <summary>Initializes a new instance of the <see cref="QueryPageSettingsEventArgs"/> class.</summary>
	/// <param name="pageSettings">The page settings for the page to be printed.</param>
	public QueryPageSettingsEventArgs(PageSettings pageSettings) { PageSettings = pageSettings; }
	/// <summary>Gets or sets the page settings for the page to be printed.</summary>
	public PageSettings PageSettings { get; set; }
}
