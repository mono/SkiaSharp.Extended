using SkiaSharp;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Internal;
using System.Drawing.Text;

namespace System.Drawing;

/// <summary>
///  Encapsulates a GDI+ drawing surface backed by SkiaSharp. This class cannot be inherited.
/// </summary>
public sealed partial class Graphics : MarshalByRefObject, IDeviceContext, IDisposable
{
	private SKCanvas _canvas = null!;
	private SKBitmap? _bitmap;
	private bool _disposed;
	private bool _ownsCanvas = true;
	private float _dpiX = 96f;
	private float _dpiY = 96f;
	private SmoothingMode _smoothingMode = SmoothingMode.Default;
	private InterpolationMode _interpolationMode = InterpolationMode.Default;
	private CompositingMode _compositingMode = CompositingMode.SourceOver;
	private CompositingQuality _compositingQuality = CompositingQuality.Default;
	private TextRenderingHint _textRenderingHint = TextRenderingHint.SystemDefault;
	private PixelOffsetMode _pixelOffsetMode = PixelOffsetMode.Default;
	private GraphicsUnit _pageUnit = GraphicsUnit.Display;
	private float _pageScale = 1f;
	private Point _renderingOrigin = Point.Empty;
	private int _textContrast = 4;
	private int _clipSaveCount;
	private Region? _clipRegion;
	private Collections.Generic.Stack<GraphicsModeState>? _savedStates;

	/// <summary>
	///  Captures all mode fields for Save/Restore.
	/// </summary>
	private sealed class GraphicsModeState
	{
		public SmoothingMode SmoothingMode;
		public InterpolationMode InterpolationMode;
		public CompositingMode CompositingMode;
		public CompositingQuality CompositingQuality;
		public TextRenderingHint TextRenderingHint;
		public PixelOffsetMode PixelOffsetMode;
		public GraphicsUnit PageUnit;
		public float PageScale;
		public Point RenderingOrigin;
		public int TextContrast;
		public int ClipSaveCount;
		public Region? ClipRegion;
	}

	internal Graphics() {}

	/// <summary>
	/// Creates an SKRect with GDI+-compatible half-pixel offset for curve rasterization.
	/// GDI+ integer coordinate methods treat pixel coordinates with a +0.5 offset for curves.
	/// </summary>
	private static SKRect GdiCurveRect(float x, float y, float width, float height)
	{
		return new SKRect(x + 0.5f, y + 0.5f, x + width + 0.5f, y + height + 0.5f);
	}

	/// <summary>
	/// Builds an SKPath for a polygon without any pixel offset (for fills).
	/// </summary>
	private static SKPath BuildPolygonPath(PointF[] points)
	{
		var path = new SKPath();
		path.MoveTo(points[0].X, points[0].Y);
		for (int i = 1; i < points.Length; i++)
			path.LineTo(points[i].X, points[i].Y);
		path.Close();
		return path;
	}

	/// <summary>
	/// Builds an SKPath for a polygon with GDI+-compatible half-pixel offset on vertices.
	/// GDI+ rasterizes polygon edges with a +0.5 pixel center offset.
	/// </summary>
	private static SKPath GdiPolygonPath(PointF[] points)
	{
		var path = new SKPath();
		path.MoveTo(points[0].X + 0.5f, points[0].Y + 0.5f);
		for (int i = 1; i < points.Length; i++)
			path.LineTo(points[i].X + 0.5f, points[i].Y + 0.5f);
		path.Close();
		return path;
	}

	/// <summary>
	///  Represents a method to be called when the DrawImage method has processed a portion of the image.
	/// </summary>
	/// <param name="callbackdata">Internal pointer specifying the data for the callback method.</param>
	/// <returns><see langword="true"/> to abort the operation; otherwise, <see langword="false"/>.</returns>
	public delegate bool DrawImageAbort(System.IntPtr callbackdata);

	/// <summary>
	///  Provides a callback method for the EnumerateMetafile method.
	/// </summary>
	public delegate bool EnumerateMetafileProc(Imaging.EmfPlusRecordType recordType, int flags, int dataSize, System.IntPtr data, Imaging.PlayRecordCallback? callbackData);

	/// <summary>
	///  Gets or sets a <see cref="Region"/> that limits the drawing region of this <see cref="Graphics"/>.
	/// </summary>
	/// <value>A <see cref="Region"/> that limits the portion of this <see cref="Graphics"/> that is currently available for drawing.</value>
	public Region Clip
	{
		get
		{
			ThrowIfDisposed();
			if (_clipRegion != null)
				return (Region)_clipRegion.Clone();
			return new Region();
		}
		set
		{
			ThrowIfDisposed();
			if (value is null) throw new ArgumentNullException(nameof(value));
			// Reset to base clip, then apply the region path
			_canvas.RestoreToCount(_clipSaveCount);
			_clipSaveCount = _canvas.Save();
			if (!value.IsInfinite(this))
			{
				_canvas.ClipPath(value.SKPath);
			}
			_clipRegion = (Region)value.Clone();
		}
	}

	/// <summary>
	///  Gets a <see cref="RectangleF"/> structure that bounds the clipping region of this <see cref="Graphics"/>.
	/// </summary>
	public RectangleF ClipBounds
	{
		get
		{
			ThrowIfDisposed();
			var bounds = _canvas.LocalClipBounds;
			return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
		}
	}

	/// <summary>
	///  Gets or sets a value that specifies how composited images are drawn to this Graphics.
	/// </summary>
	public CompositingMode CompositingMode
	{
		get { ThrowIfDisposed(); return _compositingMode; }
		set { ThrowIfDisposed(); _compositingMode = value; }
	}

	/// <summary>
	///  Gets or sets the rendering quality of composited images drawn to this Graphics.
	/// </summary>
	public CompositingQuality CompositingQuality
	{
		get { ThrowIfDisposed(); return _compositingQuality; }
		set { ThrowIfDisposed(); _compositingQuality = value; }
	}

	/// <summary>
	///  Gets the horizontal resolution of this <see cref="Graphics"/>.
	/// </summary>
	public float DpiX
	{
		get { ThrowIfDisposed(); return _dpiX; }
	}

	/// <summary>
	///  Gets the vertical resolution of this <see cref="Graphics"/>.
	/// </summary>
	public float DpiY
	{
		get { ThrowIfDisposed(); return _dpiY; }
	}

	/// <summary>
	///  Gets or sets the interpolation mode associated with this <see cref="Graphics"/>.
	/// </summary>
	public InterpolationMode InterpolationMode
	{
		get { ThrowIfDisposed(); return _interpolationMode; }
		set { ThrowIfDisposed(); _interpolationMode = value; }
	}

	/// <summary>
	///  Gets a value indicating whether the clipping region of this <see cref="Graphics"/> is empty.
	/// </summary>
	public bool IsClipEmpty
	{
		get
		{
			ThrowIfDisposed();
			var bounds = _canvas.LocalClipBounds;
			return bounds.Width <= 0 || bounds.Height <= 0;
		}
	}

	/// <summary>
	///  Gets a value indicating whether the visible clipping region of this <see cref="Graphics"/> is empty.
	/// </summary>
	public bool IsVisibleClipEmpty
	{
		get
		{
			ThrowIfDisposed();
			var bounds = _canvas.DeviceClipBounds;
			return bounds.Width <= 0 || bounds.Height <= 0;
		}
	}

	/// <summary>
	///  Gets or sets the scaling between world units and page units for this Graphics.
	/// </summary>
	public float PageScale
	{
		get { ThrowIfDisposed(); return _pageScale; }
		set { ThrowIfDisposed(); _pageScale = value; }
	}

	/// <summary>
	///  Gets or sets the unit of measure used for page coordinates in this Graphics.
	/// </summary>
	public GraphicsUnit PageUnit
	{
		get { ThrowIfDisposed(); return _pageUnit; }
		set { ThrowIfDisposed(); _pageUnit = value; }
	}

	/// <summary>
	///  Gets or sets a value specifying how pixels are offset during rendering of this Graphics.
	/// </summary>
	public PixelOffsetMode PixelOffsetMode
	{
		get { ThrowIfDisposed(); return _pixelOffsetMode; }
		set { ThrowIfDisposed(); _pixelOffsetMode = value; }
	}

	/// <summary>
	///  Gets or sets the rendering origin of this Graphics for dithering and for hatch brushes.
	/// </summary>
	public Point RenderingOrigin
	{
		get { ThrowIfDisposed(); return _renderingOrigin; }
		set { ThrowIfDisposed(); _renderingOrigin = value; }
	}

	/// <summary>
	///  Gets or sets the rendering quality for this Graphics.
	/// </summary>
	public SmoothingMode SmoothingMode
	{
		get { ThrowIfDisposed(); return _smoothingMode; }
		set { ThrowIfDisposed(); _smoothingMode = value; }
	}

	/// <summary>
	///  Gets or sets the gamma correction value for rendering text.
	/// </summary>
	public int TextContrast
	{
		get { ThrowIfDisposed(); return _textContrast; }
		set { ThrowIfDisposed(); _textContrast = value; }
	}

	/// <summary>
	///  Gets or sets the rendering mode for text associated with this Graphics.
	/// </summary>
	public TextRenderingHint TextRenderingHint
	{
		get { ThrowIfDisposed(); return _textRenderingHint; }
		set { ThrowIfDisposed(); _textRenderingHint = value; }
	}

	/// <summary>
	///  Gets or sets a copy of the geometric world transformation for this Graphics.
	/// </summary>
	public Matrix Transform
	{
		get
		{
			ThrowIfDisposed();
			return new Matrix { SKMatrix = _canvas.TotalMatrix };
		}
		set
		{
			ThrowIfDisposed();
			if (value is null) throw new ArgumentNullException(nameof(value));
			_canvas.SetMatrix(value.SKMatrix);
		}
	}

	/// <summary>
	///  Gets the bounding rectangle of the visible clipping region of this <see cref="Graphics"/>.
	/// </summary>
	public RectangleF VisibleClipBounds
	{
		get
		{
			ThrowIfDisposed();
			var bounds = _canvas.DeviceClipBounds;
			return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
		}
	}

