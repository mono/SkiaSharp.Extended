using NUnit.Framework;

namespace SkiaSharp.Extended.Tests
{
	public class SKGeometryTest
	{
		[Test]
		public void GeometryGeneratesRectPath()
		{
			var rectPath = SKGeometry.CreateTrianglePath(100);

			Assert.AreEqual(3, rectPath.PointCount);
		}

		[Test]
		public void TriangleAreaCalculation()
		{
			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(10, 20),
				new SKPoint(20, 20),
			};
			var area = SKGeometry.Area(points);

			Assert.AreEqual(50f, area);
		}

		[Test]
		public void RectangleAreaCalculation()
		{
			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(10, 20),
				new SKPoint(30, 20),
				new SKPoint(30, 10),
			};
			var area = SKGeometry.Area(points);

			Assert.AreEqual(200f, area);
		}

		[Test]
		public void PerimeterCalculation()
		{
			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(10, 40),
				new SKPoint(50, 40),
			};
			var perimeter = SKGeometry.Perimeter(points);

			Assert.AreEqual(120f, perimeter);
		}

		[Test]
		public void DistanceCalculation()
		{
			var dist = SKGeometry.Distance(new SKPoint(10, 10), new SKPoint(10, 40));

			Assert.AreEqual(30f, dist);
		}

		[Test]
		public void CreateInterpolationReturnsOriginals()
		{
			var path1 = new SKPath();
			var path2 = new SKPath();

			var interpolated = SKGeometry.CreateInterpolation(path1, path2, -1);
			Assert.AreEqual(path1, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 0);
			Assert.AreEqual(path1, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 0.5f);
			Assert.AreNotEqual(path1, interpolated);
			Assert.AreNotEqual(path2, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 1);
			Assert.AreEqual(path2, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 2);
			Assert.AreEqual(path2, interpolated);
		}
	}
}
