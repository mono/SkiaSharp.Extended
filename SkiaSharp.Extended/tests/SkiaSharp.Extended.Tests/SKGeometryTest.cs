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
	}
}
