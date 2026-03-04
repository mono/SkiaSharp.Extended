namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A collection of <see cref="SKConfettiShape"/> instances used for confetti particles.
/// </summary>
public class SKConfettiShapeCollection : List<SKConfettiShape>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiShapeCollection"/> class.
	/// </summary>
	public SKConfettiShapeCollection()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiShapeCollection"/> class with items from the specified collection.
	/// </summary>
	/// <param name="collection">The collection of shapes to add.</param>
	public SKConfettiShapeCollection(IEnumerable<SKConfettiShape> collection)
		: base(collection)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiShapeCollection"/> class with the specified initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity.</param>
	public SKConfettiShapeCollection(int capacity)
		: base(capacity)
	{
	}
}
