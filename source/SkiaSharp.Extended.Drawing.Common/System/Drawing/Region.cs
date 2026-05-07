using SkiaSharp;
using System.Drawing.Drawing2D;

namespace System.Drawing;

/// <summary>
///  Describes the interior of a graphics shape composed of rectangles and paths.
///  This class cannot be inherited.
/// </summary>
public sealed partial class Region : MarshalByRefObject, IDisposable
{
	private SKPath _path;
	private bool _disposed;
	private bool _isInfinite;

	// Large value used to represent an "infinite" region
	private const float InfiniteExtent = 4194304f; // 2^22, matches GDI+

	/// <summary>
	///  Initializes a new <see cref="Region"/> that represents an infinite interior.
	/// </summary>
	public Region()
	{
		_path = new SKPath();
		_isInfinite = true;
		SetInfiniteRect();
	}

	/// <summary>
	///  Initializes a new <see cref="Region"/> from the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">A <see cref="GraphicsPath"/> that defines the new <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public Region(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		_path = new SKPath(path.SKPath);
		_isInfinite = false;
	}

	/// <summary>
	///  Initializes a new <see cref="Region"/> from the specified data.
	/// </summary>
	/// <param name="rgnData">A <see cref="RegionData"/> that defines the interior of the new <see cref="Region"/>.</param>
	/// <exception cref="PlatformNotSupportedException">RegionData deserialization is not supported in this implementation.</exception>
	public Region(RegionData rgnData)
	{
		throw new PlatformNotSupportedException("Region construction from RegionData is not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	/// <summary>
	///  Initializes a new <see cref="Region"/> from the specified <see cref="Rectangle"/> structure.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that defines the interior of the new <see cref="Region"/>.</param>
	public Region(Rectangle rect)
	{
		_path = new SKPath();
		_path.AddRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom));
		_isInfinite = false;
	}

	/// <summary>
	///  Initializes a new <see cref="Region"/> from the specified <see cref="RectangleF"/> structure.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure that defines the interior of the new <see cref="Region"/>.</param>
	public Region(RectangleF rect)
	{
		_path = new SKPath();
		_path.AddRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom));
		_isInfinite = false;
	}

	/// <summary>
	///  Gets the backing <see cref="SKPath"/> for this region. Used internally by <see cref="Graphics"/>.
	/// </summary>
	internal SKPath SKPath => _path;

	/// <summary>
	///  Creates a <see cref="Region"/> from the specified handle to an existing GDI region.
	/// </summary>
	/// <param name="hrgn">A handle to an existing <see cref="Region"/>.</param>
	/// <returns>The new <see cref="Region"/>.</returns>
	/// <exception cref="PlatformNotSupportedException">GDI handles are not supported in this implementation.</exception>
	public static Region FromHrgn(nint hrgn)
	{
		throw new PlatformNotSupportedException("GDI region handles are not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	/// <summary>
	///  Creates an exact copy of this <see cref="Region"/>.
	/// </summary>
	/// <returns>The <see cref="Region"/> that this method creates.</returns>
	public Region Clone()
	{
		ThrowIfDisposed();
		var clone = new Region
		{
			_isInfinite = _isInfinite,
		};
		clone._path.Dispose();
		clone._path = new SKPath(_path);
		return clone;
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain the portion of the specified <see cref="GraphicsPath"/>
	///  that does not intersect with this <see cref="Region"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> to complement this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public void Complement(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		ThrowIfDisposed();
		CombineWithPath(path.SKPath, SKPathOp.ReverseDifference);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain the portion of the specified <see cref="Rectangle"/>
	///  that does not intersect with this <see cref="Region"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to complement this <see cref="Region"/>.</param>
	public void Complement(Rectangle rect)
	{
		Complement((RectangleF)rect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain the portion of the specified <see cref="RectangleF"/>
	///  that does not intersect with this <see cref="Region"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to complement this <see cref="Region"/>.</param>
	public void Complement(RectangleF rect)
	{
		ThrowIfDisposed();
		CombineWithRect(rect, SKPathOp.ReverseDifference);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain the portion of the specified <see cref="Region"/>
	///  that does not intersect with this <see cref="Region"/>.
	/// </summary>
	/// <param name="region">The <see cref="Region"/> to complement this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="region"/> is <see langword="null"/>.</exception>
	public void Complement(Region region)
	{
		if (region is null) throw new ArgumentNullException(nameof(region));
		ThrowIfDisposed();
		CombineWithPath(region._path, SKPathOp.ReverseDifference);
	}

	/// <summary>
	///  Releases all resources used by this <see cref="Region"/>.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///  Tests whether the specified <see cref="Region"/> is identical to this <see cref="Region"/>
	///  on the specified drawing surface.
	/// </summary>
	/// <param name="region">The <see cref="Region"/> to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if the interior of <paramref name="region"/> is identical to the interior of this <see cref="Region"/> when the transformation associated with the <paramref name="g"/> parameter is applied; otherwise, <see langword="false"/>.</returns>
	public bool Equals(Region region, Graphics g)
	{
		ThrowIfDisposed();
		if (region is null) throw new ArgumentNullException(nameof(region));

		// XOR the two paths; if result is empty they are equal
		using var xored = _path.Op(region._path, SKPathOp.Xor);
		return xored == null || xored.IsEmpty;
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain only the portion of its interior that does not
	///  intersect with the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> to exclude from this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public void Exclude(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		ThrowIfDisposed();
		CombineWithPath(path.SKPath, SKPathOp.Difference);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain only the portion of its interior that does not
	///  intersect with the specified <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to exclude from this <see cref="Region"/>.</param>
	public void Exclude(Rectangle rect)
	{
		Exclude((RectangleF)rect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain only the portion of its interior that does not
	///  intersect with the specified <see cref="RectangleF"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to exclude from this <see cref="Region"/>.</param>
	public void Exclude(RectangleF rect)
	{
		ThrowIfDisposed();
		CombineWithRect(rect, SKPathOp.Difference);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to contain only the portion of its interior that does not
	///  intersect with the specified <see cref="Region"/>.
	/// </summary>
	/// <param name="region">The <see cref="Region"/> to exclude from this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="region"/> is <see langword="null"/>.</exception>
	public void Exclude(Region region)
	{
		if (region is null) throw new ArgumentNullException(nameof(region));
		ThrowIfDisposed();
		CombineWithPath(region._path, SKPathOp.Difference);
	}

	/// <summary>
	///  Gets a <see cref="RectangleF"/> structure that represents a rectangle that bounds this
	///  <see cref="Region"/> on the drawing surface of a <see cref="Graphics"/> object.
	/// </summary>
	/// <param name="g">The <see cref="Graphics"/> on which this <see cref="Region"/> is drawn.</param>
	/// <returns>A <see cref="RectangleF"/> structure that represents the bounding rectangle for this <see cref="Region"/> on the specified drawing surface.</returns>
	public RectangleF GetBounds(Graphics g)
	{
		ThrowIfDisposed();
		if (_isInfinite)
			return new RectangleF(-InfiniteExtent, -InfiniteExtent, InfiniteExtent * 2, InfiniteExtent * 2);

		var bounds = _path.Bounds;
		return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
	}

	/// <summary>
	///  Returns a Windows handle to this <see cref="Region"/> in the specified graphics context.
	/// </summary>
	/// <param name="g">The <see cref="Graphics"/> on which this <see cref="Region"/> is drawn.</param>
	/// <returns>A Windows handle to this <see cref="Region"/>.</returns>
	/// <exception cref="PlatformNotSupportedException">GDI handles are not supported in this implementation.</exception>
	public nint GetHrgn(Graphics g)
	{
		throw new PlatformNotSupportedException("GDI region handles are not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	/// <summary>
	///  Returns a <see cref="RegionData"/> that represents the information that describes this <see cref="Region"/>.
	/// </summary>
	/// <returns>A <see cref="RegionData"/> that represents the information that describes this <see cref="Region"/>.</returns>
	/// <exception cref="PlatformNotSupportedException">RegionData serialization is not supported in this implementation.</exception>
	public RegionData? GetRegionData()
	{
		throw new PlatformNotSupportedException("GetRegionData is not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	/// <summary>
	///  Returns an array of <see cref="RectangleF"/> structures that approximate this <see cref="Region"/>
	///  after the specified matrix transformation is applied.
	/// </summary>
	/// <param name="matrix">A <see cref="Matrix"/> that represents a geometric transformation to apply to the region.</param>
	/// <returns>An array of <see cref="RectangleF"/> structures that approximate this <see cref="Region"/>.</returns>
	/// <exception cref="PlatformNotSupportedException">GetRegionScans is not supported in this implementation.</exception>
	public RectangleF[] GetRegionScans(Matrix matrix)
	{
		throw new PlatformNotSupportedException("GetRegionScans is not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the intersection of itself with the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> to intersect with this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public void Intersect(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		ThrowIfDisposed();
		CombineWithPath(path.SKPath, SKPathOp.Intersect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the intersection of itself with the specified <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to intersect with this <see cref="Region"/>.</param>
	public void Intersect(Rectangle rect)
	{
		Intersect((RectangleF)rect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the intersection of itself with the specified <see cref="RectangleF"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to intersect with this <see cref="Region"/>.</param>
	public void Intersect(RectangleF rect)
	{
		ThrowIfDisposed();
		CombineWithRect(rect, SKPathOp.Intersect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the intersection of itself with the specified <see cref="Region"/>.
	/// </summary>
	/// <param name="region">The <see cref="Region"/> to intersect with this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="region"/> is <see langword="null"/>.</exception>
	public void Intersect(Region region)
	{
		if (region is null) throw new ArgumentNullException(nameof(region));
		ThrowIfDisposed();
		CombineWithPath(region._path, SKPathOp.Intersect);
	}

	/// <summary>
	///  Tests whether this <see cref="Region"/> has an empty interior on the specified drawing surface.
	/// </summary>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if the interior of this <see cref="Region"/> is empty; otherwise, <see langword="false"/>.</returns>
	public bool IsEmpty(Graphics g)
	{
		ThrowIfDisposed();
		if (_isInfinite) return false;
		return _path.IsEmpty;
	}

	/// <summary>
	///  Tests whether this <see cref="Region"/> has an infinite interior on the specified drawing surface.
	/// </summary>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if the interior of this <see cref="Region"/> is infinite; otherwise, <see langword="false"/>.</returns>
	public bool IsInfinite(Graphics g)
	{
		ThrowIfDisposed();
		return _isInfinite;
	}

	/// <summary>
	///  Tests whether the specified <see cref="Point"/> is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="point">The <see cref="Point"/> to test.</param>
	/// <returns><see langword="true"/> if <paramref name="point"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(Point point)
	{
		return IsVisible(point.X, point.Y, null);
	}

	/// <summary>
	///  Tests whether the specified <see cref="Point"/> is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="point">The <see cref="Point"/> to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if <paramref name="point"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(Point point, Graphics? g)
	{
		return IsVisible(point.X, point.Y, g);
	}

	/// <summary>
	///  Tests whether the specified <see cref="PointF"/> is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="point">The <see cref="PointF"/> to test.</param>
	/// <returns><see langword="true"/> if <paramref name="point"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(PointF point)
	{
		return IsVisible(point.X, point.Y, null);
	}

	/// <summary>
	///  Tests whether the specified <see cref="PointF"/> is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="point">The <see cref="PointF"/> to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if <paramref name="point"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(PointF point, Graphics? g)
	{
		return IsVisible(point.X, point.Y, g);
	}

	/// <summary>
	///  Tests whether the specified <see cref="Rectangle"/> is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to test.</param>
	/// <returns><see langword="true"/> if any portion of <paramref name="rect"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(Rectangle rect)
	{
		return IsVisible((RectangleF)rect, null);
	}

	/// <summary>
	///  Tests whether the specified <see cref="Rectangle"/> is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if any portion of <paramref name="rect"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(Rectangle rect, Graphics? g)
	{
		return IsVisible((RectangleF)rect, g);
	}

	/// <summary>
	///  Tests whether the specified <see cref="RectangleF"/> is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to test.</param>
	/// <returns><see langword="true"/> if any portion of <paramref name="rect"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(RectangleF rect)
	{
		return IsVisible(rect, null);
	}

	/// <summary>
	///  Tests whether the specified <see cref="RectangleF"/> is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if any portion of <paramref name="rect"/> is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(RectangleF rect, Graphics? g)
	{
		ThrowIfDisposed();
		if (_isInfinite) return true;
		var skRect = new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom);
		using var rectPath = new SKPath();
		rectPath.AddRect(skRect);
		using var intersection = _path.Op(rectPath, SKPathOp.Intersect);
		return intersection != null && !intersection.IsEmpty;
	}

	/// <summary>
	///  Tests whether the specified point is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the point to test.</param>
	/// <param name="y">The y-coordinate of the point to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if the specified point is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(int x, int y, Graphics? g)
	{
		return IsVisible((float)x, (float)y, g);
	}

	/// <summary>
	///  Tests whether the specified rectangle is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="width">The width of the rectangle to test.</param>
	/// <param name="height">The height of the rectangle to test.</param>
	/// <returns><see langword="true"/> if any portion of the specified rectangle is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(int x, int y, int width, int height)
	{
		return IsVisible(new RectangleF(x, y, width, height), null);
	}

	/// <summary>
	///  Tests whether the specified rectangle is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="width">The width of the rectangle to test.</param>
	/// <param name="height">The height of the rectangle to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if any portion of the specified rectangle is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(int x, int y, int width, int height, Graphics? g)
	{
		return IsVisible(new RectangleF(x, y, width, height), g);
	}

	/// <summary>
	///  Tests whether the specified point is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the point to test.</param>
	/// <param name="y">The y-coordinate of the point to test.</param>
	/// <returns><see langword="true"/> if the specified point is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(float x, float y)
	{
		return IsVisible(x, y, null);
	}

	/// <summary>
	///  Tests whether the specified point is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the point to test.</param>
	/// <param name="y">The y-coordinate of the point to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if the specified point is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(float x, float y, Graphics? g)
	{
		ThrowIfDisposed();
		if (_isInfinite) return true;
		return _path.Contains(x, y);
	}

	/// <summary>
	///  Tests whether the specified rectangle is contained within this <see cref="Region"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="width">The width of the rectangle to test.</param>
	/// <param name="height">The height of the rectangle to test.</param>
	/// <returns><see langword="true"/> if any portion of the specified rectangle is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(float x, float y, float width, float height)
	{
		return IsVisible(new RectangleF(x, y, width, height), null);
	}

	/// <summary>
	///  Tests whether the specified rectangle is contained within this <see cref="Region"/>
	///  when drawn on the specified <see cref="Graphics"/>.
	/// </summary>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test.</param>
	/// <param name="width">The width of the rectangle to test.</param>
	/// <param name="height">The height of the rectangle to test.</param>
	/// <param name="g">A <see cref="Graphics"/> that represents a drawing surface.</param>
	/// <returns><see langword="true"/> if any portion of the specified rectangle is contained within this <see cref="Region"/>; otherwise, <see langword="false"/>.</returns>
	public bool IsVisible(float x, float y, float width, float height, Graphics? g)
	{
		return IsVisible(new RectangleF(x, y, width, height), g);
	}

	/// <summary>
	///  Initializes this <see cref="Region"/> to an empty interior.
	/// </summary>
	public void MakeEmpty()
	{
		ThrowIfDisposed();
		_path.Dispose();
		_path = new SKPath();
		_isInfinite = false;
	}

	/// <summary>
	///  Initializes this <see cref="Region"/> to an infinite interior.
	/// </summary>
	public void MakeInfinite()
	{
		ThrowIfDisposed();
		_path.Dispose();
		_path = new SKPath();
		_isInfinite = true;
		SetInfiniteRect();
	}

	/// <summary>
	///  Releases the handle to the specified GDI region.
	/// </summary>
	/// <param name="regionHandle">The handle to the GDI region.</param>
	/// <exception cref="PlatformNotSupportedException">GDI region handles are not supported in this implementation.</exception>
	public void ReleaseHrgn(nint regionHandle)
	{
		throw new PlatformNotSupportedException("GDI region handles are not supported in SkiaSharp.Extended.Drawing.Common.");
	}

	/// <summary>
	///  Transforms this <see cref="Region"/> by the specified <see cref="Matrix"/>.
	/// </summary>
	/// <param name="matrix">The <see cref="Matrix"/> by which to transform this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="matrix"/> is <see langword="null"/>.</exception>
	public void Transform(Matrix matrix)
	{
		ThrowIfDisposed();
		if (matrix is null) throw new ArgumentNullException(nameof(matrix));
		_path.Transform(matrix.SKMatrix);
	}

	/// <summary>
	///  Offsets the coordinates of this <see cref="Region"/> by the specified amount.
	/// </summary>
	/// <param name="dx">The amount to offset this <see cref="Region"/> horizontally.</param>
	/// <param name="dy">The amount to offset this <see cref="Region"/> vertically.</param>
	public void Translate(int dx, int dy)
	{
		Translate((float)dx, (float)dy);
	}

	/// <summary>
	///  Offsets the coordinates of this <see cref="Region"/> by the specified amount.
	/// </summary>
	/// <param name="dx">The amount to offset this <see cref="Region"/> horizontally.</param>
	/// <param name="dy">The amount to offset this <see cref="Region"/> vertically.</param>
	public void Translate(float dx, float dy)
	{
		ThrowIfDisposed();
		_path.Offset(dx, dy);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union of itself and the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> to unite with this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public void Union(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		ThrowIfDisposed();
		CombineWithPath(path.SKPath, SKPathOp.Union);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union of itself and the specified <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to unite with this <see cref="Region"/>.</param>
	public void Union(Rectangle rect)
	{
		Union((RectangleF)rect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union of itself and the specified <see cref="RectangleF"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to unite with this <see cref="Region"/>.</param>
	public void Union(RectangleF rect)
	{
		ThrowIfDisposed();
		CombineWithRect(rect, SKPathOp.Union);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union of itself and the specified <see cref="Region"/>.
	/// </summary>
	/// <param name="region">The <see cref="Region"/> to unite with this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="region"/> is <see langword="null"/>.</exception>
	public void Union(Region region)
	{
		if (region is null) throw new ArgumentNullException(nameof(region));
		ThrowIfDisposed();
		CombineWithPath(region._path, SKPathOp.Union);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union minus the intersection of itself and the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> to Xor with this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	public void Xor(GraphicsPath path)
	{
		if (path is null) throw new ArgumentNullException(nameof(path));
		ThrowIfDisposed();
		CombineWithPath(path.SKPath, SKPathOp.Xor);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union minus the intersection of itself and the specified <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">The <see cref="Rectangle"/> to Xor with this <see cref="Region"/>.</param>
	public void Xor(Rectangle rect)
	{
		Xor((RectangleF)rect);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union minus the intersection of itself and the specified <see cref="RectangleF"/>.
	/// </summary>
	/// <param name="rect">The <see cref="RectangleF"/> to Xor with this <see cref="Region"/>.</param>
	public void Xor(RectangleF rect)
	{
		ThrowIfDisposed();
		CombineWithRect(rect, SKPathOp.Xor);
	}

	/// <summary>
	///  Updates this <see cref="Region"/> to the union minus the intersection of itself and the specified <see cref="Region"/>.
	/// </summary>
	/// <param name="region">The <see cref="Region"/> to Xor with this <see cref="Region"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="region"/> is <see langword="null"/>.</exception>
	public void Xor(Region region)
	{
		if (region is null) throw new ArgumentNullException(nameof(region));
		ThrowIfDisposed();
		CombineWithPath(region._path, SKPathOp.Xor);
	}

	/// <summary>
	///  Allows a <see cref="Region"/> to attempt to free resources and perform other cleanup operations
	///  before the <see cref="Region"/> is reclaimed by garbage collection.
	/// </summary>
	~Region()
	{
		Dispose(false);
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_path?.Dispose();
			}
			_disposed = true;
		}
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(Region));
	}

	private void SetInfiniteRect()
	{
		_path.AddRect(new SKRect(-InfiniteExtent, -InfiniteExtent, InfiniteExtent, InfiniteExtent));
	}

	private void CombineWithRect(RectangleF rect, SKPathOp op)
	{
		using var rectPath = new SKPath();
		rectPath.AddRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom));
		CombineWithPath(rectPath, op);
	}

	private void CombineWithPath(SKPath other, SKPathOp op)
	{
		_isInfinite = false;
		var result = _path.Op(other, op);
		if (result != null)
		{
			_path.Dispose();
			_path = result;
		}
		else
		{
			// Op returned null (e.g., empty result)
			_path.Dispose();
			_path = new SKPath();
		}
	}
}
