namespace SkiaSharp.Extended.UI.Controls;

public partial class SKGestureSurfaceView
{
	/// <summary>
	/// Tracks touch events to calculate fling velocity.
	/// </summary>
	private sealed class FlingTracker
	{
		// Use only events from the last 200 ms
		private const long ThresholdTicks = 200 * TimeSpan.TicksPerMillisecond;
		private const int MaxSize = 2;

		private readonly Dictionary<long, Queue<FlingTrackerEvent>> events = new();

		public void AddEvent(long id, SKPoint location, long ticks)
		{
			if (!events.TryGetValue(id, out var queue))
			{
				queue = new Queue<FlingTrackerEvent>();
				events[id] = queue;
			}

			queue.Enqueue(new FlingTrackerEvent(location.X, location.Y, ticks));

			if (queue.Count > MaxSize)
				queue.Dequeue();
		}

		public void RemoveId(long id)
		{
			events.Remove(id);
		}

		public void Clear()
		{
			events.Clear();
		}

		public SKPoint CalculateVelocity(long id, long now)
		{
			float velocityX = 0;
			float velocityY = 0;

			if (!events.TryGetValue(id, out var queue) || queue.Count != 2)
				return SKPoint.Empty;

			var array = queue.ToArray();

			var lastItem = array[0];
			var nowItem = array[1];

			// Use last 2 events to calculate velocity
			if (now - lastItem.TimeTicks < ThresholdTicks)
			{
				var timeDelta = nowItem.TimeTicks - lastItem.TimeTicks;
				if (timeDelta > 0)
				{
					velocityX = (nowItem.X - lastItem.X) * TimeSpan.TicksPerSecond / timeDelta;
					velocityY = (nowItem.Y - lastItem.Y) * TimeSpan.TicksPerSecond / timeDelta;
				}
			}

			// Return the velocity in pixels per second
			return new SKPoint(velocityX, velocityY);
		}

		/// <summary>
		/// Represents a single event used for fling velocity calculation.
		/// </summary>
		private readonly struct FlingTrackerEvent
		{
			public FlingTrackerEvent(float x, float y, long timeTicks)
			{
				X = x;
				Y = y;
				TimeTicks = timeTicks;
			}

			public float X { get; }
			public float Y { get; }
			public long TimeTicks { get; }
		}
	}
}
