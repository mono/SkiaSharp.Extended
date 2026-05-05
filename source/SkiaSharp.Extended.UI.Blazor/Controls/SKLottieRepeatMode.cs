namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// Specifies how a Lottie animation repeats in the Blazor <see cref="SKLottieView"/>.
/// </summary>
public enum SKLottieRepeatMode
{
	/// <summary>Restart from the beginning after each cycle.</summary>
	Restart,

	/// <summary>Alternate between forward and backward (ping-pong) on each cycle.</summary>
	Reverse
}
