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
		///  Gets or sets a Region that limits the drawing region of this Graphics.
		/// </summary>
		public System.Drawing.Region Clip { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }

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
		public System.Drawing.Drawing2D.Matrix Transform { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }

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
		public System.Drawing.Drawing2D.GraphicsContainer BeginContainer() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container with the specified scale transformation.
		/// </summary>
		public System.Drawing.Drawing2D.GraphicsContainer BeginContainer(System.Drawing.Rectangle dstrect, System.Drawing.Rectangle srcrect, System.Drawing.GraphicsUnit unit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Saves a graphics container with the current state of this Graphics and opens and uses a new graphics container with the specified scale transformation.
		/// </summary>
		public System.Drawing.Drawing2D.GraphicsContainer BeginContainer(System.Drawing.RectangleF dstrect, System.Drawing.RectangleF srcrect, System.Drawing.GraphicsUnit unit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawBezier(System.Drawing.Pen pen, System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a Bezier spline defined by four PointF structures.
		/// </summary>
		public void DrawBezier(System.Drawing.Pen pen, System.Drawing.PointF pt1, System.Drawing.PointF pt2, System.Drawing.PointF pt3, System.Drawing.PointF pt4) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a Bezier spline defined by four ordered pairs of coordinates.
		/// </summary>
		public void DrawBezier(System.Drawing.Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a series of Bezier splines from an array of PointF structures.
		/// </summary>
		public void DrawBeziers(System.Drawing.Pen pen, System.Drawing.PointF[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a series of Bezier splines from an array of Point structures.
		/// </summary>
		public void DrawBeziers(System.Drawing.Pen pen, System.Drawing.Point[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a closed cardinal spline defined by an array of PointF structures.
		/// </summary>
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a closed cardinal spline defined by an array of PointF structures using the specified tension.
		/// </summary>
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, float tension, System.Drawing.Drawing2D.FillMode fillmode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a closed cardinal spline defined by an array of Point structures.
		/// </summary>
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.Point[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a closed cardinal spline defined by an array of Point structures using the specified tension.
		/// </summary>
		public void DrawClosedCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, float tension, System.Drawing.Drawing2D.FillMode fillmode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures using a specified offset and tension.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, int offset, int numberOfSegments) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures using a specified offset, number of segments, and tension.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, int offset, int numberOfSegments, float tension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws a cardinal spline through a specified array of PointF structures using a specified tension.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.PointF[] points, float tension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws a cardinal spline through a specified array of Point structures.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws a cardinal spline through a specified array of Point structures using a specified offset, number of segments, and tension.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, int offset, int numberOfSegments, float tension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws a cardinal spline through a specified array of Point structures using a specified tension.
		/// </summary>
		public void DrawCurve(System.Drawing.Pen pen, System.Drawing.Point[] points, float tension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawIcon(System.Drawing.Icon icon, System.Drawing.Rectangle targetRect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the image represented by the specified Icon at the specified coordinates.
		/// </summary>
		public void DrawIcon(System.Drawing.Icon icon, int x, int y) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the image represented by the specified Icon without scaling the image.
		/// </summary>
		public void DrawIconUnstretched(System.Drawing.Icon icon, System.Drawing.Rectangle targetRect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.PointF[] destPoints, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback, int callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Draws the specified Image at the specified location and with the specified shape and size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Point[] destPoints, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback, int callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttr, System.Drawing.Graphics.DrawImageAbort? callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs, System.Drawing.Graphics.DrawImageAbort? callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs, System.Drawing.Graphics.DrawImageAbort? callback) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified portion of the specified Image at the specified location and with the specified size.
		/// </summary>
		public void DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, System.Drawing.GraphicsUnit srcUnit, System.Drawing.Imaging.ImageAttributes? imageAttrs, System.Drawing.Graphics.DrawImageAbort? callback, nint callbackData) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawImage(System.Drawing.Image image, System.Drawing.RectangleF destRect, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawImage(System.Drawing.Image image, int x, int y, System.Drawing.Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawImage(System.Drawing.Image image, float x, float y, System.Drawing.RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void DrawPath(System.Drawing.Pen pen, System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
			using var path = new SKPath();
			path.MoveTo(points[0].X, points[0].Y);
			for (int i = 1; i < points.Length; i++)
				path.LineTo(points[i].X, points[i].Y);
			path.Close();
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
		///  Draws the specified text string at the specified location with the specified Brush and Font objects.
		/// </summary>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified text string at the specified location with the specified Brush, Font, and StringFormat objects.
		/// </summary>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.PointF point, System.Drawing.StringFormat? format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified text string in the specified rectangle with the specified Brush and Font objects.
		/// </summary>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified text string in the specified rectangle with the specified Brush, Font, and StringFormat objects.
		/// </summary>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, System.Drawing.RectangleF layoutRectangle, System.Drawing.StringFormat? format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified text string at the specified location with the specified Brush and Font objects.
		/// </summary>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Draws the specified text string at the specified location with the specified Brush, Font, and StringFormat objects.
		/// </summary>
		public void DrawString(string? s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y, System.Drawing.StringFormat? format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Closes the current graphics container and restores the state of this Graphics to the state saved by a call to BeginContainer.
		/// </summary>
		public void EndContainer(System.Drawing.Drawing2D.GraphicsContainer container) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
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
		///  Updates the clip region of this Graphics to exclude the area specified by a Rectangle.
		/// </summary>
		public void ExcludeClip(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Updates the clip region of this Graphics to exclude the area specified by a Region.
		/// </summary>
		public void ExcludeClip(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures.
		/// </summary>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures using the specified fill mode.
		/// </summary>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillmode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of PointF structures using the specified fill mode and tension.
		/// </summary>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.PointF[] points, System.Drawing.Drawing2D.FillMode fillmode, float tension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures.
		/// </summary>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures using the specified fill mode.
		/// </summary>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillmode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Fills the interior of a closed cardinal spline curve defined by an array of Point structures using the specified fill mode and tension.
		/// </summary>
		public void FillClosedCurve(System.Drawing.Brush brush, System.Drawing.Point[] points, System.Drawing.Drawing2D.FillMode fillmode, float tension) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		public void FillPath(System.Drawing.Brush brush, System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
			using var path = new SKPath();
			path.MoveTo(points[0].X, points[0].Y);
			for (int i = 1; i < points.Length; i++)
				path.LineTo(points[i].X, points[i].Y);
			path.Close();
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
			using var path = new SKPath();
			path.FillType = fillMode == FillMode.Winding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
			path.MoveTo(points[0].X, points[0].Y);
			for (int i = 1; i < points.Length; i++)
				path.LineTo(points[i].X, points[i].Y);
			path.Close();
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
		///  Fills the interior of a Region.
		/// </summary>
		public void FillRegion(System.Drawing.Brush brush, System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		///  Updates the clip region of this Graphics to the intersection of the current clip region and the specified Rectangle structure.
		/// </summary>
		public void IntersectClip(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Updates the clip region of this Graphics to the intersection of the current clip region and the specified RectangleF structure.
		/// </summary>
		public void IntersectClip(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Updates the clip region of this Graphics to the intersection of the current clip region and the specified Region.
		/// </summary>
		public void IntersectClip(System.Drawing.Region region) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified Point structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.Point point) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the specified PointF structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.PointF point) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the rectangle specified by a Rectangle structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the rectangle specified by a RectangleF structure is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the point specified by a pair of coordinates is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(int x, int y) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the rectangle specified by a pair of coordinates, a width, and a height is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(int x, int y, int width, int height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the point specified by a pair of coordinates is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(float x, float y) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Indicates whether the rectangle specified by a pair of coordinates, a width, and a height is contained within the visible clip region of this Graphics.
		/// </summary>
		public bool IsVisible(float x, float y, float width, float height) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Gets an array of Region objects, each of which bounds a range of character positions within the specified string.
		/// </summary>
		public System.Drawing.Region[] MeasureCharacterRanges(string? text, System.Drawing.Font font, System.Drawing.RectangleF layoutRect, System.Drawing.StringFormat? stringFormat) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.PointF origin, System.Drawing.StringFormat? stringFormat) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.SizeF layoutArea) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.SizeF layoutArea, System.Drawing.StringFormat? stringFormat) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, System.Drawing.SizeF layoutArea, System.Drawing.StringFormat? stringFormat, out int charactersFitted, out int linesFilled) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, int width) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Measures the specified string when drawn with the specified Font.
		/// </summary>
		public System.Drawing.SizeF MeasureString(string? text, System.Drawing.Font font, int width, System.Drawing.StringFormat? format) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Multiplies the world transformation of this Graphics and specified the Matrix.
		/// </summary>
		public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Multiplies the world transformation of this Graphics and specified the Matrix in the specified order.
		/// </summary>
		public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		///  Resets the clip region of this Graphics to an infinite region.
		/// </summary>
		public void ResetClip() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Drawing2D.GraphicsPath path) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Drawing2D.CombineMode combineMode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Graphics g) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Graphics g, System.Drawing.Drawing2D.CombineMode combineMode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Rectangle rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Rectangle rect, System.Drawing.Drawing2D.CombineMode combineMode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.RectangleF rect) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.RectangleF rect, System.Drawing.Drawing2D.CombineMode combineMode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Sets the clipping region of this Graphics.
		/// </summary>
		public void SetClip(System.Drawing.Region region, System.Drawing.Drawing2D.CombineMode combineMode) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Transforms an array of points from one coordinate space to another.
		/// </summary>
		public void TransformPoints(System.Drawing.Drawing2D.CoordinateSpace destSpace, System.Drawing.Drawing2D.CoordinateSpace srcSpace, System.Drawing.PointF[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Transforms an array of points from one coordinate space to another.
		/// </summary>
		public void TransformPoints(System.Drawing.Drawing2D.CoordinateSpace destSpace, System.Drawing.Drawing2D.CoordinateSpace srcSpace, System.Drawing.Point[] pts) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Translates the clipping region of this Graphics by specified amounts in the horizontal and vertical directions.
		/// </summary>
		public void TranslateClip(int dx, int dy) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
		/// <summary>
		///  Translates the clipping region of this Graphics by specified amounts in the horizontal and vertical directions.
		/// </summary>
		public void TranslateClip(float dx, float dy) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }

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
				if (disposing)
				{
					_canvas?.Dispose();
				}
				_disposed = true;
			}
		}
	}
}
