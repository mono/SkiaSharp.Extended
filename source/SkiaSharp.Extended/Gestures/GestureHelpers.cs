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
	private const int MaxSize = 2;

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
		var first = array[0];
		var last = array[array.Length - 1];

		// Check if events are recent enough
		if (now - first.Ticks >= ThresholdTicks)
			return SKPoint.Empty;

		var timeDelta = last.Ticks - first.Ticks;
		if (timeDelta <= 0)
			return SKPoint.Empty;

		// Calculate velocity in pixels per second
		var velocityX = (last.X - first.X) * TimeSpan.TicksPerSecond / timeDelta;
		var velocityY = (last.Y - first.Y) * TimeSpan.TicksPerSecond / timeDelta;

		return new SKPoint(velocityX, velocityY);
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
	public readonly long Id;
	public readonly SKPoint Location;
	public readonly long Ticks;
	public readonly bool InContact;

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
internal struct PinchState
{
	public SKPoint Center;
	public float Radius;
	public float Angle;

	public PinchState(SKPoint center, float radius, float angle)
	{
		Center = center;
		Radius = radius;
		Angle = angle;
	}

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
