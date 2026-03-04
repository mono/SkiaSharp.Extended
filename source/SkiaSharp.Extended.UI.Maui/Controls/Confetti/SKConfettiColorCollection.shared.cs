namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A collection of <see cref="Color"/> values used for confetti particles.
/// </summary>
[TypeConverter(typeof(Converters.SKConfettiColorCollectionTypeConverter))]
public class SKConfettiColorCollection : List<Color>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiColorCollection"/> class.
	/// </summary>
	public SKConfettiColorCollection()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiColorCollection"/> class with items from the specified collection.
	/// </summary>
	/// <param name="collection">The collection of colors to add.</param>
	public SKConfettiColorCollection(IEnumerable<Color> collection)
		: base(collection)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiColorCollection"/> class with the specified initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity.</param>
	public SKConfettiColorCollection(int capacity)
		: base(capacity)
	{
	}
}
