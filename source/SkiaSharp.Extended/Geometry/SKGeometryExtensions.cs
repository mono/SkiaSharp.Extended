namespace SkiaSharp.Extended
{
	/// <summary>
	/// Provides extension methods on <see cref="SKCanvas"/> for drawing geometric shapes centered at a given point.
	/// </summary>
	public static class SKGeometryExtensions
	{
		/// <summary>
		/// Draws a square centered at the specified point.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="c">The center point of the square.</param>
		/// <param name="side">The length of each side of the square.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawSquare(this SKCanvas canvas, SKPoint c, float side, SKPaint paint) =>
			DrawSquare(canvas, c.X, c.Y, side, paint);

		/// <summary>
		/// Draws a square centered at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="cx">The x-coordinate of the center.</param>
		/// <param name="cy">The y-coordinate of the center.</param>
		/// <param name="side">The length of each side of the square.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawSquare(this SKCanvas canvas, float cx, float cy, float side, SKPaint paint)
		{
			var path = SKGeometry.CreateSquarePath(side);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		/// <summary>
		/// Draws a triangle centered at the specified point with the given radii.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="c">The center point of the triangle.</param>
		/// <param name="r">The horizontal and vertical radii of the triangle's bounding box.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawTriangle(this SKCanvas canvas, SKPoint c, SKSize r, SKPaint paint) =>
			DrawTriangle(canvas, c.X, c.Y, r.Width, r.Height, paint);

		/// <summary>
		/// Draws a triangle centered at the specified coordinates with the given horizontal and vertical radii.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="cx">The x-coordinate of the center.</param>
		/// <param name="cy">The y-coordinate of the center.</param>
		/// <param name="rx">The horizontal radius (half the triangle width).</param>
		/// <param name="ry">The vertical radius (half the triangle height).</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawTriangle(this SKCanvas canvas, float cx, float cy, float rx, float ry, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(rx * 2, ry * 2);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		/// <summary>
		/// Draws a triangle inscribed within the specified rectangle.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="rect">The bounding rectangle for the triangle.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawTriangle(this SKCanvas canvas, SKRect rect, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(rect.Width, rect.Height);
			path.Offset(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
			canvas.DrawPath(path, paint);
		}

		/// <summary>
		/// Draws a regular (equilateral) triangle centered at the specified point with the given circumscribed radius.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="c">The center point of the triangle.</param>
		/// <param name="radius">The circumscribed circle radius.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawTriangle(this SKCanvas canvas, SKPoint c, float radius, SKPaint paint) =>
			DrawTriangle(canvas, c.X, c.Y, radius, paint);

		/// <summary>
		/// Draws a regular (equilateral) triangle centered at the specified coordinates with the given circumscribed radius.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="cx">The x-coordinate of the center.</param>
		/// <param name="cy">The y-coordinate of the center.</param>
		/// <param name="radius">The circumscribed circle radius.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawTriangle(this SKCanvas canvas, float cx, float cy, float radius, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(radius);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		/// <summary>
		/// Draws a regular polygon centered at the specified point.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="c">The center point of the polygon.</param>
		/// <param name="radius">The circumscribed circle radius.</param>
		/// <param name="points">The number of vertices of the polygon.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawRegularPolygon(this SKCanvas canvas, SKPoint c, float radius, int points, SKPaint paint) =>
			DrawRegularPolygon(canvas, c.X, c.Y, radius, points, paint);

		/// <summary>
		/// Draws a regular polygon centered at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="cx">The x-coordinate of the center.</param>
		/// <param name="cy">The y-coordinate of the center.</param>
		/// <param name="radius">The circumscribed circle radius.</param>
		/// <param name="points">The number of vertices of the polygon.</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawRegularPolygon(this SKCanvas canvas, float cx, float cy, float radius, int points, SKPaint paint)
		{
			var path = SKGeometry.CreateRegularPolygonPath(radius, points, true);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		/// <summary>
		/// Draws a star centered at the specified point.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="c">The center point of the star.</param>
		/// <param name="outerRadius">The radius of the outer vertices.</param>
		/// <param name="innerRadius">The radius of the inner vertices.</param>
		/// <param name="points">The number of star points (outer vertices).</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawStar(this SKCanvas canvas, SKPoint c, float outerRadius, float innerRadius, int points, SKPaint paint) =>
			DrawStar(canvas, c.X, c.Y, outerRadius, innerRadius, points, paint);

		/// <summary>
		/// Draws a star centered at the specified coordinates.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="cx">The x-coordinate of the center.</param>
		/// <param name="cy">The y-coordinate of the center.</param>
		/// <param name="outerRadius">The radius of the outer vertices.</param>
		/// <param name="innerRadius">The radius of the inner vertices.</param>
		/// <param name="points">The number of star points (outer vertices).</param>
		/// <param name="paint">The paint to use for drawing.</param>
		public static void DrawStar(this SKCanvas canvas, float cx, float cy, float outerRadius, float innerRadius, int points, SKPaint paint)
		{
			var path = SKGeometry.CreateRegularStarPath(outerRadius, innerRadius, points);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}
	}
}
