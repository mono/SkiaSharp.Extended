using SkiaSharp;

namespace System.Drawing.Drawing2D;

/// <summary>
///  Encapsulates a 3-by-3 affine matrix that represents a geometric transform,
///  backed by an <see cref="SKMatrix"/>.
/// </summary>
public sealed partial class Matrix : System.MarshalByRefObject, System.IDisposable
{
	private bool _disposed;

	/// <summary>
	///  The backing SkiaSharp matrix.
	/// </summary>
	internal SKMatrix SKMatrix;

	/// <summary>
	///  Initializes a new instance of the <see cref="Matrix"/> class as the identity matrix.
	/// </summary>
	public Matrix()
	{
		SKMatrix = SKMatrix.Identity;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Matrix"/> class to the geometric transform
	///  defined by the specified rectangle and array of points.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that represents the rectangle to be transformed.</param>
	/// <param name="plgpts">An array of three <see cref="Point"/> structures that represents the points of a parallelogram.</param>
	public Matrix(System.Drawing.Rectangle rect, System.Drawing.Point[] plgpts)
	{
		if (plgpts is null) throw new ArgumentNullException(nameof(plgpts));
		if (plgpts.Length != 3) throw new ArgumentException("Array must contain exactly 3 points.", nameof(plgpts));
		var plgptsF = new PointF[3];
		for (int i = 0; i < 3; i++)
			plgptsF[i] = new PointF(plgpts[i].X, plgpts[i].Y);
		InitFromRectAndPoints(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), plgptsF);
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Matrix"/> class to the geometric transform
	///  defined by the specified rectangle and array of points.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure that represents the rectangle to be transformed.</param>
	/// <param name="plgpts">An array of three <see cref="PointF"/> structures that represents the points of a parallelogram.</param>
	public Matrix(System.Drawing.RectangleF rect, System.Drawing.PointF[] plgpts)
	{
		if (plgpts is null) throw new ArgumentNullException(nameof(plgpts));
		if (plgpts.Length != 3) throw new ArgumentException("Array must contain exactly 3 points.", nameof(plgpts));
		InitFromRectAndPoints(rect, plgpts);
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="Matrix"/> class with the specified elements.
	/// </summary>
	/// <param name="m11">The value in the first row and first column (ScaleX).</param>
	/// <param name="m12">The value in the first row and second column (SkewY).</param>
	/// <param name="m21">The value in the second row and first column (SkewX).</param>
	/// <param name="m22">The value in the second row and second column (ScaleY).</param>
	/// <param name="dx">The value in the third row and first column (TransX).</param>
	/// <param name="dy">The value in the third row and second column (TransY).</param>
	public Matrix(float m11, float m12, float m21, float m22, float dx, float dy)
	{
		// GDI+ row-major mapping:
		// m11 → ScaleX, m12 → SkewY, m21 → SkewX, m22 → ScaleY, dx → TransX, dy → TransY
		SKMatrix = new SKMatrix(m11, m21, dx, m12, m22, dy, 0, 0, 1);
	}

	/// <summary>
	///  Gets an array of floating-point values that represents the elements of this <see cref="Matrix"/>.
	/// </summary>
	/// <value>An array of floating-point values {m11, m12, m21, m22, dx, dy}.</value>
	public float[] Elements
	{
		get
		{
			ThrowIfDisposed();
			return new float[]
			{
				SKMatrix.ScaleX,  // m11
				SKMatrix.SkewY,   // m12
				SKMatrix.SkewX,   // m21
				SKMatrix.ScaleY,  // m22
				SKMatrix.TransX,  // dx
				SKMatrix.TransY   // dy
			};
		}
	}

	/// <summary>
	///  Gets a value indicating whether this <see cref="Matrix"/> is the identity matrix.
	/// </summary>
	public bool IsIdentity
	{
		get
		{
			ThrowIfDisposed();
			return SKMatrix.IsIdentity;
		}
	}

	/// <summary>
	///  Gets a value indicating whether this <see cref="Matrix"/> is invertible.
	/// </summary>
	public bool IsInvertible
	{
		get
		{
			ThrowIfDisposed();
			// A matrix is invertible if its determinant is non-zero.
			float det = SKMatrix.ScaleX * SKMatrix.ScaleY - SKMatrix.SkewX * SKMatrix.SkewY;
			return det != 0f;
		}
	}

	/// <summary>
	///  Gets the x translation value (the dx value, or the element in the third row and first column) of this <see cref="Matrix"/>.
	/// </summary>
	public float OffsetX
	{
		get
		{
			ThrowIfDisposed();
			return SKMatrix.TransX;
		}
	}

	/// <summary>
	///  Gets the y translation value (the dy value, or the element in the third row and second column) of this <see cref="Matrix"/>.
	/// </summary>
	public float OffsetY
	{
		get
		{
			ThrowIfDisposed();
			return SKMatrix.TransY;
		}
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="Matrix"/>.
	/// </summary>
	/// <returns>A new <see cref="Matrix"/> that is a copy of this instance.</returns>
	public Matrix Clone()
	{
		ThrowIfDisposed();
		return new Matrix { SKMatrix = SKMatrix };
	}

	/// <summary>
	///  Releases all resources used by this <see cref="Matrix"/>.
	/// </summary>
	public void Dispose()
	{
		_disposed = true;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///  Tests whether the specified object is a <see cref="Matrix"/> and is identical to this <see cref="Matrix"/>.
	/// </summary>
	/// <param name="obj">The object to test.</param>
	/// <returns><see langword="true"/> if <paramref name="obj"/> is the specified <see cref="Matrix"/> identical to this <see cref="Matrix"/>; otherwise, <see langword="false"/>.</returns>
	public override bool Equals(object? obj)
	{
		if (obj is not Matrix other) return false;
		return SKMatrix.ScaleX == other.SKMatrix.ScaleX
			&& SKMatrix.SkewY == other.SKMatrix.SkewY
			&& SKMatrix.SkewX == other.SKMatrix.SkewX
			&& SKMatrix.ScaleY == other.SKMatrix.ScaleY
			&& SKMatrix.TransX == other.SKMatrix.TransX
			&& SKMatrix.TransY == other.SKMatrix.TransY;
	}

	/// <summary>
	///  Returns a hash code for this <see cref="Matrix"/>.
	/// </summary>
	/// <returns>A hash code for this instance.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			hash = hash * 31 + SKMatrix.ScaleX.GetHashCode();
			hash = hash * 31 + SKMatrix.SkewY.GetHashCode();
			hash = hash * 31 + SKMatrix.SkewX.GetHashCode();
			hash = hash * 31 + SKMatrix.ScaleY.GetHashCode();
			hash = hash * 31 + SKMatrix.TransX.GetHashCode();
			hash = hash * 31 + SKMatrix.TransY.GetHashCode();
			return hash;
		}
	}

	/// <summary>
	///  Inverts this <see cref="Matrix"/>, if it is invertible.
	/// </summary>
	/// <exception cref="InvalidOperationException">The matrix is not invertible.</exception>
	public void Invert()
	{
		ThrowIfDisposed();
		if (!SKMatrix.TryInvert(out var inverted))
			throw new InvalidOperationException("Matrix is not invertible.");
		SKMatrix = inverted;
	}

	/// <summary>
	///  Multiplies this <see cref="Matrix"/> by the specified <see cref="Matrix"/> by prepending the specified <see cref="Matrix"/>.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> by which this <see cref="Matrix"/> is to be multiplied.</param>
	public void Multiply(Matrix matrix)
	{
		Multiply(matrix, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Multiplies this <see cref="Matrix"/> by the matrix specified in the <paramref name="matrix"/> parameter,
	///  in the order specified by the <paramref name="order"/> parameter.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> by which this <see cref="Matrix"/> is to be multiplied.</param>
	/// <param name="order">The <see cref="MatrixOrder"/> that represents the order of the multiplication.</param>
	public void Multiply(Matrix matrix, MatrixOrder order)
	{
		ThrowIfDisposed();
		if (matrix is null) throw new ArgumentNullException(nameof(matrix));
		if (order == MatrixOrder.Prepend)
			SKMatrix = SKMatrix.PreConcat(matrix.SKMatrix);
		else
			SKMatrix = SKMatrix.PostConcat(matrix.SKMatrix);
	}

	/// <summary>
	///  Resets this <see cref="Matrix"/> to have the elements of the identity matrix.
	/// </summary>
	public void Reset()
	{
		ThrowIfDisposed();
		SKMatrix = SKMatrix.Identity;
	}

	/// <summary>
	///  Prepends a clockwise rotation of the specified angle to this <see cref="Matrix"/>.
	/// </summary>
	/// <param name="angle">The angle of the rotation, in degrees.</param>
	public void Rotate(float angle)
	{
		Rotate(angle, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Applies a clockwise rotation of the specified angle to this <see cref="Matrix"/> in the specified order.
	/// </summary>
	/// <param name="angle">The angle of the rotation, in degrees.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies the order of the operation.</param>
	public void Rotate(float angle, MatrixOrder order)
	{
		ThrowIfDisposed();
		var rotation = SKMatrix.CreateRotationDegrees(angle);
		if (order == MatrixOrder.Prepend)
			SKMatrix = SKMatrix.PreConcat(rotation);
		else
			SKMatrix = SKMatrix.PostConcat(rotation);
	}

	/// <summary>
	///  Applies a clockwise rotation about the specified point to this <see cref="Matrix"/> by prepending the rotation.
	/// </summary>
	/// <param name="angle">The angle of the rotation, in degrees.</param>
	/// <param name="point">A <see cref="PointF"/> that represents the center of the rotation.</param>
	public void RotateAt(float angle, PointF point)
	{
		RotateAt(angle, point, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Applies a clockwise rotation about the specified point to this <see cref="Matrix"/> in the specified order.
	/// </summary>
	/// <param name="angle">The angle of the rotation, in degrees.</param>
	/// <param name="point">A <see cref="PointF"/> that represents the center of the rotation.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies the order of the operation.</param>
	public void RotateAt(float angle, PointF point, MatrixOrder order)
	{
		ThrowIfDisposed();
		var rotation = SKMatrix.CreateRotationDegrees(angle, point.X, point.Y);
		if (order == MatrixOrder.Prepend)
			SKMatrix = SKMatrix.PreConcat(rotation);
		else
			SKMatrix = SKMatrix.PostConcat(rotation);
	}

	/// <summary>
	///  Applies the specified scale vector to this <see cref="Matrix"/> by prepending the scale vector.
	/// </summary>
	/// <param name="scaleX">The value by which to scale this <see cref="Matrix"/> in the x-axis direction.</param>
	/// <param name="scaleY">The value by which to scale this <see cref="Matrix"/> in the y-axis direction.</param>
	public void Scale(float scaleX, float scaleY)
	{
		Scale(scaleX, scaleY, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Applies the specified scale vector to this <see cref="Matrix"/> using the specified order.
	/// </summary>
	/// <param name="scaleX">The value by which to scale this <see cref="Matrix"/> in the x-axis direction.</param>
	/// <param name="scaleY">The value by which to scale this <see cref="Matrix"/> in the y-axis direction.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies the order of the operation.</param>
	public void Scale(float scaleX, float scaleY, MatrixOrder order)
	{
		ThrowIfDisposed();
		var scale = SKMatrix.CreateScale(scaleX, scaleY);
		if (order == MatrixOrder.Prepend)
			SKMatrix = SKMatrix.PreConcat(scale);
		else
			SKMatrix = SKMatrix.PostConcat(scale);
	}

	/// <summary>
	///  Applies the specified shear vector to this <see cref="Matrix"/> by prepending the shear transformation.
	/// </summary>
	/// <param name="shearX">The horizontal shear factor.</param>
	/// <param name="shearY">The vertical shear factor.</param>
	public void Shear(float shearX, float shearY)
	{
		Shear(shearX, shearY, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Applies the specified shear vector to this <see cref="Matrix"/> in the specified order.
	/// </summary>
	/// <param name="shearX">The horizontal shear factor.</param>
	/// <param name="shearY">The vertical shear factor.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies the order of the operation.</param>
	public void Shear(float shearX, float shearY, MatrixOrder order)
	{
		ThrowIfDisposed();
		var skew = SKMatrix.CreateSkew(shearX, shearY);
		if (order == MatrixOrder.Prepend)
			SKMatrix = SKMatrix.PreConcat(skew);
		else
			SKMatrix = SKMatrix.PostConcat(skew);
	}

	/// <summary>
	///  Applies the geometric transform represented by this <see cref="Matrix"/> to a specified array of points.
	/// </summary>
	/// <param name="pts">An array of <see cref="PointF"/> structures that represents the points to transform.</param>
	public void TransformPoints(PointF[] pts)
	{
		ThrowIfDisposed();
		if (pts is null) throw new ArgumentNullException(nameof(pts));
		var skPoints = new SKPoint[pts.Length];
		for (int i = 0; i < pts.Length; i++)
			skPoints[i] = new SKPoint(pts[i].X, pts[i].Y);
		var mapped = SKMatrix.MapPoints(skPoints);
		for (int i = 0; i < pts.Length; i++)
			pts[i] = new PointF(mapped[i].X, mapped[i].Y);
	}

	/// <summary>
	///  Applies the geometric transform represented by this <see cref="Matrix"/> to a specified array of points.
	/// </summary>
	/// <param name="pts">An array of <see cref="Point"/> structures that represents the points to transform.</param>
	public void TransformPoints(Point[] pts)
	{
		ThrowIfDisposed();
		if (pts is null) throw new ArgumentNullException(nameof(pts));
		var ptsF = new PointF[pts.Length];
		for (int i = 0; i < pts.Length; i++)
			ptsF[i] = new PointF(pts[i].X, pts[i].Y);
		TransformPoints(ptsF);
		for (int i = 0; i < pts.Length; i++)
			pts[i] = Point.Round(ptsF[i]);
	}

	/// <summary>
	///  Applies only the scale and rotate components of this <see cref="Matrix"/> to the specified array of points.
	/// </summary>
	/// <param name="pts">An array of <see cref="PointF"/> structures that represents the points to transform.</param>
	public void TransformVectors(PointF[] pts)
	{
		ThrowIfDisposed();
		if (pts is null) throw new ArgumentNullException(nameof(pts));
		// Transform without translation: use a copy with TransX/TransY zeroed out.
		var vectorMatrix = new SKMatrix(
			SKMatrix.ScaleX, SKMatrix.SkewX, 0,
			SKMatrix.SkewY, SKMatrix.ScaleY, 0,
			0, 0, 1);
		var skPoints = new SKPoint[pts.Length];
		for (int i = 0; i < pts.Length; i++)
			skPoints[i] = new SKPoint(pts[i].X, pts[i].Y);
		var mapped = vectorMatrix.MapPoints(skPoints);
		for (int i = 0; i < pts.Length; i++)
			pts[i] = new PointF(mapped[i].X, mapped[i].Y);
	}

	/// <summary>
	///  Applies only the scale and rotate components of this <see cref="Matrix"/> to the specified array of points.
	/// </summary>
	/// <param name="pts">An array of <see cref="Point"/> structures that represents the points to transform.</param>
	public void TransformVectors(Point[] pts)
	{
		ThrowIfDisposed();
		if (pts is null) throw new ArgumentNullException(nameof(pts));
		var ptsF = new PointF[pts.Length];
		for (int i = 0; i < pts.Length; i++)
			ptsF[i] = new PointF(pts[i].X, pts[i].Y);
		TransformVectors(ptsF);
		for (int i = 0; i < pts.Length; i++)
			pts[i] = Point.Round(ptsF[i]);
	}

	/// <summary>
	///  Applies the specified translation vector to this <see cref="Matrix"/> by prepending the translation vector.
	/// </summary>
	/// <param name="offsetX">The x value by which to translate this <see cref="Matrix"/>.</param>
	/// <param name="offsetY">The y value by which to translate this <see cref="Matrix"/>.</param>
	public void Translate(float offsetX, float offsetY)
	{
		Translate(offsetX, offsetY, MatrixOrder.Prepend);
	}

	/// <summary>
	///  Applies the specified translation vector to this <see cref="Matrix"/> in the specified order.
	/// </summary>
	/// <param name="offsetX">The x value by which to translate this <see cref="Matrix"/>.</param>
	/// <param name="offsetY">The y value by which to translate this <see cref="Matrix"/>.</param>
	/// <param name="order">A <see cref="MatrixOrder"/> that specifies the order of the operation.</param>
	public void Translate(float offsetX, float offsetY, MatrixOrder order)
	{
		ThrowIfDisposed();
		var translation = SKMatrix.CreateTranslation(offsetX, offsetY);
		if (order == MatrixOrder.Prepend)
			SKMatrix = SKMatrix.PreConcat(translation);
		else
			SKMatrix = SKMatrix.PostConcat(translation);
	}

	/// <summary>
	///  Multiplies each vector in an array by the matrix. The translation elements of
	///  this matrix (third row) are ignored.
	/// </summary>
	/// <param name="pts">An array of <see cref="Point"/> structures that represents the points to transform.</param>
	public void VectorTransformPoints(Point[] pts)
	{
		TransformVectors(pts);
	}

	/// <summary>
	///  Allows a <see cref="Matrix"/> object to attempt to free resources and perform
	///  other cleanup operations before the <see cref="Matrix"/> is reclaimed by garbage collection.
	/// </summary>
	~Matrix()
	{
		Dispose();
	}

	private void InitFromRectAndPoints(RectangleF rect, PointF[] plgpts)
	{
		// Map rect's top-left, top-right, bottom-left to the three parallelogram points.
		float x0 = rect.X, y0 = rect.Y;
		float w = rect.Width, h = rect.Height;

		// Source corners: (x0,y0), (x0+w,y0), (x0,y0+h)
		// Target: plgpts[0], plgpts[1], plgpts[2]
		float m11 = (plgpts[1].X - plgpts[0].X) / w;
		float m12 = (plgpts[1].Y - plgpts[0].Y) / w;
		float m21 = (plgpts[2].X - plgpts[0].X) / h;
		float m22 = (plgpts[2].Y - plgpts[0].Y) / h;
		float dx = plgpts[0].X - m11 * x0 - m21 * y0;
		float dy = plgpts[0].Y - m12 * x0 - m22 * y0;

		SKMatrix = new SKMatrix(m11, m21, dx, m12, m22, dy, 0, 0, 1);
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(Matrix));
	}
}
