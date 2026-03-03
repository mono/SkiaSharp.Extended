using System.Collections.ObjectModel;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// An observable collection of <see cref="SKConfettiSystem"/> instances.
/// </summary>
public class SKConfettiSystemCollection : ObservableCollection<SKConfettiSystem>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiSystemCollection"/> class.
	/// </summary>
	public SKConfettiSystemCollection()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiSystemCollection"/> class with items from the specified collection.
	/// </summary>
	/// <param name="collection">The collection of systems to add.</param>
	public SKConfettiSystemCollection(IEnumerable<SKConfettiSystem> collection)
		: base(collection)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKConfettiSystemCollection"/> class with items from the specified list.
	/// </summary>
	/// <param name="list">The list of systems to add.</param>
	public SKConfettiSystemCollection(List<SKConfettiSystem> list)
		: base(list)
	{
	}
}
