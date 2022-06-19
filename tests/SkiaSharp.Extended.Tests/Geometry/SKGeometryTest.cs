using Xunit;

namespace SkiaSharp.Extended.Tests
{
	public class SKGeometryTest
	{
		[Fact]
		public void GeometryGeneratesRectPath()
		{
			var rectPath = SKGeometry.CreateTrianglePath(100);

			Assert.Equal(3, rectPath.PointCount);
		}

		[Fact]
		public void TriangleAreaCalculation()
		{
			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(10, 20),
				new SKPoint(20, 20),
			};
			var area = SKGeometry.Area(points);

			Assert.Equal(50f, area);
		}

		[Fact]
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

			Assert.Equal(200f, area);
		}

		[Fact]
		public void PerimeterCalculation()
		{
			var points = new[]
			{
				new SKPoint(10, 10),
				new SKPoint(10, 40),
				new SKPoint(50, 40),
			};
			var perimeter = SKGeometry.Perimeter(points);

			Assert.Equal(120f, perimeter);
		}

		[Fact]
		public void CreateInterpolationReturnsOriginals()
		{
			var path1 = new SKPath();
			var path2 = new SKPath();

			var interpolated = SKGeometry.CreateInterpolation(path1, path2, -1);
			Assert.Equal(path1, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 0);
			Assert.Equal(path1, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 0.5f);
			Assert.NotEqual(path1, interpolated);
			Assert.NotEqual(path2, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 1);
			Assert.Equal(path2, interpolated);

			interpolated = SKGeometry.CreateInterpolation(path1, path2, 2);
			Assert.Equal(path2, interpolated);
		}
	}
}
