using System.Collections.Generic;

namespace System.Drawing.Printing;

/// <summary>
///  Specifies a print controller that displays a document on a screen as a series of images.
/// </summary>
public partial class PreviewPrintController : PrintController
{
	private readonly List<PreviewPageInfo> _pages = new List<PreviewPageInfo>();
	private Bitmap? _currentBitmap;
	private bool _useAntiAlias;

	/// <summary>Initializes a new instance of the <see cref="PreviewPrintController"/> class.</summary>
	public PreviewPrintController() { }

	/// <summary>Gets a value indicating this controller is used for print preview.</summary>
	public override bool IsPreview => true;

	/// <summary>Gets or sets a value indicating whether anti-aliasing is used when displaying the print preview.</summary>
	public virtual bool UseAntiAlias { get => _useAntiAlias; set => _useAntiAlias = value; }

	/// <summary>Captures print preview information for the document.</summary>
	/// <returns>An array of <see cref="PreviewPageInfo"/> objects.</returns>
	public PreviewPageInfo[] GetPreviewPageInfo() => _pages.ToArray();

	/// <summary>Completes the control sequence that determines when and how to preview a page in a print document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
	{
		if (_currentBitmap != null)
		{
			var bounds = e.PageBounds;
			_pages.Add(new PreviewPageInfo(_currentBitmap, new Size(bounds.Width, bounds.Height)));
			_currentBitmap = null;
		}
	}

	/// <summary>Completes the control sequence that determines when and how to preview a print document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
	{
	}

	/// <summary>Begins the control sequence that determines when and how to preview a page in a print document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	/// <returns>A <see cref="Graphics"/> that represents a page from a <see cref="PrintDocument"/>.</returns>
	public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
	{
		var bounds = e.PageBounds;
		int width = Math.Max(1, bounds.Width);
		int height = Math.Max(1, bounds.Height);
		_currentBitmap = new Bitmap(width, height);
		return Graphics.FromImage(_currentBitmap);
	}

	/// <summary>Begins the control sequence that determines when and how to preview a print document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
	{
		_pages.Clear();
	}
}
