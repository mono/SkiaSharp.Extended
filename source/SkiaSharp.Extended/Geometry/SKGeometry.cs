using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended
{
	/// <summary>
	/// Provides geometry utility methods for creating paths, calculating polygon properties, and interpolating between paths.
	/// </summary>
	public static class SKGeometry
	{
		/// <summary>
		/// The value of π as a <see cref="float"/>.
		/// </summary>
		public const float PI = (float)Math.PI;

		private const float UprightAngle = PI / 2f;
		private const float TotalAngle = 2f * PI;

		/// <summary>
		/// Creates a new <see cref="SKPath"/> that is an interpolation between two paths at the given position.
		/// </summary>
		/// <param name="from">The starting path.</param>
		/// <param name="to">The ending path.</param>
		/// <param name="t">The interpolation position, where 0 returns <paramref name="from"/> and 1 returns <paramref name="to"/>.</param>
		/// <returns>An interpolated <see cref="SKPath"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="from"/> or <paramref name="to"/> is <c>null</c>.</exception>
		public static SKPath CreateInterpolation(SKPath from, SKPath to, float t)
		{
			if (from == null)
				throw new ArgumentNullException(nameof(from));
			if (to == null)
				throw new ArgumentNullException(nameof(to));

			if (t <= float.Epsilon)
				return from;

			if (1f - t <= float.Epsilon)
				return to;

			var interpolation = new SKPathInterpolation(from, to);
			return interpolation.Interpolate(t);
		}

		/// <summary>
		/// Calculates a point on a circle with the given radius at the specified angle.
		/// </summary>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="radians">The angle in radians.</param>
		/// <returns>The <see cref="SKPoint"/> on the circle.</returns>
		public static SKPoint CirclePoint(float radius, float radians) =>
			new SKPoint(radius * (float)Math.Cos(radians), radius * (float)Math.Sin(radians));

		/// <summary>
		/// Calculates the signed area of a polygon defined by a list of points using the shoelace formula.
		/// </summary>
		/// <param name="polygon">The list of points defining the polygon.</param>
		/// <returns>The signed area of the polygon. Returns 0 if the polygon has fewer than 3 points.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="polygon"/> is <c>null</c>.</exception>
		public static float Area(IList<SKPoint> polygon)
		{
			if (polygon == null)
				throw new ArgumentNullException(nameof(polygon));

			var len = polygon.Count;

			// a polygon must have at least 3 points
			if (len < 3)
				return 0;

			var a = SKPoint.Empty;
			var b = polygon[len - 1];

			var area = 0.0f;

			var i = -1;
			while (++i < len)
			{
				a = b;
				b = polygon[i];
				area += a.Y * b.X - a.X * b.Y;
			}

			return area / 2f;
		}

		/// <summary>
		/// Calculates the perimeter of a polygon defined by a list of points.
		/// </summary>
		/// <param name="polygon">The list of points defining the polygon.</param>
		/// <returns>The perimeter of the polygon. Returns 0 if the polygon has fewer than 2 points.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="polygon"/> is <c>null</c>.</exception>
		public static float Perimeter(IList<SKPoint> polygon)
		{
			if (polygon == null)
				throw new ArgumentNullException(nameof(polygon));

			var len = polygon.Count;

			// a line must have at least 2 points
			if (len < 2)
				return 0;

			var perimeter = 0.0f;

			for (var i = 0; i < len - 1; i++)
			{
				perimeter += SKPoint.Distance(polygon[i], polygon[i + 1]);
			}

			perimeter += SKPoint.Distance(polygon[0], polygon[len - 1]);

			return perimeter;
		}

		/// <summary>
		/// Calculates a point at a given percentage along the line segment from <paramref name="a"/> to <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The start point of the segment.</param>
		/// <param name="b">The end point of the segment.</param>
		/// <param name="pct">The percentage along the segment (0.0 to 1.0).</param>
		/// <returns>The interpolated <see cref="SKPoint"/>.</returns>
		public static SKPoint PointAlong(SKPoint a, SKPoint b, float pct) =>
			new SKPoint(a.X + (b.X - a.X) * pct, a.Y + (b.Y - a.Y) * pct);

		/// <summary>
		/// Creates a square path centered at the origin with the specified side length.
		/// </summary>
		/// <param name="side">The length of each side of the square.</param>
		/// <param name="direction">The winding direction of the path.</param>
		/// <returns>A new <see cref="SKPath"/> representing the square.</returns>
		public static SKPath CreateSquarePath(float side, SKPathDirection direction = SKPathDirection.Clockwise) =>
			CreateRectanglePath(side, side, direction);

		/// <summary>
		/// Creates a rectangle path centered at the origin with the specified dimensions.
		/// </summary>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		/// <param name="direction">The winding direction of the path.</param>
		/// <returns>A new <see cref="SKPath"/> representing the rectangle.</returns>
		public static SKPath CreateRectanglePath(float width, float height, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();
			path.AddRect(new SKRect(width / -2, height / -2, width / 2, height / 2), direction);
			path.Close();
			return path;
		}

		/// <summary>
		/// Creates a triangle path centered at the origin with the specified bounding width and height.
		/// </summary>
		/// <param name="width">The width of the triangle's bounding box.</param>
		/// <param name="height">The height of the triangle's bounding box.</param>
		/// <param name="direction">The winding direction of the path.</param>
		/// <returns>A new <see cref="SKPath"/> representing the triangle.</returns>
		public static SKPath CreateTrianglePath(float width, float height, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();
			path.MoveTo(0, height / -2);
			if (direction == SKPathDirection.Clockwise)
			{
				path.LineTo(width / -2, height / 2);
				path.LineTo(width / 2, height / 2);
			}
			else
			{
				path.LineTo(width / 2, height / 2);
				path.LineTo(width / -2, height / 2);
			}
			path.Close();
			return path;
		}

		/// <summary>
		/// Creates a regular triangle (equilateral) path inscribed in a circle of the specified radius, centered at the origin.
		/// </summary>
		/// <param name="radius">The circumscribed circle radius.</param>
		/// <param name="direction">The winding direction of the path.</param>
		/// <returns>A new <see cref="SKPath"/> representing the equilateral triangle.</returns>
		public static SKPath CreateTrianglePath(float radius, SKPathDirection direction = SKPathDirection.Clockwise) =>
			CreateRegularPolygonPath(radius, 3, false, direction);

		/// <summary>
		/// Creates a regular polygon path inscribed in a circle of the specified radius, centered at the origin.
		/// </summary>
		/// <param name="radius">The circumscribed circle radius.</param>
		/// <param name="points">The number of vertices of the polygon.</param>
		/// <param name="horizontalBase">If <c>true</c>, rotates the polygon so the base edge is horizontal (for even-sided polygons).</param>
		/// <param name="direction">The winding direction of the path.</param>
		/// <returns>A new <see cref="SKPath"/> representing the regular polygon.</returns>
		public static SKPath CreateRegularPolygonPath(float radius, int points, bool horizontalBase = true, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			var stepAngle = direction == SKPathDirection.CounterClockwise
				? -TotalAngle / points
				: TotalAngle / points;

			var startAngle = horizontalBase && points % 2 == 0
				? stepAngle / 2
				: 0;

			for (int p = 0; p < points; p++)
			{
				float angle = startAngle + (stepAngle * p - UprightAngle);
				float x = radius * (float)Math.Cos(angle);
				float y = radius * (float)Math.Sin(angle);

				if (p == 0)
					path.MoveTo(x, y);
				else
					path.LineTo(x, y);
			}

			path.Close();
			return path;
		}

		/// <summary>
		/// Creates a regular star path centered at the origin with alternating outer and inner vertices.
		/// </summary>
		/// <param name="outerRadius">The radius of the outer vertices.</param>
		/// <param name="innerRadius">The radius of the inner vertices.</param>
		/// <param name="points">The number of star points (outer vertices).</param>
		/// <param name="direction">The winding direction of the path.</param>
		/// <returns>A new <see cref="SKPath"/> representing the star.</returns>
		public static SKPath CreateRegularStarPath(float outerRadius, float innerRadius, int points, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			bool isInner = false;
			points *= 2;

			var stepAngle = direction == SKPathDirection.CounterClockwise
				? -TotalAngle / points
				: TotalAngle / points;

			for (int p = 0; p < points; p++)
			{
				float radius = isInner ? innerRadius : outerRadius;

				float angle = stepAngle * p - UprightAngle;
				float x = radius * (float)Math.Cos(angle);
				float y = radius * (float)Math.Sin(angle);

				if (p == 0)
					path.MoveTo(x, y);
				else
					path.LineTo(x, y);

				isInner = !isInner;
			}

			path.Close();
			return path;
		}
	}
}
