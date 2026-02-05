namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// The touch mode indicating how many fingers are currently on the screen.
/// </summary>
internal enum TouchMode
{
	/// <summary>
	/// No fingers are touching the screen.
	/// </summary>
	None = 0,

	/// <summary>
	/// A single finger is touching the screen.
	/// </summary>
	Single = 1,

	/// <summary>
	/// Multiple fingers are touching the screen.
	/// </summary>
	Multiple = 2
}
