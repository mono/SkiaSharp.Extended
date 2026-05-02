using SkiaSharp;
using System.Drawing.Drawing2D;
using System.Drawing.Internal;

namespace System.Drawing
{
	/// <summary>
	///  Defines an object used to draw lines and curves. This class cannot be inherited.
	/// </summary>
	public sealed partial class Pen : System.MarshalByRefObject, System.ICloneable, System.IDisposable
	{
		private Color _color;
		private float _width;
		private Brush _brush;
		private DashStyle _dashStyle;
		private float[]? _dashPattern;
		private float _dashOffset;
		private DashCap _dashCap;
		private LineCap _startCap;
		private LineCap _endCap;
		private LineJoin _lineJoin;
		private float _miterLimit = 10f;
		private PenAlignment _alignment;
		private float[]? _compoundArray;
		private Matrix? _transform;
		private CustomLineCap? _customStartCap;
		private CustomLineCap? _customEndCap;
		private bool _disposed;

		/// <summary>
		///  Initializes a new instance of the <see cref="Pen"/> class with the specified <see cref="Brush"/>.
		/// </summary>
		/// <param name="brush">A <see cref="Brush"/> that determines the fill properties of this <see cref="Pen"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="brush"/> is <see langword="null"/>.</exception>
		public Pen(System.Drawing.Brush brush) : this(brush, 1f) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="Pen"/> class with the specified
		///  <see cref="Brush"/> and <paramref name="width"/>.
		/// </summary>
		/// <param name="brush">A <see cref="Brush"/> that determines the characteristics of this <see cref="Pen"/>.</param>
		/// <param name="width">The width of the new <see cref="Pen"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="brush"/> is <see langword="null"/>.</exception>
		public Pen(System.Drawing.Brush brush, float width)
		{
			_brush = brush ?? throw new ArgumentNullException(nameof(brush));
			_width = width;
			_color = brush is SolidBrush sb ? sb.Color : Color.Black;
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="Pen"/> class with the specified color.
		/// </summary>
		/// <param name="color">A <see cref="Color"/> structure that indicates the color of this <see cref="Pen"/>.</param>
		public Pen(System.Drawing.Color color) : this(color, 1f) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="Pen"/> class with the specified
		///  <see cref="Color"/> and <paramref name="width"/>.
		/// </summary>
		/// <param name="color">A <see cref="Color"/> structure that indicates the color of this <see cref="Pen"/>.</param>
		/// <param name="width">A value indicating the width of this <see cref="Pen"/>.</param>
		public Pen(System.Drawing.Color color, float width)
		{
			_color = color;
			_width = width;
			_brush = new SolidBrush(color);
		}

		/// <summary>
		///  Gets or sets the alignment for this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  A <see cref="PenAlignment"/> that represents the alignment for this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.PenAlignment Alignment
		{
			get { ThrowIfDisposed(); return _alignment; }
			set { ThrowIfDisposed(); _alignment = value; }
		}

		/// <summary>
		///  Gets or sets the <see cref="Brush"/> that determines attributes of this <see cref="Pen"/>.
		/// </summary>
		/// <value>A <see cref="Brush"/> that determines attributes of this <see cref="Pen"/>.</value>
		public System.Drawing.Brush Brush
		{
			get { ThrowIfDisposed(); return _brush; }
			set
			{
				ThrowIfDisposed();
				_brush = value ?? throw new ArgumentNullException(nameof(value));
				if (value is SolidBrush sb)
					_color = sb.Color;
			}
		}

		/// <summary>
		///  Gets or sets the color of this <see cref="Pen"/>.
		/// </summary>
		/// <value>A <see cref="Color"/> structure that represents the color of this <see cref="Pen"/>.</value>
		public System.Drawing.Color Color
		{
			get { ThrowIfDisposed(); return _color; }
			set
			{
				ThrowIfDisposed();
				_color = value;
				_brush = new SolidBrush(value);
			}
		}

		/// <summary>
		///  Gets or sets an array of values that specifies a compound pen. A compound pen
		///  draws a compound line made up of parallel lines and spaces.
		/// </summary>
		/// <value>An array of real numbers that specifies the compound array.</value>
		public float[] CompoundArray
		{
			get { ThrowIfDisposed(); return _compoundArray ?? Array.Empty<float>(); }
			set { ThrowIfDisposed(); _compoundArray = value; }
		}

		/// <summary>
		///  Gets or sets a custom cap to use at the end of lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  A <see cref="CustomLineCap"/> that represents the cap used at the end of lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.CustomLineCap CustomEndCap
		{
			get { ThrowIfDisposed(); return _customEndCap!; }
			set { ThrowIfDisposed(); _customEndCap = value; }
		}

		/// <summary>
		///  Gets or sets a custom cap to use at the beginning of lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  A <see cref="CustomLineCap"/> that represents the cap used at the beginning of lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.CustomLineCap CustomStartCap
		{
			get { ThrowIfDisposed(); return _customStartCap!; }
			set { ThrowIfDisposed(); _customStartCap = value; }
		}

		/// <summary>
		///  Gets or sets the cap style used at the end of the dashes that make up dashed
		///  lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  One of the <see cref="DashCap"/> values that represents the cap style used at the
		///  beginning and end of the dashes that make up dashed lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.DashCap DashCap
		{
			get { ThrowIfDisposed(); return _dashCap; }
			set { ThrowIfDisposed(); _dashCap = value; }
		}

		/// <summary>
		///  Gets or sets the distance from the start of a line to the beginning of a dash pattern.
		/// </summary>
		/// <value>The distance from the start of a line to the beginning of a dash pattern.</value>
		public float DashOffset
		{
			get { ThrowIfDisposed(); return _dashOffset; }
			set { ThrowIfDisposed(); _dashOffset = value; }
		}

		/// <summary>
		///  Gets or sets an array of custom dashes and spaces.
		/// </summary>
		/// <value>An array of real numbers that specifies the lengths of alternating dashes and spaces in dashed lines.</value>
		public float[] DashPattern
		{
			get { ThrowIfDisposed(); return _dashPattern ?? Array.Empty<float>(); }
			set
			{
				ThrowIfDisposed();
				_dashPattern = value;
				_dashStyle = DashStyle.Custom;
			}
		}

		/// <summary>
		///  Gets or sets the style used for dashed lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  A <see cref="DashStyle"/> that represents the style used for dashed lines drawn
		///  with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.DashStyle DashStyle
		{
			get { ThrowIfDisposed(); return _dashStyle; }
			set
			{
				ThrowIfDisposed();
				_dashStyle = value;
				if (value != DashStyle.Custom)
					_dashPattern = SkiaConversions.GetDashPattern(value);
			}
		}

		/// <summary>
		///  Gets or sets the cap style used at the end of lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  One of the <see cref="LineCap"/> values that represents the cap style used at the
		///  end of lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.LineCap EndCap
		{
			get { ThrowIfDisposed(); return _endCap; }
			set { ThrowIfDisposed(); _endCap = value; }
		}

		/// <summary>
		///  Gets or sets the join style for the ends of two consecutive lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  A <see cref="LineJoin"/> that represents the join style for the ends of two consecutive
		///  lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.LineJoin LineJoin
		{
			get { ThrowIfDisposed(); return _lineJoin; }
			set { ThrowIfDisposed(); _lineJoin = value; }
		}

		/// <summary>
		///  Gets or sets the limit of the thickness of the join on a mitered corner.
		/// </summary>
		/// <value>
		///  The limit of the thickness of the join on a mitered corner.
		/// </value>
		public float MiterLimit
		{
			get { ThrowIfDisposed(); return _miterLimit; }
			set { ThrowIfDisposed(); _miterLimit = value; }
		}

		/// <summary>
		///  Gets the style of lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  A <see cref="PenType"/> enumeration that specifies the style of lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.PenType PenType
		{
			get
			{
				ThrowIfDisposed();
				if (_brush is SolidBrush)
					return PenType.SolidColor;
				if (_brush is TextureBrush)
					return PenType.TextureFill;
				if (_brush is Drawing2D.HatchBrush)
					return PenType.HatchFill;
				if (_brush is Drawing2D.LinearGradientBrush)
					return PenType.LinearGradient;
				if (_brush is Drawing2D.PathGradientBrush)
					return PenType.PathGradient;
				return PenType.SolidColor;
			}
		}

		/// <summary>
		///  Gets or sets the cap style used at the beginning of lines drawn with this <see cref="Pen"/>.
		/// </summary>
		/// <value>
		///  One of the <see cref="LineCap"/> values that represents the cap style used at the
		///  beginning of lines drawn with this <see cref="Pen"/>.
		/// </value>
		public System.Drawing.Drawing2D.LineCap StartCap
		{
			get { ThrowIfDisposed(); return _startCap; }
			set { ThrowIfDisposed(); _startCap = value; }
		}

		/// <summary>
		///  Gets or sets a copy of the geometric transformation for this <see cref="Pen"/>.
		/// </summary>
		/// <value>A copy of the <see cref="Matrix"/> that represents the geometric transformation for this <see cref="Pen"/>.</value>
		public System.Drawing.Drawing2D.Matrix Transform
		{
			get { ThrowIfDisposed(); return _transform ?? new Matrix(); }
			set { ThrowIfDisposed(); _transform = value ?? throw new ArgumentNullException(nameof(value)); }
		}

		/// <summary>
		///  Gets or sets the width of this <see cref="Pen"/>, in units of the
		///  <see cref="Graphics"/> object used for drawing.
		/// </summary>
		/// <value>The width of this <see cref="Pen"/>.</value>
		public float Width
		{
			get { ThrowIfDisposed(); return _width; }
			set { ThrowIfDisposed(); _width = value; }
		}

		/// <summary>
		///  Creates an exact copy of this <see cref="Pen"/>.
		/// </summary>
		/// <returns>An <see cref="object"/> that can be cast to a <see cref="Pen"/>.</returns>
		public object Clone()
		{
			ThrowIfDisposed();
			var pen = new Pen(_color, _width)
			{
				_dashStyle = _dashStyle,
				_dashPattern = (float[]?)_dashPattern?.Clone(),
				_dashOffset = _dashOffset,
				_dashCap = _dashCap,
				_startCap = _startCap,
				_endCap = _endCap,
				_lineJoin = _lineJoin,
				_miterLimit = _miterLimit,
				_alignment = _alignment,
				_compoundArray = (float[]?)_compoundArray?.Clone(),
				_customStartCap = _customStartCap,
				_customEndCap = _customEndCap,
				_brush = (Brush)_brush.Clone(),
				_transform = _transform != null ? (Matrix?)_transform.Clone() : null,
			};
			return pen;
		}

		/// <summary>
		///  Releases all resources used by this <see cref="Pen"/>.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///  Multiplies the transformation matrix for this <see cref="Pen"/> by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="matrix">The <see cref="Matrix"/> object by which to multiply the transformation matrix.</param>
		public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix)
		{
			MultiplyTransform(matrix, MatrixOrder.Prepend);
		}

