namespace System.Drawing.Printing;

/// <summary>
///  Specifies the dimensions of the margins of a printed page.
/// </summary>
[System.ComponentModel.TypeConverterAttribute(typeof(System.Drawing.Printing.MarginsConverter))]
public partial class Margins : System.ICloneable
{
	/// <summary>Initializes a new instance of the <see cref="Margins"/> class with 1-inch margins.</summary>
	public Margins() { Left = 100; Right = 100; Top = 100; Bottom = 100; }

	/// <summary>Initializes a new instance of the <see cref="Margins"/> class with the specified margins.</summary>
	/// <param name="left">The left margin, in hundredths of an inch.</param>
	/// <param name="right">The right margin, in hundredths of an inch.</param>
	/// <param name="top">The top margin, in hundredths of an inch.</param>
	/// <param name="bottom">The bottom margin, in hundredths of an inch.</param>
	public Margins(int left, int right, int top, int bottom)
	{
		if (left < 0) throw new ArgumentOutOfRangeException(nameof(left));
		if (right < 0) throw new ArgumentOutOfRangeException(nameof(right));
		if (top < 0) throw new ArgumentOutOfRangeException(nameof(top));
		if (bottom < 0) throw new ArgumentOutOfRangeException(nameof(bottom));
		Left = left; Right = right; Top = top; Bottom = bottom;
	}

	/// <summary>Gets or sets the bottom margin, in hundredths of an inch.</summary>
	public int Bottom { get; set; }
	/// <summary>Gets or sets the left margin, in hundredths of an inch.</summary>
	public int Left { get; set; }
	/// <summary>Gets or sets the right margin, in hundredths of an inch.</summary>
	public int Right { get; set; }
	/// <summary>Gets or sets the top margin, in hundredths of an inch.</summary>
	public int Top { get; set; }

	/// <summary>Compares two <see cref="Margins"/> objects to determine whether they are equal.</summary>
	public static bool operator ==(System.Drawing.Printing.Margins? m1, System.Drawing.Printing.Margins? m2)
	{
		if (m1 is null) return m2 is null;
		if (m2 is null) return false;
		return m1.Left == m2.Left && m1.Right == m2.Right && m1.Top == m2.Top && m1.Bottom == m2.Bottom;
	}

	/// <summary>Compares two <see cref="Margins"/> objects to determine whether they are not equal.</summary>
	public static bool operator !=(System.Drawing.Printing.Margins? m1, System.Drawing.Printing.Margins? m2) => !(m1 == m2);

	/// <summary>Creates an exact copy of this <see cref="Margins"/>.</summary>
	public object Clone() => new Margins(Left, Right, Top, Bottom);

	/// <summary>Determines whether the specified object is equal to this <see cref="Margins"/>.</summary>
	public override bool Equals(object? obj) => obj is Margins m && this == m;

	/// <summary>Serves as a hash function for this <see cref="Margins"/>.</summary>
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			hash = hash * 31 + Left;
			hash = hash * 31 + Right;
			hash = hash * 31 + Top;
			hash = hash * 31 + Bottom;
			return hash;
		}
	}

	/// <summary>Returns a string representation of this <see cref="Margins"/>.</summary>
	public override string ToString() => $"[Margins Left={Left} Right={Right} Top={Top} Bottom={Bottom}]";
}
