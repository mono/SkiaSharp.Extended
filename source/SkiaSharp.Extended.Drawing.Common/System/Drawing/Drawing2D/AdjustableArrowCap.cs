namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Represents an adjustable arrow-shaped line cap.
	/// </summary>
	public sealed partial class AdjustableArrowCap : System.Drawing.Drawing2D.CustomLineCap
	{
		private float _width;
		private float _height;
		private bool _filled;
		private float _middleInset;

		/// <summary>Initializes a new instance of the <see cref="AdjustableArrowCap"/> class with the specified width and height.</summary>
		/// <param name="width">The width of the arrow.</param>
		/// <param name="height">The height of the arrow.</param>
		public AdjustableArrowCap(float width, float height) : this(width, height, true) { }

		/// <summary>Initializes a new instance of the <see cref="AdjustableArrowCap"/> class with the specified width, height, and fill property.</summary>
		/// <param name="width">The width of the arrow.</param>
		/// <param name="height">The height of the arrow.</param>
		/// <param name="isFilled"><see langword="true"/> to fill the arrow cap; otherwise, <see langword="false"/>.</param>
		public AdjustableArrowCap(float width, float height, bool isFilled) : base(null, null)
		{
			_width = width;
			_height = height;
			_filled = isFilled;
		}

		/// <summary>Gets or sets whether the arrow cap is filled.</summary>
		public bool Filled { get => _filled; set => _filled = value; }
		/// <summary>Gets or sets the height of the arrow cap.</summary>
		public new float Height { get => _height; set => _height = value; }
		/// <summary>Gets or sets the number of units between the outline of the arrow cap and the fill.</summary>
		public float MiddleInset { get => _middleInset; set => _middleInset = value; }
		/// <summary>Gets or sets the width of the arrow cap.</summary>
		public new float Width { get => _width; set => _width = value; }
	}
}
