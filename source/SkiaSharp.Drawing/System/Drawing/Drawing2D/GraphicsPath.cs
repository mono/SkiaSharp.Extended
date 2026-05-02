using SkiaSharp;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Represents a series of connected lines and curves, backed by an <see cref="SKPath"/>.
	/// </summary>
	public sealed partial class GraphicsPath : System.MarshalByRefObject, System.ICloneable, System.IDisposable
	{
		private bool _disposed;
		private bool _needsNewFigure;

		/// <summary>
		///  The backing SkiaSharp path.
		/// </summary>
		internal SKPath SKPath;

		/// <summary>
		///  Initializes a new instance of the <see cref="GraphicsPath"/> class with a <see cref="FillMode"/> value of <see cref="FillMode.Alternate"/>.
		/// </summary>
		public GraphicsPath()
		{
			SKPath = new SKPath { FillType = SKPathFillType.EvenOdd };
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="GraphicsPath"/> class with the specified <see cref="FillMode"/> enumeration.
		/// </summary>
		/// <param name="fillMode">The <see cref="FillMode"/> enumeration that determines how the interior of this <see cref="GraphicsPath"/> is filled.</param>
		public GraphicsPath(FillMode fillMode)
		{
			SKPath = new SKPath { FillType = ToSKFillType(fillMode) };
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="GraphicsPath"/> class with the specified <see cref="PathPointType"/> and <see cref="PointF"/> arrays.
		/// </summary>
		/// <param name="pts">An array of <see cref="PointF"/> structures that defines the coordinates of the points that make up this <see cref="GraphicsPath"/>.</param>
		/// <param name="types">An array of <see cref="PathPointType"/> enumeration elements that specifies the type of each corresponding point in the <paramref name="pts"/> array.</param>
		public GraphicsPath(PointF[] pts, byte[] types) : this(pts, types, FillMode.Alternate) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="GraphicsPath"/> class with the specified <see cref="PathPointType"/> and <see cref="PointF"/> arrays and with the specified <see cref="FillMode"/> enumeration element.
		/// </summary>
		/// <param name="pts">An array of <see cref="PointF"/> structures that defines the coordinates of the points that make up this <see cref="GraphicsPath"/>.</param>
		/// <param name="types">An array of <see cref="PathPointType"/> enumeration elements that specifies the type of each corresponding point in the <paramref name="pts"/> array.</param>
		/// <param name="fillMode">A <see cref="FillMode"/> enumeration that specifies how the interiors of shapes in this <see cref="GraphicsPath"/> are filled.</param>
		public GraphicsPath(PointF[] pts, byte[] types, FillMode fillMode)
		{
			if (pts is null) throw new ArgumentNullException(nameof(pts));
			if (types is null) throw new ArgumentNullException(nameof(types));
			if (pts.Length != types.Length) throw new ArgumentException("Arrays must have the same length.");

			SKPath = new SKPath { FillType = ToSKFillType(fillMode) };
			BuildPathFromPointsAndTypes(pts, types);
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="GraphicsPath"/> class with the specified <see cref="PathPointType"/> and <see cref="Point"/> arrays.
		/// </summary>
		/// <param name="pts">An array of <see cref="Point"/> structures that defines the coordinates of the points that make up this <see cref="GraphicsPath"/>.</param>
		/// <param name="types">An array of <see cref="PathPointType"/> enumeration elements that specifies the type of each corresponding point in the <paramref name="pts"/> array.</param>
		public GraphicsPath(Point[] pts, byte[] types) : this(ToPointFArray(pts), types, FillMode.Alternate) { }

		/// <summary>
		///  Initializes a new instance of the <see cref="GraphicsPath"/> class with the specified <see cref="PathPointType"/> and <see cref="Point"/> arrays and with the specified <see cref="FillMode"/> enumeration element.
		/// </summary>
		/// <param name="pts">An array of <see cref="Point"/> structures that defines the coordinates of the points that make up this <see cref="GraphicsPath"/>.</param>
		/// <param name="types">An array of <see cref="PathPointType"/> enumeration elements that specifies the type of each corresponding point in the <paramref name="pts"/> array.</param>
		/// <param name="fillMode">A <see cref="FillMode"/> enumeration that specifies how the interiors of shapes in this <see cref="GraphicsPath"/> are filled.</param>
		public GraphicsPath(Point[] pts, byte[] types, FillMode fillMode)
			: this(ToPointFArray(pts), types, fillMode) { }

		/// <summary>
		///  Gets or sets a <see cref="FillMode"/> enumeration that determines how the interiors of shapes in this <see cref="GraphicsPath"/> are filled.
		/// </summary>
		public FillMode FillMode
		{
			get
			{
				ThrowIfDisposed();
				return SKPath.FillType == SKPathFillType.Winding ? FillMode.Winding : FillMode.Alternate;
			}
			set
			{
				ThrowIfDisposed();
				SKPath.FillType = ToSKFillType(value);
			}
		}

		/// <summary>
		///  Gets a <see cref="PathData"/> that encapsulates arrays of points and types for this <see cref="GraphicsPath"/>.
		/// </summary>
		public PathData PathData
		{
			get
			{
				ThrowIfDisposed();
				// PathData stub is not yet implemented; return basic structure.
				throw new PlatformNotSupportedException("PathData is not yet implemented in SkiaSharp.Drawing");
			}
		}

		/// <summary>
		///  Gets the points in the path.
		/// </summary>
		public PointF[] PathPoints
		{
			get
			{
				ThrowIfDisposed();
				var skPoints = SKPath.Points;
				var result = new PointF[skPoints.Length];
				for (int i = 0; i < skPoints.Length; i++)
					result[i] = new PointF(skPoints[i].X, skPoints[i].Y);
				return result;
			}
		}

		/// <summary>
		///  Gets the types of the corresponding points in the <see cref="PathPoints"/> array.
		/// </summary>
		public byte[] PathTypes
		{
			get
			{
				ThrowIfDisposed();
				return ExtractPathTypes();
			}
		}

		/// <summary>
		///  Gets the number of elements in the <see cref="PathPoints"/> or the <see cref="PathTypes"/> array.
		/// </summary>
		public int PointCount
		{
			get
			{
				ThrowIfDisposed();
				return SKPath.PointCount;
			}
		}

		/// <summary>
		///  Appends an arc to the current figure.
		/// </summary>
		public void AddArc(Rectangle rect, float startAngle, float sweepAngle)
			=> AddArc((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Appends an arc to the current figure.
		/// </summary>
		public void AddArc(RectangleF rect, float startAngle, float sweepAngle)
			=> AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Appends an arc to the current figure.
		/// </summary>
		public void AddArc(int x, int y, int width, int height, float startAngle, float sweepAngle)
			=> AddArc((float)x, (float)y, (float)width, (float)height, startAngle, sweepAngle);

		/// <summary>
		///  Appends an arc to the current figure.
		/// </summary>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangular region that defines the ellipse from which the arc is drawn.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangular region that defines the ellipse from which the arc is drawn.</param>
		/// <param name="width">The width of the rectangular region that defines the ellipse from which the arc is drawn.</param>
		/// <param name="height">The height of the rectangular region that defines the ellipse from which the arc is drawn.</param>
		/// <param name="startAngle">The starting angle of the arc, measured in degrees clockwise from the x-axis.</param>
		/// <param name="sweepAngle">The angle between <paramref name="startAngle"/> and the end of the arc.</param>
		public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			ThrowIfDisposed();
			var rect = new SKRect(x + 0.5f, y + 0.5f, x + width + 0.5f, y + height + 0.5f);
			SKPath.AddArc(rect, startAngle, sweepAngle);
		}

		/// <summary>
		///  Adds a cubic Bézier curve to the current figure.
		/// </summary>
		public void AddBezier(Point pt1, Point pt2, Point pt3, Point pt4)
			=> AddBezier((float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y, (float)pt3.X, (float)pt3.Y, (float)pt4.X, (float)pt4.Y);

		/// <summary>
		///  Adds a cubic Bézier curve to the current figure.
		/// </summary>
		public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
			=> AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);

		/// <summary>
		///  Adds a cubic Bézier curve to the current figure.
		/// </summary>
		public void AddBezier(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
			=> AddBezier((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3, (float)x4, (float)y4);

		/// <summary>
		///  Adds a cubic Bézier curve to the current figure.
		/// </summary>
		/// <param name="x1">The x-coordinate of the starting point of the curve.</param>
		/// <param name="y1">The y-coordinate of the starting point of the curve.</param>
		/// <param name="x2">The x-coordinate of the first control point of the curve.</param>
		/// <param name="y2">The y-coordinate of the first control point of the curve.</param>
		/// <param name="x3">The x-coordinate of the second control point of the curve.</param>
		/// <param name="y3">The y-coordinate of the second control point of the curve.</param>
		/// <param name="x4">The x-coordinate of the endpoint of the curve.</param>
		/// <param name="y4">The y-coordinate of the endpoint of the curve.</param>
		public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			ThrowIfDisposed();
			SKPath.MoveTo(x1, y1);
			SKPath.CubicTo(x2, y2, x3, y3, x4, y4);
		}

		/// <summary>
		///  Adds a sequence of connected cubic Bézier curves to the current figure.
		/// </summary>
		/// <param name="points">An array of <see cref="PointF"/> structures that represents the points that define the curves.</param>
		public void AddBeziers(PointF[] points)
		{
			ThrowIfDisposed();
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 4) throw new ArgumentException("Array must contain at least 4 points.", nameof(points));

			SKPath.MoveTo(points[0].X, points[0].Y);
			for (int i = 1; i + 2 < points.Length; i += 3)
				SKPath.CubicTo(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 2].X, points[i + 2].Y);
		}

		/// <summary>
		///  Adds a sequence of connected cubic Bézier curves to the current figure.
		/// </summary>
		/// <param name="points">An array of <see cref="Point"/> structures that represents the points that define the curves.</param>
		public void AddBeziers(params Point[] points)
		{
			ThrowIfDisposed();
			if (points is null) throw new ArgumentNullException(nameof(points));
			var ptsF = ToPointFArray(points);
			AddBeziers(ptsF);
		}

		/// <summary>
		///  Adds a closed curve to this path. A cardinal spline curve is used because the curve travels through each of the points in the array.
		/// </summary>
		public void AddClosedCurve(PointF[] points) { throw new PlatformNotSupportedException("AddClosedCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a closed curve to this path with the specified tension.
		/// </summary>
		public void AddClosedCurve(PointF[] points, float tension) { throw new PlatformNotSupportedException("AddClosedCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a closed curve to this path. A cardinal spline curve is used because the curve travels through each of the points in the array.
		/// </summary>
		public void AddClosedCurve(Point[] points) { throw new PlatformNotSupportedException("AddClosedCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a closed curve to this path with the specified tension.
		/// </summary>
		public void AddClosedCurve(Point[] points, float tension) { throw new PlatformNotSupportedException("AddClosedCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a spline curve to the current figure.
		/// </summary>
		public void AddCurve(PointF[] points) { throw new PlatformNotSupportedException("AddCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a spline curve to the current figure.
		/// </summary>
		public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension) { throw new PlatformNotSupportedException("AddCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a spline curve to the current figure.
		/// </summary>
		public void AddCurve(PointF[] points, float tension) { throw new PlatformNotSupportedException("AddCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a spline curve to the current figure.
		/// </summary>
		public void AddCurve(Point[] points) { throw new PlatformNotSupportedException("AddCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a spline curve to the current figure.
		/// </summary>
		public void AddCurve(Point[] points, int offset, int numberOfSegments, float tension) { throw new PlatformNotSupportedException("AddCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a spline curve to the current figure.
		/// </summary>
		public void AddCurve(Point[] points, float tension) { throw new PlatformNotSupportedException("AddCurve is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds an ellipse to the current path.
		/// </summary>
		public void AddEllipse(Rectangle rect)
			=> AddEllipse((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		/// <summary>
		///  Adds an ellipse to the current path.
		/// </summary>
		public void AddEllipse(RectangleF rect)
			=> AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);

		/// <summary>
		///  Adds an ellipse to the current path.
		/// </summary>
		public void AddEllipse(int x, int y, int width, int height)
			=> AddEllipse((float)x, (float)y, (float)width, (float)height);

		/// <summary>
		///  Adds an ellipse to the current path.
		/// </summary>
		/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="width">The width of the bounding rectangle that defines the ellipse.</param>
		/// <param name="height">The height of the bounding rectangle that defines the ellipse.</param>
		public void AddEllipse(float x, float y, float width, float height)
		{
			ThrowIfDisposed();
			var rect = new SKRect(x + 0.5f, y + 0.5f, x + width + 0.5f, y + height + 0.5f);
			SKPath.AddOval(rect);
		}

		/// <summary>
		///  Appends a line segment to this <see cref="GraphicsPath"/>.
		/// </summary>
		public void AddLine(Point pt1, Point pt2)
			=> AddLine((float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y);

		/// <summary>
		///  Appends a line segment to this <see cref="GraphicsPath"/>.
		/// </summary>
		public void AddLine(PointF pt1, PointF pt2)
			=> AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);

		/// <summary>
		///  Appends a line segment to this <see cref="GraphicsPath"/>.
		/// </summary>
		public void AddLine(int x1, int y1, int x2, int y2)
			=> AddLine((float)x1, (float)y1, (float)x2, (float)y2);

		/// <summary>
		///  Appends a line segment to this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="x1">The x-coordinate of the starting point of the line.</param>
		/// <param name="y1">The y-coordinate of the starting point of the line.</param>
		/// <param name="x2">The x-coordinate of the endpoint of the line.</param>
		/// <param name="y2">The y-coordinate of the endpoint of the line.</param>
		public void AddLine(float x1, float y1, float x2, float y2)
		{
			ThrowIfDisposed();
			if (_needsNewFigure || SKPath.PointCount == 0)
			{
				SKPath.MoveTo(x1, y1);
				_needsNewFigure = false;
			}
			else
			{
				// If the last point is not at (x1,y1), move to it to start a connected segment.
				var last = SKPath.LastPoint;
				if (last.X != x1 || last.Y != y1)
					SKPath.MoveTo(x1, y1);
			}
			SKPath.LineTo(x2, y2);
		}

		/// <summary>
		///  Appends a series of connected line segments to the end of this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="points">An array of <see cref="PointF"/> structures that represents the points that define the line segments to add.</param>
		public void AddLines(PointF[] points)
		{
			ThrowIfDisposed();
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 2) return;

			if (_needsNewFigure || SKPath.PointCount == 0)
			{
				SKPath.MoveTo(points[0].X, points[0].Y);
				_needsNewFigure = false;
			}
			else
			{
				var last = SKPath.LastPoint;
				if (last.X != points[0].X || last.Y != points[0].Y)
					SKPath.MoveTo(points[0].X, points[0].Y);
			}
			for (int i = 1; i < points.Length; i++)
				SKPath.LineTo(points[i].X, points[i].Y);
		}

		/// <summary>
		///  Appends a series of connected line segments to the end of this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="points">An array of <see cref="Point"/> structures that represents the points that define the line segments to add.</param>
		public void AddLines(Point[] points)
		{
			ThrowIfDisposed();
			if (points is null) throw new ArgumentNullException(nameof(points));
			AddLines(ToPointFArray(points));
		}

		/// <summary>
		///  Appends the specified <see cref="GraphicsPath"/> to this path.
		/// </summary>
		/// <param name="addingPath">The <see cref="GraphicsPath"/> to add.</param>
		/// <param name="connect">A Boolean value that specifies whether the first figure in the added path is part of the last figure in this path.</param>
		public void AddPath(GraphicsPath addingPath, bool connect)
		{
			ThrowIfDisposed();
			if (addingPath is null) throw new ArgumentNullException(nameof(addingPath));
			if (connect && SKPath.PointCount > 0 && addingPath.SKPath.PointCount > 0)
			{
				// Connect the paths by continuing from the last point.
				SKPath.AddPath(addingPath.SKPath, SKPathAddMode.Append);
			}
			else
			{
				SKPath.AddPath(addingPath.SKPath, SKPathAddMode.Append);
			}
		}

		/// <summary>
		///  Adds a pie shape to this path.
		/// </summary>
		public void AddPie(Rectangle rect, float startAngle, float sweepAngle)
			=> AddPie((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, startAngle, sweepAngle);

		/// <summary>
		///  Adds a pie shape to this path.
		/// </summary>
		public void AddPie(int x, int y, int width, int height, float startAngle, float sweepAngle)
			=> AddPie((float)x, (float)y, (float)width, (float)height, startAngle, sweepAngle);

		/// <summary>
		///  Adds a pie shape to this path.
		/// </summary>
		/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
		/// <param name="width">The width of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
		/// <param name="height">The height of the bounding rectangle that defines the ellipse from which the pie is drawn.</param>
		/// <param name="startAngle">The starting angle for the pie section, measured in degrees clockwise from the x-axis.</param>
		/// <param name="sweepAngle">The angle between <paramref name="startAngle"/> and the end of the pie section, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
		public void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			ThrowIfDisposed();
			var rect = new SKRect(x + 0.5f, y + 0.5f, x + width + 0.5f, y + height + 0.5f);
			float cx = rect.MidX;
			float cy = rect.MidY;
			SKPath.MoveTo(cx, cy);
			SKPath.ArcTo(rect, startAngle, sweepAngle, false);
			SKPath.Close();
		}

		/// <summary>
		///  Adds a polygon to this path.
		/// </summary>
		/// <param name="points">An array of <see cref="PointF"/> structures that defines the polygon to add.</param>
		public void AddPolygon(PointF[] points)
		{
			ThrowIfDisposed();
			if (points is null) throw new ArgumentNullException(nameof(points));
			if (points.Length < 3) throw new ArgumentException("Array must contain at least 3 points.", nameof(points));

			SKPath.MoveTo(points[0].X, points[0].Y);
			for (int i = 1; i < points.Length; i++)
				SKPath.LineTo(points[i].X, points[i].Y);
			SKPath.Close();
		}

		/// <summary>
		///  Adds a polygon to this path.
		/// </summary>
		/// <param name="points">An array of <see cref="Point"/> structures that defines the polygon to add.</param>
		public void AddPolygon(Point[] points)
		{
			ThrowIfDisposed();
			if (points is null) throw new ArgumentNullException(nameof(points));
			AddPolygon(ToPointFArray(points));
		}

		/// <summary>
		///  Adds a rectangle to this path.
		/// </summary>
		public void AddRectangle(Rectangle rect)
			=> AddRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

		/// <summary>
		///  Adds a rectangle to this path.
		/// </summary>
		/// <param name="rect">A <see cref="RectangleF"/> that represents the rectangle to add.</param>
		public void AddRectangle(RectangleF rect)
		{
			ThrowIfDisposed();
			SKPath.AddRect(new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom));
		}

		/// <summary>
		///  Adds a series of rectangles to this path.
		/// </summary>
		/// <param name="rects">An array of <see cref="RectangleF"/> structures that represents the rectangles to add.</param>
		public void AddRectangles(RectangleF[] rects)
		{
			ThrowIfDisposed();
			if (rects is null) throw new ArgumentNullException(nameof(rects));
			foreach (var rect in rects)
				AddRectangle(rect);
		}

		/// <summary>
		///  Adds a series of rectangles to this path.
		/// </summary>
		/// <param name="rects">An array of <see cref="Rectangle"/> structures that represents the rectangles to add.</param>
		public void AddRectangles(Rectangle[] rects)
		{
			ThrowIfDisposed();
			if (rects is null) throw new ArgumentNullException(nameof(rects));
			foreach (var rect in rects)
				AddRectangle(rect);
		}

		/// <summary>
		///  Adds a text string to this path. Not yet implemented; requires Font support.
		/// </summary>
		public void AddString(string s, FontFamily family, int style, float emSize, Point origin, StringFormat? format) { throw new PlatformNotSupportedException("AddString is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a text string to this path. Not yet implemented; requires Font support.
		/// </summary>
		public void AddString(string s, FontFamily family, int style, float emSize, PointF origin, StringFormat? format) { throw new PlatformNotSupportedException("AddString is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a text string to this path. Not yet implemented; requires Font support.
		/// </summary>
		public void AddString(string s, FontFamily family, int style, float emSize, Rectangle layoutRect, StringFormat? format) { throw new PlatformNotSupportedException("AddString is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Adds a text string to this path. Not yet implemented; requires Font support.
		/// </summary>
		public void AddString(string s, FontFamily family, int style, float emSize, RectangleF layoutRect, StringFormat? format) { throw new PlatformNotSupportedException("AddString is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Clears all markers from this path.
		/// </summary>
		public void ClearMarkers()
		{
			ThrowIfDisposed();
			// Markers are not supported by SkiaSharp; no-op.
		}

		/// <summary>
		///  Creates an exact copy of this path.
		/// </summary>
		/// <returns>The <see cref="GraphicsPath"/> this method creates, cast as an object.</returns>
		public object Clone()
		{
			ThrowIfDisposed();
			var clone = new GraphicsPath();
			clone.SKPath.Dispose();
			clone.SKPath = new SKPath(SKPath);
			clone._needsNewFigure = _needsNewFigure;
			return clone;
		}

		/// <summary>
		///  Closes all open figures in this path and starts a new figure. It closes each open figure by connecting a line from its endpoint to its starting point.
		/// </summary>
		public void CloseAllFigures()
		{
			ThrowIfDisposed();
			// Iterate and close each contour.
			using var iter = SKPath.CreateIterator(false);
			var newPath = new SKPath { FillType = SKPath.FillType };
			var pts = new SKPoint[4];
			while (true)
			{
				var verb = iter.Next(pts);
				if (verb == SKPathVerb.Done) break;
				switch (verb)
				{
					case SKPathVerb.Move: newPath.MoveTo(pts[0]); break;
					case SKPathVerb.Line: newPath.LineTo(pts[1]); break;
					case SKPathVerb.Cubic: newPath.CubicTo(pts[1], pts[2], pts[3]); break;
					case SKPathVerb.Quad: newPath.QuadTo(pts[1], pts[2]); break;
					case SKPathVerb.Conic: newPath.ConicTo(pts[1], pts[2], iter.ConicWeight()); break;
					case SKPathVerb.Close: newPath.Close(); break;
				}
			}
			newPath.Close();
			var old = SKPath;
			SKPath = newPath;
			old.Dispose();
		}

		/// <summary>
		///  Closes the current figure and starts a new figure.
		/// </summary>
		public void CloseFigure()
		{
			ThrowIfDisposed();
			SKPath.Close();
		}

		/// <summary>
		///  Releases all resources used by this <see cref="GraphicsPath"/>.
		/// </summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				SKPath?.Dispose();
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///  Converts each curve in this path into a sequence of connected line segments. Not yet implemented.
		/// </summary>
		public void Flatten() { throw new PlatformNotSupportedException("Flatten is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Converts each curve in this path into a sequence of connected line segments. Not yet implemented.
		/// </summary>
		public void Flatten(Matrix? matrix) { throw new PlatformNotSupportedException("Flatten is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Converts each curve in this path into a sequence of connected line segments. Not yet implemented.
		/// </summary>
		public void Flatten(Matrix? matrix, float flatness) { throw new PlatformNotSupportedException("Flatten is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Returns a rectangle that bounds this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <returns>A <see cref="RectangleF"/> that represents a rectangle that bounds this <see cref="GraphicsPath"/>.</returns>
		public RectangleF GetBounds()
		{
			ThrowIfDisposed();
			var bounds = SKPath.Bounds;
			return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
		}

		/// <summary>
		///  Returns a rectangle that bounds this <see cref="GraphicsPath"/> when this path is transformed by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="matrix">The <see cref="Matrix"/> that specifies a transformation to be applied to this path before the bounding rectangle is calculated. This path is not permanently transformed; the transformation is used only during the process of calculating the bounding rectangle.</param>
		/// <returns>A <see cref="RectangleF"/> that represents a rectangle that bounds this <see cref="GraphicsPath"/>.</returns>
		public RectangleF GetBounds(Matrix? matrix)
		{
			ThrowIfDisposed();
			if (matrix is null || matrix.IsIdentity) return GetBounds();
			using var transformed = new SKPath(SKPath);
			transformed.Transform(matrix.SKMatrix);
			var bounds = transformed.Bounds;
			return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
		}

		/// <summary>
		///  Returns a rectangle that bounds this <see cref="GraphicsPath"/> when the current path is transformed by the specified <see cref="Matrix"/> and drawn with the specified <see cref="Pen"/>.
		/// </summary>
		/// <param name="matrix">The <see cref="Matrix"/> that specifies a transformation to be applied to this path before the bounding rectangle is calculated.</param>
		/// <param name="pen">The <see cref="Pen"/> with which to draw the <see cref="GraphicsPath"/>.</param>
		/// <returns>A <see cref="RectangleF"/> that represents a rectangle that bounds this <see cref="GraphicsPath"/>.</returns>
		public RectangleF GetBounds(Matrix? matrix, Pen? pen)
		{
			// Pen stroke expansion is not currently accounted for.
			return GetBounds(matrix);
		}

		/// <summary>
		///  Gets the last point in the <see cref="PathPoints"/> array of this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <returns>A <see cref="PointF"/> that represents the last point in this <see cref="GraphicsPath"/>.</returns>
		public PointF GetLastPoint()
		{
			ThrowIfDisposed();
			if (SKPath.PointCount == 0)
				throw new ArgumentException("Path has no points.");
			var last = SKPath.LastPoint;
			return new PointF(last.X, last.Y);
		}

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(Point point, Pen pen) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(Point pt, Pen pen, Graphics? graphics) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(PointF point, Pen pen) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(PointF pt, Pen pen, Graphics? graphics) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(int x, int y, Pen pen) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(int x, int y, Pen pen, Graphics? graphics) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(float x, float y, Pen pen) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within the outline of this <see cref="GraphicsPath"/> when drawn with the specified <see cref="Pen"/>.
		/// </summary>
		public bool IsOutlineVisible(float x, float y, Pen pen, Graphics? graphics) { throw new PlatformNotSupportedException("IsOutlineVisible is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		public bool IsVisible(Point point) => IsVisible((float)point.X, (float)point.Y);

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		public bool IsVisible(Point pt, Graphics? graphics) => IsVisible((float)pt.X, (float)pt.Y);

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		public bool IsVisible(PointF point) => IsVisible(point.X, point.Y);

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		public bool IsVisible(PointF pt, Graphics? graphics) => IsVisible(pt.X, pt.Y);

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		public bool IsVisible(int x, int y) => IsVisible((float)x, (float)y);

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		public bool IsVisible(int x, int y, Graphics? graphics) => IsVisible((float)x, (float)y);

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="x">The x-coordinate of the point to test.</param>
		/// <param name="y">The y-coordinate of the point to test.</param>
		/// <returns><see langword="true"/> if the specified point is contained within this path; otherwise, <see langword="false"/>.</returns>
		public bool IsVisible(float x, float y)
		{
			ThrowIfDisposed();
			return SKPath.Contains(x, y);
		}

		/// <summary>
		///  Indicates whether the specified point is contained within this <see cref="GraphicsPath"/>, using the specified <see cref="Graphics"/>.
		/// </summary>
		public bool IsVisible(float x, float y, Graphics? graphics)
		{
			return IsVisible(x, y);
		}

		/// <summary>
		///  Empties the <see cref="PathPoints"/> and <see cref="PathTypes"/> arrays and sets the <see cref="FillMode"/> to <see cref="FillMode.Alternate"/>.
		/// </summary>
		public void Reset()
		{
			ThrowIfDisposed();
			SKPath.Reset();
			_needsNewFigure = false;
		}

		/// <summary>
		///  Reverses the order of points in the <see cref="PathPoints"/> array of this <see cref="GraphicsPath"/>.
		/// </summary>
		public void Reverse()
		{
			ThrowIfDisposed();
			// Create a reversed copy of the path by iterating backwards.
			using var iter = SKPath.CreateIterator(false);
			var verbs = new System.Collections.Generic.List<(SKPathVerb verb, SKPoint[] pts, float weight)>();
			var pts = new SKPoint[4];
			while (true)
			{
				var verb = iter.Next(pts);
				if (verb == SKPathVerb.Done) break;
				verbs.Add((verb, (SKPoint[])pts.Clone(), verb == SKPathVerb.Conic ? iter.ConicWeight() : 0));
			}

			var newPath = new SKPath { FillType = SKPath.FillType };
			for (int i = verbs.Count - 1; i >= 0; i--)
			{
				var (verb, p, weight) = verbs[i];
				switch (verb)
				{
					case SKPathVerb.Move:
						// Becomes a move in the reversed path.
						newPath.MoveTo(p[0]);
						break;
					case SKPathVerb.Line:
						if (newPath.PointCount == 0) newPath.MoveTo(p[1]);
						newPath.LineTo(p[0]);
						break;
					case SKPathVerb.Cubic:
						if (newPath.PointCount == 0) newPath.MoveTo(p[3]);
						newPath.CubicTo(p[2], p[1], p[0]);
						break;
					case SKPathVerb.Quad:
						if (newPath.PointCount == 0) newPath.MoveTo(p[2]);
						newPath.QuadTo(p[1], p[0]);
						break;
					case SKPathVerb.Conic:
						if (newPath.PointCount == 0) newPath.MoveTo(p[2]);
						newPath.ConicTo(p[1], p[0], weight);
						break;
					case SKPathVerb.Close:
						newPath.Close();
						break;
				}
			}
			var old = SKPath;
			SKPath = newPath;
			old.Dispose();
		}

		/// <summary>
		///  Sets a marker on this <see cref="GraphicsPath"/>.
		/// </summary>
		public void SetMarkers()
		{
			ThrowIfDisposed();
			// Markers are not supported by SkiaSharp; no-op.
		}

		/// <summary>
		///  Starts a new figure without closing the current figure. All subsequent points added to the path are added to this new figure.
		/// </summary>
		public void StartFigure()
		{
			ThrowIfDisposed();
			_needsNewFigure = true;
		}

		/// <summary>
		///  Applies a transform matrix to this <see cref="GraphicsPath"/>.
		/// </summary>
		/// <param name="matrix">A <see cref="Matrix"/> that represents the transformation to apply.</param>
		public void Transform(Matrix matrix)
		{
			ThrowIfDisposed();
			if (matrix is null) throw new ArgumentNullException(nameof(matrix));
			SKPath.Transform(matrix.SKMatrix);
		}

		/// <summary>
		///  Applies a warp transform to this <see cref="GraphicsPath"/>. Not yet implemented.
		/// </summary>
		public void Warp(PointF[] destPoints, RectangleF srcRect) { throw new PlatformNotSupportedException("Warp is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Applies a warp transform to this <see cref="GraphicsPath"/>. Not yet implemented.
		/// </summary>
		public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix? matrix) { throw new PlatformNotSupportedException("Warp is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Applies a warp transform to this <see cref="GraphicsPath"/>. Not yet implemented.
		/// </summary>
		public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix? matrix, WarpMode warpMode) { throw new PlatformNotSupportedException("Warp is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Applies a warp transform to this <see cref="GraphicsPath"/>. Not yet implemented.
		/// </summary>
		public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix? matrix, WarpMode warpMode, float flatness) { throw new PlatformNotSupportedException("Warp is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Replaces this path with curves that enclose the area filled when this path is drawn by the specified pen. Not yet implemented.
		/// </summary>
		public void Widen(Pen pen) { throw new PlatformNotSupportedException("Widen is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Replaces this path with curves that enclose the area filled when this path is drawn by the specified pen. Not yet implemented.
		/// </summary>
		public void Widen(Pen pen, Matrix? matrix) { throw new PlatformNotSupportedException("Widen is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Replaces this path with curves that enclose the area filled when this path is drawn by the specified pen. Not yet implemented.
		/// </summary>
		public void Widen(Pen pen, Matrix? matrix, float flatness) { throw new PlatformNotSupportedException("Widen is not yet implemented in SkiaSharp.Drawing"); }

		/// <summary>
		///  Allows a <see cref="GraphicsPath"/> to attempt to free resources before being reclaimed by garbage collection.
		/// </summary>
		~GraphicsPath()
		{
			if (!_disposed)
			{
				SKPath?.Dispose();
				_disposed = true;
			}
		}

		private static SKPathFillType ToSKFillType(FillMode fillMode)
			=> fillMode == FillMode.Winding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;

		private static PointF[] ToPointFArray(Point[] points)
		{
			if (points is null) throw new ArgumentNullException(nameof(points));
			var result = new PointF[points.Length];
			for (int i = 0; i < points.Length; i++)
				result[i] = new PointF(points[i].X, points[i].Y);
			return result;
		}

		private void BuildPathFromPointsAndTypes(PointF[] pts, byte[] types)
		{
			int i = 0;
			while (i < pts.Length)
			{
				byte type = (byte)(types[i] & (byte)PathPointType.PathTypeMask);
				bool close = (types[i] & (byte)PathPointType.CloseSubpath) != 0;

				switch (type)
				{
					case (byte)PathPointType.Start:
						SKPath.MoveTo(pts[i].X, pts[i].Y);
						i++;
						break;
					case (byte)PathPointType.Line:
						SKPath.LineTo(pts[i].X, pts[i].Y);
						i++;
						break;
					case (byte)PathPointType.Bezier:
						if (i + 2 < pts.Length)
						{
							SKPath.CubicTo(pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y, pts[i + 2].X, pts[i + 2].Y);
							// Check close flag on the last point of the curve.
							close = (types[i + 2] & (byte)PathPointType.CloseSubpath) != 0;
							i += 3;
						}
						else
						{
							i++;
						}
						break;
					default:
						i++;
						break;
				}

				if (close)
					SKPath.Close();
			}
		}

		private byte[] ExtractPathTypes()
		{
			using var iter = SKPath.CreateIterator(false);
			var typesList = new System.Collections.Generic.List<byte>();
			var pts = new SKPoint[4];

			SKPathVerb? nextVerb = null;
			while (true)
			{
				var verb = iter.Next(pts);
				if (verb == SKPathVerb.Done) break;

				switch (verb)
				{
					case SKPathVerb.Move:
						typesList.Add((byte)PathPointType.Start);
						break;
					case SKPathVerb.Line:
						typesList.Add((byte)PathPointType.Line);
						break;
					case SKPathVerb.Cubic:
						typesList.Add((byte)PathPointType.Bezier);
						typesList.Add((byte)PathPointType.Bezier);
						typesList.Add((byte)PathPointType.Bezier);
						break;
					case SKPathVerb.Quad:
						typesList.Add((byte)PathPointType.Bezier);
						typesList.Add((byte)PathPointType.Bezier);
						break;
					case SKPathVerb.Close:
						// Mark the last point as closing the subpath.
						if (typesList.Count > 0)
							typesList[typesList.Count - 1] |= (byte)PathPointType.CloseSubpath;
						break;
				}
			}

			return typesList.ToArray();
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(GraphicsPath));
		}
	}
}
