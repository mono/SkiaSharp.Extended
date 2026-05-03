namespace System.Drawing.Imaging
{
	/// <summary>
	///  Used to pass a value, or an array of values, to an image encoder.
	/// </summary>
	public sealed partial class EncoderParameter : System.IDisposable
	{
		private Encoder _encoder = null!;
		private EncoderParameterValueType _type;
		private int _numberOfValues;

		/// <summary>Initializes a new instance with the specified Encoder and byte value.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeByte; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder, byte value, and undefined flag.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte value, bool undefined) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = undefined ? EncoderParameterValueType.ValueTypeUndefined : EncoderParameterValueType.ValueTypeByte; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder and byte array.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte[] value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeByte; _numberOfValues = value?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder, byte array, and undefined flag.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte[] value, bool undefined) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = undefined ? EncoderParameterValueType.ValueTypeUndefined : EncoderParameterValueType.ValueTypeByte; _numberOfValues = value?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder and short value.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, short value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeShort; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder and short array.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, short[] value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeShort; _numberOfValues = value?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder, number of values, value type, and pointer.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numberValues, System.Drawing.Imaging.EncoderParameterValueType type, nint value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = type; _numberOfValues = numberValues; }
		/// <summary>Initializes a new instance with the specified Encoder and a pair of integers (numerator/denominator).</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numerator, int denominator) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeRational; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder, number of values, type, and value pointer (obsolete).</summary>
		[System.ObsoleteAttribute("This constructor has been deprecated. Use EncoderParameter(Encoder encoder, int numberValues, EncoderParameterValueType type, IntPtr value) instead.")]
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int NumberOfValues, int Type, int Value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = (EncoderParameterValueType)Type; _numberOfValues = NumberOfValues; }
		/// <summary>Initializes a new instance with the specified Encoder and two pairs of integers (range of rationals).</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numerator1, int demoninator1, int numerator2, int demoninator2) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeRationalRange; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder and arrays of numerators and denominators.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int[] numerator, int[] denominator) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeRational; _numberOfValues = numerator?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder and arrays of rational ranges.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int[] numerator1, int[] denominator1, int[] numerator2, int[] denominator2) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeRationalRange; _numberOfValues = numerator1?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder and long value.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeLong; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder and a range of long values.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long rangebegin, long rangeend) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeLongRange; _numberOfValues = 1; }
		/// <summary>Initializes a new instance with the specified Encoder and long array.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long[] value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeLong; _numberOfValues = value?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder and arrays of long ranges.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long[] rangebegin, long[] rangeend) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeLongRange; _numberOfValues = rangebegin?.Length ?? 0; }
		/// <summary>Initializes a new instance with the specified Encoder and string value.</summary>
		public EncoderParameter(System.Drawing.Imaging.Encoder encoder, string value) { _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder)); _type = EncoderParameterValueType.ValueTypeAscii; _numberOfValues = value?.Length ?? 0; }

		/// <summary>Gets or sets the <see cref="Imaging.Encoder"/> object associated with this <see cref="EncoderParameter"/>.</summary>
		public System.Drawing.Imaging.Encoder Encoder { get => _encoder; set => _encoder = value ?? throw new ArgumentNullException(nameof(value)); }
		/// <summary>Gets the number of elements in the array of values stored in this <see cref="EncoderParameter"/> object.</summary>
		public int NumberOfValues => _numberOfValues;
		/// <summary>Gets the data type of the values stored in this <see cref="EncoderParameter"/> object.</summary>
		public System.Drawing.Imaging.EncoderParameterValueType Type => _type;
		/// <summary>Gets the data type of the values stored in this <see cref="EncoderParameter"/> object.</summary>
		public System.Drawing.Imaging.EncoderParameterValueType ValueType => _type;

		/// <summary>Releases all resources used by this <see cref="EncoderParameter"/> object.</summary>
		public void Dispose() { GC.SuppressFinalize(this); }
		/// <summary>Allows an <see cref="EncoderParameter"/> object to attempt to free resources before being reclaimed by garbage collection.</summary>
		~EncoderParameter() { }
	}
}
