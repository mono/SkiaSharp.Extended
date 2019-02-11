using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended
{
	public class SKPathInterpolation
	{
		private List<SKPoint> pointsFrom;
		private List<SKPoint> pointsTo;

		public SKPathInterpolation(SKPath from, SKPath to, float maxSegmentLength = 5f)
		{
			if (from == null)
				throw new ArgumentNullException(nameof(from));
			if (to == null)
				throw new ArgumentNullException(nameof(to));

			From = from;
			To = to;
			MaxSegmentLength = maxSegmentLength;
		}

		public SKPath From { get; private set; }

		public SKPath To { get; private set; }

		public float MaxSegmentLength { get; private set; }

		public void Prepare()
		{
			pointsFrom = NormalizePath(From, MaxSegmentLength);
			pointsTo = NormalizePath(To, MaxSegmentLength);
			InterpolatePath(pointsFrom, pointsTo);
		}

		public SKPath Interpolate(float t)
		{
			if (pointsFrom == null || pointsTo == null)
			{
				Prepare();
			}

			var points = Interpolate(pointsFrom, pointsTo, t);

			var path = new SKPath();
			path.AddPoly(points.ToArray());
			return path;
		}

		private static List<SKPoint> NormalizePath(SKPath parsed, float maxSegmentLength)
		{
			var skipBisect = false;
			var points = CreateLinearPathPoints(parsed);
			if (points == null)
			{
				points = CreateCurvedPathPoints(parsed, maxSegmentLength);
				skipBisect = true;
			}

			// TODO: 
			//// no duplicate closing point for now
			//if (points.Count > 1 && SKGeometry.Distance(points[0], points[points.Count - 1]) < 1e-9f)
			//{
			//	points.pop();
			//}

			// 3+ points to make a polygon
			if (points.Count < 3)
			{
				return null;
			}

			// make all rings clockwise
			if (SKGeometry.Area(points) > 0)
			{
				points.Reverse();
			}

			if (!skipBisect && maxSegmentLength > 0)
			{
				Bisect(points, maxSegmentLength);
			}

			return points;
		}

		private static List<SKPoint> CreateLinearPathPoints(SKPath path)
		{
			var ring = new List<SKPoint>();

			using (var iterator = path.CreateRawIterator())
			{
				var points = new SKPoint[4];

				// we must start with a move
				var verb = iterator.Next(points);
				if (verb == SKPathVerb.Move)
				{
					ring.Add(points[0]);

					// loop through all the lines
					while ((verb = iterator.Next(points)) == SKPathVerb.Line)
					{
						ring.Add(points[1]);
					}

					// if we encountered a curve of some sort, then this is invalid
					if (verb != SKPathVerb.Move && verb != SKPathVerb.Close && verb != SKPathVerb.Done)
					{
						ring.Clear();
					}
				}
			}

			return ring.Count > 0 ? ring : null;
		}

		private static List<SKPoint> CreateCurvedPathPoints(SKPath path, float maxSegmentLength)
		{
			using (var m = new SKPathMeasure(path))
			{
				var len = m.Length;

				var numPoints = 3;
				if (maxSegmentLength > 0)
				{
					numPoints = Math.Max(numPoints, (int)Math.Ceiling(len / maxSegmentLength));
				}

				var ring = new List<SKPoint>();
				for (var i = 0; i < numPoints; i++)
				{
					SKPoint p;
					m.GetPosition(len * i / numPoints, out p);
					ring.Add(p);
				}

				return ring;
			}
		}

		private static void Bisect(List<SKPoint> ring, float maxSegmentLength)
		{
			// skip if we have asked that the path not be bisected
			if (maxSegmentLength <= 0)
			{
				return;
			}

			for (var i = 0; i < ring.Count; i++)
			{
				var a = ring[i];
				var b = i == ring.Count - 1 ? ring[0] : ring[i + 1];

				// could splice the whole set for a segment instead, but a bit messy
				while (SKGeometry.Distance(a, b) > maxSegmentLength)
				{
					b = SKGeometry.PointAlong(a, b, 0.5f);
					ring.Insert(i + 1, b);
				}
			}
		}

		private static void Subdivide(List<SKPoint> path, int desiredPointCount)
		{
			var desiredLength = path.Count + desiredPointCount;
			var step = SKGeometry.Perimeter(path) / desiredPointCount;

			var i = 0;
			var cursor = 0.0f;
			var insertAt = step / 2f;

			while (path.Count < desiredLength)
			{
				var a = path[i];
				var b = path[(i + 1) % path.Count];
				var segment = SKGeometry.Distance(a, b);

				if (insertAt <= cursor + segment)
				{
					path.Insert(i + 1, segment != 0 ? SKGeometry.PointAlong(a, b, (insertAt - cursor) / segment) : a);
					insertAt += step;
					continue;
				}

				cursor += segment;
				i++;
			}
		}

		private static void Rotate(List<SKPoint> from, List<SKPoint> to)
		{
			var bestOffset = 0;

			var min = float.PositiveInfinity;
			var len = from.Count;
			for (var offset = 0; offset < len; offset++)
			{
				var sumOfSquares = 0.0f;
				for (var i = 0; i < len; i++)
				{
					var d = SKGeometry.Distance(from[(offset + i) % len], to[i]);
					sumOfSquares += d * d;
				}

				if (sumOfSquares < min)
				{
					min = sumOfSquares;
					bestOffset = offset;
				}
			}

			if (bestOffset != 0)
			{
				Rotate(from, bestOffset);
			}
		}

		private static void Rotate<T>(IList<T> items, int offset)
		{
			var len = items.Count;
			offset = offset % len;

			if (offset == 0 || len == 0)
				return;

			var changes = 0;
			for (int start = 0; changes < items.Count - 1; start++)
			{
				var curr = start;
				var next = (curr + offset + len) % len;
				var temp = items[curr];

				while (next != start)
				{
					items[curr] = items[next];
					changes++;

					curr = next;
					next = (curr + offset + len) % len;
				}

				items[curr] = temp;
				changes++;
			}
		}

		private static void InterpolatePath(List<SKPoint> from, List<SKPoint> to)
		{
			var diff = from.Count - to.Count;
			if (diff < 0)
			{
				Subdivide(from, diff * -1);
			}
			else if (diff > 0)
			{
				Subdivide(to, diff);
			}

			Rotate(from, to);
		}

		private static List<SKPoint> Interpolate(List<SKPoint> from, List<SKPoint> to, float t)
		{
			return from.Zip(to, (a, b) => InterpolatePoint(a, b, t)).ToList();
		}

		private static SKPoint InterpolatePoint(SKPoint a, SKPoint b, float t)
		{
			return new SKPoint(
				a.X + t * (b.X - a.X),
				a.Y + t * (b.Y - a.Y));
		}
	}
}
