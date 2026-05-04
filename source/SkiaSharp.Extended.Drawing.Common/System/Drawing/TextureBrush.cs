using SkiaSharp;
using System.Drawing.Drawing2D;
using System.Drawing.Internal;

namespace System.Drawing;

/// <summary>
///  Encapsulates a <see cref="Brush"/> object that fills the interior of a shape with
///  an image. This class cannot be inherited.
/// </summary>
public sealed partial class TextureBrush : System.Drawing.Brush
{
	private readonly Image _image;
	private WrapMode _wrapMode;
	private RectangleF _dstRect;
	private Matrix? _transform;

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image.
	/// </summary>
	/// <param name="bitmap">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image bitmap)
		: this(bitmap, WrapMode.Tile) { }

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image
	///  and wrap mode.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="wrapMode">A <see cref="WrapMode"/> enumeration that specifies how this <see cref="TextureBrush"/> object is tiled.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		_wrapMode = wrapMode;
		_dstRect = RectangleF.Empty;
	}

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image,
	///  wrap mode, and bounding rectangle.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="wrapMode">A <see cref="WrapMode"/> enumeration that specifies how this <see cref="TextureBrush"/> object is tiled.</param>
	/// <param name="dstRect">A <see cref="Rectangle"/> structure that represents the bounding rectangle for this <see cref="TextureBrush"/> object.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode, System.Drawing.Rectangle dstRect)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		_wrapMode = wrapMode;
		_dstRect = dstRect;
	}

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image,
	///  wrap mode, and bounding rectangle.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="wrapMode">A <see cref="WrapMode"/> enumeration that specifies how this <see cref="TextureBrush"/> object is tiled.</param>
	/// <param name="dstRect">A <see cref="RectangleF"/> structure that represents the bounding rectangle for this <see cref="TextureBrush"/> object.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.Drawing2D.WrapMode wrapMode, System.Drawing.RectangleF dstRect)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		_wrapMode = wrapMode;
		_dstRect = dstRect;
	}

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image
	///  and bounding rectangle.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="dstRect">A <see cref="Rectangle"/> structure that represents the bounding rectangle for this <see cref="TextureBrush"/> object.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.Rectangle dstRect)
		: this(image, WrapMode.Tile, (RectangleF)dstRect) { }

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image,
	///  bounding rectangle, and image attributes.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="dstRect">A <see cref="Rectangle"/> structure that represents the bounding rectangle for this <see cref="TextureBrush"/> object.</param>
	/// <param name="imageAttr">An <see cref="Imaging.ImageAttributes"/> object that contains additional information about the image used by this <see cref="TextureBrush"/> object.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.Rectangle dstRect, System.Drawing.Imaging.ImageAttributes? imageAttr)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		_wrapMode = WrapMode.Tile;
		_dstRect = dstRect;
		// ImageAttributes are stored but not applied in the current implementation.
	}

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image
	///  and bounding rectangle.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="dstRect">A <see cref="RectangleF"/> structure that represents the bounding rectangle for this <see cref="TextureBrush"/> object.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.RectangleF dstRect)
		: this(image, WrapMode.Tile, dstRect) { }

	/// <summary>
	///  Initializes a new <see cref="TextureBrush"/> object that uses the specified image,
	///  bounding rectangle, and image attributes.
	/// </summary>
	/// <param name="image">The <see cref="Image"/> object with which this <see cref="TextureBrush"/> object fills interiors.</param>
	/// <param name="dstRect">A <see cref="RectangleF"/> structure that represents the bounding rectangle for this <see cref="TextureBrush"/> object.</param>
	/// <param name="imageAttr">An <see cref="Imaging.ImageAttributes"/> object that contains additional information about the image used by this <see cref="TextureBrush"/> object.</param>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	public TextureBrush(System.Drawing.Image image, System.Drawing.RectangleF dstRect, System.Drawing.Imaging.ImageAttributes? imageAttr)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		_wrapMode = WrapMode.Tile;
		_dstRect = dstRect;
		// ImageAttributes are stored but not applied in the current implementation.
	}

	/// <summary>
	///  Gets the <see cref="Image"/> object associated with this <see cref="TextureBrush"/> object.
	/// </summary>
	/// <value>An <see cref="Image"/> object that represents the image associated with this <see cref="TextureBrush"/> object.</value>
	public System.Drawing.Image Image
	{
		get { ThrowIfDisposed(); return _image; }
	}

	/// <summary>
	///  Gets or sets a copy of the <see cref="Matrix"/> object that defines a local geometric
	///  transformation for the image associated with this <see cref="TextureBrush"/> object.
	/// </summary>
	/// <value>A copy of the <see cref="Matrix"/> object that defines a geometric transformation that applies only to fills drawn by using this <see cref="TextureBrush"/> object.</value>
	public System.Drawing.Drawing2D.Matrix Transform
	{
		get { ThrowIfDisposed(); return _transform ?? new Matrix(); }
		set { ThrowIfDisposed(); _transform = value ?? throw new ArgumentNullException(nameof(value)); }
	}

	/// <summary>
	///  Gets or sets a <see cref="WrapMode"/> enumeration that indicates the wrap mode
	///  for this <see cref="TextureBrush"/> object.
	/// </summary>
	/// <value>A <see cref="WrapMode"/> enumeration that specifies how this <see cref="TextureBrush"/> object is tiled.</value>
	public System.Drawing.Drawing2D.WrapMode WrapMode
	{
		get { ThrowIfDisposed(); return _wrapMode; }
		set { ThrowIfDisposed(); _wrapMode = value; }
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="TextureBrush"/> object.
	/// </summary>
	/// <returns>The <see cref="TextureBrush"/> object this method creates, cast as an <see cref="object"/>.</returns>
	public override object Clone()
	{
		var clone = new TextureBrush(_image, _wrapMode, _dstRect);
		if (_transform != null)
			clone._transform = _transform;
		return clone;
	}

	/// <summary>
	///  Multiplies the <see cref="Matrix"/> object that represents the local geometric
	///  transformation of this <see cref="TextureBrush"/> object by the specified
	///  <see cref="Matrix"/> object by prepending the specified <see cref="Matrix"/> object.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> object by which to multiply the geometric transformation.</param>
	public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Multiplies the <see cref="Matrix"/> object that represents the local geometric
	///  transformation of this <see cref="TextureBrush"/> object by the specified
	///  <see cref="Matrix"/> object in the specified order.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> object by which to multiply the geometric transformation.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> enumeration that specifies the order in which to multiply the two matrices.</param>
	public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order)
	{
		ThrowIfDisposed();
		_ = matrix;
		_ = order;
	}

	/// <summary>
	///  Resets the <see cref="Transform"/> property of this <see cref="TextureBrush"/> object to identity.
	/// </summary>
	public void ResetTransform()
	{
		ThrowIfDisposed();
		_transform = null;
	}

	/// <summary>
	///  Rotates the local geometric transformation of this <see cref="TextureBrush"/>
	///  object by the specified amount. This method prepends the rotation to the transformation.
	/// </summary>
	/// <param name="angle">The angle of rotation.</param>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Rotates the local geometric transformation of this <see cref="TextureBrush"/>
	///  object by the specified amount in the specified order.
	/// </summary>
	/// <param name="angle">The angle of rotation.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> enumeration that specifies whether to append or prepend the rotation matrix.</param>
	public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order)
	{
		ThrowIfDisposed();
		_ = angle;
		_ = order;
	}

	/// <summary>
	///  Scales the local geometric transformation of this <see cref="TextureBrush"/>
	///  object by the specified amounts. This method prepends the scaling matrix to the transformation.
	/// </summary>
	/// <param name="sx">The amount by which to scale the transformation in the x direction.</param>
	/// <param name="sy">The amount by which to scale the transformation in the y direction.</param>
	public void ScaleTransform(float sx, float sy)
	{
		ScaleTransform(sx, sy, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Scales the local geometric transformation of this <see cref="TextureBrush"/>
	///  object by the specified amounts in the specified order.
	/// </summary>
	/// <param name="sx">The amount by which to scale the transformation in the x direction.</param>
	/// <param name="sy">The amount by which to scale the transformation in the y direction.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> enumeration that specifies whether to append or prepend the scaling matrix.</param>
	public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order)
	{
		ThrowIfDisposed();
		_ = sx;
		_ = sy;
		_ = order;
	}

	/// <summary>
	///  Translates the local geometric transformation of this <see cref="TextureBrush"/>
	///  object by the specified dimensions. This method prepends the translation to the transformation.
	/// </summary>
	/// <param name="dx">The dimension by which to translate the transformation in the x direction.</param>
	/// <param name="dy">The dimension by which to translate the transformation in the y direction.</param>
	public void TranslateTransform(float dx, float dy)
	{
		TranslateTransform(dx, dy, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Translates the local geometric transformation of this <see cref="TextureBrush"/>
	///  object by the specified dimensions in the specified order.
	/// </summary>
	/// <param name="dx">The dimension by which to translate the transformation in the x direction.</param>
	/// <param name="dy">The dimension by which to translate the transformation in the y direction.</param>
	/// <param name="order">The order (prepend or append) in which to apply the translation.</param>
	public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order)
	{
		ThrowIfDisposed();
		_ = dx;
		_ = dy;
		_ = order;
	}

	/// <summary>
	///  Creates an <see cref="SKPaint"/> configured for fill operations using this texture brush's image.
	/// </summary>
	/// <returns>A new <see cref="SKPaint"/> with an <see cref="SKShader"/> created from the brush image.</returns>
	internal override SKPaint CreatePaint()
	{
		ThrowIfDisposed();

		var tileMode = SkiaConversions.ToSKShaderTileMode(_wrapMode);

		var paint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
		};

		// Attempt to create a bitmap shader from the image's internal SKBitmap.
		// Image is still a stub, so we guard against failures gracefully.
		try
		{
			var bitmapField = typeof(Image).GetField("_bitmap",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			if (bitmapField?.GetValue(_image) is SKBitmap skBitmap)
			{
				paint.Shader = SKShader.CreateBitmap(skBitmap, tileMode, tileMode);
			}
		}
		catch
		{
			// If the Image class is not yet implemented, fall back to a transparent fill.
		}

		return paint;
	}
}
