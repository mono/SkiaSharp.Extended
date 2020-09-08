namespace SkiaSharp.Extended.Controls
{
	public partial class SKGestureSurfaceView
	{
		private class TouchEvent
		{
			public TouchEvent(long id, SKPoint screenPosition, long tick, bool inContact)
			{
				Id = id;
				Location = screenPosition;
				Tick = tick;
				InContact = inContact;
			}

			public long Id { get; }

			public SKPoint Location { get; }

			public long Tick { get; }

			public bool InContact { get; }
		}
	}
}
