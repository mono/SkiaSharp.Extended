namespace SkiaSharp.Extended.UI.Controls;

public class SKLottieAnimationLoadedEventArgs : EventArgs
{
	public SKLottieAnimationLoadedEventArgs(Size size, TimeSpan duration, double fps)
	{
		Size = size;
		Duration = duration;
		Fps = fps;
	}

	public Size Size { get; }

	public TimeSpan Duration { get; }

	public double Fps { get; }

	internal static SKLottieAnimationLoadedEventArgs Create(Skottie.Animation animation)
	{
		var s = animation.Size;
		var size = new Size(s.Width, s.Height);
		var duration = animation.Duration;
		var fps = animation.Fps;

		return new SKLottieAnimationLoadedEventArgs(size, duration, fps);
	}
}
