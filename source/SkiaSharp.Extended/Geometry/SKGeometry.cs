using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended
{
	public static class SKGeometry
	{
		public const float PI = (float)Math.PI;

		private const float UprightAngle = PI / 2f;
		private const float TotalAngle = 2f * PI;

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

		public static SKPoint CirclePoint(float radius, float radians) =>
			new SKPoint(radius * (float)Math.Cos(radians), radius * (float)Math.Sin(radians));

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

		public static SKPoint PointAlong(SKPoint a, SKPoint b, float pct) =>
			new SKPoint(a.X + (b.X - a.X) * pct, a.Y + (b.Y - a.Y) * pct);

		public static SKPath CreateSquarePath(float side, SKPathDirection direction = SKPathDirection.Clockwise) =>
			CreateRectanglePath(side, side, direction);

		public static SKPath CreateRectanglePath(float width, float height, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();
			path.AddRect(new SKRect(width / -2, height / -2, width / 2, height / 2), direction);
			path.Close();
			return path;
		}

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

		public static SKPath CreateTrianglePath(float radius, SKPathDirection direction = SKPathDirection.Clockwise) =>
			CreateRegularPolygonPath(radius, 3, false, direction);

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
