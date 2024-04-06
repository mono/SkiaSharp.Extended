namespace SkiaSharp.Extended.UI.Controls;

public class SKLottieAnimationFailedEventArgs : EventArgs
{
	public SKLottieAnimationFailedEventArgs()
	{
	}

	public SKLottieAnimationFailedEventArgs(Exception? exception)
	{
		Exception = exception;
	}

	public Exception? Exception { get; }
}
