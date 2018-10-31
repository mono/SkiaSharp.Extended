using System;

namespace SkiaSharp.Extended.Svg
{
	internal struct SKRadialGradient
	{
		public SKRadialGradient(SKPoint center, float radius, float[] positions, SKColor[] colors, SKShaderTileMode tileMode, SKMatrix matrix)
		{
			Center = center;
			Radius = radius;
			Positions = positions;
			Colors = colors;
			TileMode = tileMode;
			Matrix = matrix;
		}

		public SKPoint Center { get; set; }

		public float Radius { get; set; }

		public float[] Positions { get; set; }

		public SKColor[] Colors { get; set; }

		public SKMatrix Matrix { get; set; }

		public SKShaderTileMode TileMode { get; set; }

		public SKPoint GetCenterPoint(float x, float y, float width, float height)
		{
			if (Math.Max(Center.X, Center.Y) > 1f)
				return new SKPoint(Center.X, Center.Y);

			var x0 = x + (Center.X * width);
			var y0 = y + (Center.Y * height);

			return new SKPoint((float)x0, y0);
		}

		public float GetRadius(float width, float height)
		{
			if (Radius > 1f)
				return Radius;

			return Math.Min(width, height) * Radius;
		}
	}
}
