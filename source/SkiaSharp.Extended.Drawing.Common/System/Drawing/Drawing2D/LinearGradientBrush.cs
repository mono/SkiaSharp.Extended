using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing.Drawing2D;

/// <summary>
///  Encapsulates a <see cref="Brush"/> with a linear gradient. This class cannot be inherited.
/// </summary>
public sealed partial class LinearGradientBrush : Brush
{
	private PointF _point1;
	private PointF _point2;
	private Color _color1;
	private Color _color2;
	private RectangleF _rect;
	private WrapMode _wrapMode;
	private Matrix _transform;
	private Blend? _blend;
	private ColorBlend? _interpolationColors;
	private bool _gammaCorrection;

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class with the specified points and colors.
	/// </summary>
	/// <param name="point1">A <see cref="Point"/> structure that represents the starting point of the linear gradient.</param>
	/// <param name="point2">A <see cref="Point"/> structure that represents the endpoint of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color of the linear gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color of the linear gradient.</param>
	public LinearGradientBrush(Point point1, Point point2, Color color1, Color color2)
		: this(new PointF(point1.X, point1.Y), new PointF(point2.X, point2.Y), color1, color2) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class with the specified points and colors.
	/// </summary>
	/// <param name="point1">A <see cref="PointF"/> structure that represents the starting point of the linear gradient.</param>
	/// <param name="point2">A <see cref="PointF"/> structure that represents the endpoint of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color of the linear gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color of the linear gradient.</param>
	public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2)
	{
		_point1 = point1;
		_point2 = point2;
		_color1 = color1;
		_color2 = color2;
		_rect = RectangleF.FromLTRB(
			Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y),
			Math.Max(point1.X, point2.X), Math.Max(point1.Y, point2.Y));
		_wrapMode = WrapMode.Tile;
		_transform = new Matrix();
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class based on a rectangle, starting and ending colors, and a gradient mode.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that specifies the bounds of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color for the gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color for the gradient.</param>
	/// <param name="linearGradientMode">A <see cref="LinearGradientMode"/> enumeration element that specifies the orientation of the gradient.</param>
	public LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
		: this((RectangleF)rect, color1, color2, linearGradientMode) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class based on a rectangle, starting and ending colors, and an angle.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that specifies the bounds of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color for the gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color for the gradient.</param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line.</param>
	public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle)
		: this((RectangleF)rect, color1, color2, angle, false) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class based on a rectangle, starting and ending colors, an angle, and whether the angle is scaleable.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that specifies the bounds of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color for the gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color for the gradient.</param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line.</param>
	/// <param name="isAngleScaleable"><see langword="true"/> to specify that the angle is affected by the transform associated with this <see cref="LinearGradientBrush"/>; otherwise, <see langword="false"/>.</param>
	public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable)
		: this((RectangleF)rect, color1, color2, angle, isAngleScaleable) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class based on a rectangle, starting and ending colors, and a gradient mode.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure that specifies the bounds of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color for the gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color for the gradient.</param>
	/// <param name="linearGradientMode">A <see cref="LinearGradientMode"/> enumeration element that specifies the orientation of the gradient.</param>
	public LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
	{
		_rect = rect;
		_color1 = color1;
		_color2 = color2;
		_wrapMode = WrapMode.Tile;
		_transform = new Matrix();

		switch (linearGradientMode)
		{
			case LinearGradientMode.Horizontal:
				_point1 = new PointF(rect.X, rect.Y);
				_point2 = new PointF(rect.Right, rect.Y);
				break;
			case LinearGradientMode.Vertical:
				_point1 = new PointF(rect.X, rect.Y);
				_point2 = new PointF(rect.X, rect.Bottom);
				break;
			case LinearGradientMode.ForwardDiagonal:
				_point1 = new PointF(rect.X, rect.Y);
				_point2 = new PointF(rect.Right, rect.Bottom);
				break;
			case LinearGradientMode.BackwardDiagonal:
				_point1 = new PointF(rect.Right, rect.Y);
				_point2 = new PointF(rect.X, rect.Bottom);
				break;
			default:
				_point1 = new PointF(rect.X, rect.Y);
				_point2 = new PointF(rect.Right, rect.Y);
				break;
		}
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class based on a rectangle, starting and ending colors, and an angle.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure that specifies the bounds of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color for the gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color for the gradient.</param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line.</param>
	public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle)
		: this(rect, color1, color2, angle, false) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="LinearGradientBrush"/> class based on a rectangle, starting and ending colors, an angle, and whether the angle is scaleable.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure that specifies the bounds of the linear gradient.</param>
	/// <param name="color1">A <see cref="Color"/> structure that represents the starting color for the gradient.</param>
	/// <param name="color2">A <see cref="Color"/> structure that represents the ending color for the gradient.</param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line.</param>
	/// <param name="isAngleScaleable"><see langword="true"/> to specify that the angle is affected by the transform associated with this <see cref="LinearGradientBrush"/>; otherwise, <see langword="false"/>.</param>
	public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
	{
		_rect = rect;
		_color1 = color1;
		_color2 = color2;
		_wrapMode = WrapMode.Tile;
		_transform = new Matrix();

		float cx = rect.X + rect.Width / 2f;
		float cy = rect.Y + rect.Height / 2f;
		double rad = angle * Math.PI / 180.0;
		float halfExtent = (float)(Math.Abs(Math.Cos(rad)) * rect.Width + Math.Abs(Math.Sin(rad)) * rect.Height) / 2f;
		float dx = (float)Math.Cos(rad) * halfExtent;
		float dy = (float)Math.Sin(rad) * halfExtent;

		_point1 = new PointF(cx - dx, cy - dy);
		_point2 = new PointF(cx + dx, cy + dy);
	}

	/// <summary>
	///  Gets or sets a <see cref="Drawing2D.Blend"/> that specifies positions and factors that define a custom falloff for the gradient.
	/// </summary>
	/// <value>A <see cref="Drawing2D.Blend"/> that represents a custom falloff for the gradient.</value>
	public Blend? Blend
	{
		get { ThrowIfDisposed(); return _blend; }
		set { ThrowIfDisposed(); _blend = value; }
	}

	/// <summary>
	///  Gets or sets a value indicating whether gamma correction is enabled for this <see cref="LinearGradientBrush"/>.
	/// </summary>
	/// <value><see langword="true"/> if gamma correction is enabled; otherwise, <see langword="false"/>.</value>
	public bool GammaCorrection
	{
		get { ThrowIfDisposed(); return _gammaCorrection; }
		set { ThrowIfDisposed(); _gammaCorrection = value; }
	}

	/// <summary>
	///  Gets or sets a <see cref="ColorBlend"/> that defines a multicolor linear gradient.
	/// </summary>
	/// <value>A <see cref="ColorBlend"/> that defines a multicolor linear gradient.</value>
	public ColorBlend InterpolationColors
	{
		get { ThrowIfDisposed(); return _interpolationColors ?? new ColorBlend(); }
		set { ThrowIfDisposed(); _interpolationColors = value ?? throw new ArgumentNullException(nameof(value)); }
	}

	/// <summary>
	///  Gets or sets the starting and ending colors of the gradient.
	/// </summary>
	/// <value>An array of two <see cref="Color"/> structures that represents the starting and ending colors of the gradient.</value>
	public Color[] LinearColors
	{
		get { ThrowIfDisposed(); return new[] { _color1, _color2 }; }
		set
		{
			ThrowIfDisposed();
			if (value is null || value.Length < 2)
				throw new ArgumentException("Array must contain at least two colors.", nameof(value));
			_color1 = value[0];
			_color2 = value[1];
		}
	}

	/// <summary>
	///  Gets a rectangular region that defines the starting and ending points of the gradient.
	/// </summary>
	/// <value>A <see cref="RectangleF"/> structure that specifies the starting and ending points of the gradient.</value>
	public RectangleF Rectangle
	{
		get { ThrowIfDisposed(); return _rect; }
	}

	/// <summary>
	///  Gets or sets a copy of the <see cref="Matrix"/> that defines a local geometric transform for this <see cref="LinearGradientBrush"/>.
	/// </summary>
	/// <value>A copy of the <see cref="Matrix"/> that defines a geometric transform that applies only to fills drawn with this <see cref="LinearGradientBrush"/>.</value>
	public Matrix Transform
	{
		get { ThrowIfDisposed(); return _transform.Clone(); }
		set { ThrowIfDisposed(); _transform = value ?? throw new ArgumentNullException(nameof(value)); }
	}

	/// <summary>
	///  Gets or sets a <see cref="Drawing2D.WrapMode"/> enumeration that indicates the wrap mode for this <see cref="LinearGradientBrush"/>.
	/// </summary>
	/// <value>A <see cref="Drawing2D.WrapMode"/> that specifies how fills drawn with this <see cref="LinearGradientBrush"/> are tiled.</value>
	public WrapMode WrapMode
	{
		get { ThrowIfDisposed(); return _wrapMode; }
		set { ThrowIfDisposed(); _wrapMode = value; }
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="LinearGradientBrush"/>.
	/// </summary>
	/// <returns>The <see cref="LinearGradientBrush"/> this method creates, cast as an <see cref="object"/>.</returns>
	public override object Clone()
	{
		var clone = new LinearGradientBrush(_point1, _point2, _color1, _color2)
		{
			_rect = _rect,
			_wrapMode = _wrapMode,
			_gammaCorrection = _gammaCorrection,
			_blend = _blend,
			_interpolationColors = _interpolationColors,
			_transform = _transform.Clone(),
		};
		return clone;
	}

	/// <summary>
	///  Multiplies the <see cref="Matrix"/> that represents the local geometric transform of this <see cref="LinearGradientBrush"/> by the specified <see cref="Matrix"/> by prepending the specified <see cref="Matrix"/>.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> by which to multiply the geometric transform.</param>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Multiplies the <see cref="Matrix"/> that represents the local geometric transform of this <see cref="LinearGradientBrush"/> by the specified <see cref="Matrix"/> in the specified order.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> by which to multiply the geometric transform.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> enumeration that specifies the order in which to multiply the two matrices.</param>
	public void MultiplyTransform(Matrix matrix, MatrixOrder order)
	{
		ThrowIfDisposed();
		if (matrix is null) throw new ArgumentNullException(nameof(matrix));
		_transform.Multiply(matrix, order);
	}

	/// <summary>
	///  Resets the <see cref="Transform"/> property to identity.
	/// </summary>
	public void ResetTransform()
	{
		ThrowIfDisposed();
		_transform.Reset();
	}

	/// <summary>
	///  Rotates the local geometric transform by the specified amount. This method prepends the rotation to the transform.
	/// </summary>
	/// <param name="angle">The angle of rotation.</param>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Rotates the local geometric transform by the specified amount in the specified order.
	/// </summary>
	/// <param name="angle">The angle of rotation.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies whether to append or prepend the rotation matrix.</param>
	public void RotateTransform(float angle, MatrixOrder order)
	{
		ThrowIfDisposed();
		_transform.Rotate(angle, order);
	}

	/// <summary>
	///  Scales the local geometric transform by the specified amounts. This method prepends the scaling matrix to the transform.
	/// </summary>
	/// <param name="sx">The amount by which to scale the transform in the x-axis direction.</param>
	/// <param name="sy">The amount by which to scale the transform in the y-axis direction.</param>
	public void ScaleTransform(float sx, float sy)
	{
		ScaleTransform(sx, sy, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Scales the local geometric transform by the specified amounts in the specified order.
	/// </summary>
	/// <param name="sx">The amount by which to scale the transform in the x-axis direction.</param>
	/// <param name="sy">The amount by which to scale the transform in the y-axis direction.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies whether to append or prepend the scaling matrix.</param>
	public void ScaleTransform(float sx, float sy, MatrixOrder order)
	{
		ThrowIfDisposed();
		_transform.Scale(sx, sy, order);
	}

	/// <summary>
	///  Creates a linear gradient with a center color and a linear falloff to a single color on both sides.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the gradient is composed of only the ending color).</param>
	public void SetBlendTriangularShape(float focus)
	{
		SetBlendTriangularShape(focus, 1f);
	}

	/// <summary>
	///  Creates a linear gradient with a center color and a linear falloff to a single color on both sides.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the gradient is composed of only the ending color).</param>
	/// <param name="scale">A value from 0 through 1 that specifies how fast the colors falloff from the <paramref name="focus"/>.</param>
	public void SetBlendTriangularShape(float focus, float scale)
	{
		ThrowIfDisposed();
		var blend = new Blend(3);
		blend.Positions = new[] { 0f, focus, 1f };
		blend.Factors = new[] { 0f, scale, 0f };
		_blend = blend;
	}

	/// <summary>
	///  Creates a gradient falloff based on a bell-shaped curve.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the starting color and ending color are blended equally).</param>
	public void SetSigmaBellShape(float focus)
	{
		SetSigmaBellShape(focus, 1f);
	}

	/// <summary>
	///  Creates a gradient falloff based on a bell-shaped curve.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the gradient is composed of only the ending color).</param>
	/// <param name="scale">A value from 0 through 1 that specifies how fast the colors falloff from the <paramref name="focus"/>.</param>
	public void SetSigmaBellShape(float focus, float scale)
	{
		ThrowIfDisposed();
		// Approximate sigma bell curve with a 7-point blend
		const int points = 7;
		var blend = new Blend(points);
		blend.Positions = new float[points];
		blend.Factors = new float[points];

		for (int i = 0; i < points; i++)
		{
			float pos = (float)i / (points - 1);
			blend.Positions[i] = pos;

			float dist = pos - focus;
			// Sigma approximation using squared falloff
			float factor = (float)Math.Exp(-4.0 * dist * dist);
			blend.Factors[i] = factor * scale;
		}
		_blend = blend;
	}

	/// <summary>
	///  Translates the local geometric transform by the specified dimensions. This method prepends the translation to the transform.
	/// </summary>
	/// <param name="dx">The value of the translation in x.</param>
	/// <param name="dy">The value of the translation in y.</param>
	public void TranslateTransform(float dx, float dy)
	{
		TranslateTransform(dx, dy, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Translates the local geometric transform by the specified dimensions in the specified order.
	/// </summary>
	/// <param name="dx">The value of the translation in x.</param>
	/// <param name="dy">The value of the translation in y.</param>
	/// <param name="order">The order (prepend or append) in which to apply the translation.</param>
	public void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		ThrowIfDisposed();
		_transform.Translate(dx, dy, order);
	}

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations with this linear gradient brush.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with a linear gradient <see cref="SKShader"/>.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();

		var tileMode = SkiaConversions.ToSKShaderTileMode(_wrapMode);
		SKColor[] colors;
		float[]? colorPositions = null;

		if (_interpolationColors != null && _interpolationColors.Colors.Length >= 2)
		{
			colors = new SKColor[_interpolationColors.Colors.Length];
			for (int i = 0; i < _interpolationColors.Colors.Length; i++)
				colors[i] = SkiaConversions.ToSKColor(_interpolationColors.Colors[i]);
			colorPositions = _interpolationColors.Positions;
		}
		else if (_blend != null && _blend.Factors.Length >= 2)
		{
			// Apply blend factors to interpolate between the two colors
			int count = _blend.Factors.Length;
			colors = new SKColor[count];
			colorPositions = _blend.Positions;
			var skColor1 = SkiaConversions.ToSKColor(_color1);
			var skColor2 = SkiaConversions.ToSKColor(_color2);
			for (int i = 0; i < count; i++)
			{
				float t = _blend.Factors[i];
				byte r = (byte)(skColor1.Red + (skColor2.Red - skColor1.Red) * t);
				byte g = (byte)(skColor1.Green + (skColor2.Green - skColor1.Green) * t);
				byte b = (byte)(skColor1.Blue + (skColor2.Blue - skColor1.Blue) * t);
				byte a = (byte)(skColor1.Alpha + (skColor2.Alpha - skColor1.Alpha) * t);
				colors[i] = new SKColor(r, g, b, a);
			}
		}
		else
		{
			colors = new[] { SkiaConversions.ToSKColor(_color1), SkiaConversions.ToSKColor(_color2) };
		}

		var start = new SKPoint(_point1.X + 0.5f, _point1.Y + 0.5f);
		var end = new SKPoint(_point2.X + 0.5f, _point2.Y + 0.5f);

		SKShader shader;
		if (!_transform.IsIdentity)
		{
			var localMatrix = _transform.SKMatrix;
			shader = SKShader.CreateLinearGradient(start, end, colors, colorPositions, tileMode, localMatrix);
		}
		else
		{
			shader = SKShader.CreateLinearGradient(start, end, colors, colorPositions, tileMode);
		}

		return new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Shader = shader,
			IsAntialias = true,
		};
	}

	/// <summary>
	///  Releases the unmanaged resources used by the <see cref="LinearGradientBrush"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_transform?.Dispose();
		}
		base.Dispose(disposing);
	}
}
