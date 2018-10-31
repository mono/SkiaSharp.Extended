using System;

namespace SkiaSharp.Extended.Svg
{
	internal struct SKLinearGradient
	{
		public SKLinearGradient(SKPoint start, SKPoint end, float[] positions, SKColor[] colors, SKShaderTileMode tileMode, SKMatrix matrix)
		{
			Start = start;
			End = end;
			Positions = positions;
			Colors = colors;
			TileMode = tileMode;
			Matrix = matrix;
		}

		public SKPoint Start { get; set; }

		public SKPoint End { get; set; }

		public float[] Positions { get; set; }

		public SKColor[] Colors { get; set; }

		public SKMatrix Matrix { get; set; }

		public SKShaderTileMode TileMode { get; set; }

		public SKPoint GetStartPoint(float x, float y, float width, float height)
		{
			if (Math.Max(Start.X, Start.Y) > 1f)
				return new SKPoint(Start.X, Start.Y);

			var x0 = x + Start.X * width;
			var y0 = y + Start.Y * height;

			return new SKPoint(x0, y0);
		}

		public SKPoint GetEndPoint(float x, float y, float width, float height)
		{
			if (Math.Max(End.X, End.Y) > 1f)
				return new SKPoint(End.X, End.Y);

			var x0 = x + End.X * width;
			var y0 = y + End.Y * height;

			return new SKPoint(x0, y0);
		}
	}
}
