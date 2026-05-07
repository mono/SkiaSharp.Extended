namespace System.Drawing.Imaging;

/// <summary>
///  Provides properties that get the frame dimensions of an image. Not inheritable.
/// </summary>
public sealed partial class FrameDimension
{
	private readonly Guid _guid;

	private static readonly FrameDimension s_page = new FrameDimension(new Guid("{7462dc86-6180-4c7e-8e3f-ee7333a7a483}"));
	private static readonly FrameDimension s_resolution = new FrameDimension(new Guid("{84236f7b-3bd3-428f-8dab-4ea1439ca315}"));
	private static readonly FrameDimension s_time = new FrameDimension(new Guid("{6aedbd6d-3fb5-418a-83a6-7f45229dc872}"));

	/// <summary>
	///  Initializes a new instance of the <see cref="FrameDimension"/> class using the specified <see cref="System.Guid"/> structure.
	/// </summary>
	public FrameDimension(Guid guid) { _guid = guid; }

	/// <summary>
	///  Gets a globally unique identifier (GUID) that represents this <see cref="FrameDimension"/> object.
	/// </summary>
	public Guid Guid { get { return _guid; } }

	/// <summary>
	///  Gets the page dimension.
	/// </summary>
	public static FrameDimension Page { get { return s_page; } }

	/// <summary>
	///  Gets the resolution dimension.
	/// </summary>
	public static FrameDimension Resolution { get { return s_resolution; } }

	/// <summary>
	///  Gets the time dimension.
	/// </summary>
	public static FrameDimension Time { get { return s_time; } }

	/// <summary>
	///  Returns a value that indicates whether the specified object is a <see cref="FrameDimension"/> equivalent to this <see cref="FrameDimension"/> object.
	/// </summary>
	public override bool Equals(object? o) { return o is FrameDimension other && _guid == other._guid; }

	/// <summary>
	///  Returns a hash code for this <see cref="FrameDimension"/> object.
	/// </summary>
	public override int GetHashCode() { return _guid.GetHashCode(); }

	/// <summary>
	///  Converts this <see cref="FrameDimension"/> object to a human-readable string.
	/// </summary>
	public override string ToString()
	{
		if (_guid == Page.Guid) return "Page";
		if (_guid == Resolution.Guid) return "Resolution";
		if (_guid == Time.Guid) return "Time";
		return "[FrameDimension: " + _guid.ToString() + "]";
	}
}
