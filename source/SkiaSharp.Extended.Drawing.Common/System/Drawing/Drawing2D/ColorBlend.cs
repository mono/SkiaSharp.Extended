namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Defines arrays of colors and positions used for interpolating color blending in a multicolor gradient.
	/// </summary>
	public sealed partial class ColorBlend
	{
		private Color[] _colors;
		private float[] _positions;

		/// <summary>
		///  Initializes a new instance of the <see cref="ColorBlend"/> class with one color and position.
		/// </summary>
		public ColorBlend()
		{
			_colors = new Color[1];
			_positions = new float[1];
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="ColorBlend"/> class with the specified number of colors and positions.
		/// </summary>
		/// <param name="count">The number of colors and positions in this <see cref="ColorBlend"/>.</param>
		public ColorBlend(int count)
		{
			_colors = new Color[count];
			_positions = new float[count];
		}

		/// <summary>
		///  Gets or sets an array of colors that represents the colors to use at corresponding positions along a gradient.
		/// </summary>
		/// <value>An array of <see cref="Color"/> structures that represents the colors to use at corresponding positions along a gradient.</value>
		public System.Drawing.Color[] Colors
		{
			get => _colors;
			set => _colors = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		///  Gets or sets the positions along a gradient line.
		/// </summary>
		/// <value>An array of values that specify percentages of distance along the gradient line.</value>
		public float[] Positions
		{
			get => _positions;
			set => _positions = value ?? throw new ArgumentNullException(nameof(value));
		}
	}
}
