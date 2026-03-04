namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for when a Lottie animation has been successfully loaded.
/// </summary>
public class SKLottieAnimationLoadedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieAnimationLoadedEventArgs"/> class.
	/// </summary>
	/// <param name="size">The natural size of the animation.</param>
	/// <param name="duration">The total duration of the animation.</param>
	/// <param name="fps">The frames per second of the animation.</param>
	public SKLottieAnimationLoadedEventArgs(Size size, TimeSpan duration, double fps)
	{
		Size = size;
		Duration = duration;
		Fps = fps;
	}

	/// <summary>
	/// Gets the natural size of the animation.
	/// </summary>
	public Size Size { get; }

	/// <summary>
	/// Gets the total duration of the animation.
	/// </summary>
	public TimeSpan Duration { get; }

	/// <summary>
	/// Gets the frames per second of the animation.
	/// </summary>
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
