namespace System.Drawing.Printing;

/// <summary>
///  Defines a reusable object that sends output to a printer, when printing from a Windows Forms application.
/// </summary>
[System.ComponentModel.DefaultEventAttribute("PrintPage")]
[System.ComponentModel.DefaultPropertyAttribute("DocumentName")]
public partial class PrintDocument : System.ComponentModel.Component
{
	private string _documentName = "document";
	private PageSettings? _defaultPageSettings;
	private PrinterSettings? _printerSettings;
	private PrintController? _printController;
	private bool _originAtMargins;

	private event PrintEventHandler? _beginPrint;
	private event PrintEventHandler? _endPrint;
	private event PrintPageEventHandler? _printPage;
	private event QueryPageSettingsEventHandler? _queryPageSettings;

	/// <summary>Initializes a new instance of the <see cref="PrintDocument"/> class.</summary>
	public PrintDocument() { }

	/// <summary>Gets or sets page settings that are used as defaults for all pages to be printed.</summary>
	[System.ComponentModel.BrowsableAttribute(false)]
	[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
	public System.Drawing.Printing.PageSettings DefaultPageSettings
	{
		get => _defaultPageSettings ??= new PageSettings(PrinterSettings);
		set => _defaultPageSettings = value;
	}

	/// <summary>Gets or sets the document name to display while printing the document.</summary>
	[System.ComponentModel.DefaultValueAttribute("document")]
	public string DocumentName
	{
		get => _documentName;
		set => _documentName = value ?? string.Empty;
	}

	/// <summary>Gets or sets a value indicating whether the position of a graphics object associated with a page is located just inside the user-specified margins or at the top-left corner of the printable area of the page.</summary>
	[System.ComponentModel.DefaultValueAttribute(false)]
	public bool OriginAtMargins { get => _originAtMargins; set => _originAtMargins = value; }

	/// <summary>Gets or sets the print controller that guides the printing process.</summary>
	[System.ComponentModel.BrowsableAttribute(false)]
	[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
	public System.Drawing.Printing.PrintController PrintController
	{
		get => _printController ??= new StandardPrintController();
		set => _printController = value;
	}

	/// <summary>Gets or sets the printer that prints the document.</summary>
	[System.ComponentModel.BrowsableAttribute(false)]
	[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
	public System.Drawing.Printing.PrinterSettings PrinterSettings
	{
		get => _printerSettings ??= new PrinterSettings();
		set => _printerSettings = value;
	}

	/// <summary>Occurs when the <see cref="Print"/> method is called and before the first page of the document prints.</summary>
	public event System.Drawing.Printing.PrintEventHandler BeginPrint
	{
		add => _beginPrint += value;
		remove => _beginPrint -= value;
	}

	/// <summary>Occurs when the last page of the document has printed.</summary>
	public event System.Drawing.Printing.PrintEventHandler EndPrint
	{
		add => _endPrint += value;
		remove => _endPrint -= value;
	}

	/// <summary>Occurs when the output to print for the current page is needed.</summary>
	public event System.Drawing.Printing.PrintPageEventHandler PrintPage
	{
		add => _printPage += value;
		remove => _printPage -= value;
	}

	/// <summary>Occurs immediately before each <see cref="PrintPage"/> event.</summary>
	public event System.Drawing.Printing.QueryPageSettingsEventHandler QueryPageSettings
	{
		add => _queryPageSettings += value;
		remove => _queryPageSettings -= value;
	}

	/// <summary>Starts the document's printing process.</summary>
	public void Print()
	{
		var controller = PrintController;
		var printArgs = new PrintEventArgs();
		controller.OnStartPrint(this, printArgs);
		if (printArgs.Cancel) return;

		try
		{
			OnBeginPrint(printArgs);
			if (printArgs.Cancel) return;

			bool hasMorePages = true;
			while (hasMorePages)
			{
				var queryArgs = new QueryPageSettingsEventArgs((PageSettings)DefaultPageSettings.Clone());
				OnQueryPageSettings(queryArgs);
				if (queryArgs.Cancel) break;

				var margins = queryArgs.PageSettings.Margins;
				var bounds = queryArgs.PageSettings.Bounds;
				var marginBounds = new Rectangle(
					margins.Left, margins.Top,
					bounds.Width - margins.Left - margins.Right,
					bounds.Height - margins.Top - margins.Bottom);

				var pageArgs = new PrintPageEventArgs(null, marginBounds, bounds, queryArgs.PageSettings);

				var g = controller.OnStartPage(this, pageArgs);
				pageArgs.Graphics = g;

				try
				{
					OnPrintPage(pageArgs);
				}
				finally
				{
					controller.OnEndPage(this, pageArgs);
					pageArgs.Graphics?.Dispose();
				}

				hasMorePages = pageArgs.HasMorePages && !pageArgs.Cancel;
			}
		}
		finally
		{
			var endArgs = new PrintEventArgs();
			OnEndPrint(endArgs);
			controller.OnEndPrint(this, endArgs);
		}
	}

	/// <summary>Raises the <see cref="BeginPrint"/> event.</summary>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	protected internal virtual void OnBeginPrint(System.Drawing.Printing.PrintEventArgs e)
	{
		_beginPrint?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="EndPrint"/> event.</summary>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	protected internal virtual void OnEndPrint(System.Drawing.Printing.PrintEventArgs e)
	{
		_endPrint?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="PrintPage"/> event.</summary>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	protected internal virtual void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e)
	{
		_printPage?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="QueryPageSettings"/> event.</summary>
	/// <param name="e">A <see cref="QueryPageSettingsEventArgs"/> that contains the event data.</param>
	protected internal virtual void OnQueryPageSettings(System.Drawing.Printing.QueryPageSettingsEventArgs e)
	{
		_queryPageSettings?.Invoke(this, e);
	}

	/// <summary>Returns a string representation of this <see cref="PrintDocument"/>.</summary>
	public override string ToString() => $"[PrintDocument {DocumentName}]";
}
