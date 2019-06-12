namespace SkiaSharp.Extended.Controls
{
	public partial class SKGestureSurfaceView
	{
		private struct FlingTrackerEvent
		{
			public FlingTrackerEvent(float x, float y, long time)
			{
				X = x;
				Y = y;
				Time = time;
			}

			public float X { get; }

			public float Y { get; }

			public long Time { get; }
		}
	}
}
