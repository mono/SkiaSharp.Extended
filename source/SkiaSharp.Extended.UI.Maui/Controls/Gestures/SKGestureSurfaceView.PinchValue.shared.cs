namespace SkiaSharp.Extended.UI.Controls;

public partial class SKGestureSurfaceView
{
	/// <summary>
	/// Represents the computed values of a pinch gesture.
	/// </summary>
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

		/// <summary>
		/// Calculates pinch values from an array of touch locations.
		/// </summary>
		/// <param name="locations">At least 2 touch locations.</param>
		/// <returns>The computed pinch values.</returns>
		/// <exception cref="ArgumentException">Thrown when fewer than 2 locations are provided.</exception>
		public static PinchValue FromLocations(SKPoint[] locations)
		{
			if (locations is null || locations.Length < 2)
				throw new ArgumentException("At least 2 locations are required.", nameof(locations));

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
