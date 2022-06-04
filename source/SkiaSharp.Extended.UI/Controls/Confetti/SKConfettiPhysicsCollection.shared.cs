namespace SkiaSharp.Extended.UI.Controls;

public class SKConfettiPhysicsCollection : List<SKConfettiPhysics>
{
	public SKConfettiPhysicsCollection()
	{
	}

	public SKConfettiPhysicsCollection(IEnumerable<SKConfettiPhysics> collection)
		: base(collection)
	{
	}

	public SKConfettiPhysicsCollection(int capacity)
		: base(capacity)
	{
	}
}
