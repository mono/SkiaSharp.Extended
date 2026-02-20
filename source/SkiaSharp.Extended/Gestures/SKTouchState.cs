namespace SkiaSharp.Extended.Gestures;

/// <summary>
/// Represents the state of a touch point.
/// </summary>
internal readonly struct SKTouchState
{
	public long Id { get; }
	public SKPoint Location { get; }
	public long Ticks { get; }
	public bool InContact { get; }

	public SKTouchState(long id, SKPoint location, long ticks, bool inContact)
	{
		Id = id;
		Location = location;
		Ticks = ticks;
		InContact = inContact;
	}
}
