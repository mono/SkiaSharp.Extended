namespace SkiaSharp.Extended
{
	public static class SKGeometryExtensions
	{
		public static void DrawSquare(this SKCanvas canvas, SKPoint c, float side, SKPaint paint) =>
			DrawSquare(canvas, c.X, c.Y, side, paint);

		public static void DrawSquare(this SKCanvas canvas, float cx, float cy, float side, SKPaint paint)
		{
			var path = SKGeometry.CreateSquarePath(side);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawTriangle(this SKCanvas canvas, SKPoint c, SKSize r, SKPaint paint) =>
			DrawTriangle(canvas, c.X, c.Y, r.Width, r.Height, paint);

		public static void DrawTriangle(this SKCanvas canvas, float cx, float cy, float rx, float ry, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(rx * 2, ry * 2);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawTriangle(this SKCanvas canvas, SKRect rect, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(rect.Width, rect.Height);
			path.Offset(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
			canvas.DrawPath(path, paint);
		}

		public static void DrawTriangle(this SKCanvas canvas, SKPoint c, float radius, SKPaint paint) =>
			DrawTriangle(canvas, c.X, c.Y, radius, paint);

		public static void DrawTriangle(this SKCanvas canvas, float cx, float cy, float radius, SKPaint paint)
		{
			var path = SKGeometry.CreateTrianglePath(radius);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawRegularPolygon(this SKCanvas canvas, SKPoint c, float radius, int points, SKPaint paint) =>
			DrawRegularPolygon(canvas, c.X, c.Y, radius, points, paint);

		public static void DrawRegularPolygon(this SKCanvas canvas, float cx, float cy, float radius, int points, SKPaint paint)
		{
			var path = SKGeometry.CreateRegularPolygonPath(radius, points, true);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}

		public static void DrawStar(this SKCanvas canvas, SKPoint c, float outerRadius, float innerRadius, int points, SKPaint paint) =>
			DrawStar(canvas, c.X, c.Y, outerRadius, innerRadius, points, paint);

		public static void DrawStar(this SKCanvas canvas, float cx, float cy, float outerRadius, float innerRadius, int points, SKPaint paint)
		{
			var path = SKGeometry.CreateRegularStarPath(outerRadius, innerRadius, points);
			path.Offset(cx, cy);
			canvas.DrawPath(path, paint);
		}
	}
}
