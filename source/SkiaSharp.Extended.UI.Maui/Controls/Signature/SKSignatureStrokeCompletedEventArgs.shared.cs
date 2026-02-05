namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a stroke is completed on the signature pad.
/// </summary>
public class SKSignatureStrokeCompletedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance of the event arguments.
	/// </summary>
	/// <param name="strokeCount">The total number of strokes after completion.</param>
	public SKSignatureStrokeCompletedEventArgs(int strokeCount)
	{
		StrokeCount = strokeCount;
	}

	/// <summary>
	/// Gets the total number of strokes on the signature pad.
	/// </summary>
	public int StrokeCount { get; }
}
