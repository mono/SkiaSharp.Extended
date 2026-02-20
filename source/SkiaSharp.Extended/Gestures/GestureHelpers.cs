using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Tracks touch events to calculate fling velocity.
/// </summary>
internal sealed class FlingTracker
{
	// Use only events from the last 200 ms
	private const long ThresholdTicks = 200 * TimeSpan.TicksPerMillisecond;
	private const int MaxSize = 5;

	private readonly Dictionary<long, Queue<FlingEvent>> _events = new();

	public void AddEvent(long id, SKPoint location, long ticks)
	{
		if (!_events.TryGetValue(id, out var queue))
		{
			queue = new Queue<FlingEvent>();
			_events[id] = queue;
		}

		queue.Enqueue(new FlingEvent(location.X, location.Y, ticks));

		if (queue.Count > MaxSize)
			queue.Dequeue();
	}

	public void RemoveId(long id)
	{
		_events.Remove(id);
	}

	public void Clear()
	{
		_events.Clear();
	}

	public SKPoint CalculateVelocity(long id, long now)
	{
		if (!_events.TryGetValue(id, out var queue) || queue.Count < 2)
			return SKPoint.Empty;

		var array = queue.ToArray();

		// Find the oldest event within the threshold window
		var startIndex = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (now - array[i].Ticks < ThresholdTicks)
			{
				startIndex = i;
				break;
			}
		}

		if (startIndex < 0 || startIndex >= array.Length - 1)
			return SKPoint.Empty;

		// Use weighted average of velocities between consecutive events,
		// favoring more recent events
		float totalVelocityX = 0, totalVelocityY = 0, totalWeight = 0;
		for (int i = startIndex; i < array.Length - 1; i++)
		{
			var dt = array[i + 1].Ticks - array[i].Ticks;
			if (dt <= 0)
				continue;

			var vx = (array[i + 1].X - array[i].X) * TimeSpan.TicksPerSecond / dt;
			var vy = (array[i + 1].Y - array[i].Y) * TimeSpan.TicksPerSecond / dt;

			// Weight increases for more recent events
			var weight = (float)(i - startIndex + 1);
			totalVelocityX += vx * weight;
			totalVelocityY += vy * weight;
			totalWeight += weight;
		}

		if (totalWeight <= 0)
			return SKPoint.Empty;

		return new SKPoint(totalVelocityX / totalWeight, totalVelocityY / totalWeight);
	}

	private readonly struct FlingEvent
	{
		public readonly float X;
		public readonly float Y;
		public readonly long Ticks;

		public FlingEvent(float x, float y, long ticks)
		{
			X = x;
			Y = y;
			Ticks = ticks;
		}
	}
}

/// <summary>
/// Represents the state of a touch point.
/// </summary>
internal readonly struct TouchState
{
	public long Id { get; }
	public SKPoint Location { get; }
	public long Ticks { get; }
	public bool InContact { get; }

	public TouchState(long id, SKPoint location, long ticks, bool inContact)
	{
		Id = id;
		Location = location;
		Ticks = ticks;
		InContact = inContact;
	}
}

/// <summary>
/// Represents the state of a pinch gesture.
/// </summary>
internal readonly struct PinchState
{
	public SKPoint Center { get; }
	public float Radius { get; }
	public float Angle { get; }

	public PinchState(SKPoint center, float radius, float angle)
	{
		Center = center;
		Radius = radius;
		Angle = angle;
	}

	/// <summary>
	/// Creates a PinchState from an array of touch locations.
	/// </summary>
	public static PinchState FromLocations(SKPoint[] locations)
	{
		if (locations == null || locations.Length < 2)
			return new PinchState(locations?.Length > 0 ? locations[0] : SKPoint.Empty, 0, 0);

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

		return new PinchState(center, radius, angle);
	}
}
