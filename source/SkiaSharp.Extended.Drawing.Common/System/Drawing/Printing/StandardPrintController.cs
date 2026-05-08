using SkiaSharp;
using System.IO;

namespace System.Drawing.Printing;

/// <summary>
///  Specifies a print controller that sends information to a printer, rendering pages to PDF via SkiaSharp.
/// </summary>
public partial class StandardPrintController : PrintController
{
	/// <summary>Points per inch (typographic standard).</summary>
	private const float PointsPerInch = 72f;

	/// <summary>Hundredths of an inch per inch (GDI+ page bounds unit).</summary>
	private const float HundredthsPerInch = 100f;

	private SKDocument? _document;
	private Stream? _stream;
	private bool _ownsStream;

	/// <summary>Initializes a new instance of the <see cref="StandardPrintController"/> class.</summary>
	public StandardPrintController() { }

	/// <summary>Completes the control sequence that determines when and how to print a page of a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
	{
		_document?.EndPage();
	}

	/// <summary>Completes the control sequence that determines when and how to print a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
	{
		_document?.Close();
		_document?.Dispose();
		_document = null;

		if (_ownsStream)
		{
			_stream?.Dispose();
		}
		_stream = null;
		_ownsStream = false;
	}

	/// <summary>Begins the control sequence that determines when and how to print a page of a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	/// <returns>A <see cref="Graphics"/> that represents a page from a <see cref="PrintDocument"/>.</returns>
	public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
	{
		if (_document is null)
			throw new InvalidOperationException("OnStartPrint must be called before OnStartPage.");

		var bounds = e.PageBounds;
		// Convert hundredths of an inch to points (72 points per inch)
		float widthPt = bounds.Width * PointsPerInch / HundredthsPerInch;
		float heightPt = bounds.Height * PointsPerInch / HundredthsPerInch;

		var canvas = _document.BeginPage(widthPt, heightPt);
		// Scale canvas so drawing in hundredths-of-an-inch maps to points
		canvas.Scale(PointsPerInch / HundredthsPerInch, PointsPerInch / HundredthsPerInch);

		return Graphics.FromCanvas(canvas, ownsClipSave: false);
	}

	/// <summary>Begins the control sequence that determines when and how to print a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
	{
		var settings = document.PrinterSettings;

		if (settings.PrintToFile && !string.IsNullOrEmpty(settings.PrintFileName))
		{
			_stream = new FileStream(settings.PrintFileName, FileMode.Create, FileAccess.Write);
			_ownsStream = true;
		}
		else
		{
			// Default to a temp file when no output file is specified
			var tempPath = Path.Combine(Path.GetTempPath(), $"{document.DocumentName}_{Guid.NewGuid()}.pdf");
			settings.PrintFileName = tempPath;
			settings.PrintToFile = true;
			_stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
			_ownsStream = true;
		}

		_document = SKDocument.CreatePdf(_stream);
	}
}
