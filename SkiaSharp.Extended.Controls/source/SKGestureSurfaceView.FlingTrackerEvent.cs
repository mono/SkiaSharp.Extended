namespace SkiaSharp.Extended.Controls
{
	public partial class SKGestureSurfaceView
	{
		private struct FlingTrackerEvent
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