	/// <summary>
	///  Creates a new Graphics from the specified handle to a device context.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public static Graphics FromHdc(nint hdc) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Creates a new Graphics from the specified handle to a device context and handle to a device.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public static Graphics FromHdc(nint hdc, nint hdevice) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Returns a Graphics for the specified device context.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public static Graphics FromHdcInternal(nint hdc) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Creates a new Graphics from the specified handle to a window.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public static Graphics FromHwnd(nint hwnd) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Creates a new Graphics for the specified Windows handle.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public static Graphics FromHwndInternal(nint hwnd) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Creates a new <see cref="Graphics"/> from the specified <see cref="Image"/>.
	/// </summary>
	/// <param name="image"><see cref="Image"/> from which to create the new <see cref="Graphics"/>.</param>
	/// <returns>This method returns a new <see cref="Graphics"/> for the specified <see cref="Image"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">The backing bitmap of <paramref name="image"/> is <see langword="null"/>.</exception>
	public static Graphics FromImage(Image image)
	{
		if (image is null) throw new ArgumentNullException(nameof(image));
		if (image.SKBitmapBacking is null)
			throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));

		var graphics = new Graphics();
		graphics._bitmap = image.SKBitmapBacking;
		graphics._canvas = new SKCanvas(image.SKBitmapBacking);
		graphics._dpiX = image._horizontalResolution;
		graphics._dpiY = image._verticalResolution;
		graphics._clipSaveCount = graphics._canvas.Save();
		return graphics;
	}

	/// <summary>
	///  Creates a new <see cref="Graphics"/> from the specified <see cref="SKCanvas"/>.
	///  The caller retains ownership of the canvas; it will not be disposed when this Graphics is disposed.
	/// </summary>
	internal static Graphics FromCanvas(SKCanvas canvas)
	{
		return FromCanvas(canvas, ownsClipSave: true);
	}

	/// <summary>
	///  Creates a new <see cref="Graphics"/> from the specified <see cref="SKCanvas"/>.
	///  When <paramref name="ownsClipSave"/> is false, no canvas save is performed for clip management.
	/// </summary>
	internal static Graphics FromCanvas(SKCanvas canvas, bool ownsClipSave)
	{
		if (canvas is null) throw new ArgumentNullException(nameof(canvas));

		var graphics = new Graphics();
		graphics._canvas = canvas;
		graphics._ownsCanvas = false;
		if (ownsClipSave)
		{
			graphics._clipSaveCount = graphics._canvas.Save();
		}
		else
		{
			graphics._clipSaveCount = -1;
		}
		return graphics;
	}

	/// <summary>
	///  Returns a Windows halftone palette.
	/// </summary>
	/// <returns>An internal pointer to the handle of the palette.</returns>
	public static nint GetHalftonePalette() { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Adds a comment to the current Metafile.
	/// </summary>
	/// <param name="data">Array of bytes that contains the comment.</param>
	public void AddMetafileComment(byte[] data) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container.
	/// </summary>
	/// <returns>A <see cref="GraphicsContainer"/> that represents the state of this Graphics.</returns>
	public GraphicsContainer BeginContainer()
	{
		ThrowIfDisposed();
		int count = _canvas.Save();
		return new GraphicsContainer(count);
	}

	/// <summary>
	///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container with the specified scale transformation.
	/// </summary>
	/// <param name="dstrect">A <see cref="Rectangle"/> structure that, together with the <paramref name="srcrect"/> parameter, specifies a scale transformation for the new graphics container.</param>
	/// <param name="srcrect">A <see cref="Rectangle"/> structure that, together with the <paramref name="dstrect"/> parameter, specifies a scale transformation for the new graphics container.</param>
	/// <param name="unit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the unit of measure for the container.</param>
	/// <returns>A <see cref="GraphicsContainer"/> that represents the state of this Graphics.</returns>
	public GraphicsContainer BeginContainer(Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
		=> BeginContainer((RectangleF)dstrect, (RectangleF)srcrect, unit);

	/// <summary>
	///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container with the specified scale transformation.
	/// </summary>
	/// <param name="dstrect">A <see cref="RectangleF"/> structure that, together with the <paramref name="srcrect"/> parameter, specifies a scale transformation for the new graphics container.</param>
	/// <param name="srcrect">A <see cref="RectangleF"/> structure that, together with the <paramref name="dstrect"/> parameter, specifies a scale transformation for the new graphics container.</param>
	/// <param name="unit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the unit of measure for the container.</param>
	/// <returns>A <see cref="GraphicsContainer"/> that represents the state of this Graphics.</returns>
	public GraphicsContainer BeginContainer(RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
	{
		ThrowIfDisposed();
		int count = _canvas.Save();
		// Apply scaling from source to destination rectangle
		if (srcrect.Width != 0 && srcrect.Height != 0)
		{
			_canvas.Translate(dstrect.X, dstrect.Y);
			_canvas.Scale(dstrect.Width / srcrect.Width, dstrect.Height / srcrect.Height);
			_canvas.Translate(-srcrect.X, -srcrect.Y);
		}
		return new GraphicsContainer(count);
	}

	/// <summary>
	///  Clears the entire drawing surface and fills it with the specified background color.
	/// </summary>
	/// <param name="color"><see cref="Color"/> structure that represents the background color of the drawing surface.</param>
	public void Clear(Color color)
	{
		ThrowIfDisposed();
		_canvas.Clear(SkiaConversions.ToSKColor(color));
	}

	/// <summary>
	///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
	/// </summary>
	public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
	/// </summary>
	public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize, CopyPixelOperation copyPixelOperation) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
	/// </summary>
	public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
	/// </summary>
	public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Releases all resources used by this <see cref="Graphics"/>.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///  Draws an arc representing a portion of an ellipse specified by a <see cref="Rectangle"/> structure.
	/// </summary>
	public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		=> DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

	/// <summary>
	///  Draws an arc representing a portion of an ellipse specified by a <see cref="RectangleF"/> structure.
	/// </summary>
	public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		=> DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

	/// <summary>
	///  Draws an arc representing a portion of an ellipse specified by a pair of coordinates, a width, and a height.
	/// </summary>
	public void DrawArc(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		=> DrawArc(pen, (float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);

	/// <summary>
	///  Draws an arc representing a portion of an ellipse specified by a pair of coordinates, a width, and a height.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the arc.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
	/// <param name="width">Width of the rectangle that defines the ellipse.</param>
	/// <param name="height">Height of the rectangle that defines the ellipse.</param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc.</param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle"/> parameter to ending point of the arc.</param>
	public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		var rect = GdiCurveRect(x, y, width, height);
		using var path = new SKPath();
		path.AddArc(rect, startAngle, sweepAngle);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a Bezier spline defined by four Point structures.
	/// </summary>
	/// <param name="pen">The <see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="pt1">A <see cref="Point"/> structure that represents the starting point of the curve.</param>
	/// <param name="pt2">A <see cref="Point"/> structure that represents the first control point of the curve.</param>
	/// <param name="pt3">A <see cref="Point"/> structure that represents the second control point of the curve.</param>
	/// <param name="pt4">A <see cref="Point"/> structure that represents the ending point of the curve.</param>
	public void DrawBezier(Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		=> DrawBezier(pen, (float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y, (float)pt3.X, (float)pt3.Y, (float)pt4.X, (float)pt4.Y);

	/// <summary>
	///  Draws a Bezier spline defined by four PointF structures.
	/// </summary>
	/// <param name="pen">The <see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="pt1">A <see cref="PointF"/> structure that represents the starting point of the curve.</param>
	/// <param name="pt2">A <see cref="PointF"/> structure that represents the first control point of the curve.</param>
	/// <param name="pt3">A <see cref="PointF"/> structure that represents the second control point of the curve.</param>
	/// <param name="pt4">A <see cref="PointF"/> structure that represents the ending point of the curve.</param>
	public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		=> DrawBezier(pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);

	/// <summary>
	///  Draws a Bezier spline defined by four ordered pairs of coordinates.
	/// </summary>
	/// <param name="pen">The <see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="x1">The x-coordinate of the starting point of the curve.</param>
	/// <param name="y1">The y-coordinate of the starting point of the curve.</param>
	/// <param name="x2">The x-coordinate of the first control point of the curve.</param>
	/// <param name="y2">The y-coordinate of the first control point of the curve.</param>
	/// <param name="x3">The x-coordinate of the second control point of the curve.</param>
	/// <param name="y3">The y-coordinate of the second control point of the curve.</param>
	/// <param name="x4">The x-coordinate of the ending point of the curve.</param>
	/// <param name="y4">The y-coordinate of the ending point of the curve.</param>
	public void DrawBezier(Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		using var path = new SKPath();
		path.MoveTo(x1, y1);
		path.CubicTo(x2, y2, x3, y3, x4, y4);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a series of Bézier splines from an array of <see cref="PointF"/> structures.
	/// </summary>
	/// <param name="pen">The <see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">An array of <see cref="PointF"/> structures that represent the points that determine the curve. The number of points in the array should be a multiple of 3 plus 1, such as 4, 7, or 10.</param>
	public void DrawBeziers(Pen pen, PointF[] points)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 4) throw new ArgumentException("Array must contain at least 4 points.", nameof(points));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		using var path = new SKPath();
		path.MoveTo(points[0].X, points[0].Y);
		for (int i = 1; i + 2 < points.Length; i += 3)
			path.CubicTo(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 2].X, points[i + 2].Y);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a series of Bézier splines from an array of <see cref="Point"/> structures.
	/// </summary>
	/// <param name="pen">The <see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">An array of <see cref="Point"/> structures that represent the points that determine the curve.</param>
	public void DrawBeziers(Pen pen, Point[] points)
	{
		ThrowIfDisposed();
		if (points is null) throw new ArgumentNullException(nameof(points));
		var ptsF = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			ptsF[i] = new PointF(points[i].X, points[i].Y);
		DrawBeziers(pen, ptsF);
	}

	/// <summary>
	///  Draws a closed cardinal spline defined by an array of PointF structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	public void DrawClosedCurve(Pen pen, PointF[] points)
		=> DrawClosedCurve(pen, points, 0.5f, Drawing2D.FillMode.Alternate);

	/// <summary>
	///  Draws a closed cardinal spline defined by an array of PointF structures using the specified tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
	/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
	public void DrawClosedCurve(Pen pen, PointF[] points, float tension, FillMode fillmode)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 3) throw new ArgumentException("Array must contain at least 3 points.", nameof(points));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		using var path = BuildClosedCardinalSplinePath(points, tension);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a closed cardinal spline defined by an array of Point structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	public void DrawClosedCurve(Pen pen, Point[] points)
		=> DrawClosedCurve(pen, ToPointFArray(points));

	/// <summary>
	///  Draws a closed cardinal spline defined by an array of Point structures using the specified tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
	/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
	public void DrawClosedCurve(Pen pen, Point[] points, float tension, FillMode fillmode)
		=> DrawClosedCurve(pen, ToPointFArray(points), tension, fillmode);

	/// <summary>
	///  Draws a cardinal spline through a specified array of PointF structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	public void DrawCurve(Pen pen, PointF[] points)
		=> DrawCurve(pen, points, 0, points?.Length - 1 ?? 0, 0.5f);

	/// <summary>
	///  Draws a cardinal spline through a specified array of PointF structures using a specified offset and tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	/// <param name="offset">Offset from the first element in the array to the starting point of the curve.</param>
	/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
	public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments)
		=> DrawCurve(pen, points, offset, numberOfSegments, 0.5f);

	/// <summary>
	///  Draws a cardinal spline through a specified array of PointF structures using a specified offset, number of segments, and tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	/// <param name="offset">Offset from the first element in the array to the starting point of the curve.</param>
	/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
	public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments, float tension)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 2) throw new ArgumentException("Array must contain at least 2 points.", nameof(points));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		using var path = BuildCardinalSplinePath(points, offset, numberOfSegments, tension);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a cardinal spline through a specified array of PointF structures using a specified tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
	public void DrawCurve(Pen pen, PointF[] points, float tension)
		=> DrawCurve(pen, points, 0, points?.Length - 1 ?? 0, tension);

	/// <summary>
	///  Draws a cardinal spline through a specified array of Point structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	public void DrawCurve(Pen pen, Point[] points)
		=> DrawCurve(pen, ToPointFArray(points));

	/// <summary>
	///  Draws a cardinal spline through a specified array of Point structures using a specified offset, number of segments, and tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	/// <param name="offset">Offset from the first element in the array to the starting point of the curve.</param>
	/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
	public void DrawCurve(Pen pen, Point[] points, int offset, int numberOfSegments, float tension)
		=> DrawCurve(pen, ToPointFArray(points), offset, numberOfSegments, tension);

	/// <summary>
	///  Draws a cardinal spline through a specified array of Point structures using a specified tension.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
	public void DrawCurve(Pen pen, Point[] points, float tension)
		=> DrawCurve(pen, ToPointFArray(points), tension);

	/// <summary>
	///  Draws an ellipse specified by a bounding <see cref="Rectangle"/> structure.
	/// </summary>
	public void DrawEllipse(Pen pen, Rectangle rect)
		=> DrawEllipse(pen, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

	/// <summary>
	///  Draws an ellipse defined by a bounding <see cref="RectangleF"/>.
	/// </summary>
	public void DrawEllipse(Pen pen, RectangleF rect)
		=> DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);

	/// <summary>
	///  Draws an ellipse defined by a bounding rectangle specified by coordinates.
	/// </summary>
	public void DrawEllipse(Pen pen, int x, int y, int width, int height)
		=> DrawEllipse(pen, (float)x, (float)y, (float)width, (float)height);

	/// <summary>
	///  Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates, a height, and a width.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the ellipse.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
	public void DrawEllipse(Pen pen, float x, float y, float width, float height)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		_canvas.DrawOval(GdiCurveRect(x, y, width, height), paint);
	}

	/// <summary>
	///  Draws the image represented by the specified Icon within the area specified by a Rectangle structure.
	/// </summary>
	public void DrawIcon(Icon icon, Rectangle targetRect)
	{
		ThrowIfDisposed();
		if (icon is null) throw new ArgumentNullException(nameof(icon));
		using var bmp = icon.ToBitmap();
		DrawImage(bmp, targetRect);
	}
	/// <summary>
	///  Draws the image represented by the specified Icon at the specified coordinates.
	/// </summary>
	public void DrawIcon(Icon icon, int x, int y)
	{
		ThrowIfDisposed();
		if (icon is null) throw new ArgumentNullException(nameof(icon));
		using var bmp = icon.ToBitmap();
		DrawImage(bmp, x, y);
	}
	/// <summary>
	///  Draws the image represented by the specified Icon without scaling the image.
	/// </summary>
	public void DrawIconUnstretched(Icon icon, Rectangle targetRect)
	{
		ThrowIfDisposed();
		if (icon is null) throw new ArgumentNullException(nameof(icon));
		using var bmp = icon.ToBitmap();
		DrawImage(bmp, targetRect.X, targetRect.Y);
	}

	/// <summary>
	///  Draws the specified <see cref="Image"/> at the specified location.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="point"><see cref="Point"/> structure that represents the upper-left corner of the drawn image.</param>
	public void DrawImage(Image image, Point point)
		=> DrawImage(image, (float)point.X, (float)point.Y);

	/// <summary>
	///  Draws the specified <see cref="Image"/> at the specified location.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="point"><see cref="PointF"/> structure that represents the upper-left corner of the drawn image.</param>
	public void DrawImage(Image image, PointF point)
		=> DrawImage(image, point.X, point.Y);

	/// <summary>
	///  Draws the specified Image at the specified location and with the specified shape and size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="destPoints">Array of three <see cref="PointF"/> structures that define a parallelogram.</param>
	public void DrawImage(Image image, PointF[] destPoints)
	{
		ThrowIfDisposed();
		if (image is null) throw new ArgumentNullException(nameof(image));
		if (destPoints is null) throw new ArgumentNullException(nameof(destPoints));
		if (destPoints.Length != 3) throw new ArgumentException("Destination points must contain exactly 3 points.", nameof(destPoints));
		if (image.SKBitmapBacking is null)
			throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
		DrawImageWithParallelogram(image.SKBitmapBacking, null, destPoints);
	}
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
	{
		ThrowIfDisposed();
		if (image is null) throw new ArgumentNullException(nameof(image));
		if (destPoints is null || destPoints.Length != 3) throw new ArgumentException("Destination points must contain exactly 3 points.", nameof(destPoints));
		if (image.SKBitmapBacking is null)
			throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
		var src = new SKRect(srcRect.X, srcRect.Y, srcRect.Right, srcRect.Bottom);
		DrawImageWithParallelogram(image.SKBitmapBacking, src, destPoints);
	}
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr)
		=> DrawImage(image, destPoints, srcRect, srcUnit);
	// TODO: Apply imageAttr to parallelogram-based DrawImage when imageAttr is non-null
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr, DrawImageAbort? callback)
		=> DrawImage(image, destPoints, srcRect, srcUnit);
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr, DrawImageAbort? callback, int callbackData)
		=> DrawImage(image, destPoints, srcRect, srcUnit);

	/// <summary>
	///  Draws the specified Image at the specified location and with the specified shape and size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="destPoints">Array of three <see cref="Point"/> structures that define a parallelogram.</param>
	public void DrawImage(Image image, Point[] destPoints)
	{
		if (destPoints is null) throw new ArgumentNullException(nameof(destPoints));
		var ptsF = new PointF[destPoints.Length];
		for (int i = 0; i < destPoints.Length; i++)
			ptsF[i] = new PointF(destPoints[i].X, destPoints[i].Y);
		DrawImage(image, ptsF);
	}
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
	{
		if (destPoints is null) throw new ArgumentNullException(nameof(destPoints));
		var ptsF = new PointF[destPoints.Length];
		for (int i = 0; i < destPoints.Length; i++)
			ptsF[i] = new PointF(destPoints[i].X, destPoints[i].Y);
		DrawImage(image, ptsF, (RectangleF)srcRect, srcUnit);
	}
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr)
	{
		if (destPoints is null) throw new ArgumentNullException(nameof(destPoints));
		var ptsF = new PointF[destPoints.Length];
		for (int i = 0; i < destPoints.Length; i++)
			ptsF[i] = new PointF(destPoints[i].X, destPoints[i].Y);
		DrawImage(image, ptsF, (RectangleF)srcRect, srcUnit, imageAttr);
	}
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr, DrawImageAbort? callback)
	{
		if (destPoints is null) throw new ArgumentNullException(nameof(destPoints));
		var ptsF = new PointF[destPoints.Length];
		for (int i = 0; i < destPoints.Length; i++)
			ptsF[i] = new PointF(destPoints[i].X, destPoints[i].Y);
		DrawImage(image, ptsF, (RectangleF)srcRect, srcUnit);
	}
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr, DrawImageAbort? callback, int callbackData)
	{
		if (destPoints is null) throw new ArgumentNullException(nameof(destPoints));
		var ptsF = new PointF[destPoints.Length];
		for (int i = 0; i < destPoints.Length; i++)
			ptsF[i] = new PointF(destPoints[i].X, destPoints[i].Y);
		DrawImage(image, ptsF, (RectangleF)srcRect, srcUnit);
	}

	/// <summary>
	///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="rect"><see cref="Rectangle"/> structure that specifies the location and size of the drawn image.</param>
	public void DrawImage(Image image, Rectangle rect)
		=> DrawImage(image, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="destRect">A <see cref="Rectangle"/> structure that specifies the location and size of the drawn image.</param>
	/// <param name="srcRect">A <see cref="Rectangle"/> structure that specifies the portion of the image to draw.</param>
	/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
	public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
		=> DrawImageCore(image, (RectangleF)destRect, (RectangleF)srcRect);

	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight), imageAttr);
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttr, DrawImageAbort? callback)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight), imageAttr);
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttrs, DrawImageAbort? callback, nint callbackData)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight), imageAttrs);
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttrs)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight), imageAttrs);
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttrs, DrawImageAbort? callback)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight), imageAttrs);
	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes? imageAttrs, DrawImageAbort? callback, nint callbackData)
		=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight), imageAttrs);

	/// <summary>
	///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="rect"><see cref="RectangleF"/> structure that specifies the location and size of the drawn image.</param>
	public void DrawImage(Image image, RectangleF rect)
		=> DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);

	/// <summary>
	///  Draws the specified portion of the specified Image at the specified location and with the specified size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="destRect">A <see cref="RectangleF"/> structure that specifies the location and size of the drawn image.</param>
	/// <param name="srcRect">A <see cref="RectangleF"/> structure that specifies the portion of the image to draw.</param>
	/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
	public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
		=> DrawImageCore(image, destRect, srcRect);

	/// <summary>
	///  Draws the specified image, using its original physical size, at the location specified by a coordinate pair.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	public void DrawImage(Image image, int x, int y)
		=> DrawImage(image, (float)x, (float)y);

	/// <summary>
	///  Draws a portion of an image at a specified location.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="srcRect">A <see cref="Rectangle"/> structure that specifies the portion of the image to draw.</param>
	/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
	public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
	{
		DrawImageCore(image, new RectangleF(x, y, srcRect.Width, srcRect.Height), new RectangleF(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height));
	}

	/// <summary>
	///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="width">Width of the drawn image.</param>
	/// <param name="height">Height of the drawn image.</param>
	public void DrawImage(Image image, int x, int y, int width, int height)
		=> DrawImage(image, (float)x, (float)y, (float)width, (float)height);

	/// <summary>
	///  Draws the specified <see cref="Image"/>, using its original physical size, at the specified location.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	public void DrawImage(Image image, float x, float y)
	{
		ThrowIfDisposed();
		if (image is null) throw new ArgumentNullException(nameof(image));
		if (image.SKBitmapBacking is null)
			throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
		_canvas.DrawBitmap(image.SKBitmapBacking, x, y);
	}

	/// <summary>
	///  Draws a portion of an image at a specified location.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="srcRect">A <see cref="RectangleF"/> structure that specifies the portion of the image to draw.</param>
	/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
	public void DrawImage(Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
	{
		DrawImageCore(image, new RectangleF(x, y, srcRect.Width, srcRect.Height), new RectangleF(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height));
	}

	/// <summary>
	///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="width">Width of the drawn image.</param>
	/// <param name="height">Height of the drawn image.</param>
	public void DrawImage(Image image, float x, float y, float width, float height)
	{
		ThrowIfDisposed();
		if (image is null) throw new ArgumentNullException(nameof(image));
		DrawImageCore(image, new RectangleF(x, y, width, height), new RectangleF(0, 0, image.Width, image.Height));
	}

	/// <summary>
	///  Draws the specified image using its original physical size at the location specified by a Point structure.
	/// </summary>
	public void DrawImageUnscaled(Image image, Point point)
		=> DrawImage(image, (float)point.X, (float)point.Y);

	/// <summary>
	///  Draws a specified image using its original physical size at a specified location.
	/// </summary>
	public void DrawImageUnscaled(Image image, Rectangle rect)
		=> DrawImage(image, (float)rect.X, (float)rect.Y);

	/// <summary>
	///  Draws the specified image using its original physical size at the location specified by a coordinate pair.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
	public void DrawImageUnscaled(Image image, int x, int y)
		=> DrawImage(image, (float)x, (float)y);

	/// <summary>
	///  Draws a specified image using its original physical size at a specified location.
	/// </summary>
	public void DrawImageUnscaled(Image image, int x, int y, int width, int height)
		=> DrawImage(image, (float)x, (float)y);

	/// <summary>
	///  Draws the specified image without scaling and clips it, if necessary, to fit in the specified rectangle.
	/// </summary>
	/// <param name="image"><see cref="Image"/> to draw.</param>
	/// <param name="rect">The <see cref="Rectangle"/> in which to draw the image.</param>
	public void DrawImageUnscaledAndClipped(Image image, Rectangle rect)
	{
		ThrowIfDisposed();
		if (image is null) throw new ArgumentNullException(nameof(image));
		if (image.SKBitmapBacking is null)
			throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
		int count = _canvas.Save();
		_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom));
		_canvas.DrawBitmap(image.SKBitmapBacking, rect.X, rect.Y);
		_canvas.RestoreToCount(count);
	}

	/// <summary>
	///  Draws a line connecting two <see cref="Point"/> structures.
	/// </summary>
	public void DrawLine(Pen pen, Point pt1, Point pt2)
		=> DrawLine(pen, (float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y);

	/// <summary>
	///  Draws a line connecting two <see cref="PointF"/> structures.
	/// </summary>
	public void DrawLine(Pen pen, PointF pt1, PointF pt2)
		=> DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);

	/// <summary>
	///  Draws a line connecting the two points specified by the coordinate pairs.
	/// </summary>
	public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
		=> DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);

	/// <summary>
	///  Draws a line connecting the two points specified by the coordinate pairs.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line.</param>
	/// <param name="x1">The x-coordinate of the first point.</param>
	/// <param name="y1">The y-coordinate of the first point.</param>
	/// <param name="x2">The x-coordinate of the second point.</param>
	/// <param name="y2">The y-coordinate of the second point.</param>
	public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		_canvas.DrawLine(x1, y1, x2, y2, paint);
	}

	/// <summary>
	///  Draws a series of line segments that connect an array of <see cref="PointF"/> structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line segments.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that represent the points to connect.</param>
	public void DrawLines(Pen pen, PointF[] points)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 2) throw new ArgumentException(null, nameof(points));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		using var path = new SKPath();
		path.MoveTo(points[0].X, points[0].Y);
		for (int i = 1; i < points.Length; i++)
			path.LineTo(points[i].X, points[i].Y);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a series of line segments that connect an array of <see cref="Point"/> structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line segments.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that represent the points to connect.</param>
	public void DrawLines(Pen pen, Point[] points)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		var ptsF = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			ptsF[i] = new PointF(points[i].X, points[i].Y);
		DrawLines(pen, ptsF);
	}

	/// <summary>
	///  Draws a GraphicsPath.
	/// </summary>
	public void DrawPath(Pen pen, GraphicsPath path)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		if (path is null) throw new ArgumentNullException(nameof(path));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		_canvas.DrawPath(path.SKPath, paint);
	}

	/// <summary>
	///  Draws a pie shape defined by an ellipse specified by a Rectangle structure and two radial lines.
	/// </summary>
	public void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		=> DrawPie(pen, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, startAngle, sweepAngle);

	/// <summary>
	///  Draws a pie shape defined by an ellipse specified by a RectangleF structure and two radial lines.
	/// </summary>
	public void DrawPie(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		=> DrawPie(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

	/// <summary>
	///  Draws a pie shape defined by an ellipse and two radial lines.
	/// </summary>
	public void DrawPie(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		=> DrawPie(pen, (float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);

	/// <summary>
	///  Draws a pie shape defined by an ellipse specified by a coordinate pair, a width, a height, and two radial lines.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the pie shape.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
	/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
	/// <param name="sweepAngle">Angle measured in degrees clockwise from the <paramref name="startAngle"/> parameter to the second side of the pie shape.</param>
	public void DrawPie(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		var oval = GdiCurveRect(x, y, width, height);
		using var path = new SKPath();
		path.MoveTo(oval.MidX, oval.MidY);
		path.ArcTo(oval, startAngle, sweepAngle, false);
		path.Close();
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a polygon defined by an array of <see cref="PointF"/> structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the polygon.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that represent the vertices of the polygon.</param>
	public void DrawPolygon(Pen pen, PointF[] points)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 2) throw new ArgumentException(null, nameof(points));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		using var path = BuildPolygonPath(points);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Draws a polygon defined by an array of <see cref="Point"/> structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the polygon.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that represent the vertices of the polygon.</param>
	public void DrawPolygon(Pen pen, Point[] points)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		var ptsF = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			ptsF[i] = new PointF(points[i].X, points[i].Y);
		DrawPolygon(pen, ptsF);
	}

	/// <summary>
	///  Draws a rectangle specified by a <see cref="Rectangle"/> structure.
	/// </summary>
	public void DrawRectangle(Pen pen, Rectangle rect)
		=> DrawRectangle(pen, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

	/// <summary>
	///  Draws a rectangle specified by a coordinate pair, a width, and a height.
	/// </summary>
	public void DrawRectangle(Pen pen, int x, int y, int width, int height)
		=> DrawRectangle(pen, (float)x, (float)y, (float)width, (float)height);

	/// <summary>
	///  Draws a rectangle specified by a coordinate pair, a width, and a height.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the rectangle.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
	/// <param name="width">The width of the rectangle to draw.</param>
	/// <param name="height">The height of the rectangle to draw.</param>
	public void DrawRectangle(Pen pen, float x, float y, float width, float height)
	{
		ThrowIfDisposed();
		if (pen is null) throw new ArgumentNullException(nameof(pen));
		using var paint = pen.CreatePaint();
		ApplyState(paint);
		_canvas.DrawRect(new SKRect(x, y, x + width, y + height), paint);
	}

	/// <summary>
	///  Draws a series of rectangles specified by <see cref="RectangleF"/> structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the outlines of the rectangles.</param>
	/// <param name="rects">Array of <see cref="RectangleF"/> structures that represent the rectangles to draw.</param>
	public void DrawRectangles(Pen pen, RectangleF[] rects)
	{
		if (rects is null) throw new ArgumentNullException(nameof(rects));
		for (int i = 0; i < rects.Length; i++)
			DrawRectangle(pen, rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);
	}

	/// <summary>
	///  Draws a series of rectangles specified by <see cref="Rectangle"/> structures.
	/// </summary>
	/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the outlines of the rectangles.</param>
	/// <param name="rects">Array of <see cref="Rectangle"/> structures that represent the rectangles to draw.</param>
	public void DrawRectangles(Pen pen, Rectangle[] rects)
	{
		if (rects is null) throw new ArgumentNullException(nameof(rects));
		for (int i = 0; i < rects.Length; i++)
			DrawRectangle(pen, rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);
	}

	/// <summary>
	///  Draws the specified text string at the specified location with the specified <see cref="Brush"/> and <see cref="Font"/> objects.
	/// </summary>
	/// <param name="s">String to draw.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
	/// <param name="point"><see cref="PointF"/> structure that specifies the upper-left corner of the drawn text.</param>
	public void DrawString(string? s, Font font, Brush brush, PointF point)
		=> DrawString(s, font, brush, point.X, point.Y, null);

	/// <summary>
	///  Draws the specified text string at the specified location with the specified <see cref="Brush"/>, <see cref="Font"/>, and <see cref="StringFormat"/> objects.
	/// </summary>
	/// <param name="s">String to draw.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
	/// <param name="point"><see cref="PointF"/> structure that specifies the upper-left corner of the drawn text.</param>
	/// <param name="format"><see cref="StringFormat"/> that specifies formatting attributes applied to the drawn text.</param>
	public void DrawString(string? s, Font font, Brush brush, PointF point, StringFormat? format)
		=> DrawString(s, font, brush, point.X, point.Y, format);

	/// <summary>
	///  Draws the specified text string in the specified rectangle with the specified <see cref="Brush"/> and <see cref="Font"/> objects.
	/// </summary>
	/// <param name="s">String to draw.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
	/// <param name="layoutRectangle"><see cref="RectangleF"/> structure that specifies the location of the drawn text.</param>
	public void DrawString(string? s, Font font, Brush brush, RectangleF layoutRectangle)
		=> DrawString(s, font, brush, layoutRectangle, null);

	/// <summary>
	///  Draws the specified text string in the specified rectangle with the specified <see cref="Brush"/>, <see cref="Font"/>, and <see cref="StringFormat"/> objects.
	/// </summary>
	/// <param name="s">String to draw.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
	/// <param name="layoutRectangle"><see cref="RectangleF"/> structure that specifies the location of the drawn text.</param>
	/// <param name="format"><see cref="StringFormat"/> that specifies formatting attributes applied to the drawn text.</param>
	public void DrawString(string? s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat? format)
	{
		ThrowIfDisposed();
		if (s is null) throw new ArgumentNullException(nameof(s));
		if (font is null) throw new ArgumentNullException(nameof(font));
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (s.Length == 0) return;

		using var paint = brush.CreatePaint();
		ApplyState(paint);

		var skFont = font.SKFont;
		var m = skFont.Metrics;
		float lineHeight = Math.Abs(m.Ascent) + Math.Abs(m.Descent) + Math.Abs(m.Leading);

		bool noWrap = format != null && (format.FormatFlags & StringFormatFlags.NoWrap) != 0;

		// Split text into visual lines (respecting newlines and word wrap)
		var lines = new Collections.Generic.List<string>();
		var textLines = s.Split('\n');
		foreach (var textLine in textLines)
		{
			var line = textLine.TrimEnd('\r');
			if (noWrap || layoutRectangle.Width <= 0)
			{
				lines.Add(line);
			}
			else
			{
				// Word-wrap this line
				int pos = 0;
				while (pos < line.Length)
				{
					int charsConsumed = MeasureLineChars(skFont, line, pos, layoutRectangle.Width);
					if (charsConsumed == 0)
						charsConsumed = 1;
					lines.Add(line.Substring(pos, charsConsumed));
					pos += charsConsumed;
					// Skip trailing spaces at break point
					while (pos < line.Length && line[pos] == ' ')
						pos++;
				}
				if (line.Length == 0)
					lines.Add(string.Empty);
			}
		}

		// Compute vertical start position
		float totalTextHeight = lines.Count * lineHeight;
		float y = layoutRectangle.Y;
		if (format != null && format.LineAlignment != StringAlignment.Near)
		{
			if (format.LineAlignment == StringAlignment.Center)
				y += (layoutRectangle.Height - totalTextHeight) / 2f;
			else if (format.LineAlignment == StringAlignment.Far)
				y += layoutRectangle.Height - totalTextHeight;
		}

		// Clip to the layout rectangle unless NoClip is set
		bool clip = format == null || (format.FormatFlags & StringFormatFlags.NoClip) == 0;
		if (clip && layoutRectangle.Width > 0 && layoutRectangle.Height > 0)
		{
			_canvas.Save();
			_canvas.ClipRect(new SKRect(layoutRectangle.X, layoutRectangle.Y,
				layoutRectangle.X + layoutRectangle.Width, layoutRectangle.Y + layoutRectangle.Height));
		}

		// Draw each line
		foreach (var line in lines)
		{
			float x = layoutRectangle.X;

			// Apply horizontal alignment
			if (format != null && format.Alignment != StringAlignment.Near && line.Length > 0)
			{
				float textWidth = skFont.MeasureText(line, paint);
				if (format.Alignment == StringAlignment.Center)
					x += (layoutRectangle.Width - textWidth) / 2f;
				else if (format.Alignment == StringAlignment.Far)
					x += layoutRectangle.Width - textWidth;
			}

			float baselineY = y - m.Ascent; // ascent is negative
			_canvas.DrawText(line, x, baselineY, skFont, paint);
			y += lineHeight;
		}

		if (clip && layoutRectangle.Width > 0 && layoutRectangle.Height > 0)
			_canvas.Restore();
	}

	/// <summary>
	///  Draws the specified text string at the specified location with the specified <see cref="Brush"/> and <see cref="Font"/> objects.
	/// </summary>
	/// <param name="s">String to draw.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn text.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn text.</param>
	public void DrawString(string? s, Font font, Brush brush, float x, float y)
		=> DrawString(s, font, brush, x, y, null);

	/// <summary>
	///  Draws the specified text string at the specified location with the specified <see cref="Brush"/>, <see cref="Font"/>, and <see cref="StringFormat"/> objects.
	/// </summary>
	/// <param name="s">String to draw.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn text.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn text.</param>
	/// <param name="format"><see cref="StringFormat"/> that specifies formatting attributes applied to the drawn text.</param>
	public void DrawString(string? s, Font font, Brush brush, float x, float y, StringFormat? format)
	{
		ThrowIfDisposed();
		if (s is null) throw new ArgumentNullException(nameof(s));
		if (font is null) throw new ArgumentNullException(nameof(font));
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (s.Length == 0) return;

		using var paint = brush.CreatePaint();
		ApplyState(paint);

		// GDI+ DrawString takes the top-left of the text bounding box; SkiaSharp DrawText takes baseline.
		float baselineY = y - font.SKFont.Metrics.Ascent; // ascent is negative
		_canvas.DrawText(s, x, baselineY, font.SKFont, paint);
	}

	/// <summary>
	///  Closes the current graphics container and restores the state of this Graphics to the state saved by a call to BeginContainer.
	/// </summary>
	/// <param name="container">A <see cref="GraphicsContainer"/> that represents the container this method restores.</param>
	public void EndContainer(GraphicsContainer container)
	{
		ThrowIfDisposed();
		if (container is null) throw new ArgumentNullException(nameof(container));
		_canvas.RestoreToCount(container.SaveCount);
	}
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point destPoint, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point destPoint, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point destPoint, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF destPoint, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, nint callbackData) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Sends the records in the specified Metafile to a callback method for display.
	/// </summary>
	public void EnumerateMetafile(Imaging.Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, nint callbackData, Imaging.ImageAttributes? imageAttr) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Updates the clip region of this <see cref="Graphics"/> to exclude the area specified by a <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that specifies the rectangle to exclude from the clip region.</param>
	public void ExcludeClip(Rectangle rect)
	{
		ThrowIfDisposed();
		_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom), SKClipOperation.Difference);
		_clipRegion?.Exclude(rect);
	}
	/// <summary>
	///  Updates the clip region of this <see cref="Graphics"/> to exclude the area specified by a <see cref="Region"/>.
	/// </summary>
	/// <param name="region">A <see cref="Region"/> that specifies the region to exclude from the clip region.</param>
	public void ExcludeClip(Region region)
	{
		ThrowIfDisposed();
		if (region is null) throw new ArgumentNullException(nameof(region));
		_canvas.ClipPath(region.SKPath, SKClipOperation.Difference);
		_clipRegion?.Exclude(region);
	}

	/// <summary>
	///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	public void FillClosedCurve(Brush brush, PointF[] points)
		=> FillClosedCurve(brush, points, Drawing2D.FillMode.Alternate, 0.5f);

	/// <summary>
	///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures using the specified fill mode.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
	public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode)
		=> FillClosedCurve(brush, points, fillmode, 0.5f);

	/// <summary>
	///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures using the specified fill mode and tension.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
	/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
	public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode, float tension)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 3) throw new ArgumentException("Array must contain at least 3 points.", nameof(points));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		using var path = BuildClosedCardinalSplinePath(points, tension);
		path.FillType = fillmode == Drawing2D.FillMode.Winding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	public void FillClosedCurve(Brush brush, Point[] points)
		=> FillClosedCurve(brush, ToPointFArray(points));

	/// <summary>
	///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures using the specified fill mode.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
	public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode)
		=> FillClosedCurve(brush, ToPointFArray(points), fillmode);

	/// <summary>
	///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures using the specified fill mode and tension.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
	/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
	/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
	public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode, float tension)
		=> FillClosedCurve(brush, ToPointFArray(points), fillmode, tension);

	/// <summary>
	///  Fills the interior of an ellipse defined by a bounding rectangle specified by a <see cref="Rectangle"/> structure.
	/// </summary>
	public void FillEllipse(Brush brush, Rectangle rect)
		=> FillEllipse(brush, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

	/// <summary>
	///  Fills the interior of an ellipse defined by a bounding rectangle specified by a <see cref="RectangleF"/> structure.
	/// </summary>
	public void FillEllipse(Brush brush, RectangleF rect)
		=> FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);

	/// <summary>
	///  Fills the interior of an ellipse defined by a bounding rectangle.
	/// </summary>
	public void FillEllipse(Brush brush, int x, int y, int width, int height)
		=> FillEllipse(brush, (float)x, (float)y, (float)width, (float)height);

	/// <summary>
	///  Fills the interior of an ellipse defined by a bounding rectangle specified by a pair of coordinates, a width, and a height.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
	public void FillEllipse(Brush brush, float x, float y, float width, float height)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		_canvas.DrawOval(GdiCurveRect(x, y, width, height), paint);
	}

	/// <summary>
	///  Fills the interior of a GraphicsPath.
	/// </summary>
	public void FillPath(Brush brush, GraphicsPath path)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (path is null) throw new ArgumentNullException(nameof(path));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		_canvas.DrawPath(path.SKPath, paint);
	}

	/// <summary>
	///  Fills the interior of a pie section defined by an ellipse specified by a Rectangle structure and two radial lines.
	/// </summary>
	public void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		=> FillPie(brush, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, startAngle, sweepAngle);

	/// <summary>
	///  Fills the interior of a pie section defined by an ellipse and two radial lines.
	/// </summary>
	public void FillPie(Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		=> FillPie(brush, (float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);

	/// <summary>
	///  Fills the interior of a pie section defined by an ellipse specified by a pair of coordinates, a width, a height, and two radial lines.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the pie section.</param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle"/> parameter to the second side of the pie section.</param>
	public void FillPie(Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		var oval = new SKRect(x, y, x + width, y + height);
		using var path = new SKPath();
		path.MoveTo(oval.MidX, oval.MidY);
		path.ArcTo(oval, startAngle, sweepAngle, false);
		path.Close();
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Fills the interior of a polygon defined by an array of points specified by <see cref="PointF"/> structures.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that represent the vertices of the polygon to fill.</param>
	public void FillPolygon(Brush brush, PointF[] points)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 2) throw new ArgumentException(null, nameof(points));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		using var path = BuildPolygonPath(points);
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Fills the interior of a polygon defined by an array of points specified by <see cref="PointF"/> structures using the specified fill mode.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="PointF"/> structures that represent the vertices of the polygon to fill.</param>
	/// <param name="fillMode">Member of the <see cref="FillMode"/> enumeration that determines the style of the fill.</param>
	public void FillPolygon(Brush brush, PointF[] points, FillMode fillMode)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (points is null) throw new ArgumentNullException(nameof(points));
		if (points.Length < 2) throw new ArgumentException(null, nameof(points));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		using var path = BuildPolygonPath(points);
		path.FillType = fillMode == FillMode.Winding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
		_canvas.DrawPath(path, paint);
	}

	/// <summary>
	///  Fills the interior of a polygon defined by an array of points specified by <see cref="Point"/> structures.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that represent the vertices of the polygon to fill.</param>
	public void FillPolygon(Brush brush, Point[] points)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		var ptsF = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			ptsF[i] = new PointF(points[i].X, points[i].Y);
		FillPolygon(brush, ptsF);
	}

	/// <summary>
	///  Fills the interior of a polygon defined by an array of points specified by <see cref="Point"/> structures using the specified fill mode.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="points">Array of <see cref="Point"/> structures that represent the vertices of the polygon to fill.</param>
	/// <param name="fillMode">Member of the <see cref="FillMode"/> enumeration that determines the style of the fill.</param>
	public void FillPolygon(Brush brush, Point[] points, FillMode fillMode)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		var ptsF = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			ptsF[i] = new PointF(points[i].X, points[i].Y);
		FillPolygon(brush, ptsF, fillMode);
	}

	/// <summary>
	///  Fills the interior of a rectangle specified by a <see cref="Rectangle"/> structure.
	/// </summary>
	public void FillRectangle(Brush brush, Rectangle rect)
		=> FillRectangle(brush, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

	/// <summary>
	///  Fills the interior of a rectangle specified by a <see cref="RectangleF"/> structure.
	/// </summary>
	public void FillRectangle(Brush brush, RectangleF rect)
		=> FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);

	/// <summary>
	///  Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
	/// </summary>
	public void FillRectangle(Brush brush, int x, int y, int width, int height)
		=> FillRectangle(brush, (float)x, (float)y, (float)width, (float)height);

	/// <summary>
	///  Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill.</param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill.</param>
	/// <param name="width">Width of the rectangle to fill.</param>
	/// <param name="height">Height of the rectangle to fill.</param>
	public void FillRectangle(Brush brush, float x, float y, float width, float height)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		_canvas.DrawRect(new SKRect(x, y, x + width, y + height), paint);
	}

	/// <summary>
	///  Fills the interiors of a series of rectangles specified by <see cref="RectangleF"/> structures.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="rects">Array of <see cref="RectangleF"/> structures that represent the rectangles to fill.</param>
	public void FillRectangles(Brush brush, RectangleF[] rects)
	{
		if (rects is null) throw new ArgumentNullException(nameof(rects));
		for (int i = 0; i < rects.Length; i++)
			FillRectangle(brush, rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);
	}

	/// <summary>
	///  Fills the interiors of a series of rectangles specified by <see cref="Rectangle"/> structures.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="rects">Array of <see cref="Rectangle"/> structures that represent the rectangles to fill.</param>
	public void FillRectangles(Brush brush, Rectangle[] rects)
	{
		if (rects is null) throw new ArgumentNullException(nameof(rects));
		for (int i = 0; i < rects.Length; i++)
			FillRectangle(brush, rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);
	}

	/// <summary>
	///  Fills the interior of a <see cref="Region"/>.
	/// </summary>
	/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
	/// <param name="region"><see cref="Region"/> that represents the area to fill.</param>
	public void FillRegion(Brush brush, Region region)
	{
		ThrowIfDisposed();
		if (brush is null) throw new ArgumentNullException(nameof(brush));
		if (region is null) throw new ArgumentNullException(nameof(region));
		using var paint = brush.CreatePaint();
		ApplyState(paint);
		_canvas.DrawPath(region.SKPath, paint);
	}

	/// <summary>
	///  Forces execution of all pending graphics operations and returns immediately without waiting for the operations to finish.
	/// </summary>
	public void Flush()
	{
		ThrowIfDisposed();
		_canvas.Flush();
	}

	/// <summary>
	///  Forces execution of all pending graphics operations with the method waiting or not waiting, as specified, to return before the operations finish.
	/// </summary>
	/// <param name="intention">Member of the <see cref="FlushIntention"/> enumeration that specifies whether the method returns immediately or waits for any existing operations to finish.</param>
	public void Flush(FlushIntention intention)
	{
		ThrowIfDisposed();
		_canvas.Flush();
	}

	/// <summary>
	///  Gets the cumulative graphics context.
	/// </summary>
	public object GetContextInfo() { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Gets the handle to the device context associated with this Graphics.
	/// </summary>
	public nint GetHdc() { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Gets the nearest color to the specified <see cref="Color"/> structure.
	/// </summary>
	/// <param name="color"><see cref="Color"/> structure for which to find a match.</param>
	/// <returns>A <see cref="Color"/> structure that represents the nearest color to the one specified with the <paramref name="color"/> parameter.</returns>
	public Color GetNearestColor(Color color)
	{
		ThrowIfDisposed();
		return color;
	}

	/// <summary>
	///  Updates the clip region of this <see cref="Graphics"/> to the intersection of the current clip region and the specified <see cref="Rectangle"/> structure.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure to intersect with the current clip region.</param>
	public void IntersectClip(Rectangle rect)
	{
		ThrowIfDisposed();
		_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom), SKClipOperation.Intersect);
		_clipRegion?.Intersect(rect);
	}
	/// <summary>
	///  Updates the clip region of this <see cref="Graphics"/> to the intersection of the current clip region and the specified <see cref="RectangleF"/> structure.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure to intersect with the current clip region.</param>
	public void IntersectClip(RectangleF rect)
	{
		ThrowIfDisposed();
		_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom), SKClipOperation.Intersect);
		_clipRegion?.Intersect(rect);
	}
	/// <summary>
	///  Updates the clip region of this <see cref="Graphics"/> to the intersection of the current clip region and the specified <see cref="Region"/>.
	/// </summary>
	/// <param name="region">A <see cref="Region"/> to intersect with the current region.</param>
	public void IntersectClip(Region region)
	{
		ThrowIfDisposed();
		if (region is null) throw new ArgumentNullException(nameof(region));
		_canvas.ClipPath(region.SKPath, SKClipOperation.Intersect);
		_clipRegion?.Intersect(region);
	}

	/// <summary>
	///  Indicates whether the specified Point structure is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(Point point) => IsVisible((float)point.X, (float)point.Y);
	/// <summary>
	///  Indicates whether the specified PointF structure is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(PointF point) => IsVisible(point.X, point.Y);
	/// <summary>
	///  Indicates whether the rectangle specified by a Rectangle structure is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(Rectangle rect) => IsVisible((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
	/// <summary>
	///  Indicates whether the rectangle specified by a RectangleF structure is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(RectangleF rect) => IsVisible(rect.X, rect.Y, rect.Width, rect.Height);
	/// <summary>
	///  Indicates whether the point specified by a pair of coordinates is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(int x, int y) => IsVisible((float)x, (float)y);
	/// <summary>
	///  Indicates whether the rectangle specified by a pair of coordinates, a width, and a height is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(int x, int y, int width, int height) => IsVisible((float)x, (float)y, (float)width, (float)height);
	/// <summary>
	///  Indicates whether the point specified by a pair of coordinates is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(float x, float y)
	{
		ThrowIfDisposed();
		var clipBounds = _canvas.DeviceClipBounds;
		return x >= clipBounds.Left && x < clipBounds.Right && y >= clipBounds.Top && y < clipBounds.Bottom;
	}
	/// <summary>
	///  Indicates whether the rectangle specified by a pair of coordinates, a width, and a height is contained within the visible clip region of this Graphics.
	/// </summary>
	public bool IsVisible(float x, float y, float width, float height)
	{
		ThrowIfDisposed();
		var clipBounds = _canvas.DeviceClipBounds;
		var testRect = new SKRect(x, y, x + width, y + height);
		var clipRect = new SKRect(clipBounds.Left, clipBounds.Top, clipBounds.Right, clipBounds.Bottom);
		return testRect.IntersectsWith(clipRect);
	}

	/// <summary>
	///  Gets an array of Region objects, each of which bounds a range of character positions within the specified string.
	/// </summary>
	public Region[] MeasureCharacterRanges(string? text, Font font, RectangleF layoutRect, StringFormat? stringFormat)
	{
		ThrowIfDisposed();
		if (font is null) throw new ArgumentNullException(nameof(font));

		var ranges = stringFormat != null ? GetMeasurableRanges(stringFormat) : Array.Empty<CharacterRange>();
		if (ranges.Length == 0 || string.IsNullOrEmpty(text))
			return new Region[] { new Region(new GraphicsPath()) };

		var regions = new Region[ranges.Length];
		using var paint = new SKPaint();
		paint.TextSize = font.SKFont.Size;
		paint.Typeface = font.SKTypeface;

		for (int i = 0; i < ranges.Length; i++)
		{
			var range = ranges[i];
			int start = Math.Max(0, range.First);
			int len = Math.Min(range.Length, text.Length - start);
			if (len <= 0)
			{
				regions[i] = new Region(new RectangleF(layoutRect.X, layoutRect.Y, 0, 0));
				continue;
			}
			// Measure prefix up to start
			float xOffset = 0;
			if (start > 0)
			{
				xOffset = font.SKFont.MeasureText(text.AsSpan(0, start), paint);
			}
			float rangeWidth = font.SKFont.MeasureText(text.AsSpan(start, len), paint);
			var m = font.SKFont.Metrics;
			float rangeHeight = Math.Abs(m.Ascent) + Math.Abs(m.Descent);
			regions[i] = new Region(new RectangleF(layoutRect.X + xOffset, layoutRect.Y, rangeWidth, rangeHeight));
		}
		return regions;
	}

	private static CharacterRange[] GetMeasurableRanges(StringFormat sf)
	{
		// Access the ranges via reflection of the internal field since it's stored privately
		var field = typeof(StringFormat).GetField("_measurableRanges", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return field?.GetValue(sf) as CharacterRange[] ?? Array.Empty<CharacterRange>();
	}
	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/>.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string, in the units specified by the PageUnit property.</returns>
	public SizeF MeasureString(string? text, Font font)
		=> MeasureString(text, font, new SizeF(float.MaxValue, float.MaxValue), null);

	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/>.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="origin"><see cref="PointF"/> structure that represents the upper-left corner of the string. This is currently ignored.</param>
	/// <param name="stringFormat"><see cref="StringFormat"/> that represents formatting information for the string.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
	public SizeF MeasureString(string? text, Font font, PointF origin, StringFormat? stringFormat)
		=> MeasureString(text, font, new SizeF(float.MaxValue, float.MaxValue), stringFormat);

	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/> within the specified layout area.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="layoutArea"><see cref="SizeF"/> structure that specifies the maximum layout area for the text.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
	public SizeF MeasureString(string? text, Font font, SizeF layoutArea)
		=> MeasureString(text, font, layoutArea, null);

	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/> within the specified layout area.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="layoutArea"><see cref="SizeF"/> structure that specifies the maximum layout area for the text.</param>
	/// <param name="stringFormat"><see cref="StringFormat"/> that represents formatting information for the string.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
	public SizeF MeasureString(string? text, Font font, SizeF layoutArea, StringFormat? stringFormat)
	{
		int charactersFitted;
		int linesFilled;
		return MeasureString(text, font, layoutArea, stringFormat, out charactersFitted, out linesFilled);
	}

	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/> within the specified layout area.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="layoutArea"><see cref="SizeF"/> structure that specifies the maximum layout area for the text.</param>
	/// <param name="stringFormat"><see cref="StringFormat"/> that represents formatting information for the string.</param>
	/// <param name="charactersFitted">Number of characters in the string.</param>
	/// <param name="linesFilled">Number of text lines in the string.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
	public SizeF MeasureString(string? text, Font font, SizeF layoutArea, StringFormat? stringFormat, out int charactersFitted, out int linesFilled)
	{
		ThrowIfDisposed();
		charactersFitted = 0;
		linesFilled = 0;

		if (string.IsNullOrEmpty(text))
			return SizeF.Empty;
		if (font is null) throw new ArgumentNullException(nameof(font));

		var skFont = font.SKFont;
		var m = skFont.Metrics;
		float lineHeight = Math.Abs(m.Ascent) + Math.Abs(m.Descent) + Math.Abs(m.Leading);

		// GDI+ MeasureString adds ~1/6 em padding on each side for compatibility
		bool addPadding = stringFormat == null ||
			(stringFormat.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) == 0;
		float emPadding = addPadding ? skFont.Size / 6f : 0f;

		bool noWrap = stringFormat != null &&
			(stringFormat.FormatFlags & StringFormatFlags.NoWrap) != 0;

		float maxWidth = layoutArea.Width;
		if (maxWidth < float.MaxValue && addPadding)
			maxWidth = Math.Max(0, maxWidth - 2 * emPadding);

		if (noWrap || maxWidth >= float.MaxValue)
		{
			// Single-line measurement
			float textWidth = skFont.MeasureText(text);
			charactersFitted = text!.Length;
			linesFilled = 1;
			return new SizeF(textWidth + 2 * emPadding, lineHeight);
		}

		// Multi-line word wrap measurement
		float totalHeight = 0;
		float maxLineWidth = 0;
		int totalChars = 0;
		int lines = 0;
		int pos = 0;

		while (pos < text!.Length)
		{
			// Check if adding another line would exceed the layout height
			if (layoutArea.Height < float.MaxValue && totalHeight + lineHeight > layoutArea.Height + 0.5f)
				break;

			// Find how many characters fit in the available width
			int charsConsumed = MeasureLineChars(skFont, text, pos, maxWidth);
			if (charsConsumed == 0)
				charsConsumed = 1; // At least one character per line to avoid infinite loops

			float lineWidth = skFont.MeasureText(text.Substring(pos, charsConsumed));
			maxLineWidth = Math.Max(maxLineWidth, lineWidth);
			totalHeight += lineHeight;
			totalChars += charsConsumed;
			lines++;
			pos += charsConsumed;

			// Skip newline characters
			if (pos < text.Length && text[pos] == '\n')
				pos++;
			else if (pos < text.Length && text[pos] == '\r')
			{
				pos++;
				if (pos < text.Length && text[pos] == '\n')
					pos++;
			}
		}

		charactersFitted = totalChars;
		linesFilled = lines;
		return new SizeF(maxLineWidth + 2 * emPadding, totalHeight);
	}

	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/> within the specified width.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="width">Maximum width of the string in pixels.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
	public SizeF MeasureString(string? text, Font font, int width)
		=> MeasureString(text, font, new SizeF(width, float.MaxValue), null);

	/// <summary>
	///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/> within the specified width.
	/// </summary>
	/// <param name="text">String to measure.</param>
	/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
	/// <param name="width">Maximum width of the string in pixels.</param>
	/// <param name="format"><see cref="StringFormat"/> that represents formatting information for the string.</param>
	/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
	public SizeF MeasureString(string? text, Font font, int width, StringFormat? format)
		=> MeasureString(text, font, new SizeF(width, float.MaxValue), format);

	/// <summary>
	///  Measures the number of characters from <paramref name="text"/> starting at <paramref name="start"/>
	///  that fit within the specified <paramref name="maxWidth"/> using word-break logic.
	/// </summary>
	private static int MeasureLineChars(SKFont skFont, string text, int start, float maxWidth)
	{
		int lastBreak = 0;
		for (int i = start; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '\n' || c == '\r')
				return i - start;

			if (c == ' ' || c == '\t')
				lastBreak = i - start + 1;

			float width = skFont.MeasureText(text.Substring(start, i - start + 1));
			if (width > maxWidth)
			{
				if (lastBreak > 0)
					return lastBreak;
				// No break point found; break at this character
				return Math.Max(1, i - start);
			}
		}
		return text.Length - start;
	}

	/// <summary>
	///  Multiplies the world transformation of this <see cref="Graphics"/> and specified the <see cref="Matrix"/>.
	/// </summary>
	/// <param name="matrix">A <see cref="Matrix"/> that multiplies the world transformation.</param>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, Drawing2D.MatrixOrder.Prepend);
	}
	/// <summary>
	///  Multiplies the world transformation of this <see cref="Graphics"/> and specified the <see cref="Matrix"/> in the specified order.
	/// </summary>
	/// <param name="matrix">A <see cref="Matrix"/> that multiplies the world transformation.</param>
	/// <param name="order">Member of the <see cref="MatrixOrder"/> enumeration that determines the order of the multiplication.</param>
	public void MultiplyTransform(Matrix matrix, MatrixOrder order)
	{
		ThrowIfDisposed();
		if (matrix is null) throw new ArgumentNullException(nameof(matrix));
		if (order == Drawing2D.MatrixOrder.Prepend)
		{
			_canvas.Concat(matrix.SKMatrix);
		}
		else
		{
			var current = _canvas.TotalMatrix;
			_canvas.SetMatrix(current.PreConcat(matrix.SKMatrix));
		}
	}

	/// <summary>
	///  Releases a device context handle obtained by a previous call to the GetHdc method of this Graphics.
	/// </summary>
	public void ReleaseHdc() { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Releases a device context handle obtained by a previous call to the GetHdc method of this Graphics.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	public void ReleaseHdc(nint hdc) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }
	/// <summary>
	///  Releases a handle to a device context.
	/// </summary>
	[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void ReleaseHdcInternal(nint hdc) { throw new PlatformNotSupportedException("Not yet implemented in SkiaSharp.Extended.Drawing.Common"); }

	/// <summary>
	///  Resets the clip region of this <see cref="Graphics"/> to an infinite region.
	/// </summary>
	public void ResetClip()
	{
		ThrowIfDisposed();
		_canvas.RestoreToCount(_clipSaveCount);
		_clipSaveCount = _canvas.Save();
		_clipRegion = null;
	}

	/// <summary>
	///  Resets the world transformation matrix of this <see cref="Graphics"/> to the identity matrix.
	/// </summary>
	public void ResetTransform()
	{
		ThrowIfDisposed();
		_canvas.ResetMatrix();
	}

	/// <summary>
	///  Restores the state of this <see cref="Graphics"/> to the state represented by a <see cref="GraphicsState"/>.
	/// </summary>
	/// <param name="gstate">A <see cref="GraphicsState"/> that represents the state to which to restore this <see cref="Graphics"/>.</param>
	public void Restore(GraphicsState gstate)
	{
		ThrowIfDisposed();
		if (gstate is null) throw new ArgumentNullException(nameof(gstate));
		_canvas.RestoreToCount(gstate.SaveCount);

		// Restore mode state from saved stack
		if (_savedStates != null && _savedStates.Count > 0)
		{
			var state = _savedStates.Pop();
			_smoothingMode = state.SmoothingMode;
			_interpolationMode = state.InterpolationMode;
			_compositingMode = state.CompositingMode;
			_compositingQuality = state.CompositingQuality;
			_textRenderingHint = state.TextRenderingHint;
			_pixelOffsetMode = state.PixelOffsetMode;
			_pageUnit = state.PageUnit;
			_pageScale = state.PageScale;
			_renderingOrigin = state.RenderingOrigin;
			_textContrast = state.TextContrast;
			_clipSaveCount = state.ClipSaveCount;
			_clipRegion = state.ClipRegion;
		}
	}

	/// <summary>
	///  Applies the specified rotation to the transformation matrix of this <see cref="Graphics"/>.
	/// </summary>
	/// <param name="angle">Angle of rotation in degrees.</param>
	public void RotateTransform(float angle)
	{
		ThrowIfDisposed();
		_canvas.RotateDegrees(angle);
	}

	/// <summary>
	///  Applies the specified rotation to the transformation matrix of this <see cref="Graphics"/> in the specified order.
	/// </summary>
	/// <param name="angle">Angle of rotation in degrees.</param>
	/// <param name="order">Member of the <see cref="MatrixOrder"/> enumeration that specifies whether the rotation is appended or prepended to the matrix transformation.</param>
	public void RotateTransform(float angle, MatrixOrder order)
	{
		ThrowIfDisposed();
		if (order == Drawing2D.MatrixOrder.Prepend)
		{
			_canvas.RotateDegrees(angle);
		}
		else
		{
			var cur = _canvas.TotalMatrix;
			_canvas.SetMatrix(cur.PreConcat(SKMatrix.CreateRotationDegrees(angle)));
		}
	}

	/// <summary>
	///  Saves the current state of this <see cref="Graphics"/> and identifies the saved state with a <see cref="GraphicsState"/>.
	/// </summary>
	/// <returns>This method returns a <see cref="GraphicsState"/> that represents the saved state of this <see cref="Graphics"/>.</returns>
	public GraphicsState Save()
	{
		ThrowIfDisposed();

		// Save mode state
		_savedStates ??= new Collections.Generic.Stack<GraphicsModeState>();
		_savedStates.Push(new GraphicsModeState
		{
			SmoothingMode = _smoothingMode,
			InterpolationMode = _interpolationMode,
			CompositingMode = _compositingMode,
			CompositingQuality = _compositingQuality,
			TextRenderingHint = _textRenderingHint,
			PixelOffsetMode = _pixelOffsetMode,
			PageUnit = _pageUnit,
			PageScale = _pageScale,
			RenderingOrigin = _renderingOrigin,
			TextContrast = _textContrast,
			ClipSaveCount = _clipSaveCount,
			ClipRegion = _clipRegion != null ? (Region)_clipRegion.Clone() : null,
		});

		int count = _canvas.Save();
		return new GraphicsState(count);
	}

	/// <summary>
	///  Applies the specified scaling operation to the transformation matrix of this <see cref="Graphics"/> by prepending it to the object's transformation matrix.
	/// </summary>
	/// <param name="sx">Scale factor in the x direction.</param>
	/// <param name="sy">Scale factor in the y direction.</param>
	public void ScaleTransform(float sx, float sy)
	{
		ThrowIfDisposed();
		_canvas.Scale(sx, sy);
	}

	/// <summary>
	///  Applies the specified scaling operation to the transformation matrix of this <see cref="Graphics"/> in the specified order.
	/// </summary>
	/// <param name="sx">Scale factor in the x direction.</param>
	/// <param name="sy">Scale factor in the y direction.</param>
	/// <param name="order">Member of the <see cref="MatrixOrder"/> enumeration that specifies whether the scaling operation is prepended or appended to the transformation matrix.</param>
	public void ScaleTransform(float sx, float sy, MatrixOrder order)
	{
		ThrowIfDisposed();
		if (order == Drawing2D.MatrixOrder.Prepend)
		{
			_canvas.Scale(sx, sy);
		}
		else
		{
			var cur = _canvas.TotalMatrix;
			_canvas.SetMatrix(cur.PreConcat(SKMatrix.CreateScale(sx, sy)));
		}
	}

	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> that represents the new clip region.</param>
	public void SetClip(GraphicsPath path)
	{
		SetClip(path, Drawing2D.CombineMode.Replace);
	}
	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified combine operation of the current clip region and the specified <see cref="GraphicsPath"/>.
	/// </summary>
	/// <param name="path">The <see cref="GraphicsPath"/> to combine.</param>
	/// <param name="combineMode">The <see cref="CombineMode"/> to use.</param>
	public void SetClip(GraphicsPath path, CombineMode combineMode)
	{
		ThrowIfDisposed();
		if (path is null) throw new ArgumentNullException(nameof(path));
		switch (combineMode)
		{
			case Drawing2D.CombineMode.Replace:
				_canvas.RestoreToCount(_clipSaveCount);
				_clipSaveCount = _canvas.Save();
				_canvas.ClipPath(path.SKPath);
				_clipRegion = new Region(path);
				break;
			case Drawing2D.CombineMode.Intersect:
				_canvas.ClipPath(path.SKPath, SKClipOperation.Intersect);
				_clipRegion?.Intersect(path);
				break;
			case Drawing2D.CombineMode.Exclude:
				_canvas.ClipPath(path.SKPath, SKClipOperation.Difference);
				_clipRegion?.Exclude(path);
				break;
			case Drawing2D.CombineMode.Union:
			case Drawing2D.CombineMode.Xor:
			case Drawing2D.CombineMode.Complement:
				ApplyPathCombineMode(path.SKPath, combineMode);
				break;
		}
	}
	/// <summary>
	///  Sets the clipping region of this Graphics.
	/// </summary>
	public void SetClip(Graphics g)
	{
		SetClip(g, Drawing2D.CombineMode.Replace);
	}
	/// <summary>
	///  Sets the clipping region of this Graphics to the Clip property of the specified Graphics.
	/// </summary>
	public void SetClip(Graphics g, CombineMode combineMode)
	{
		ThrowIfDisposed();
		if (g is null) throw new ArgumentNullException(nameof(g));
		// Use the clip bounds of the source graphics as a rectangle clip
		var bounds = g.ClipBounds;
		SetClip(bounds, combineMode);
	}
	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the specified <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure that represents the new clip region.</param>
	public void SetClip(Rectangle rect)
	{
		SetClip(rect, Drawing2D.CombineMode.Replace);
	}
	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified operation combining the current clip region and the specified <see cref="Rectangle"/>.
	/// </summary>
	/// <param name="rect">A <see cref="Rectangle"/> structure to combine.</param>
	/// <param name="combineMode">A <see cref="CombineMode"/> enumeration that specifies the combining operation to use.</param>
	public void SetClip(Rectangle rect, CombineMode combineMode)
	{
		SetClip((RectangleF)rect, combineMode);
	}
	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the specified <see cref="RectangleF"/>.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure that represents the new clip region.</param>
	public void SetClip(RectangleF rect)
	{
		SetClip(rect, Drawing2D.CombineMode.Replace);
	}
	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified operation combining the current clip region and the specified <see cref="RectangleF"/>.
	/// </summary>
	/// <param name="rect">A <see cref="RectangleF"/> structure to combine.</param>
	/// <param name="combineMode">A <see cref="CombineMode"/> enumeration that specifies the combining operation to use.</param>
	public void SetClip(RectangleF rect, CombineMode combineMode)
	{
		ThrowIfDisposed();
		var skRect = new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom);
		switch (combineMode)
		{
			case Drawing2D.CombineMode.Replace:
				_canvas.RestoreToCount(_clipSaveCount);
				_clipSaveCount = _canvas.Save();
				_canvas.ClipRect(skRect);
				_clipRegion = new Region(rect);
				break;
			case Drawing2D.CombineMode.Intersect:
				_canvas.ClipRect(skRect, SKClipOperation.Intersect);
				_clipRegion?.Intersect(rect);
				break;
			case Drawing2D.CombineMode.Exclude:
				_canvas.ClipRect(skRect, SKClipOperation.Difference);
				_clipRegion?.Exclude(rect);
				break;
			case Drawing2D.CombineMode.Union:
			case Drawing2D.CombineMode.Xor:
			case Drawing2D.CombineMode.Complement:
				var rectPath = new SKPath();
				rectPath.AddRect(skRect);
				ApplyPathCombineMode(rectPath, combineMode);
				rectPath.Dispose();
				break;
		}
	}
	/// <summary>
	///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified operation combining the current clip region and the specified <see cref="Region"/>.
	/// </summary>
	/// <param name="region">A <see cref="Region"/> to combine.</param>
	/// <param name="combineMode">A <see cref="CombineMode"/> enumeration that specifies the combining operation to use.</param>
	public void SetClip(Region region, CombineMode combineMode)
	{
		ThrowIfDisposed();
		if (region is null) throw new ArgumentNullException(nameof(region));
		switch (combineMode)
		{
			case Drawing2D.CombineMode.Replace:
				_canvas.RestoreToCount(_clipSaveCount);
				_clipSaveCount = _canvas.Save();
				if (!region.IsInfinite(this))
				{
					_canvas.ClipPath(region.SKPath);
				}
				_clipRegion = (Region)region.Clone();
				break;
			case Drawing2D.CombineMode.Intersect:
				_canvas.ClipPath(region.SKPath, SKClipOperation.Intersect);
				_clipRegion?.Intersect(region);
				break;
			case Drawing2D.CombineMode.Exclude:
				_canvas.ClipPath(region.SKPath, SKClipOperation.Difference);
				_clipRegion?.Exclude(region);
				break;
			case Drawing2D.CombineMode.Union:
			case Drawing2D.CombineMode.Xor:
			case Drawing2D.CombineMode.Complement:
				ApplyPathCombineMode(region.SKPath, combineMode);
				break;
		}
	}

	/// <summary>
	///  Transforms an array of points from one coordinate space to another.
	/// </summary>
	public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF[] pts)
	{
		ThrowIfDisposed();
		if (pts is null) throw new ArgumentNullException(nameof(pts));
		if (srcSpace == destSpace) return;

		// Apply or invert the current transform matrix
		var matrix = _canvas.TotalMatrix;
		if (srcSpace == Drawing2D.CoordinateSpace.World && destSpace == Drawing2D.CoordinateSpace.Device)
		{
			for (int i = 0; i < pts.Length; i++)
			{
				var mapped = matrix.MapPoint(pts[i].X, pts[i].Y);
				pts[i] = new PointF(mapped.X, mapped.Y);
			}
		}
		else if (srcSpace == Drawing2D.CoordinateSpace.Device && destSpace == Drawing2D.CoordinateSpace.World)
		{
			if (matrix.TryInvert(out var inverse))
			{
				for (int i = 0; i < pts.Length; i++)
				{
					var mapped = inverse.MapPoint(pts[i].X, pts[i].Y);
					pts[i] = new PointF(mapped.X, mapped.Y);
				}
			}
		}
		// Page space is treated the same as World in this implementation
		else
		{
			for (int i = 0; i < pts.Length; i++)
			{
				var mapped = matrix.MapPoint(pts[i].X, pts[i].Y);
				pts[i] = new PointF(mapped.X, mapped.Y);
			}
		}
	}
	/// <summary>
	///  Transforms an array of points from one coordinate space to another.
	/// </summary>
	public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, Point[] pts)
	{
		ThrowIfDisposed();
		if (pts is null) throw new ArgumentNullException(nameof(pts));
		var ptf = new PointF[pts.Length];
		for (int i = 0; i < pts.Length; i++)
			ptf[i] = new PointF(pts[i].X, pts[i].Y);
		TransformPoints(destSpace, srcSpace, ptf);
		for (int i = 0; i < pts.Length; i++)
			pts[i] = Point.Round(ptf[i]);
	}

	/// <summary>
	///  Translates the clipping region of this <see cref="Graphics"/> by specified amounts in the horizontal and vertical directions.
	/// </summary>
	/// <param name="dx">The x-component of the translation.</param>
	/// <param name="dy">The y-component of the translation.</param>
	public void TranslateClip(int dx, int dy)
	{
		TranslateClip((float)dx, (float)dy);
	}
	/// <summary>
	///  Translates the clipping region of this <see cref="Graphics"/> by specified amounts in the horizontal and vertical directions.
	/// </summary>
	/// <param name="dx">The x-component of the translation.</param>
	/// <param name="dy">The y-component of the translation.</param>
	public void TranslateClip(float dx, float dy)
	{
		ThrowIfDisposed();
		// SkiaSharp does not support translating clips directly.
		// This is a best-effort implementation using translate + re-clip.
		_canvas.Translate(dx, dy);
	}

	/// <summary>
	///  Changes the origin of the coordinate system by prepending the specified translation to the transformation matrix of this <see cref="Graphics"/>.
	/// </summary>
	/// <param name="dx">The x-coordinate of the translation.</param>
	/// <param name="dy">The y-coordinate of the translation.</param>
	public void TranslateTransform(float dx, float dy)
	{
		ThrowIfDisposed();
		_canvas.Translate(dx, dy);
	}

	/// <summary>
	///  Changes the origin of the coordinate system by applying the specified translation to the transformation matrix of this <see cref="Graphics"/> in the specified order.
	/// </summary>
	/// <param name="dx">The x-coordinate of the translation.</param>
	/// <param name="dy">The y-coordinate of the translation.</param>
	/// <param name="order">Member of the <see cref="MatrixOrder"/> enumeration that specifies whether the translation is prepended or appended to the transformation matrix.</param>
	public void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		ThrowIfDisposed();
		if (order == Drawing2D.MatrixOrder.Prepend)
		{
			_canvas.Translate(dx, dy);
		}
		else
		{
			var cur = _canvas.TotalMatrix;
			_canvas.SetMatrix(cur.PreConcat(SKMatrix.CreateTranslation(dx, dy)));
		}
	}

	/// <summary>
	///  Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
	/// </summary>
	~Graphics()
	{
		Dispose(false);
	}

	private void ApplyState(SKPaint paint)
	{
		paint.IsAntialias = _smoothingMode != SmoothingMode.None
		                 && _smoothingMode != SmoothingMode.HighSpeed;
	}

	private void ThrowIfDisposed()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(Graphics));
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// Always restore the clip save count if we created one
				if (_clipSaveCount >= 0 && _canvas != null)
				{
					_canvas.RestoreToCount(_clipSaveCount);
				}

				if (_ownsCanvas)
				{
					_canvas?.Dispose();
				}
			}
			_disposed = true;
		}
	}

	/// <summary>
	///  Converts a Point array to a PointF array.
	/// </summary>
	private static PointF[] ToPointFArray(Point[] points)
	{
		if (points is null) throw new ArgumentNullException(nameof(points));
		var ptsF = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
			ptsF[i] = new PointF(points[i].X, points[i].Y);
		return ptsF;
	}

	/// <summary>
	///  Applies a Union, Xor, or Complement clip combine mode by computing the combined path
	///  with SKPath.Op and re-applying as a clip.
	/// </summary>
	private void ApplyPathCombineMode(SKPath newPath, CombineMode combineMode)
	{
		SKPathOp op;
		switch (combineMode)
		{
			case Drawing2D.CombineMode.Union: op = SKPathOp.Union; break;
			case Drawing2D.CombineMode.Xor: op = SKPathOp.Xor; break;
			case Drawing2D.CombineMode.Complement: op = SKPathOp.ReverseDifference; break;
			default: return;
		}

		// Get the current clip as a path
		var currentClipPath = _clipRegion?.SKPath;
		if (currentClipPath == null || currentClipPath.PointCount == 0)
		{
			// No existing clip — for Union, just use the new path; for others, use canvas bounds
			var bounds = _canvas.DeviceClipBounds;
			currentClipPath = new SKPath();
			currentClipPath.AddRect(new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom));
		}
		else
		{
			currentClipPath = new SKPath(currentClipPath);
		}

		var combined = new SKPath();
		if (currentClipPath.Op(newPath, op, combined))
		{
			_canvas.RestoreToCount(_clipSaveCount);
			_clipSaveCount = _canvas.Save();
			_canvas.ClipPath(combined);

			// Update clip region
			using var regionPath = new GraphicsPath();
			regionPath.SKPath.Dispose();
			regionPath.SKPath = new SKPath(combined);
			_clipRegion = new Region(regionPath);
		}

		currentClipPath.Dispose();
		combined.Dispose();
	}

	/// <summary>
	///  Builds an open cardinal spline SKPath from a subset of points.
	///  Uses the formula: control points cp[i] = p[i] ± tension * (p[i+1] - p[i-1]) / 2
	/// </summary>
	private static SKPath BuildCardinalSplinePath(PointF[] points, int offset, int numberOfSegments, float tension)
	{
		if (offset < 0 || offset >= points.Length) throw new ArgumentOutOfRangeException(nameof(offset));
		if (numberOfSegments < 1) throw new ArgumentOutOfRangeException(nameof(numberOfSegments));
		int endIndex = offset + numberOfSegments;
		if (endIndex >= points.Length) throw new ArgumentException("offset + numberOfSegments exceeds array length.");

		var path = new SKPath();
		path.MoveTo(points[offset].X, points[offset].Y);

		float k = tension * 0.3f;

		for (int i = offset; i < endIndex; i++)
		{
			// Current segment goes from points[i] to points[i+1]
			var p0 = points[i];
			var p1 = points[i + 1];

			// Tangent at p0: use predecessor if available, else p0 itself
			var pPrev = (i > 0) ? points[i - 1] : p0;
			var pNext = p1;

			float cp1x = p0.X + k * (pNext.X - pPrev.X);
			float cp1y = p0.Y + k * (pNext.Y - pPrev.Y);

			// Tangent at p1: use successor if available, else p1 itself
			var p1Prev = p0;
			var p1Next = (i + 2 < points.Length) ? points[i + 2] : p1;

			float cp2x = p1.X - k * (p1Next.X - p1Prev.X);
			float cp2y = p1.Y - k * (p1Next.Y - p1Prev.Y);

			path.CubicTo(cp1x, cp1y, cp2x, cp2y, p1.X, p1.Y);
		}

		return path;
	}

	/// <summary>
	///  Builds a closed cardinal spline SKPath (the last point connects back to the first).
	///  Uses the formula: control points cp[i] = p[i] ± tension * (p[i+1] - p[i-1]) / 2
	/// </summary>
	private static SKPath BuildClosedCardinalSplinePath(PointF[] points, float tension)
	{
		int n = points.Length;
		var path = new SKPath();
		path.MoveTo(points[0].X, points[0].Y);

		float k = tension * 0.3f;

		for (int i = 0; i < n; i++)
		{
			var p0 = points[i];
			var p1 = points[(i + 1) % n];
			var pPrev = points[(i - 1 + n) % n];
			var pNext = points[(i + 2) % n];

			float cp1x = p0.X + k * (p1.X - pPrev.X);
			float cp1y = p0.Y + k * (p1.Y - pPrev.Y);

			float cp2x = p1.X - k * (pNext.X - p0.X);
			float cp2y = p1.Y - k * (pNext.Y - p0.Y);

			path.CubicTo(cp1x, cp1y, cp2x, cp2y, p1.X, p1.Y);
		}

		path.Close();
		return path;
	}

	/// <summary>
	///  Core helper to draw a portion of an image into a destination rectangle.
	/// </summary>
	private void DrawImageCore(Image image, RectangleF destRect, RectangleF srcRect)
	{
		DrawImageCore(image, destRect, srcRect, null);
	}

	/// <summary>
	///  Core helper to draw a portion of an image into a destination rectangle, with optional ImageAttributes.
	/// </summary>
	private void DrawImageCore(Image image, RectangleF destRect, RectangleF srcRect, Imaging.ImageAttributes? imageAttr)
	{
		ThrowIfDisposed();
		if (image is null) throw new ArgumentNullException(nameof(image));
		if (image.SKBitmapBacking is null)
			throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
		var src = new SKRect(srcRect.X, srcRect.Y, srcRect.Right, srcRect.Bottom);
		var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);

		using var paint = new SKPaint();

		// Map InterpolationMode to SKSamplingOptions
		SKSamplingOptions sampling;
		switch (_interpolationMode)
		{
			case InterpolationMode.NearestNeighbor:
				sampling = new SKSamplingOptions(SKFilterMode.Nearest);
				break;
			case InterpolationMode.HighQualityBilinear:
				sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
				break;
			case InterpolationMode.HighQualityBicubic:
			case InterpolationMode.Bicubic:
			case InterpolationMode.High:
				sampling = new SKSamplingOptions(new SKCubicResampler(1f / 3f, 1f / 3f));
				break;
			case InterpolationMode.Bilinear:
			case InterpolationMode.Low:
			default:
				sampling = new SKSamplingOptions(SKFilterMode.Linear);
				break;
		}

		// Apply color matrix from ImageAttributes if present
		if (imageAttr != null)
		{
			var colorFilter = imageAttr.CreateColorFilter();
			if (colorFilter != null)
				paint.ColorFilter = colorFilter;
		}

		using var skImage = SKImage.FromBitmap(image.SKBitmapBacking);
		_canvas.DrawImage(skImage, src, dest, sampling, paint);
	}

	/// <summary>
	///  Draws a bitmap into a parallelogram defined by 3 destination points (top-left, top-right, bottom-left).
	///  Uses an SKMatrix to map the source rectangle to the destination parallelogram.
	/// </summary>
	private void DrawImageWithParallelogram(SKBitmap bitmap, SKRect? srcRect, PointF[] destPoints)
	{
		// destPoints[0] = top-left, destPoints[1] = top-right, destPoints[2] = bottom-left
		var src = srcRect ?? new SKRect(0, 0, bitmap.Width, bitmap.Height);

		// Build a matrix that maps the source rectangle to the parallelogram
		var srcPts = new SKPoint[] { new(src.Left, src.Top), new(src.Right, src.Top), new(src.Left, src.Bottom) };
		var dstPts = new SKPoint[] { new(destPoints[0].X, destPoints[0].Y), new(destPoints[1].X, destPoints[1].Y), new(destPoints[2].X, destPoints[2].Y) };

		var matrix = new SKMatrix();
		// Use the 3-point mapping via Poly2Poly if available, else manual calculation
		// Manual affine matrix:
		// [a b c]   [src.Left   src.Right  src.Left ]   [dst0.X dst1.X dst2.X]
		// [d e f] * [src.Top    src.Top    src.Bottom] = [dst0.Y dst1.Y dst2.Y]
		// We need M such that M * srcPt = dstPt
		float sx = src.Width;
		float sy = src.Height;
		if (sx == 0 || sy == 0) return;

		float a = (dstPts[1].X - dstPts[0].X) / sx;
		float b = (dstPts[2].X - dstPts[0].X) / sy;
		float c = dstPts[0].X - a * src.Left - b * src.Top;
		float d = (dstPts[1].Y - dstPts[0].Y) / sx;
		float e = (dstPts[2].Y - dstPts[0].Y) / sy;
		float f = dstPts[0].Y - d * src.Left - e * src.Top;

		matrix = new SKMatrix(a, b, c, d, e, f, 0, 0, 1);

		int count = _canvas.Save();
		_canvas.Concat(matrix);
		if (srcRect.HasValue)
			_canvas.DrawBitmap(bitmap, src, src);
		else
			_canvas.DrawBitmap(bitmap, src.Left, src.Top);
		_canvas.RestoreToCount(count);
	}
}
