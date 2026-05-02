using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Defines a rectangular brush with a hatch style, a foreground color, and a background color.
	///  This class cannot be inherited.
	/// </summary>
	public sealed partial class HatchBrush : System.Drawing.Brush
	{
		private readonly HatchStyle _hatchStyle;
		private readonly Color _foreColor;
		private readonly Color _backColor;

		/// <summary>
		///  Initializes a new instance of the <see cref="HatchBrush"/> class with the specified
		///  <see cref="Drawing2D.HatchStyle"/> enumeration and foreground color.
		/// </summary>
		/// <param name="hatchstyle">One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern drawn by this <see cref="HatchBrush"/>.</param>
		/// <param name="foreColor">The <see cref="Color"/> structure that represents the color of lines drawn by this <see cref="HatchBrush"/>.</param>
		public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, System.Drawing.Color foreColor)
			: this(hatchstyle, foreColor, Color.Black) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="HatchBrush"/> class with the specified
		///  <see cref="Drawing2D.HatchStyle"/> enumeration, foreground color, and background color.
		/// </summary>
		/// <param name="hatchstyle">One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern drawn by this <see cref="HatchBrush"/>.</param>
		/// <param name="foreColor">The <see cref="Color"/> structure that represents the color of lines drawn by this <see cref="HatchBrush"/>.</param>
		/// <param name="backColor">The <see cref="Color"/> structure that represents the color of spaces between the lines drawn by this <see cref="HatchBrush"/>.</param>
		public HatchBrush(System.Drawing.Drawing2D.HatchStyle hatchstyle, System.Drawing.Color foreColor, System.Drawing.Color backColor)
		{
			_hatchStyle = hatchstyle;
			_foreColor = foreColor;
			_backColor = backColor;
		}

		/// <summary>
		///  Gets the color of spaces between the hatch lines drawn by this <see cref="HatchBrush"/> object.
		/// </summary>
		/// <value>A <see cref="Color"/> that represents the background color for this <see cref="HatchBrush"/>.</value>
		public System.Drawing.Color BackgroundColor
		{
			get { ThrowIfDisposed(); return _backColor; }
		}

		/// <summary>
		///  Gets the color of hatch lines drawn by this <see cref="HatchBrush"/> object.
		/// </summary>
		/// <value>A <see cref="Color"/> that represents the foreground color for this <see cref="HatchBrush"/>.</value>
		public System.Drawing.Color ForegroundColor
		{
			get { ThrowIfDisposed(); return _foreColor; }
		}

		/// <summary>
		///  Gets the hatch style of this <see cref="HatchBrush"/> object.
		/// </summary>
		/// <value>One of the <see cref="Drawing2D.HatchStyle"/> values that represents the pattern of this <see cref="HatchBrush"/>.</value>
		public System.Drawing.Drawing2D.HatchStyle HatchStyle
		{
			get { ThrowIfDisposed(); return _hatchStyle; }
		}

		/// <summary>
		///  Creates an exact copy of this <see cref="HatchBrush"/> object.
		/// </summary>
		/// <returns>The <see cref="HatchBrush"/> object this method creates, cast as an <see cref="object"/>.</returns>
		public override object Clone() => new HatchBrush(_hatchStyle, _foreColor, _backColor);

		/// <summary>
		///  Creates an <see cref="SKPaint"/> configured for fill operations with this hatch brush.
		///  Currently uses the foreground color as a solid fill; full hatch pattern rendering is not yet implemented.
		/// </summary>
		/// <returns>A new <see cref="SKPaint"/> with <see cref="SKPaintStyle.Fill"/> and the foreground color.</returns>
		internal override SKPaint CreatePaint()
		{
			ThrowIfDisposed();
			return new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SkiaConversions.ToSKColor(_foreColor),
				IsAntialias = true,
			};
		}
	}
}
