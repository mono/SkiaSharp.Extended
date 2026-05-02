using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace System.Drawing.Printing
{
	/// <summary>
	///  Specifies information about how a document is printed, including the printer that prints it.
	/// </summary>
	public partial class PrinterSettings : System.ICloneable
	{
		/// <summary>A collection of <see cref="PaperSize"/> objects.</summary>
		public partial class PaperSizeCollection : System.Collections.ICollection, System.Collections.IEnumerable
		{
			private readonly List<PaperSize> _list;

			/// <summary>Initializes a new instance of the <see cref="PaperSizeCollection"/> class.</summary>
			/// <param name="array">An array of <see cref="PaperSize"/> objects.</param>
			public PaperSizeCollection(System.Drawing.Printing.PaperSize[] array) { _list = new List<PaperSize>(array); }

			/// <summary>Gets the number of paper sizes in the collection.</summary>
			public int Count => _list.Count;

			/// <summary>Gets the <see cref="PaperSize"/> at a specified index.</summary>
			public virtual System.Drawing.Printing.PaperSize this[int index] => _list[index];

			/// <summary>Adds a <see cref="PaperSize"/> to the end of the collection.</summary>
			[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
			public int Add(System.Drawing.Printing.PaperSize paperSize) { _list.Add(paperSize); return _list.Count - 1; }

			/// <summary>Copies the contents of the collection to an array.</summary>
			public void CopyTo(System.Drawing.Printing.PaperSize[] paperSizes, int index) { _list.CopyTo(paperSizes, index); }

			/// <summary>Returns an enumerator that can iterate through the collection.</summary>
			public System.Collections.IEnumerator GetEnumerator() => _list.GetEnumerator();

			void System.Collections.ICollection.CopyTo(System.Array array, int index) { ((ICollection)_list).CopyTo(array, index); }
			bool System.Collections.ICollection.IsSynchronized => false;
			object System.Collections.ICollection.SyncRoot => ((ICollection)_list).SyncRoot;
		}

		/// <summary>A collection of <see cref="PaperSource"/> objects.</summary>
		public partial class PaperSourceCollection : System.Collections.ICollection, System.Collections.IEnumerable
		{
			private readonly List<PaperSource> _list;

			/// <summary>Initializes a new instance of the <see cref="PaperSourceCollection"/> class.</summary>
			/// <param name="array">An array of <see cref="PaperSource"/> objects.</param>
			public PaperSourceCollection(System.Drawing.Printing.PaperSource[] array) { _list = new List<PaperSource>(array); }

			/// <summary>Gets the number of paper sources in the collection.</summary>
			public int Count => _list.Count;

			/// <summary>Gets the <see cref="PaperSource"/> at a specified index.</summary>
			public virtual System.Drawing.Printing.PaperSource this[int index] => _list[index];

			/// <summary>Adds a <see cref="PaperSource"/> to the end of the collection.</summary>
			[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
			public int Add(System.Drawing.Printing.PaperSource paperSource) { _list.Add(paperSource); return _list.Count - 1; }

			/// <summary>Copies the contents of the collection to an array.</summary>
			public void CopyTo(System.Drawing.Printing.PaperSource[] paperSources, int index) { _list.CopyTo(paperSources, index); }

			/// <summary>Returns an enumerator that can iterate through the collection.</summary>
			public System.Collections.IEnumerator GetEnumerator() => _list.GetEnumerator();

			void System.Collections.ICollection.CopyTo(System.Array array, int index) { ((ICollection)_list).CopyTo(array, index); }
			bool System.Collections.ICollection.IsSynchronized => false;
			object System.Collections.ICollection.SyncRoot => ((ICollection)_list).SyncRoot;
		}

		/// <summary>A collection of <see cref="PrinterResolution"/> objects.</summary>
		public partial class PrinterResolutionCollection : System.Collections.ICollection, System.Collections.IEnumerable
		{
			private readonly List<PrinterResolution> _list;

			/// <summary>Initializes a new instance of the <see cref="PrinterResolutionCollection"/> class.</summary>
			/// <param name="array">An array of <see cref="PrinterResolution"/> objects.</param>
			public PrinterResolutionCollection(System.Drawing.Printing.PrinterResolution[] array) { _list = new List<PrinterResolution>(array); }

			/// <summary>Gets the number of resolutions in the collection.</summary>
			public int Count => _list.Count;

			/// <summary>Gets the <see cref="PrinterResolution"/> at a specified index.</summary>
			public virtual System.Drawing.Printing.PrinterResolution this[int index] => _list[index];

			/// <summary>Adds a <see cref="PrinterResolution"/> to the end of the collection.</summary>
			[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
			public int Add(System.Drawing.Printing.PrinterResolution printerResolution) { _list.Add(printerResolution); return _list.Count - 1; }

			/// <summary>Copies the contents of the collection to an array.</summary>
			public void CopyTo(System.Drawing.Printing.PrinterResolution[] printerResolutions, int index) { _list.CopyTo(printerResolutions, index); }

			/// <summary>Returns an enumerator that can iterate through the collection.</summary>
			public System.Collections.IEnumerator GetEnumerator() => _list.GetEnumerator();

			void System.Collections.ICollection.CopyTo(System.Array array, int index) { ((ICollection)_list).CopyTo(array, index); }
			bool System.Collections.ICollection.IsSynchronized => false;
			object System.Collections.ICollection.SyncRoot => ((ICollection)_list).SyncRoot;
		}

		/// <summary>A collection of strings.</summary>
		public partial class StringCollection : System.Collections.ICollection, System.Collections.IEnumerable
		{
			private readonly List<string> _list;

			/// <summary>Initializes a new instance of the <see cref="StringCollection"/> class.</summary>
			/// <param name="array">An array of strings.</param>
			public StringCollection(string[] array) { _list = new List<string>(array); }

			/// <summary>Gets the number of strings in the collection.</summary>
			public int Count => _list.Count;

			/// <summary>Gets the string at a specified index.</summary>
			public virtual string this[int index] => _list[index];

			/// <summary>Adds a string to the end of the collection.</summary>
			[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
			public int Add(string value) { _list.Add(value); return _list.Count - 1; }

			/// <summary>Copies the contents of the collection to an array.</summary>
			public void CopyTo(string[] strings, int index) { _list.CopyTo(strings, index); }

			/// <summary>Returns an enumerator that can iterate through the collection.</summary>
			public System.Collections.IEnumerator GetEnumerator() => _list.GetEnumerator();

			void System.Collections.ICollection.CopyTo(System.Array array, int index) { ((ICollection)_list).CopyTo(array, index); }
			bool System.Collections.ICollection.IsSynchronized => false;
			object System.Collections.ICollection.SyncRoot => ((ICollection)_list).SyncRoot;
		}

		private string _printerName = "PDF";
		private short _copies = 1;
		private bool _collate;
		private Duplex _duplex = Duplex.Default;
		private int _fromPage;
		private int _toPage;
		private int _maximumPage = int.MaxValue;
		private int _minimumPage;
		private PrintRange _printRange = PrintRange.AllPages;
		private bool _printToFile;
		private string _printFileName = string.Empty;
		private PageSettings? _defaultPageSettings;

		private static StringCollection? _installedPrinters;

		/// <summary>Initializes a new instance of the <see cref="PrinterSettings"/> class.</summary>
		public PrinterSettings() { }

		/// <summary>Gets a value indicating whether the printer supports double-sided printing.</summary>
		public bool CanDuplex => false;

		/// <summary>Gets or sets a value indicating whether the printed document is collated.</summary>
		public bool Collate { get => _collate; set => _collate = value; }

		/// <summary>Gets or sets the number of copies of the document to print.</summary>
		public short Copies { get => _copies; set => _copies = value; }

		/// <summary>Gets the default page settings for this printer.</summary>
		public System.Drawing.Printing.PageSettings DefaultPageSettings => _defaultPageSettings ??= new PageSettings(this);

		/// <summary>Gets or sets the printer setting for double-sided printing.</summary>
		public System.Drawing.Printing.Duplex Duplex { get => _duplex; set => _duplex = value; }

		/// <summary>Gets or sets the page number of the first page to print.</summary>
		public int FromPage { get => _fromPage; set => _fromPage = value; }

		/// <summary>Gets the names of all printers installed on the computer.</summary>
		public static System.Drawing.Printing.PrinterSettings.StringCollection InstalledPrinters =>
			_installedPrinters ??= new StringCollection(new[] { "PDF" });

		/// <summary>Gets a value indicating whether the <see cref="PrinterName"/> property designates the default printer.</summary>
		public bool IsDefaultPrinter => true;

		/// <summary>Gets a value indicating whether the printer is a plotter.</summary>
		public bool IsPlotter => false;

		/// <summary>Gets a value indicating whether the <see cref="PrinterName"/> property designates a valid printer.</summary>
		public bool IsValid => true;

		/// <summary>Gets the angle, in degrees, that the portrait orientation is rotated to produce the landscape orientation.</summary>
		public int LandscapeAngle => 90;

		/// <summary>Gets the maximum number of copies that the printer enables the user to print at a time.</summary>
		public int MaximumCopies => 1;

		/// <summary>Gets or sets the maximum <see cref="FromPage"/> or <see cref="ToPage"/> that can be selected in a PrintDialog.</summary>
		public int MaximumPage { get => _maximumPage; set => _maximumPage = value; }

		/// <summary>Gets or sets the minimum <see cref="FromPage"/> or <see cref="ToPage"/> that can be selected in a PrintDialog.</summary>
		public int MinimumPage { get => _minimumPage; set => _minimumPage = value; }

		/// <summary>Gets the paper sizes that are supported by this printer.</summary>
		public System.Drawing.Printing.PrinterSettings.PaperSizeCollection PaperSizes => new PaperSizeCollection(new[]
		{
			new PaperSize("Letter", 850, 1100) { RawKind = (int)PaperKind.Letter },
			new PaperSize("Legal", 850, 1400) { RawKind = (int)PaperKind.Legal },
			new PaperSize("A4", 827, 1169) { RawKind = (int)PaperKind.A4 },
			new PaperSize("A3", 1169, 1654) { RawKind = (int)PaperKind.A3 },
			new PaperSize("Tabloid", 1100, 1700) { RawKind = (int)PaperKind.Tabloid },
			new PaperSize("Executive", 725, 1050) { RawKind = (int)PaperKind.Executive },
			new PaperSize("A5", 583, 827) { RawKind = (int)PaperKind.A5 },
			new PaperSize("B4 (JIS)", 1012, 1433) { RawKind = (int)PaperKind.B4 },
			new PaperSize("B5 (JIS)", 717, 1012) { RawKind = (int)PaperKind.B5 },
		});

		/// <summary>Gets the paper source trays that are available on the printer.</summary>
		public System.Drawing.Printing.PrinterSettings.PaperSourceCollection PaperSources => new PaperSourceCollection(new[]
		{
			new PaperSource { RawKind = (int)PaperSourceKind.AutomaticFeed, SourceName = "Auto" },
		});

		/// <summary>Gets or sets the printer name.</summary>
		public string PrinterName { get => _printerName; set => _printerName = value ?? string.Empty; }

		/// <summary>Gets all the resolutions that are supported by this printer.</summary>
		public System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection PrinterResolutions => new PrinterResolutionCollection(new[]
		{
			new PrinterResolution { Kind = PrinterResolutionKind.High, X = 1200, Y = 1200 },
			new PrinterResolution { Kind = PrinterResolutionKind.Medium, X = 600, Y = 600 },
			new PrinterResolution { Kind = PrinterResolutionKind.Low, X = 300, Y = 300 },
			new PrinterResolution { Kind = PrinterResolutionKind.Draft, X = 150, Y = 150 },
		});

		/// <summary>Gets or sets the file name to print to, when <see cref="PrintToFile"/> is <see langword="true"/>.</summary>
		public string PrintFileName
		{
			get => _printFileName;
			set => _printFileName = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>Gets or sets the pages that the user has specified to be printed.</summary>
		public System.Drawing.Printing.PrintRange PrintRange { get => _printRange; set => _printRange = value; }

		/// <summary>Gets or sets a value indicating whether the printing output is sent to a file instead of a port.</summary>
		public bool PrintToFile { get => _printToFile; set => _printToFile = value; }

		/// <summary>Gets a value indicating whether the printer supports color printing.</summary>
		public bool SupportsColor => true;

		/// <summary>Gets or sets the number of the last page to print.</summary>
		public int ToPage { get => _toPage; set => _toPage = value; }

		/// <summary>Creates a copy of this <see cref="PrinterSettings"/>.</summary>
		public object Clone()
		{
			var clone = (PrinterSettings)MemberwiseClone();
			clone._defaultPageSettings = null;
			return clone;
		}

		/// <summary>Returns a <see cref="Graphics"/> that contains printer information useful for creating a PrintPreview image.</summary>
		public System.Drawing.Graphics CreateMeasurementGraphics()
		{
			return CreateMeasurementGraphics(DefaultPageSettings, false);
		}

		/// <summary>Returns a <see cref="Graphics"/> that contains printer information useful for creating a PrintPreview image.</summary>
		/// <param name="honorOriginAtMargins">
		///  <see langword="true"/> to indicate the origin is at the margins; <see langword="false"/> to indicate the origin is at the top-left of the printable area.
		/// </param>
		public System.Drawing.Graphics CreateMeasurementGraphics(bool honorOriginAtMargins)
		{
			return CreateMeasurementGraphics(DefaultPageSettings, honorOriginAtMargins);
		}

		/// <summary>Returns a <see cref="Graphics"/> that contains printer information associated with the specified <see cref="PageSettings"/>.</summary>
		/// <param name="pageSettings">The <see cref="PageSettings"/> to retrieve a <see cref="Graphics"/> from.</param>
		public System.Drawing.Graphics CreateMeasurementGraphics(System.Drawing.Printing.PageSettings pageSettings)
		{
			return CreateMeasurementGraphics(pageSettings, false);
		}

		/// <summary>Returns a <see cref="Graphics"/> that contains printer information associated with the specified <see cref="PageSettings"/>.</summary>
		/// <param name="pageSettings">The <see cref="PageSettings"/> to retrieve a <see cref="Graphics"/> from.</param>
		/// <param name="honorOriginAtMargins">
		///  <see langword="true"/> to indicate the origin is at the margins; <see langword="false"/> to indicate the origin is at the top-left of the printable area.
		/// </param>
		public System.Drawing.Graphics CreateMeasurementGraphics(System.Drawing.Printing.PageSettings pageSettings, bool honorOriginAtMargins)
		{
			var bounds = pageSettings.Bounds;
			int width = Math.Max(1, bounds.Width);
			int height = Math.Max(1, bounds.Height);
			var bmp = new Bitmap(width, height);
			return Graphics.FromImage(bmp);
		}

		/// <summary>Gets an HDEVMODE structure for the printer settings.</summary>
		public nint GetHdevmode() { throw new System.PlatformNotSupportedException("GetHdevmode requires Windows GDI and is not supported on this platform."); }

		/// <summary>Gets an HDEVMODE structure for the specified page settings.</summary>
		public nint GetHdevmode(System.Drawing.Printing.PageSettings pageSettings) { throw new System.PlatformNotSupportedException("GetHdevmode requires Windows GDI and is not supported on this platform."); }

		/// <summary>Gets an HDEVNAMES structure for the printer settings.</summary>
		public nint GetHdevnames() { throw new System.PlatformNotSupportedException("GetHdevnames requires Windows GDI and is not supported on this platform."); }

		/// <summary>Returns a value indicating whether the printer supports printing the specified image directly.</summary>
		public bool IsDirectPrintingSupported(System.Drawing.Image image) => false;

		/// <summary>Returns a value indicating whether the printer supports printing the specified image format directly.</summary>
		public bool IsDirectPrintingSupported(System.Drawing.Imaging.ImageFormat imageFormat) => false;

		/// <summary>Sets the HDEVMODE structure for the printer settings.</summary>
		public void SetHdevmode(nint hdevmode) { throw new System.PlatformNotSupportedException("SetHdevmode requires Windows GDI and is not supported on this platform."); }

		/// <summary>Sets the HDEVNAMES structure for the printer settings.</summary>
		public void SetHdevnames(nint hdevnames) { throw new System.PlatformNotSupportedException("SetHdevnames requires Windows GDI and is not supported on this platform."); }

		/// <summary>Returns a string representation of this <see cref="PrinterSettings"/>.</summary>
		public override string ToString() =>
			$"[PrinterSettings {PrinterName} Copies={Copies} Collate={Collate} Duplex={Duplex} FromPage={FromPage} LandscapeAngle={LandscapeAngle} MaximumCopies={MaximumCopies} OutputPort= ToPage={ToPage}]";
	}
}
