namespace System.Drawing.Printing;

/// <summary>
///  Specifies settings that apply to a single, printed page.
/// </summary>
public partial class PageSettings : System.ICloneable
{
	private bool _color = true;
	private bool _landscape;
	private Margins _margins = new Margins();
	private PaperSize _paperSize = new PaperSize("Letter", 850, 1100) { RawKind = (int)PaperKind.Letter };
	private PaperSource _paperSource = new PaperSource { RawKind = (int)PaperSourceKind.AutomaticFeed, SourceName = "Auto" };
	private PrinterResolution _printerResolution = new PrinterResolution { Kind = PrinterResolutionKind.Custom, X = 600, Y = 600 };
	private PrinterSettings? _printerSettings;

	/// <summary>Initializes a new instance of the <see cref="PageSettings"/> class using the default printer.</summary>
	public PageSettings() { }

	/// <summary>Initializes a new instance of the <see cref="PageSettings"/> class using the specified printer settings.</summary>
	/// <param name="printerSettings">The <see cref="PrinterSettings"/> that describes the printer to use.</param>
	public PageSettings(System.Drawing.Printing.PrinterSettings printerSettings)
	{
		_printerSettings = printerSettings;
	}

	/// <summary>Gets the bounds of the page, taking into account the <see cref="Landscape"/> property.</summary>
	public System.Drawing.Rectangle Bounds
	{
		get
		{
			int w = _landscape ? _paperSize.Height : _paperSize.Width;
			int h = _landscape ? _paperSize.Width : _paperSize.Height;
			return new Rectangle(0, 0, w, h);
		}
	}

	/// <summary>Gets or sets a value indicating whether the page should be printed in color.</summary>
	public bool Color { get => _color; set => _color = value; }

	/// <summary>Gets the x-coordinate, in hundredths of an inch, of the hard margin at the left of the page.</summary>
	public float HardMarginX => 0f;

	/// <summary>Gets the y-coordinate, in hundredths of an inch, of the hard margin at the top of the page.</summary>
	public float HardMarginY => 0f;

	/// <summary>Gets or sets a value indicating whether the page is printed in landscape or portrait orientation.</summary>
	public bool Landscape { get => _landscape; set => _landscape = value; }

	/// <summary>Gets or sets the margins for this page.</summary>
	public System.Drawing.Printing.Margins Margins
	{
		get => _margins;
		set => _margins = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>Gets or sets the paper size for the page.</summary>
	public System.Drawing.Printing.PaperSize PaperSize
	{
		get => _paperSize;
		set => _paperSize = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>Gets or sets the page's paper source.</summary>
	public System.Drawing.Printing.PaperSource PaperSource
	{
		get => _paperSource;
		set => _paperSource = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>Gets the bounds of the printable area of the page for the printer.</summary>
	public System.Drawing.RectangleF PrintableArea
	{
		get
		{
			var bounds = Bounds;
			return new RectangleF(
				_margins.Left, _margins.Top,
				bounds.Width - _margins.Left - _margins.Right,
				bounds.Height - _margins.Top - _margins.Bottom);
		}
	}

	/// <summary>Gets or sets the printer resolution for the page.</summary>
	public System.Drawing.Printing.PrinterResolution PrinterResolution
	{
		get => _printerResolution;
		set => _printerResolution = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>Gets or sets the printer settings associated with the page.</summary>
	public System.Drawing.Printing.PrinterSettings PrinterSettings
	{
		get => _printerSettings ??= new PrinterSettings();
		set => _printerSettings = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>Creates a copy of this <see cref="PageSettings"/>.</summary>
	public object Clone()
	{
		var clone = (PageSettings)MemberwiseClone();
		clone._margins = (Margins)_margins.Clone();
		clone._paperSize = new PaperSize(_paperSize.PaperName, _paperSize.Width, _paperSize.Height) { RawKind = _paperSize.RawKind };
		clone._paperSource = new PaperSource { RawKind = _paperSource.RawKind, SourceName = _paperSource.SourceName };
		clone._printerResolution = new PrinterResolution { Kind = _printerResolution.Kind, X = _printerResolution.X, Y = _printerResolution.Y };
		return clone;
	}

	/// <summary>Copies the relevant information from the <see cref="PageSettings"/> to the specified DEVMODE structure.</summary>
	public void CopyToHdevmode(nint hdevmode) { throw new System.PlatformNotSupportedException("CopyToHdevmode requires Windows GDI and is not supported on this platform."); }

	/// <summary>Sets the relevant information from the specified DEVMODE structure to this <see cref="PageSettings"/>.</summary>
	public void SetHdevmode(nint hdevmode) { throw new System.PlatformNotSupportedException("SetHdevmode requires Windows GDI and is not supported on this platform."); }

	/// <summary>Returns a string representation of this <see cref="PageSettings"/>.</summary>
	public override string ToString() =>
		$"[PageSettings: Color={Color}, Landscape={Landscape}, Margins={Margins}, PaperSize={PaperSize}]";
}
