using System;

namespace SkiaSharp.Extended.Controls
{
	public partial class SKGestureSurfaceView
	{
		private struct PinchValue
		{
			public PinchValue(float centerX, float centerY, float radius, float angle)
			{
				Center = new SKPoint(centerX, centerY);
				Radius = radius;
				Angle = angle;
			}

			public PinchValue(SKPoint center, float radius, float angle)
			{
				Center = center;
				Radius = radius;
				Angle = angle;
			}

			public SKPoint Center { get; set; }

			public float Radius { get; set; }

			public float Angle { get; set; }

			public static PinchValue FromLocations(SKPoint[] locations)
			{
				if (locations == null || locations.Length < 2)
					throw new ArgumentException();

				var centerX = 0.0;
				var centerY = 0.0;
				foreach (var location in locations)
				{
					centerX += location.X;
					centerY += location.Y;
				}
				centerX /= locations.Length;
				centerY /= locations.Length;

				var radius = SKPoint.Distance(new SKPoint((float)centerX, (float)centerY), locations[0]);
				var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

				return new PinchValue((float)centerX, (float)centerY, radius, (float)angle);
			}
		}
	}
}
