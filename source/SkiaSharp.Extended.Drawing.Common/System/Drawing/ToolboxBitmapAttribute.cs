namespace System.Drawing;

/// <summary>Allows you to specify an icon to represent a control in a container.</summary>
[System.AttributeUsageAttribute(System.AttributeTargets.Class)]
public partial class ToolboxBitmapAttribute : System.Attribute
{
	private readonly Type? _type;
	private readonly string? _imageFile;
	private readonly string? _name;

	/// <summary>A <see cref="ToolboxBitmapAttribute"/> object that has its image set to null.</summary>
	public static readonly ToolboxBitmapAttribute Default = new ToolboxBitmapAttribute();

	private ToolboxBitmapAttribute() { }

	/// <summary>Initializes a new <see cref="ToolboxBitmapAttribute"/> object with an image from a specified file.</summary>
	public ToolboxBitmapAttribute(string imageFile) { _imageFile = imageFile; }

	/// <summary>Initializes a new <see cref="ToolboxBitmapAttribute"/> object based on a 16x16 bitmap that is embedded as a resource.</summary>
	public ToolboxBitmapAttribute(Type t) { _type = t; }

	/// <summary>Initializes a new <see cref="ToolboxBitmapAttribute"/> object based on a 16x16 bitmap that is embedded as a resource in a specified assembly.</summary>
	public ToolboxBitmapAttribute(Type t, string name) { _type = t; _name = name; }

	/// <summary>Gets an <see cref="Image"/> from a resource in the specified assembly.</summary>
	public static Image? GetImageFromResource(Type t, string? imageName, bool large) => null;

	/// <summary>Indicates whether the specified object is a <see cref="ToolboxBitmapAttribute"/> and is identical to this.</summary>
	public override bool Equals(object? value)
		=> value is ToolboxBitmapAttribute other &&
		   _type == other._type &&
		   _imageFile == other._imageFile &&
		   _name == other._name;

	/// <summary>Gets a hash code for this instance.</summary>
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			hash = hash * 31 + (_type?.GetHashCode() ?? 0);
			hash = hash * 31 + (_imageFile?.GetHashCode() ?? 0);
			hash = hash * 31 + (_name?.GetHashCode() ?? 0);
			return hash;
		}
	}

	/// <summary>Gets the small or large <see cref="Image"/> associated with this attribute.</summary>
	public Image? GetImage(object? component) => GetImage(component?.GetType(), false);
	/// <summary>Gets the small or large <see cref="Image"/> associated with this attribute.</summary>
	public Image? GetImage(object? component, bool large) => GetImage(component?.GetType(), large);
	/// <summary>Gets the small or large <see cref="Image"/> associated with this attribute.</summary>
	public Image? GetImage(Type type) => GetImage(type, false);
	/// <summary>Gets the small or large <see cref="Image"/> associated with this attribute.</summary>
	public Image? GetImage(Type type, bool large) => GetImage(type, _name, large);
	/// <summary>Gets the small or large <see cref="Image"/> associated with this attribute.</summary>
	public Image? GetImage(Type type, string? imgName, bool large) => null;
}
