using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Tracks touch events to calculate fling velocity.
/// </summary>
internal sealed class SKFlingTracker
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
		// with time-based weighting (more recent = higher weight)
		float totalVelocityX = 0, totalVelocityY = 0, totalWeight = 0;
		var windowStart = array[startIndex].Ticks;
		var windowSpan = (float)(now - windowStart);
		if (windowSpan <= 0)
			windowSpan = 1;

		for (int i = startIndex; i < array.Length - 1; i++)
		{
			var dt = array[i + 1].Ticks - array[i].Ticks;
			if (dt <= 0)
				continue;

			var vx = (array[i + 1].X - array[i].X) * TimeSpan.TicksPerSecond / dt;
			var vy = (array[i + 1].Y - array[i].Y) * TimeSpan.TicksPerSecond / dt;

			// Time-based weight: how recent is this segment (0..1, 1 = most recent)
			var recency = (float)(array[i + 1].Ticks - windowStart) / windowSpan;
			var weight = 0.5f + recency; // range [0.5, 1.5] — still uses older data but favors newer
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
