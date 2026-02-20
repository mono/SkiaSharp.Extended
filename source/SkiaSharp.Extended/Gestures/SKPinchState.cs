using System;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Represents the state of a pinch gesture.
/// </summary>
internal readonly struct SKPinchState
{
	public SKPoint Center { get; }
	public float Radius { get; }
	public float Angle { get; }

	public SKPinchState(SKPoint center, float radius, float angle)
	{
		Center = center;
		Radius = radius;
		Angle = angle;
	}

	/// <summary>
	/// Creates a SKPinchState from an array of touch locations.
	/// </summary>
	public static SKPinchState FromLocations(SKPoint[] locations)
	{
		if (locations == null || locations.Length < 2)
			return new SKPinchState(locations?.Length > 0 ? locations[0] : SKPoint.Empty, 0, 0);

		var centerX = 0f;
		var centerY = 0f;
		foreach (var loc in locations)
		{
			centerX += loc.X;
			centerY += loc.Y;
		}
		centerX /= locations.Length;
		centerY /= locations.Length;

		var center = new SKPoint(centerX, centerY);
		var radius = SKPoint.Distance(center, locations[0]);
		var angle = (float)(Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180 / Math.PI);

		return new SKPinchState(center, radius, angle);
	}
}
