namespace System.Drawing.Printing
{
	/// <summary>
	///  Specifies the size of a piece of paper.
	/// </summary>
	public partial class PaperSize
	{
		private int _rawKind;
		private string _name = string.Empty;

		/// <summary>Initializes a new instance of the <see cref="PaperSize"/> class.</summary>
		public PaperSize() { _rawKind = (int)PaperKind.Custom; }

		/// <summary>Initializes a new instance of the <see cref="PaperSize"/> class.</summary>
		/// <param name="name">The name of the paper.</param>
		/// <param name="width">The width of the paper, in hundredths of an inch.</param>
		/// <param name="height">The height of the paper, in hundredths of an inch.</param>
		public PaperSize(string name, int width, int height)
		{
			_name = name ?? string.Empty;
			Width = width;
			Height = height;
			_rawKind = (int)PaperKind.Custom;
		}

		/// <summary>Gets or sets the height of the paper, in hundredths of an inch.</summary>
		public int Height { get; set; }
		/// <summary>Gets the type of paper.</summary>
		public System.Drawing.Printing.PaperKind Kind => (PaperKind)_rawKind;
		/// <summary>Gets or sets the name of the type of paper.</summary>
		public string PaperName { get => _name; set => _name = value ?? string.Empty; }
		/// <summary>Gets or sets the paper size raw kind value.</summary>
		public int RawKind { get => _rawKind; set => _rawKind = value; }
		/// <summary>Gets or sets the width of the paper, in hundredths of an inch.</summary>
		public int Width { get; set; }
		/// <summary>Returns a string representation of this <see cref="PaperSize"/>.</summary>
		public override string ToString() => $"[PaperSize {PaperName} Kind={Kind} Height={Height} Width={Width}]";
	}
}
