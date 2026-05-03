namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Defines a blend pattern for a <see cref="LinearGradientBrush"/> object.
	/// </summary>
	public sealed partial class Blend
	{
		private float[] _factors;
		private float[] _positions;

		/// <summary>
		///  Initializes a new instance of the <see cref="Blend"/> class with one element.
		/// </summary>
		public Blend()
		{
			_factors = new float[1];
			_positions = new float[1];
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="Blend"/> class with the specified number of factors and positions.
		/// </summary>
		/// <param name="count">The number of elements in the <see cref="Factors"/> and <see cref="Positions"/> arrays.</param>
		public Blend(int count)
		{
			_factors = new float[count];
			_positions = new float[count];
		}

		/// <summary>
		///  Gets or sets an array of blend factors for the gradient.
		/// </summary>
		/// <value>An array of blend factors that specify the percentages of the starting color and the ending color to be used at the corresponding position.</value>
		public float[] Factors
		{
			get => _factors;
			set => _factors = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		///  Gets or sets an array of blend positions for the gradient.
		/// </summary>
		/// <value>An array of blend positions that specify the percentages of distance along the gradient line.</value>
		public float[] Positions
		{
			get => _positions;
			set => _positions = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}
