namespace SkiaSharp.Extended.Svg
{
	internal struct SKCircle
	{
		public SKCircle(SKPoint center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		public SKPoint Center { get; }

		public float Radius { get; }
	}
}
