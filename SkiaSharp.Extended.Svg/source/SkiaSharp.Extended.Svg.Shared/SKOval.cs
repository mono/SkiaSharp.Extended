using System;
using System.Linq;

namespace SkiaSharp.Extended.Svg
{
	internal struct SKOval
	{
		public SKPoint Center { get; }

		public float RadiusX { get; }
		public float RadiusY { get; }

		public SKOval(SKPoint center, float rx, float ry)
		{
			Center = center;
			RadiusX = rx;
			RadiusY = ry;
		}

		public SKRect BoundingRect => new SKRect(Center.X - RadiusX, Center.Y - RadiusY, Center.X + RadiusX, Center.Y + RadiusY);
	}
}
