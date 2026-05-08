using SkiaSharp;
using System.Drawing.Internal;

namespace System.Drawing.Drawing2D;

/// <summary>
///  Encapsulates a <see cref="Brush"/> object that fills the interior of a <see cref="GraphicsPath"/>
///  object with a gradient. This class cannot be inherited.
/// </summary>
public sealed partial class PathGradientBrush : Brush
{
	/// <summary>Number of line segments used to subdivide a quadratic Bézier curve for boundary approximation.</summary>
	private const int QuadSubdivisions = 4;

	/// <summary>Number of line segments used to subdivide a conic or cubic Bézier curve for boundary approximation.</summary>
	private const int CubicConicSubdivisions = 8;

	private PointF[] _points;
	private RectangleF _rect;
	private Color _centerColor;
	private Color[] _surroundColors;
	private PointF _centerPoint;
	private PointF _focusScales;
	private WrapMode _wrapMode;
	private Matrix _transform;
	private Blend? _blend;
	private ColorBlend? _interpolationColors;

	/// <summary>
	///  Initializes a new instance of the <see cref="PathGradientBrush"/> class with the specified path.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> that defines the area filled by this <see cref="PathGradientBrush"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public PathGradientBrush(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		var bounds = path.SKPath.Bounds;
		_rect = new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
		_centerPoint = new PointF(_rect.X + _rect.Width / 2f, _rect.Y + _rect.Height / 2f);
		// Extract points from the path
		int count = path.SKPath.PointCount;
		_points = new PointF[count > 0 ? count : 1];
		if (count > 0)
		{
			var skPoints = path.SKPath.Points;
			for (int i = 0; i < count; i++)
				_points[i] = new PointF(skPoints[i].X, skPoints[i].Y);
		}
		else
		{
			_points[0] = _centerPoint;
		}
		_centerColor = Color.Black;
		_surroundColors = new[] { Color.White };
		_wrapMode = WrapMode.Clamp;
		_transform = new Matrix();
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="PathGradientBrush"/> class with the specified points.
	/// </summary>
	/// <param name="points">An array of <see cref="PointF"/> structures that represents the points that make up the vertices of the path.</param>
	/// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
	public PathGradientBrush(PointF[] points)
		: this(points, WrapMode.Clamp) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="PathGradientBrush"/> class with the specified points and wrap mode.
	/// </summary>
	/// <param name="points">An array of <see cref="PointF"/> structures that represents the points that make up the vertices of the path.</param>
	/// <param name="wrapMode">A <see cref="WrapMode"/> that specifies how fills drawn with this <see cref="PathGradientBrush"/> are tiled.</param>
	/// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
	public PathGradientBrush(PointF[] points, WrapMode wrapMode)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		_points = (PointF[])points.Clone();
		_wrapMode = wrapMode;
		_transform = new Matrix();
		ComputeBounds();
		_centerColor = Color.Black;
		_surroundColors = new[] { Color.White };
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="PathGradientBrush"/> class with the specified points.
	/// </summary>
	/// <param name="points">An array of <see cref="Point"/> structures that represents the points that make up the vertices of the path.</param>
	/// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
	public PathGradientBrush(Point[] points)
		: this(points, WrapMode.Clamp) { }

