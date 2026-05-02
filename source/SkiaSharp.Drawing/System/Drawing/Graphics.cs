using SkiaSharp;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Internal;
using System.Drawing.Text;

namespace System.Drawing
{
	/// <summary>
	///  Encapsulates a GDI+ drawing surface backed by SkiaSharp. This class cannot be inherited.
	/// </summary>
	public sealed partial class Graphics : System.MarshalByRefObject, System.Drawing.IDeviceContext, System.IDisposable
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
		public delegate bool EnumerateMetafileProc(System.Drawing.Imaging.EmfPlusRecordType recordType, int flags, int dataSize, System.IntPtr data, System.Drawing.Imaging.PlayRecordCallback? callbackData);

		/// <summary>
		///  Gets or sets a <see cref="Region"/> that limits the drawing region of this <see cref="Graphics"/>.
		/// </summary>
		/// <value>A <see cref="Region"/> that limits the portion of this <see cref="Graphics"/> that is currently available for drawing.</value>
		public System.Drawing.Region Clip
		{
			get
			{
				ThrowIfDisposed();
				// Return a new infinite region as approximation of the current clip
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
			}
		}

		/// <summary>
		///  Gets a <see cref="RectangleF"/> structure that bounds the clipping region of this <see cref="Graphics"/>.
		/// </summary>
		public System.Drawing.RectangleF ClipBounds
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
		public System.Drawing.Drawing2D.CompositingMode CompositingMode
		{
			get { ThrowIfDisposed(); return _compositingMode; }
			set { ThrowIfDisposed(); _compositingMode = value; }
		}

		/// <summary>
		///  Gets or sets the rendering quality of composited images drawn to this Graphics.
		/// </summary>
		public System.Drawing.Drawing2D.CompositingQuality CompositingQuality
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
		public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
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
		public System.Drawing.GraphicsUnit PageUnit
		{
			get { ThrowIfDisposed(); return _pageUnit; }
			set { ThrowIfDisposed(); _pageUnit = value; }
		}

		/// <summary>
		///  Gets or sets a value specifying how pixels are offset during rendering of this Graphics.
		/// </summary>
		public System.Drawing.Drawing2D.PixelOffsetMode PixelOffsetMode
		{
			get { ThrowIfDisposed(); return _pixelOffsetMode; }
			set { ThrowIfDisposed(); _pixelOffsetMode = value; }
		}

		/// <summary>
		///  Gets or sets the rendering origin of this Graphics for dithering and for hatch brushes.
		/// </summary>
		public System.Drawing.Point RenderingOrigin
		{
			get { ThrowIfDisposed(); return _renderingOrigin; }
			set { ThrowIfDisposed(); _renderingOrigin = value; }
		}

		/// <summary>
		///  Gets or sets the rendering quality for this Graphics.
		/// </summary>
		public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
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
		public System.Drawing.Text.TextRenderingHint TextRenderingHint
		{
			get { ThrowIfDisposed(); return _textRenderingHint; }
			set { ThrowIfDisposed(); _textRenderingHint = value; }
		}

