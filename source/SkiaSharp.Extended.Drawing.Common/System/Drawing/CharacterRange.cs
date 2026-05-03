namespace System.Drawing
{
	/// <summary>
	///  Specifies a range of character positions within a string.
	/// </summary>
	public partial struct CharacterRange
	{
		private int _first;
		private int _length;

		/// <summary>
		///  Initializes a new instance of the <see cref="CharacterRange"/> structure,
		///  specifying a range of character positions within a string.
		/// </summary>
		/// <param name="First">The position of the first character in the range.</param>
		/// <param name="Length">The number of positions in the range.</param>
		public CharacterRange(int First, int Length)
		{
			_first = First;
			_length = Length;
		}

		/// <summary>
		///  Gets or sets the position in the string of the first character of this <see cref="CharacterRange"/>.
		/// </summary>
		public int First
		{
			get => _first;
			set => _first = value;
		}

		/// <summary>
		///  Gets or sets the number of positions in this <see cref="CharacterRange"/>.
		/// </summary>
		public int Length
		{
			get => _length;
			set => _length = value;
		}

		/// <summary>
		///  Compares two <see cref="CharacterRange"/> objects for equality.
		/// </summary>
		public static bool operator ==(CharacterRange cr1, CharacterRange cr2)
			=> cr1._first == cr2._first && cr1._length == cr2._length;

		/// <summary>
		///  Compares two <see cref="CharacterRange"/> objects for inequality.
		/// </summary>
		public static bool operator !=(CharacterRange cr1, CharacterRange cr2)
			=> !(cr1 == cr2);

		/// <summary>
		///  Indicates whether the specified object is a <see cref="CharacterRange"/> equivalent to this one.
		/// </summary>
		public override bool Equals(object? obj)
			=> obj is CharacterRange other && this == other;

		/// <summary>
		///  Returns a hash code for this <see cref="CharacterRange"/>.
		/// </summary>
		public override int GetHashCode()
			=> (_first * 397) ^ _length;
	}
}