	/// <summary>
	///  Initializes a new instance of the <see cref="PathGradientBrush"/> class with the specified points and wrap mode.
	/// </summary>
	/// <param name="points">An array of <see cref="Point"/> structures that represents the points that make up the vertices of the path.</param>
	/// <param name="wrapMode">A <see cref="WrapMode"/> that specifies how fills drawn with this <see cref="PathGradientBrush"/> are tiled.</param>
	/// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
	public PathGradientBrush(Point[] points, WrapMode wrapMode)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		_points = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			_points[i] = new PointF(points[i].X, points[i].Y);
		_wrapMode = wrapMode;
		_transform = new Matrix();
		ComputeBounds();
		_centerColor = Color.Black;
		_surroundColors = new[] { Color.White };
	}

	/// <summary>
	///  Gets or sets a <see cref="Drawing2D.Blend"/> that specifies positions and factors that define a custom falloff for the gradient.
	/// </summary>
	/// <value>A <see cref="Drawing2D.Blend"/> that represents a custom falloff for the gradient.</value>
	public Blend Blend
	{
		get { ThrowIfDisposed(); return _blend ?? new Blend(); }
		set { ThrowIfDisposed(); _blend = value; }
	}

	/// <summary>
	///  Gets or sets the color at the center of the path gradient.
	/// </summary>
	/// <value>A <see cref="Color"/> that represents the color at the center of the path gradient.</value>
	public Color CenterColor
	{
		get { ThrowIfDisposed(); return _centerColor; }
		set { ThrowIfDisposed(); _centerColor = value; }
	}

	/// <summary>
	///  Gets or sets the center point of the path gradient.
	/// </summary>
	/// <value>A <see cref="PointF"/> that represents the center point of the path gradient.</value>
	public PointF CenterPoint
	{
		get { ThrowIfDisposed(); return _centerPoint; }
		set { ThrowIfDisposed(); _centerPoint = value; }
	}

	/// <summary>
	///  Gets or sets the focus point for the gradient falloff.
	/// </summary>
	/// <value>A <see cref="PointF"/> that represents the focus scales for the gradient falloff.</value>
	public PointF FocusScales
	{
		get { ThrowIfDisposed(); return _focusScales; }
		set { ThrowIfDisposed(); _focusScales = value; }
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
	///  Gets a bounding rectangle for this <see cref="PathGradientBrush"/>.
	/// </summary>
	/// <value>A <see cref="RectangleF"/> that represents a rectangular region that bounds the path this <see cref="PathGradientBrush"/> fills.</value>
	public RectangleF Rectangle
	{
		get { ThrowIfDisposed(); return _rect; }
	}

	/// <summary>
	///  Gets or sets an array of colors that correspond to the points in the path this <see cref="PathGradientBrush"/> fills.
	/// </summary>
	/// <value>An array of <see cref="Color"/> structures that represents the colors associated with each point in the path this <see cref="PathGradientBrush"/> fills.</value>
	public Color[] SurroundColors
	{
		get { ThrowIfDisposed(); return (Color[])_surroundColors.Clone(); }
		set { ThrowIfDisposed(); _surroundColors = value ?? throw new ArgumentNullException(nameof(value)); }
	}

	/// <summary>
	///  Gets or sets a copy of the <see cref="Matrix"/> that defines a local geometric transform for this <see cref="PathGradientBrush"/>.
	/// </summary>
	/// <value>A copy of the <see cref="Matrix"/> that defines a geometric transform that applies only to fills drawn with this <see cref="PathGradientBrush"/>.</value>
	public Matrix Transform
	{
		get { ThrowIfDisposed(); return _transform.Clone(); }
		set { ThrowIfDisposed(); _transform = value ?? throw new ArgumentNullException(nameof(value)); }
	}

	/// <summary>
	///  Gets or sets a <see cref="Drawing2D.WrapMode"/> that indicates the wrap mode for this <see cref="PathGradientBrush"/>.
	/// </summary>
	/// <value>A <see cref="Drawing2D.WrapMode"/> that specifies how fills drawn with this <see cref="PathGradientBrush"/> are tiled.</value>
	public WrapMode WrapMode
	{
		get { ThrowIfDisposed(); return _wrapMode; }
		set { ThrowIfDisposed(); _wrapMode = value; }
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="PathGradientBrush"/>.
	/// </summary>
	/// <returns>The <see cref="PathGradientBrush"/> this method creates, cast as an <see cref="object"/>.</returns>
	public override object Clone()
	{
		var clone = new PathGradientBrush(_points, _wrapMode)
		{
			_centerColor = _centerColor,
			_surroundColors = (Color[])_surroundColors.Clone(),
			_centerPoint = _centerPoint,
			_focusScales = _focusScales,
			_blend = _blend,
			_interpolationColors = _interpolationColors,
			_transform = _transform.Clone(),
		};
		return clone;
	}

	/// <summary>
	///  Multiplies the <see cref="Matrix"/> that represents the local geometric transform of this <see cref="PathGradientBrush"/> by the specified <see cref="Matrix"/> by prepending the specified <see cref="Matrix"/>.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> by which to multiply the geometric transform.</param>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Multiplies the <see cref="Matrix"/> that represents the local geometric transform of this <see cref="PathGradientBrush"/> by the specified <see cref="Matrix"/> in the specified order.
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
	/// <param name="angle">The angle (extent) of rotation.</param>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Rotates the local geometric transform by the specified amount in the specified order.
	/// </summary>
	/// <param name="angle">The angle (extent) of rotation.</param>
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
	///  Creates a gradient with a center color and a linear falloff to each surrounding color.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center point to the boundary, the center color will be at its highest intensity.</param>
	public void SetBlendTriangularShape(float focus)
	{
		SetBlendTriangularShape(focus, 1f);
	}

	/// <summary>
	///  Creates a gradient with a center color and a linear falloff to each surrounding color.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center point to the boundary, the center color will be at its highest intensity.</param>
	/// <param name="scale">A value from 0 through 1 that specifies the maximum intensity of the center color that gets blended with the boundary color.</param>
	public void SetBlendTriangularShape(float focus, float scale)
	{
		ThrowIfDisposed();
		var blend = new Blend(3);
		blend.Positions = new[] { 0f, focus, 1f };
		blend.Factors = new[] { 0f, scale, 0f };
		_blend = blend;
	}

	/// <summary>
	///  Creates a gradient brush that changes color starting from the center of the path outward to the path's boundary based on a bell-shaped curve.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center point to the boundary, the center color will be at its highest intensity.</param>
	public void SetSigmaBellShape(float focus)
	{
		SetSigmaBellShape(focus, 1f);
	}

	/// <summary>
	///  Creates a gradient brush that changes color starting from the center of the path outward to the path's boundary based on a bell-shaped curve.
	/// </summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center point to the boundary, the center color will be at its highest intensity.</param>
	/// <param name="scale">A value from 0 through 1 that specifies the maximum intensity of the center color that gets blended with the boundary color.</param>
	public void SetSigmaBellShape(float focus, float scale)
	{
		ThrowIfDisposed();
		const int points = 7;
		var blend = new Blend(points);
		blend.Positions = new float[points];
		blend.Factors = new float[points];

		for (int i = 0; i < points; i++)
		{
			float pos = (float)i / (points - 1);
			blend.Positions[i] = pos;
			float dist = pos - focus;
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
	///  Renders the path gradient directly onto a canvas using a triangle fan with per-vertex colors.
	///  This produces a gradient that follows the path shape, unlike a radial gradient.
	/// </summary>
	internal void FillOnCanvas(SKCanvas canvas, SKPath clipPath, bool antialias)
	{
		ThrowIfDisposed();

		var center = new SKPoint(_centerPoint.X, _centerPoint.Y);
		var skCenterColor = SkiaConversions.ToSKColor(_centerColor);

		// Flatten the path to get boundary points
		var boundaryPoints = new System.Collections.Generic.List<SKPoint>();
		using var iter = clipPath.CreateIterator(false);
		var pts = new SKPoint[4];
		SKPathVerb verb;
		while ((verb = iter.Next(pts)) != SKPathVerb.Done)
		{
			switch (verb)
			{
				case SKPathVerb.Move:
					boundaryPoints.Add(pts[0]);
					break;
				case SKPathVerb.Line:
					boundaryPoints.Add(pts[1]);
					break;
				case SKPathVerb.Quad:
					// Subdivide quadratic into line segments
					for (int s = 1; s <= QuadSubdivisions; s++)
					{
						float t = s / (float)QuadSubdivisions;
						float x = (1 - t) * (1 - t) * pts[0].X + 2 * (1 - t) * t * pts[1].X + t * t * pts[2].X;
						float y = (1 - t) * (1 - t) * pts[0].Y + 2 * (1 - t) * t * pts[1].Y + t * t * pts[2].Y;
						boundaryPoints.Add(new SKPoint(x, y));
					}
					break;
				case SKPathVerb.Conic:
					// Rational quadratic (conic) — subdivide with weight from iterator
					float w = iter.ConicWeight();
					for (int s = 1; s <= CubicConicSubdivisions; s++)
					{
						float t = s / (float)CubicConicSubdivisions;
						float u = 1 - t;
						float denom = u * u + 2 * u * t * w + t * t;
						float cx = (u * u * pts[0].X + 2 * u * t * w * pts[1].X + t * t * pts[2].X) / denom;
						float cy = (u * u * pts[0].Y + 2 * u * t * w * pts[1].Y + t * t * pts[2].Y) / denom;
						boundaryPoints.Add(new SKPoint(cx, cy));
					}
					break;
				case SKPathVerb.Cubic:
					// Subdivide cubic into line segments
					for (int s = 1; s <= CubicConicSubdivisions; s++)
					{
						float t = s / (float)CubicConicSubdivisions;
						float u = 1 - t;
						float x = u * u * u * pts[0].X + 3 * u * u * t * pts[1].X + 3 * u * t * t * pts[2].X + t * t * t * pts[3].X;
						float y = u * u * u * pts[0].Y + 3 * u * u * t * pts[1].Y + 3 * u * t * t * pts[2].Y + t * t * t * pts[3].Y;
						boundaryPoints.Add(new SKPoint(x, y));
					}
					break;
				case SKPathVerb.Close:
					break;
			}
		}

		int n = boundaryPoints.Count;
		if (n < 3) return;

		// Build triangle fan: center + N boundary + close
		var fanPts = new SKPoint[n + 2];
		var fanCols = new SKColor[n + 2];

		fanPts[0] = center;
		fanCols[0] = skCenterColor;

		for (int i = 0; i < n; i++)
		{
			fanPts[i + 1] = boundaryPoints[i];
			fanCols[i + 1] = SkiaConversions.ToSKColor(_surroundColors[i % _surroundColors.Length]);
		}

		// Close the fan
		fanPts[n + 1] = fanPts[1];
		fanCols[n + 1] = fanCols[1];

		using var verts = SKVertices.CreateCopy(SKVertexMode.TriangleFan, fanPts, fanCols);
		using var paint = new SKPaint { IsAntialias = antialias, Color = SKColors.White };

		canvas.Save();
		canvas.ClipPath(clipPath);
		canvas.DrawVertices(verts, SKBlendMode.Modulate, paint);
		canvas.Restore();
	}

	/// <summary>
	///  Creates an <see cref="SKPaint"/> as fallback for cases where direct canvas rendering isn't available.
	///  Uses <see cref="SKShader.CreateRadialGradient(SKPoint, float, SKColor[], SKShaderTileMode)"/> as an approximation.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with a radial gradient <see cref="SKShader"/>.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();

		var tileMode = SkiaConversions.ToSKShaderTileMode(_wrapMode);
		var center = new SKPoint(_centerPoint.X, _centerPoint.Y);

		// Compute radius as the max distance from center to rect edge
		float dx = Math.Max(Math.Abs(_centerPoint.X - _rect.Left), Math.Abs(_centerPoint.X - _rect.Right));
		float dy = Math.Max(Math.Abs(_centerPoint.Y - _rect.Top), Math.Abs(_centerPoint.Y - _rect.Bottom));
		float radius = (float)Math.Sqrt(dx * dx + dy * dy);
		if (radius <= 0) radius = 1f;

		SKColor[] colors;
		float[]? colorPositions = null;
		var surroundColor = _surroundColors.Length > 0 ? _surroundColors[0] : Color.White;

		if (_interpolationColors != null && _interpolationColors.Colors.Length >= 2)
		{
			colors = new SKColor[_interpolationColors.Colors.Length];
			for (int i = 0; i < _interpolationColors.Colors.Length; i++)
				colors[i] = SkiaConversions.ToSKColor(_interpolationColors.Colors[i]);
			colorPositions = _interpolationColors.Positions;
		}
		else
		{
			// Radial gradient goes from center outward
			colors = new[] { SkiaConversions.ToSKColor(_centerColor), SkiaConversions.ToSKColor(surroundColor) };
		}

		SKShader shader;
		if (!_transform.IsIdentity)
		{
			var localMatrix = _transform.SKMatrix;
			shader = SKShader.CreateRadialGradient(center, radius, colors, colorPositions, tileMode, localMatrix);
		}
		else
		{
			shader = SKShader.CreateRadialGradient(center, radius, colors, colorPositions, tileMode);
		}

		return new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Shader = shader,
			IsAntialias = true,
		};
	}

	/// <summary>
	///  Releases the unmanaged resources used by the <see cref="PathGradientBrush"/> and optionally releases the managed resources.
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

	private void ComputeBounds()
	{
		if (_points.Length == 0)
		{
			_rect = RectangleF.Empty;
			_centerPoint = PointF.Empty;
			return;
		}

		float minX = _points[0].X, maxX = _points[0].X;
		float minY = _points[0].Y, maxY = _points[0].Y;
		float sumX = 0, sumY = 0;
		for (int i = 0; i < _points.Length; i++)
		{
			if (_points[i].X < minX) minX = _points[i].X;
			if (_points[i].X > maxX) maxX = _points[i].X;
			if (_points[i].Y < minY) minY = _points[i].Y;
			if (_points[i].Y > maxY) maxY = _points[i].Y;
			sumX += _points[i].X;
			sumY += _points[i].Y;
		}
		_rect = new RectangleF(minX, minY, maxX - minX, maxY - minY);
		_centerPoint = new PointF(sumX / _points.Length, sumY / _points.Length);
	}
}