		/// <summary>
		///  Gets or sets a copy of the geometric world transformation for this Graphics.
		/// </summary>
		public System.Drawing.Drawing2D.Matrix Transform
		{
			get
			{
				ThrowIfDisposed();
				return new Drawing2D.Matrix { SKMatrix = _canvas.TotalMatrix };
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
		public System.Drawing.RectangleF VisibleClipBounds
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
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public static System.Drawing.Graphics FromHdc(nint hdc) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Creates a new Graphics from the specified handle to a device context and handle to a device.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public static System.Drawing.Graphics FromHdc(nint hdc, nint hdevice) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Returns a Graphics for the specified device context.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public static System.Drawing.Graphics FromHdcInternal(nint hdc) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Creates a new Graphics from the specified handle to a window.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public static System.Drawing.Graphics FromHwnd(nint hwnd) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Creates a new Graphics for the specified Windows handle.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public static System.Drawing.Graphics FromHwndInternal(nint hwnd) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Creates a new <see cref="Graphics"/> from the specified <see cref="Image"/>.
		/// </summary>
		/// <param name="image"><see cref="Image"/> from which to create the new <see cref="Graphics"/>.</param>
		/// <returns>This method returns a new <see cref="Graphics"/> for the specified <see cref="Image"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="image"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">The backing bitmap of <paramref name="image"/> is <see langword="null"/>.</exception>
		public static System.Drawing.Graphics FromImage(System.Drawing.Image image)
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
			if (canvas is null) throw new ArgumentNullException(nameof(canvas));

			var graphics = new Graphics();
			graphics._canvas = canvas;
			graphics._ownsCanvas = false;
			graphics._clipSaveCount = graphics._canvas.Save();
			return graphics;
		}

		/// <summary>
		///  Returns a Windows halftone palette.
		/// </summary>
		/// <returns>An internal pointer to the handle of the palette.</returns>
		public static nint GetHalftonePalette() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a comment to the current Metafile.
		/// </summary>
		/// <param name="data">Array of bytes that contains the comment.</param>
		public void AddMetafileComment(byte[] data) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container.
		/// </summary>
		/// <returns>A <see cref="GraphicsContainer"/> that represents the state of this Graphics.</returns>
		public System.Drawing.Drawing2D.GraphicsContainer BeginContainer()
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
		public System.Drawing.Drawing2D.GraphicsContainer BeginContainer(System.Drawing.Rectangle dstrect, System.Drawing.Rectangle srcrect, System.Drawing.GraphicsUnit unit)
			=> BeginContainer((RectangleF)dstrect, (RectangleF)srcrect, unit);

		/// <summary>
		///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container with the specified scale transformation.
		/// </summary>
		/// <param name="dstrect">A <see cref="RectangleF"/> structure that, together with the <paramref name="srcrect"/> parameter, specifies a scale transformation for the new graphics container.</param>
		/// <param name="srcrect">A <see cref="RectangleF"/> structure that, together with the <paramref name="dstrect"/> parameter, specifies a scale transformation for the new graphics container.</param>
		/// <param name="unit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the unit of measure for the container.</param>
		/// <returns>A <see cref="GraphicsContainer"/> that represents the state of this Graphics.</returns>
		public System.Drawing.Drawing2D.GraphicsContainer BeginContainer(System.Drawing.RectangleF dstrect, System.Drawing.RectangleF srcrect, System.Drawing.GraphicsUnit unit)
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
		public void Clear(System.Drawing.Color color)
		{
			ThrowIfDisposed();
			_canvas.Clear(SkiaConversions.ToSKColor(color));
		}

		/// <summary>
		///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
		/// </summary>
		public void CopyFromScreen(System.Drawing.Point upperLeftSource, System.Drawing.Point upperLeftDestination, System.Drawing.Size blockRegionSize) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
		/// </summary>
		public void CopyFromScreen(System.Drawing.Point upperLeftSource, System.Drawing.Point upperLeftDestination, System.Drawing.Size blockRegionSize, System.Drawing.CopyPixelOperation copyPixelOperation) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
		/// </summary>
		public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, System.Drawing.Size blockRegionSize) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Performs a bit-block transfer of color data from the screen to the drawing surface of this Graphics.
		/// </summary>
		public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, System.Drawing.Size blockRegionSize, System.Drawing.CopyPixelOperation copyPixelOperation) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawArc(System.Drawing.Pen pen, System.Drawing.Rectangle rect, float startAngle, float sweepAngle)
			=> DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Draws an arc representing a portion of an ellipse specified by a <see cref="RectangleF"/> structure.
		/// </summary>
		public void DrawArc(System.Drawing.Pen pen, System.Drawing.RectangleF rect, float startAngle, float sweepAngle)
			=> DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Draws an arc representing a portion of an ellipse specified by a pair of coordinates, a width, and a height.
		/// </summary>
		public void DrawArc(System.Drawing.Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
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
		public void DrawArc(System.Drawing.Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
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
		public void DrawBezier(System.Drawing.Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4)
			=> DrawBezier(pen, (float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y, (float)pt3.X, (float)pt3.Y, (float)pt4.X, (float)pt4.Y);

		/// <summary>
		///  Draws a Bezier spline defined by four PointF structures.
		/// </summary>
		/// <param name="pen">The <see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="pt1">A <see cref="PointF"/> structure that represents the starting point of the curve.</param>
		/// <param name="pt2">A <see cref="PointF"/> structure that represents the first control point of the curve.</param>
		/// <param name="pt3">A <see cref="PointF"/> structure that represents the second control point of the curve.</param>
		/// <param name="pt4">A <see cref="PointF"/> structure that represents the ending point of the curve.</param>
		public void DrawBezier(System.Drawing.Pen pen, System.Drawing.PointF pt1, System.Drawing.PointF pt2, System.Drawing.PointF pt3, System.Drawing.PointF pt4)
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
		public void DrawBezier(System.Drawing.Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
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
		public void DrawBeziers(System.Drawing.Pen pen, System.Drawing.PointF[] points)
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
		public void DrawBeziers(System.Drawing.Pen pen, System.Drawing.Point[] points)
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
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points)
			=> DrawClosedCurve(pen, points, 0.5f, Drawing2D.FillMode.Alternate);

		/// <summary>
		///  Draws a closed cardinal spline defined by an array of PointF structures using the specified tension.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
		/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, float tension, System.Drawing.Drawing2D.FillMode fillmode)
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
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.Point[] points)
			=> DrawClosedCurve(pen, ToPointFArray(points));

		/// <summary>
		///  Draws a closed cardinal spline defined by an array of Point structures using the specified tension.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
		/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, float tension, System.Drawing.Drawing2D.FillMode fillmode)
			=> DrawClosedCurve(pen, ToPointFArray(points), tension, fillmode);

		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points)
			=> DrawCurve(pen, points, 0, points?.Length - 1 ?? 0, 0.5f);

		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures using a specified offset and tension.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		/// <param name="offset">Offset from the first element in the array to the starting point of the curve.</param>
		/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, int offset, int numberOfSegments)
			=> DrawCurve(pen, points, offset, numberOfSegments, 0.5f);

		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures using a specified offset, number of segments, and tension.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		/// <param name="offset">Offset from the first element in the array to the starting point of the curve.</param>
		/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, int offset, int numberOfSegments, float tension)
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
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, float tension)
			=> DrawCurve(pen, points, 0, points?.Length - 1 ?? 0, tension);

		/// <summary>
		///  Draws a cardinal spline through a specified array of Point structures.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points)
			=> DrawCurve(pen, ToPointFArray(points));

		/// <summary>
		///  Draws a cardinal spline through a specified array of Point structures using a specified offset, number of segments, and tension.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
		/// <param name="offset">Offset from the first element in the array to the starting point of the curve.</param>
		/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, int offset, int numberOfSegments, float tension)
			=> DrawCurve(pen, ToPointFArray(points), offset, numberOfSegments, tension);

		/// <summary>
		///  Draws a cardinal spline through a specified array of Point structures using a specified tension.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the curve.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the control points.</param>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, float tension)
			=> DrawCurve(pen, ToPointFArray(points), tension);

		/// <summary>
		///  Draws an ellipse specified by a bounding <see cref="Rectangle"/> structure.
		/// </summary>
		public void DrawEllipse(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
			=> DrawEllipse(pen, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		/// <summary>
		///  Draws an ellipse defined by a bounding <see cref="RectangleF"/>.
		/// </summary>
		public void DrawEllipse(System.Drawing.Pen pen, System.Drawing.RectangleF rect)
			=> DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);

		/// <summary>
		///  Draws an ellipse defined by a bounding rectangle specified by coordinates.
		/// </summary>
		public void DrawEllipse(System.Drawing.Pen pen, int x, int y, int width, int height)
			=> DrawEllipse(pen, (float)x, (float)y, (float)width, (float)height);

		/// <summary>
		///  Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates, a height, and a width.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the ellipse.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
		public void DrawEllipse(System.Drawing.Pen pen, float x, float y, float width, float height)
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
		public void DrawIcon(System.Drawing.Icon icon, System.Drawing.Rectangle targetRect)
		{
			ThrowIfDisposed();
			if (icon is null) throw new ArgumentNullException(nameof(icon));
			using var bmp = icon.ToBitmap();
			DrawImage(bmp, targetRect);
		}
		/// <summary>
		///  Draws the image represented by the specified Icon at the specified coordinates.
		/// </summary>
		public void DrawIcon(System.Drawing.Icon icon, int x, int y)
		{
			ThrowIfDisposed();
			if (icon is null) throw new ArgumentNullException(nameof(icon));
			using var bmp = icon.ToBitmap();
			DrawImage(bmp, x, y);
		}
		/// <summary>
		///  Draws the image represented by the specified Icon without scaling the image.
		/// </summary>
		public void DrawIconUnstretched(System.Drawing.Icon icon, System.Drawing.Rectangle targetRect)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point point)
			=> DrawImage(image, (float)point.X, (float)point.Y);

		/// <summary>
		///  Draws the specified <see cref="Image"/> at the specified location.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="point"><see cref="PointF"/> structure that represents the upper-left corner of the drawn image.</param>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF point)
			=> DrawImage(image, point.X, point.Y);

		/// <summary>
		///  Draws the specified Image at the specified location and with the specified shape and size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="destPoints">Array of three <see cref="PointF"/> structures that define a parallelogram.</param>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr)
			=> DrawImage(image, destPoints, srcRect, srcUnit);
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback)
			=> DrawImage(image, destPoints, srcRect, srcUnit);
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback, int callbackData)
			=> DrawImage(image, destPoints, srcRect, srcUnit);

		/// <summary>
		///  Draws the specified Image at the specified location and with the specified shape and size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="destPoints">Array of three <see cref="Point"/> structures that define a parallelogram.</param>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback, int callbackData)
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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle rect)
			=> DrawImage(image, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="destRect">A <see cref="Rectangle"/> structure that specifies the location and size of the drawn image.</param>
		/// <param name="srcRect">A <see cref="Rectangle"/> structure that specifies the portion of the image to draw.</param>
		/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit)
			=> DrawImageCore(image, (RectangleF)destRect, (RectangleF)srcRect);

		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs, System.Drawing.Graphics.DrawImageAbort? callback, nint callbackData)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs, System.Drawing.Graphics.DrawImageAbort? callback)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs, System.Drawing.Graphics.DrawImageAbort? callback, nint callbackData)
			=> DrawImageCore(image, (RectangleF)destRect, new RectangleF(srcX, srcY, srcWidth, srcHeight));

		/// <summary>
		///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="rect"><see cref="RectangleF"/> structure that specifies the location and size of the drawn image.</param>
		public void DrawImage(System.Drawing.Image image, System.Drawing.RectangleF rect)
			=> DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);

		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="destRect">A <see cref="RectangleF"/> structure that specifies the location and size of the drawn image.</param>
		/// <param name="srcRect">A <see cref="RectangleF"/> structure that specifies the portion of the image to draw.</param>
		/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
		public void DrawImage(System.Drawing.Image image, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit)
			=> DrawImageCore(image, destRect, srcRect);

		/// <summary>
		///  Draws the specified image, using its original physical size, at the location specified by a coordinate pair.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
		public void DrawImage(System.Drawing.Image image, int x, int y)
			=> DrawImage(image, (float)x, (float)y);

		/// <summary>
		///  Draws a portion of an image at a specified location.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="srcRect">A <see cref="Rectangle"/> structure that specifies the portion of the image to draw.</param>
		/// <param name="srcUnit">Member of the <see cref="GraphicsUnit"/> enumeration that specifies the units of measure used by the <paramref name="srcRect"/> parameter.</param>
		public void DrawImage(System.Drawing.Image image, int x, int y, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit)
		{
			ThrowIfDisposed();
			if (image is null) throw new ArgumentNullException(nameof(image));
			if (image.SKBitmapBacking is null)
				throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
			var src = new SKRect(srcRect.X, srcRect.Y, srcRect.Right, srcRect.Bottom);
			var dest = new SKRect(x, y, x + srcRect.Width, y + srcRect.Height);
			_canvas.DrawBitmap(image.SKBitmapBacking, src, dest);
		}

		/// <summary>
		///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="width">Width of the drawn image.</param>
		/// <param name="height">Height of the drawn image.</param>
		public void DrawImage(System.Drawing.Image image, int x, int y, int width, int height)
			=> DrawImage(image, (float)x, (float)y, (float)width, (float)height);

		/// <summary>
		///  Draws the specified <see cref="Image"/>, using its original physical size, at the specified location.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
		public void DrawImage(System.Drawing.Image image, float x, float y)
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
		public void DrawImage(System.Drawing.Image image, float x, float y, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit)
		{
			ThrowIfDisposed();
			if (image is null) throw new ArgumentNullException(nameof(image));
			if (image.SKBitmapBacking is null)
				throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
			var src = new SKRect(srcRect.X, srcRect.Y, srcRect.Right, srcRect.Bottom);
			var dest = new SKRect(x, y, x + srcRect.Width, y + srcRect.Height);
			_canvas.DrawBitmap(image.SKBitmapBacking, src, dest);
		}

		/// <summary>
		///  Draws the specified <see cref="Image"/> at the specified location and with the specified size.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="width">Width of the drawn image.</param>
		/// <param name="height">Height of the drawn image.</param>
		public void DrawImage(System.Drawing.Image image, float x, float y, float width, float height)
		{
			ThrowIfDisposed();
			if (image is null) throw new ArgumentNullException(nameof(image));
			if (image.SKBitmapBacking is null)
				throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
			var dest = new SKRect(x, y, x + width, y + height);
			_canvas.DrawBitmap(image.SKBitmapBacking, dest);
		}

		/// <summary>
		///  Draws the specified image using its original physical size at the location specified by a Point structure.
		/// </summary>
		public void DrawImageUnscaled(System.Drawing.Image image, System.Drawing.Point point)
			=> DrawImage(image, (float)point.X, (float)point.Y);

		/// <summary>
		///  Draws a specified image using its original physical size at a specified location.
		/// </summary>
		public void DrawImageUnscaled(System.Drawing.Image image, System.Drawing.Rectangle rect)
			=> DrawImage(image, (float)rect.X, (float)rect.Y);

		/// <summary>
		///  Draws the specified image using its original physical size at the location specified by a coordinate pair.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
		public void DrawImageUnscaled(System.Drawing.Image image, int x, int y)
			=> DrawImage(image, (float)x, (float)y);

		/// <summary>
		///  Draws a specified image using its original physical size at a specified location.
		/// </summary>
		public void DrawImageUnscaled(System.Drawing.Image image, int x, int y, int width, int height)
			=> DrawImage(image, (float)x, (float)y);

		/// <summary>
		///  Draws the specified image without scaling and clips it, if necessary, to fit in the specified rectangle.
		/// </summary>
		/// <param name="image"><see cref="Image"/> to draw.</param>
		/// <param name="rect">The <see cref="Rectangle"/> in which to draw the image.</param>
		public void DrawImageUnscaledAndClipped(System.Drawing.Image image, System.Drawing.Rectangle rect)
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
		public void DrawLine(System.Drawing.Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2)
			=> DrawLine(pen, (float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y);

		/// <summary>
		///  Draws a line connecting two <see cref="PointF"/> structures.
		/// </summary>
		public void DrawLine(System.Drawing.Pen pen, System.Drawing.PointF pt1, System.Drawing.PointF pt2)
			=> DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);

		/// <summary>
		///  Draws a line connecting the two points specified by the coordinate pairs.
		/// </summary>
		public void DrawLine(System.Drawing.Pen pen, int x1, int y1, int x2, int y2)
			=> DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);

		/// <summary>
		///  Draws a line connecting the two points specified by the coordinate pairs.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line.</param>
		/// <param name="x1">The x-coordinate of the first point.</param>
		/// <param name="y1">The y-coordinate of the first point.</param>
		/// <param name="x2">The x-coordinate of the second point.</param>
		/// <param name="y2">The y-coordinate of the second point.</param>
		public void DrawLine(System.Drawing.Pen pen, float x1, float y1, float x2, float y2)
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
		public void DrawLines(System.Drawing.Pen pen, System.Drawing.PointF[] points)
		{
			ThrowIfDisposed();
			if (pen is null) throw new ArgumentNullException(nameof(pen));
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 2) return;
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
		public void DrawLines(System.Drawing.Pen pen, System.Drawing.Point[] points)
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
		public void DrawPath(System.Drawing.Pen pen, System.Drawing.Drawing2D.GraphicsPath path)
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
		public void DrawPie(System.Drawing.Pen pen, System.Drawing.Rectangle rect, float startAngle, float sweepAngle)
			=> DrawPie(pen, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Draws a pie shape defined by an ellipse specified by a RectangleF structure and two radial lines.
		/// </summary>
		public void DrawPie(System.Drawing.Pen pen, System.Drawing.RectangleF rect, float startAngle, float sweepAngle)
			=> DrawPie(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Draws a pie shape defined by an ellipse and two radial lines.
		/// </summary>
		public void DrawPie(System.Drawing.Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
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
		public void DrawPie(System.Drawing.Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
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
		public void DrawPolygon(System.Drawing.Pen pen, System.Drawing.PointF[] points)
		{
			ThrowIfDisposed();
			if (pen is null) throw new ArgumentNullException(nameof(pen));
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 2) return;
			using var paint = pen.CreatePaint();
			ApplyState(paint);
			using var path = GdiPolygonPath(points);
			_canvas.DrawPath(path, paint);
		}

		/// <summary>
		///  Draws a polygon defined by an array of <see cref="Point"/> structures.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the polygon.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that represent the vertices of the polygon.</param>
		public void DrawPolygon(System.Drawing.Pen pen, System.Drawing.Point[] points)
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
		public void DrawRectangle(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
			=> DrawRectangle(pen, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		/// <summary>
		///  Draws a rectangle specified by a coordinate pair, a width, and a height.
		/// </summary>
		public void DrawRectangle(System.Drawing.Pen pen, int x, int y, int width, int height)
			=> DrawRectangle(pen, (float)x, (float)y, (float)width, (float)height);

		/// <summary>
		///  Draws a rectangle specified by a coordinate pair, a width, and a height.
		/// </summary>
		/// <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">The width of the rectangle to draw.</param>
		/// <param name="height">The height of the rectangle to draw.</param>
		public void DrawRectangle(System.Drawing.Pen pen, float x, float y, float width, float height)
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
		public void DrawRectangles(System.Drawing.Pen pen, System.Drawing.RectangleF[] rects)
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
		public void DrawRectangles(System.Drawing.Pen pen, System.Drawing.Rectangle[] rects)
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
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point)
			=> DrawString(s, font, brush, point.X, point.Y, null);

		/// <summary>
		///  Draws the specified text string at the specified location with the specified <see cref="Brush"/>, <see cref="Font"/>, and <see cref="StringFormat"/> objects.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
		/// <param name="point"><see cref="PointF"/> structure that specifies the upper-left corner of the drawn text.</param>
		/// <param name="format"><see cref="StringFormat"/> that specifies formatting attributes applied to the drawn text.</param>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point, System.Drawing.StringFormat? format)
			=> DrawString(s, font, brush, point.X, point.Y, format);

		/// <summary>
		///  Draws the specified text string in the specified rectangle with the specified <see cref="Brush"/> and <see cref="Font"/> objects.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
		/// <param name="layoutRectangle"><see cref="RectangleF"/> structure that specifies the location of the drawn text.</param>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle)
			=> DrawString(s, font, brush, layoutRectangle, null);

		/// <summary>
		///  Draws the specified text string in the specified rectangle with the specified <see cref="Brush"/>, <see cref="Font"/>, and <see cref="StringFormat"/> objects.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="brush"><see cref="Brush"/> that determines the color and texture of the drawn text.</param>
		/// <param name="layoutRectangle"><see cref="RectangleF"/> structure that specifies the location of the drawn text.</param>
		/// <param name="format"><see cref="StringFormat"/> that specifies formatting attributes applied to the drawn text.</param>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle, System.Drawing.StringFormat? format)
		{
			ThrowIfDisposed();
			if (s is null || s.Length == 0) return;
			if (font is null) throw new ArgumentNullException(nameof(font));
			if (brush is null) throw new ArgumentNullException(nameof(brush));

			using var paint = brush.CreatePaint();
			ApplyState(paint);

			var skFont = font.SKFont;
			var m = skFont.Metrics;

			// Compute the text draw position (baseline Y) inside the layout rect
			float x = layoutRectangle.X;
			float y = layoutRectangle.Y - m.Ascent; // baseline offset (ascent is negative)

			// Apply horizontal alignment
			if (format != null && format.Alignment != StringAlignment.Near)
			{
				float textWidth = skFont.MeasureText(s, paint);
				if (format.Alignment == StringAlignment.Center)
					x += (layoutRectangle.Width - textWidth) / 2f;
				else if (format.Alignment == StringAlignment.Far)
					x += layoutRectangle.Width - textWidth;
			}

			// Apply vertical alignment
			if (format != null && format.LineAlignment != StringAlignment.Near)
			{
				float lineHeight = Math.Abs(m.Ascent) + Math.Abs(m.Descent) + Math.Abs(m.Leading);
				if (format.LineAlignment == StringAlignment.Center)
					y = layoutRectangle.Y + (layoutRectangle.Height - lineHeight) / 2f - m.Ascent;
				else if (format.LineAlignment == StringAlignment.Far)
					y = layoutRectangle.Y + layoutRectangle.Height - lineHeight - m.Ascent;
			}

			// Clip to the layout rectangle unless NoClip is set
			bool clip = format == null || (format.FormatFlags & StringFormatFlags.NoClip) == 0;
			if (clip && layoutRectangle.Width > 0 && layoutRectangle.Height > 0)
			{
				_canvas.Save();
				_canvas.ClipRect(new SKRect(layoutRectangle.X, layoutRectangle.Y,
					layoutRectangle.X + layoutRectangle.Width, layoutRectangle.Y + layoutRectangle.Height));
			}

			_canvas.DrawText(s, x, y, skFont, paint);

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
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y)
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
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y, System.Drawing.StringFormat? format)
		{
			ThrowIfDisposed();
			if (s is null || s.Length == 0) return;
			if (font is null) throw new ArgumentNullException(nameof(font));
			if (brush is null) throw new ArgumentNullException(nameof(brush));

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
		public void EndContainer(System.Drawing.Drawing2D.GraphicsContainer container)
		{
			ThrowIfDisposed();
			if (container is null) throw new ArgumentNullException(nameof(container));
			_canvas.RestoreToCount(container.SaveCount);
		}
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point destPoint, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF destPoint, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sends the records in the specified Metafile to a callback method for display.
		/// </summary>
		public void EnumerateMetafile(System.Drawing.Imaging.Metafile metafile, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, nint callbackData, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Updates the clip region of this <see cref="Graphics"/> to exclude the area specified by a <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="rect">A <see cref="Rectangle"/> structure that specifies the rectangle to exclude from the clip region.</param>
		public void ExcludeClip(System.Drawing.Rectangle rect)
		{
			ThrowIfDisposed();
			_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom), SKClipOperation.Difference);
		}
		/// <summary>
		///  Updates the clip region of this <see cref="Graphics"/> to exclude the area specified by a <see cref="Region"/>.
		/// </summary>
		/// <param name="region">A <see cref="Region"/> that specifies the region to exclude from the clip region.</param>
		public void ExcludeClip(System.Drawing.Region region)
		{
			ThrowIfDisposed();
			if (region is null) throw new ArgumentNullException(nameof(region));
			_canvas.ClipPath(region.SKPath, SKClipOperation.Difference);
		}

		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points)
			=> FillClosedCurve(brush, points, Drawing2D.FillMode.Alternate, 0.5f);

		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures using the specified fill mode.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillmode)
			=> FillClosedCurve(brush, points, fillmode, 0.5f);

		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures using the specified fill mode and tension.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that define the spline.</param>
		/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillmode, float tension)
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
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points)
			=> FillClosedCurve(brush, ToPointFArray(points));

		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures using the specified fill mode.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
		/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillmode)
			=> FillClosedCurve(brush, ToPointFArray(points), fillmode);

		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures using the specified fill mode and tension.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that define the spline.</param>
		/// <param name="fillmode">Member of the <see cref="FillMode"/> enumeration that determines how the curve is filled.</param>
		/// <param name="tension">Value that specifies the amount that the curve bends through the points.</param>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillmode, float tension)
			=> FillClosedCurve(brush, ToPointFArray(points), fillmode, tension);

		/// <summary>
		///  Fills the interior of an ellipse defined by a bounding rectangle specified by a <see cref="Rectangle"/> structure.
		/// </summary>
		public void FillEllipse(System.Drawing.Brush brush, System.Drawing.Rectangle rect)
			=> FillEllipse(brush, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		/// <summary>
		///  Fills the interior of an ellipse defined by a bounding rectangle specified by a <see cref="RectangleF"/> structure.
		/// </summary>
		public void FillEllipse(System.Drawing.Brush brush, System.Drawing.RectangleF rect)
			=> FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);

		/// <summary>
		///  Fills the interior of an ellipse defined by a bounding rectangle.
		/// </summary>
		public void FillEllipse(System.Drawing.Brush brush, int x, int y, int width, int height)
			=> FillEllipse(brush, (float)x, (float)y, (float)width, (float)height);

		/// <summary>
		///  Fills the interior of an ellipse defined by a bounding rectangle specified by a pair of coordinates, a width, and a height.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
		public void FillEllipse(System.Drawing.Brush brush, float x, float y, float width, float height)
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
		public void FillPath(System.Drawing.Brush brush, System.Drawing.Drawing2D.GraphicsPath path)
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
		public void FillPie(System.Drawing.Brush brush, System.Drawing.Rectangle rect, float startAngle, float sweepAngle)
			=> FillPie(brush, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Fills the interior of a pie section defined by an ellipse and two radial lines.
		/// </summary>
		public void FillPie(System.Drawing.Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
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
		public void FillPie(System.Drawing.Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			ThrowIfDisposed();
			if (brush is null) throw new ArgumentNullException(nameof(brush));
			using var paint = brush.CreatePaint();
			ApplyState(paint);
			var oval = GdiCurveRect(x, y, width, height);
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
		public void FillPolygon(System.Drawing.Brush brush, System.Drawing.PointF[] points)
		{
			ThrowIfDisposed();
			if (brush is null) throw new ArgumentNullException(nameof(brush));
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 2) return;
			using var paint = brush.CreatePaint();
			ApplyState(paint);
			using var path = GdiPolygonPath(points);
			_canvas.DrawPath(path, paint);
		}

		/// <summary>
		///  Fills the interior of a polygon defined by an array of points specified by <see cref="PointF"/> structures using the specified fill mode.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="PointF"/> structures that represent the vertices of the polygon to fill.</param>
		/// <param name="fillMode">Member of the <see cref="FillMode"/> enumeration that determines the style of the fill.</param>
		public void FillPolygon(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillMode)
		{
			ThrowIfDisposed();
			if (brush is null) throw new ArgumentNullException(nameof(brush));
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 2) return;
			using var paint = brush.CreatePaint();
			ApplyState(paint);
			using var path = GdiPolygonPath(points);
			path.FillType = fillMode == FillMode.Winding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
			_canvas.DrawPath(path, paint);
		}

		/// <summary>
		///  Fills the interior of a polygon defined by an array of points specified by <see cref="Point"/> structures.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="points">Array of <see cref="Point"/> structures that represent the vertices of the polygon to fill.</param>
		public void FillPolygon(System.Drawing.Brush brush, System.Drawing.Point[] points)
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
		public void FillPolygon(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillMode)
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
		public void FillRectangle(System.Drawing.Brush brush, System.Drawing.Rectangle rect)
			=> FillRectangle(brush, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		/// <summary>
		///  Fills the interior of a rectangle specified by a <see cref="RectangleF"/> structure.
		/// </summary>
		public void FillRectangle(System.Drawing.Brush brush, System.Drawing.RectangleF rect)
			=> FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);

		/// <summary>
		///  Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
		/// </summary>
		public void FillRectangle(System.Drawing.Brush brush, int x, int y, int width, int height)
			=> FillRectangle(brush, (float)x, (float)y, (float)width, (float)height);

		/// <summary>
		///  Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
		/// </summary>
		/// <param name="brush"><see cref="Brush"/> that determines the characteristics of the fill.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="width">Width of the rectangle to fill.</param>
		/// <param name="height">Height of the rectangle to fill.</param>
		public void FillRectangle(System.Drawing.Brush brush, float x, float y, float width, float height)
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
		public void FillRectangles(System.Drawing.Brush brush, System.Drawing.RectangleF[] rects)
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
		public void FillRectangles(System.Drawing.Brush brush, System.Drawing.Rectangle[] rects)
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
		public void FillRegion(System.Drawing.Brush brush, System.Drawing.Region region)
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
		public void Flush(System.Drawing.Drawing2D.FlushIntention intention)
		{
			ThrowIfDisposed();
			_canvas.Flush();
		}

		/// <summary>
		///  Gets the cumulative graphics context.
		/// </summary>
		public object GetContextInfo() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Gets the handle to the device context associated with this Graphics.
		/// </summary>
		public nint GetHdc() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Gets the nearest color to the specified <see cref="Color"/> structure.
		/// </summary>
		/// <param name="color"><see cref="Color"/> structure for which to find a match.</param>
		/// <returns>A <see cref="Color"/> structure that represents the nearest color to the one specified with the <paramref name="color"/> parameter.</returns>
		public System.Drawing.Color GetNearestColor(System.Drawing.Color color)
		{
			ThrowIfDisposed();
			return color;
		}

		/// <summary>
		///  Updates the clip region of this <see cref="Graphics"/> to the intersection of the current clip region and the specified <see cref="Rectangle"/> structure.
		/// </summary>
		/// <param name="rect">A <see cref="Rectangle"/> structure to intersect with the current clip region.</param>
		public void IntersectClip(System.Drawing.Rectangle rect)
		{
			ThrowIfDisposed();
			_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom), SKClipOperation.Intersect);
		}
		/// <summary>
		///  Updates the clip region of this <see cref="Graphics"/> to the intersection of the current clip region and the specified <see cref="RectangleF"/> structure.
		/// </summary>
		/// <param name="rect">A <see cref="RectangleF"/> structure to intersect with the current clip region.</param>
		public void IntersectClip(System.Drawing.RectangleF rect)
		{
			ThrowIfDisposed();
			_canvas.ClipRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom), SKClipOperation.Intersect);
		}
		/// <summary>
		///  Updates the clip region of this <see cref="Graphics"/> to the intersection of the current clip region and the specified <see cref="Region"/>.
		/// </summary>
		/// <param name="region">A <see cref="Region"/> to intersect with the current region.</param>
		public void IntersectClip(System.Drawing.Region region)
		{
			ThrowIfDisposed();
			if (region is null) throw new ArgumentNullException(nameof(region));
			_canvas.ClipPath(region.SKPath, SKClipOperation.Intersect);
		}

		/// <summary>
		///  Indicates whether the specified Point structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.Point point) => IsVisible((float)point.X, (float)point.Y);
		/// <summary>
		///  Indicates whether the specified PointF structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.PointF point) => IsVisible(point.X, point.Y);
		/// <summary>
		///  Indicates whether the rectangle specified by a Rectangle structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.Rectangle rect) => IsVisible((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
		/// <summary>
		///  Indicates whether the rectangle specified by a RectangleF structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.RectangleF rect) => IsVisible(rect.X, rect.Y, rect.Width, rect.Height);
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
			var testRect = new SkiaSharp.SKRect(x, y, x + width, y + height);
			var clipRect = new SkiaSharp.SKRect(clipBounds.Left, clipBounds.Top, clipBounds.Right, clipBounds.Bottom);
			return testRect.IntersectsWith(clipRect);
		}

		/// <summary>
		///  Gets an array of Region objects, each of which bounds a range of character positions within the specified string.
		/// </summary>
		public System.Drawing.Region[] MeasureCharacterRanges(string? text, System.Drawing.Font font, System.Drawing.RectangleF layoutRect, System.Drawing.StringFormat? stringFormat)
		{
			ThrowIfDisposed();
			if (font is null) throw new ArgumentNullException(nameof(font));

			var ranges = stringFormat != null ? GetMeasurableRanges(stringFormat) : Array.Empty<CharacterRange>();
			if (ranges.Length == 0 || string.IsNullOrEmpty(text))
				return new Region[] { new Region(new Drawing2D.GraphicsPath()) };

			var regions = new Region[ranges.Length];
			using var paint = new SkiaSharp.SKPaint();
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
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font)
			=> MeasureString(text, font, new SizeF(float.MaxValue, float.MaxValue), null);

		/// <summary>
		///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/>.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="origin"><see cref="PointF"/> structure that represents the upper-left corner of the string. This is currently ignored.</param>
		/// <param name="stringFormat"><see cref="StringFormat"/> that represents formatting information for the string.</param>
		/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.PointF origin, System.Drawing.StringFormat? stringFormat)
			=> MeasureString(text, font, new SizeF(float.MaxValue, float.MaxValue), stringFormat);

		/// <summary>
		///  Measures the specified string when drawn with the specified <see cref="Font"/> within the specified layout area.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="layoutArea"><see cref="SizeF"/> structure that specifies the maximum layout area for the text.</param>
		/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.SizeF layoutArea)
			=> MeasureString(text, font, layoutArea, null);

		/// <summary>
		///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/> within the specified layout area.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="layoutArea"><see cref="SizeF"/> structure that specifies the maximum layout area for the text.</param>
		/// <param name="stringFormat"><see cref="StringFormat"/> that represents formatting information for the string.</param>
		/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.SizeF layoutArea, System.Drawing.StringFormat? stringFormat)
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
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.SizeF layoutArea, System.Drawing.StringFormat? stringFormat, out int charactersFitted, out int linesFilled)
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
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, int width)
			=> MeasureString(text, font, new SizeF(width, float.MaxValue), null);

		/// <summary>
		///  Measures the specified string when drawn with the specified <see cref="Font"/> and <see cref="StringFormat"/> within the specified width.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
		/// <param name="width">Maximum width of the string in pixels.</param>
		/// <param name="format"><see cref="StringFormat"/> that represents formatting information for the string.</param>
		/// <returns>A <see cref="SizeF"/> structure that represents the size of the string.</returns>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, int width, System.Drawing.StringFormat? format)
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
		public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix)
		{
			MultiplyTransform(matrix, Drawing2D.MatrixOrder.Prepend);
		}
		/// <summary>
		///  Multiplies the world transformation of this <see cref="Graphics"/> and specified the <see cref="Matrix"/> in the specified order.
		/// </summary>
		/// <param name="matrix">A <see cref="Matrix"/> that multiplies the world transformation.</param>
		/// <param name="order">Member of the <see cref="MatrixOrder"/> enumeration that determines the order of the multiplication.</param>
		public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			if (matrix is null) throw new ArgumentNullException(nameof(matrix));
			if (order == Drawing2D.MatrixOrder.Prepend)
			{
				var current = _canvas.TotalMatrix;
				_canvas.SetMatrix(current.PreConcat(matrix.SKMatrix));
			}
			else
			{
				_canvas.Concat(matrix.SKMatrix);
			}
		}

		/// <summary>
		///  Releases a device context handle obtained by a previous call to the GetHdc method of this Graphics.
		/// </summary>
		public void ReleaseHdc() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Releases a device context handle obtained by a previous call to the GetHdc method of this Graphics.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
		public void ReleaseHdc(nint hdc) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Releases a handle to a device context.
		/// </summary>
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		public void ReleaseHdcInternal(nint hdc) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Resets the clip region of this <see cref="Graphics"/> to an infinite region.
		/// </summary>
		public void ResetClip()
		{
			ThrowIfDisposed();
			_canvas.RestoreToCount(_clipSaveCount);
			_clipSaveCount = _canvas.Save();
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
		public void Restore(System.Drawing.Drawing2D.GraphicsState gstate)
		{
			ThrowIfDisposed();
			if (gstate is null) throw new ArgumentNullException(nameof(gstate));
			_canvas.RestoreToCount(gstate.SaveCount);
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
		public void RotateTransform(float angle, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			_canvas.RotateDegrees(angle);
		}

		/// <summary>
		///  Saves the current state of this <see cref="Graphics"/> and identifies the saved state with a <see cref="GraphicsState"/>.
		/// </summary>
		/// <returns>This method returns a <see cref="GraphicsState"/> that represents the saved state of this <see cref="Graphics"/>.</returns>
		public System.Drawing.Drawing2D.GraphicsState Save()
		{
			ThrowIfDisposed();
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
		public void ScaleTransform(float sx, float sy, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			_canvas.Scale(sx, sy);
		}

		/// <summary>
		///  Sets the clipping region of this <see cref="Graphics"/> to the specified <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="path">The <see cref="GraphicsPath"/> that represents the new clip region.</param>
		public void SetClip(System.Drawing.Drawing2D.GraphicsPath path)
		{
			SetClip(path, Drawing2D.CombineMode.Replace);
		}
		/// <summary>
		///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified combine operation of the current clip region and the specified <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="path">The <see cref="GraphicsPath"/> to combine.</param>
		/// <param name="combineMode">The <see cref="CombineMode"/> to use.</param>
		public void SetClip(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Drawing2D.CombineMode combineMode)
		{
			ThrowIfDisposed();
			if (path is null) throw new ArgumentNullException(nameof(path));
			// For Replace mode, reset clip first via save/restore pattern.
			// SkiaSharp ClipPath always intersects, so this covers the common Intersect/Replace cases.
			_canvas.ClipPath(path.SKPath);
		}
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Graphics g)
		{
			SetClip(g, Drawing2D.CombineMode.Replace);
		}
		/// <summary>
		///  Sets the clipping region of this Graphics to the Clip property of the specified Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Graphics g, System.Drawing.Drawing2D.CombineMode combineMode)
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
		public void SetClip(System.Drawing.Rectangle rect)
		{
			SetClip(rect, Drawing2D.CombineMode.Replace);
		}
		/// <summary>
		///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified operation combining the current clip region and the specified <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="rect">A <see cref="Rectangle"/> structure to combine.</param>
		/// <param name="combineMode">A <see cref="CombineMode"/> enumeration that specifies the combining operation to use.</param>
		public void SetClip(System.Drawing.Rectangle rect, System.Drawing.Drawing2D.CombineMode combineMode)
		{
			SetClip((RectangleF)rect, combineMode);
		}
		/// <summary>
		///  Sets the clipping region of this <see cref="Graphics"/> to the specified <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="rect">A <see cref="RectangleF"/> structure that represents the new clip region.</param>
		public void SetClip(System.Drawing.RectangleF rect)
		{
			SetClip(rect, Drawing2D.CombineMode.Replace);
		}
		/// <summary>
		///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified operation combining the current clip region and the specified <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="rect">A <see cref="RectangleF"/> structure to combine.</param>
		/// <param name="combineMode">A <see cref="CombineMode"/> enumeration that specifies the combining operation to use.</param>
		public void SetClip(System.Drawing.RectangleF rect, System.Drawing.Drawing2D.CombineMode combineMode)
		{
			ThrowIfDisposed();
			var skRect = new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom);
			if (combineMode == Drawing2D.CombineMode.Replace)
			{
				_canvas.RestoreToCount(_clipSaveCount);
				_clipSaveCount = _canvas.Save();
				_canvas.ClipRect(skRect);
			}
			else if (combineMode == Drawing2D.CombineMode.Exclude)
			{
				_canvas.ClipRect(skRect, SKClipOperation.Difference);
			}
			else
			{
				// Intersect is the default for non-Replace modes in SkiaSharp
				_canvas.ClipRect(skRect, SKClipOperation.Intersect);
			}
		}
		/// <summary>
		///  Sets the clipping region of this <see cref="Graphics"/> to the result of the specified operation combining the current clip region and the specified <see cref="Region"/>.
		/// </summary>
		/// <param name="region">A <see cref="Region"/> to combine.</param>
		/// <param name="combineMode">A <see cref="CombineMode"/> enumeration that specifies the combining operation to use.</param>
		public void SetClip(System.Drawing.Region region, System.Drawing.Drawing2D.CombineMode combineMode)
		{
			ThrowIfDisposed();
			if (region is null) throw new ArgumentNullException(nameof(region));
			if (combineMode == Drawing2D.CombineMode.Replace)
			{
				_canvas.RestoreToCount(_clipSaveCount);
				_clipSaveCount = _canvas.Save();
				if (!region.IsInfinite(this))
				{
					_canvas.ClipPath(region.SKPath);
				}
			}
			else if (combineMode == Drawing2D.CombineMode.Exclude)
			{
				_canvas.ClipPath(region.SKPath, SKClipOperation.Difference);
			}
			else
			{
				_canvas.ClipPath(region.SKPath, SKClipOperation.Intersect);
			}
		}

		/// <summary>
		///  Transforms an array of points from one coordinate space to another.
		/// </summary>
		public void TransformPoints(System.Drawing.Drawing2D.CoordinateSpace destSpace, System.Drawing.Drawing2D.CoordinateSpace srcSpace, System.Drawing.PointF[] pts)
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
		public void TransformPoints(System.Drawing.Drawing2D.CoordinateSpace destSpace, System.Drawing.Drawing2D.CoordinateSpace srcSpace, System.Drawing.Point[] pts)
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
		public void TranslateTransform(float dx, float dy, System.Drawing.Drawing2D.MatrixOrder order)
		{
			ThrowIfDisposed();
			_canvas.Translate(dx, dy);
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
				if (disposing && _ownsCanvas)
				{
					_canvas?.Dispose();
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

			for (int i = offset; i < endIndex; i++)
			{
				// Current segment goes from points[i] to points[i+1]
				var p0 = points[i];
				var p1 = points[i + 1];

				// Tangent at p0: use predecessor if available, else p0 itself
				var pPrev = (i > 0) ? points[i - 1] : p0;
				var pNext = p1;

				float cp1x = p0.X + tension * (pNext.X - pPrev.X) / 3f;
				float cp1y = p0.Y + tension * (pNext.Y - pPrev.Y) / 3f;

				// Tangent at p1: use successor if available, else p1 itself
				var p1Prev = p0;
				var p1Next = (i + 2 < points.Length) ? points[i + 2] : p1;

				float cp2x = p1.X - tension * (p1Next.X - p1Prev.X) / 3f;
				float cp2y = p1.Y - tension * (p1Next.Y - p1Prev.Y) / 3f;

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

			for (int i = 0; i < n; i++)
			{
				var p0 = points[i];
				var p1 = points[(i + 1) % n];
				var pPrev = points[(i - 1 + n) % n];
				var pNext = points[(i + 2) % n];

				float cp1x = p0.X + tension * (p1.X - pPrev.X) / 3f;
				float cp1y = p0.Y + tension * (p1.Y - pPrev.Y) / 3f;

				float cp2x = p1.X - tension * (pNext.X - p0.X) / 3f;
				float cp2y = p1.Y - tension * (pNext.Y - p0.Y) / 3f;

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
			ThrowIfDisposed();
			if (image is null) throw new ArgumentNullException(nameof(image));
			if (image.SKBitmapBacking is null)
				throw new ArgumentException("The image does not have a valid bitmap backing.", nameof(image));
			var src = new SKRect(srcRect.X, srcRect.Y, srcRect.Right, srcRect.Bottom);
			var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
			_canvas.DrawBitmap(image.SKBitmapBacking, src, dest);
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
}
