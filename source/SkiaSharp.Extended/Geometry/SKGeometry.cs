using System;
using System.Collections.Generic;
using System.Linq;

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
			{
				return from;
			}

			if (1f - t <= float.Epsilon)
			{
				return to;
			}

			var interpolation = new SKPathInterpolation(from, to);
			return interpolation.Interpolate(t);
		}

		public static SKPoint CirclePoint(float radius, float radians)
		{
			return new SKPoint(radius * (float)Math.Cos(radians), radius * (float)Math.Sin(radians));
		}

		public static float Area(IList<SKPoint> polygon)
		{
			if (polygon == null)
				throw new ArgumentNullException(nameof(polygon));

			var len = polygon.Count;

			// a polygon must have at least 3 points
			if (len < 3)
			{
				return 0;
			}

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

		public static float Perimeter(IList<SKPoint> polygon, bool close = true)
		{
			if (polygon == null)
				throw new ArgumentNullException(nameof(polygon));

			var len = polygon.Count;

			// a line must have at least 2 points
			if (len < 2)
			{
				return 0;
			}

			var perimeter = 0.0f;

			for (var i = 0; i < len - 1; i++)
			{
				perimeter += Distance(polygon[i], polygon[i + 1]);
			}
			if (close)
			{
				perimeter += Distance(polygon[0], polygon[len - 1]);
			}

			return perimeter;
		}

		public static float Distance(SKPoint a, SKPoint b)
		{
			var dx = a.X - b.X;
			var dy = a.Y - b.Y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

		public static SKPoint PointAlong(SKPoint a, SKPoint b, float pct)
		{
			return new SKPoint(a.X + (b.X - a.X) * pct, a.Y + (b.Y - a.Y) * pct);
		}

		public static SKPath CreateSectorPath(float start, float end, float outerRadius, float innerRadius = 0.0f, float margin = 0.0f, float explodeDistance = 0.0f, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			// if the sector has no size, then it has no path
			if (start == end)
			{
				return path;
			}

			// the the sector is a full circle, then do that
			if (end - start == 1.0f)
			{
				path.AddCircle(0, 0, outerRadius, direction);
				path.AddCircle(0, 0, innerRadius, direction);
				path.FillType = SKPathFillType.EvenOdd;
				return path;
			}

			// calculate the angles
			var startAngle = TotalAngle * start - UprightAngle;
			var endAngle = TotalAngle * end - UprightAngle;
			var large = endAngle - startAngle > PI ? SKPathArcSize.Large : SKPathArcSize.Small;
			var sectorCenterAngle = (endAngle - startAngle) / 2f + startAngle;

			//// get the radius bits
			//var sectorCenterRadius = (outerRadius - innerRadius) / 2f + innerRadius;

			// move explosion around 90 degrees, since matrix use down as 0
			var explosionMatrix = SKMatrix.MakeRotation(sectorCenterAngle - (PI / 2f));
			var offset = explosionMatrix.MapPoint(new SKPoint(0, explodeDistance));

			// calculate the angle for the margins
			margin = direction == SKPathDirection.Clockwise ? margin : -margin;
			var offsetR = outerRadius == 0 ? 0 : ((margin / (TotalAngle * outerRadius)) * TotalAngle);
			var offsetr = innerRadius == 0 ? 0 : ((margin / (TotalAngle * innerRadius)) * TotalAngle);

			// get the points
			var a = CirclePoint(outerRadius, startAngle + offsetR) + offset;
			var b = CirclePoint(outerRadius, endAngle - offsetR) + offset;
			var c = CirclePoint(innerRadius, endAngle - offsetr) + offset;
			var d = CirclePoint(innerRadius, startAngle + offsetr) + offset;

			// add the points to the path
			path.MoveTo(a);
			path.ArcTo(outerRadius, outerRadius, 0, large, direction, b.X, b.Y);
			path.LineTo(c);
			if (innerRadius == 0.0f)
			{
				// take a short cut
				path.LineTo(d);
			}
			else
			{
				var reverseDirection = direction == SKPathDirection.Clockwise ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise;
				path.ArcTo(innerRadius, innerRadius, 0, large, reverseDirection, d.X, d.Y);
			}
			path.Close();

			return path;
		}

		public static SKPath CreatePiePath(IEnumerable<float> sectorSizes, float outerRadius, float innerRadius = 0.0f, float spacing = 0.0f, float explodeDistance = 0.0f, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			float cursor = 0;
			//var sum = sectorSizes.Sum();
			foreach (var sectorSize in sectorSizes)
			{
				var sector = CreateSectorPath(cursor, cursor + sectorSize, outerRadius, innerRadius, spacing / 2f, explodeDistance, direction);

				cursor += sectorSize;

				path.AddPath(sector, SKPathAddMode.Append);
			}

			return path;
		}

		public static SKPath CreateSquarePath(float side, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			return CreateRectanglePath(side, side, direction);
		}

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

		public static SKPath CreateTrianglePath(float radius, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			return CreateRegularPolygonPath(radius, 3, direction);
		}

		public static SKPath CreateRegularPolygonPath(float radius, int points, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			var path = new SKPath();

			float stepAngle = TotalAngle / points;
			if (direction == SKPathDirection.CounterClockwise)
			{
				stepAngle = -stepAngle;
			}

			for (int p = 0; p < points; p++)
			{
				float angle = stepAngle * p - UprightAngle;
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

			float stepAngle = TotalAngle / points;
			if (direction == SKPathDirection.CounterClockwise)
			{
				stepAngle = -stepAngle;
			}

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
