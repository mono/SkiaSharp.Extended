namespace SkiaSharp.Extended.UI.Controls;

public partial class SKGestureSurfaceView
{
	/// <summary>
	/// Represents a single touch event.
	/// </summary>
	private sealed class TouchEvent
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
