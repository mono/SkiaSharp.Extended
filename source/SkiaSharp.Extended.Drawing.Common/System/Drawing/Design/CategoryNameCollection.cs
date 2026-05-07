namespace System.Drawing.Design;

/// <summary>Represents a collection of category name strings.</summary>
public sealed partial class CategoryNameCollection : Collections.ReadOnlyCollectionBase
{
	/// <summary>Initializes a new instance of the <see cref="CategoryNameCollection"/> class with the specified collection.</summary>
	public CategoryNameCollection(CategoryNameCollection value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		InnerList.AddRange(value.InnerList);
	}

	/// <summary>Initializes a new instance of the <see cref="CategoryNameCollection"/> class from an array of strings.</summary>
	public CategoryNameCollection(string[] value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		InnerList.AddRange(value);
	}

	/// <summary>Gets the string at the specified index.</summary>
	public string this[int index] => (string)InnerList[index]!;

	/// <summary>Indicates whether the specified category name is in the collection.</summary>
	public bool Contains(string value) => InnerList.Contains(value);

	/// <summary>Copies the collection to the specified array, starting at the specified index.</summary>
	public void CopyTo(string[] array, int index) => InnerList.CopyTo(array, index);

	/// <summary>Returns the index of the specified category name.</summary>
	public int IndexOf(string value) => InnerList.IndexOf(value);
}
