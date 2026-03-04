namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A collection of <see cref="SKConfettiPhysics"/> configurations for confetti particles.
/// </summary>
public class SKConfettiPhysicsCollection : List<SKConfettiPhysics>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiPhysicsCollection"/> class.
	/// </summary>
	public SKConfettiPhysicsCollection()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiPhysicsCollection"/> class with items from the specified collection.
	/// </summary>
	/// <param name="collection">The collection of physics configurations to add.</param>
	public SKConfettiPhysicsCollection(IEnumerable<SKConfettiPhysics> collection)
		: base(collection)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiPhysicsCollection"/> class with the specified initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity.</param>
	public SKConfettiPhysicsCollection(int capacity)
		: base(capacity)
	{
	}
}
