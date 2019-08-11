using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Controls
{
	public partial class SKGestureSurfaceView
	{
		private class FlingTracker
		{
			private const long thresholdTicks = 200 * TimeSpan.TicksPerMillisecond;  // Use only events from the last 200 ms

			private const int maxSize = 2;

			private readonly Dictionary<long, Queue<FlingTrackerEvent>> events = new Dictionary<long, Queue<FlingTrackerEvent>>();

			public void AddEvent(long id, SKPoint location, long ticks)
			{
				if (!events.TryGetValue(id, out var queue))
				{
					queue = new Queue<FlingTrackerEvent>();
					events[id] = queue;
				}

				queue.Enqueue(new FlingTrackerEvent(location.X, location.Y, ticks));

				if (queue.Count > maxSize)
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

				// use last 2 events
				if (now - lastItem.TimeTicks < thresholdTicks)
				{
					velocityX = (nowItem.X - lastItem.X) * TimeSpan.TicksPerSecond / (nowItem.TimeTicks - lastItem.TimeTicks);
					velocityY = (nowItem.Y - lastItem.Y) * TimeSpan.TicksPerSecond / (nowItem.TimeTicks - lastItem.TimeTicks);
				}

				// return the velocity in pixels per second
				return new SKPoint(velocityX, velocityY);
			}
		}
	}
}