		/// <summary>
		///  Multiplies the transformation matrix for this <see cref="Pen"/> by the specified
		///  <see cref="Matrix"/> in the specified order.
		/// </summary>
		/// <param name="matrix">The <see cref="Matrix"/> by which to multiply the transformation matrix.</param>
		/// <param name="order">The order in which to perform the multiplication operation.</param>
		public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			if (matrix is null) throw new ArgumentNullException(nameof(matrix));
			_transform ??= new Matrix();
			_transform.Multiply(matrix, order);
		}

		/// <summary>
		///  Resets the geometric transformation matrix for this <see cref="Pen"/> to identity.
		/// </summary>
		public void ResetTransform()
		{
			ThrowIfDisposed();
			_transform = null;
		}

		/// <summary>
		///  Rotates the local geometric transformation by the specified angle.
		/// </summary>
		/// <param name="angle">The angle of rotation.</param>
		public void RotateTransform(float angle)
		{
			RotateTransform(angle, MatrixOrder.Prepend);
		}

		/// <summary>
		///  Rotates the local geometric transformation by the specified angle in the specified order.
		/// </summary>
		/// <param name="angle">The angle of rotation.</param>
		/// <param name="order">A <see cref="MatrixOrder"/> that specifies whether to append or prepend the rotation matrix.</param>
		public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			_transform ??= new Matrix();
			_transform.Rotate(angle, order);
		}

		/// <summary>
		///  Scales the local geometric transformation by the specified factors.
		/// </summary>
		/// <param name="sx">The factor by which to scale the transformation in the x-axis direction.</param>
		/// <param name="sy">The factor by which to scale the transformation in the y-axis direction.</param>
		public void ScaleTransform(float sx, float sy)
		{
			ScaleTransform(sx, sy, MatrixOrder.Prepend);
		}

		/// <summary>
		///  Scales the local geometric transformation by the specified factors in the specified order.
		/// </summary>
		/// <param name="sx">The factor by which to scale the transformation in the x-axis direction.</param>
		/// <param name="sy">The factor by which to scale the transformation in the y-axis direction.</param>
		/// <param name="order">A <see cref="MatrixOrder"/> that specifies whether to append or prepend the scaling matrix.</param>
		public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			_transform ??= new Matrix();
			_transform.Scale(sx, sy, order);
		}

		/// <summary>
		///  Sets the values that determine the style of cap used to end lines drawn by this <see cref="Pen"/>.
		/// </summary>
		/// <param name="startCap">A <see cref="LineCap"/> that represents the cap style to use at the beginning of lines drawn with this <see cref="Pen"/>.</param>
		/// <param name="endCap">A <see cref="LineCap"/> that represents the cap style to use at the end of lines drawn with this <see cref="Pen"/>.</param>
		/// <param name="dashCap">A <see cref="DashCap"/> that represents the cap style to use at the beginning or end of dashed lines drawn with this <see cref="Pen"/>.</param>
		public void SetLineCap(System.Drawing.Drawing2D.LineCap startCap, System.Drawing.Drawing2D.LineCap endCap, System.Drawing.Drawing2D.DashCap dashCap)
		{
			ThrowIfDisposed();
			_startCap = startCap;
			_endCap = endCap;
			_dashCap = dashCap;
		}

		/// <summary>
		///  Translates the local geometric transformation by the specified dimensions.
		/// </summary>
		/// <param name="dx">The value of the translation in x.</param>
		/// <param name="dy">The value of the translation in y.</param>
		public void TranslateTransform(float dx, float dy)
		{
			TranslateTransform(dx, dy, MatrixOrder.Prepend);
		}

		/// <summary>
		///  Translates the local geometric transformation by the specified dimensions in the specified order.
		/// </summary>
		/// <param name="dx">The value of the translation in x.</param>
		/// <param name="dy">The value of the translation in y.</param>
		/// <param name="order">The order (prepend or append) in which to apply the translation.</param>
		public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			_transform ??= new Matrix();
			_transform.Translate(dx, dy, order);
		}

		/// <summary>
		///  Allows a <see cref="Pen"/> object to attempt to free resources and perform other
		///  cleanup operations before the <see cref="Pen"/> object is reclaimed by garbage collection.
		/// </summary>
		~Pen()
		{
			Dispose(false);
		}

		/// <summary>
		///  Creates an <see cref="SKPaint"/> configured for stroke operations from this pen.
		/// </summary>
		/// <returns>A new <see cref="SKPaint"/> with <see cref="SKPaintStyle.Stroke"/> and mapped pen properties.</returns>
		internal SKPaint CreatePaint()
		{
			ThrowIfDisposed();

			SKPaint paint;
			if (_brush != null && _brush is not SolidBrush)
			{
				// Get paint from the brush (includes shader for gradients, textures, etc.)
				paint = _brush.CreatePaint();
			}
			else
			{
				paint = new SKPaint
				{
					Color = SkiaConversions.ToSKColor(_color),
					IsAntialias = true,
				};
			}

			// Overlay stroke properties
			paint.Style = SKPaintStyle.Stroke;
			paint.StrokeWidth = _width;
			paint.StrokeCap = SkiaConversions.ToSKStrokeCap(_endCap);
			paint.StrokeJoin = SkiaConversions.ToSKStrokeJoin(_lineJoin);
			paint.StrokeMiter = _miterLimit;

			// Apply dash pattern
			var pattern = _dashStyle == DashStyle.Custom ? _dashPattern : SkiaConversions.GetDashPattern(_dashStyle);
			if (pattern != null && pattern.Length >= 2)
			{
				// Scale dash pattern by stroke width (GDI+ convention)
				var scaledPattern = new float[pattern.Length];
				var scale = _width > 0 ? _width : 1f;
				for (int i = 0; i < pattern.Length; i++)
					scaledPattern[i] = pattern[i] * scale;

				paint.PathEffect = SKPathEffect.CreateDash(scaledPattern, _dashOffset * scale);
			}

			return paint;
		}

		private void Dispose(bool disposing)
		{
			_disposed = true;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(Pen));
		}
	}
}
