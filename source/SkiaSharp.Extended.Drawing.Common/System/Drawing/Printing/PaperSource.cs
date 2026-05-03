namespace System.Drawing.Printing
{
	/// <summary>
	///  Specifies the paper tray from which the printer gets paper.
	/// </summary>
	public partial class PaperSource
	{
		private int _rawKind;
		private string _name = string.Empty;

		/// <summary>Initializes a new instance of the <see cref="PaperSource"/> class.</summary>
		public PaperSource() { _rawKind = (int)PaperSourceKind.Custom; }

		/// <summary>Gets the paper source.</summary>
		public System.Drawing.Printing.PaperSourceKind Kind => (PaperSourceKind)_rawKind;
		/// <summary>Gets or sets the paper source raw kind value.</summary>
		public int RawKind { get => _rawKind; set => _rawKind = value; }
		/// <summary>Gets or sets the name of the paper source.</summary>
		public string SourceName { get => _name; set => _name = value ?? string.Empty; }
		/// <summary>Returns a string representation of this <see cref="PaperSource"/>.</summary>
		public override string ToString() => $"[PaperSource {SourceName} Kind={Kind}]";
	}
}
