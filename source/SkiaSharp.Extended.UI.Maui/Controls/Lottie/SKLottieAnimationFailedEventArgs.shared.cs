namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a Lottie animation fails to load.
/// </summary>
public class SKLottieAnimationFailedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieAnimationFailedEventArgs"/> class.
	/// </summary>
	public SKLottieAnimationFailedEventArgs()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieAnimationFailedEventArgs"/> class with the specified exception.
	/// </summary>
	/// <param name="exception">The exception that caused the failure, or <see langword="null"/>.</param>
	public SKLottieAnimationFailedEventArgs(Exception? exception)
	{
		Exception = exception;
	}

	/// <summary>
	/// Gets the exception that caused the failure, or <see langword="null"/>.
	/// </summary>
	public Exception? Exception { get; }
}
