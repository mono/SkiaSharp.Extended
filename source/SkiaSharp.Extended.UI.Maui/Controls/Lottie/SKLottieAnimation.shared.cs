namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Wraps a loaded Skottie animation instance.
/// </summary>
public class SKLottieAnimation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieAnimation"/> class with no animation loaded.
	/// </summary>
	public SKLottieAnimation()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SKLottieAnimation"/> class with the specified animation.
	/// </summary>
	/// <param name="animation">The Skottie animation instance, or <see langword="null"/>.</param>
	public SKLottieAnimation(Skottie.Animation? animation)
	{
		Animation = animation;
	}

	/// <summary>
	/// Gets the underlying Skottie animation instance, or <see langword="null"/> if not loaded.
	/// </summary>
	public Skottie.Animation? Animation { get; }

	/// <summary>
	/// Gets a value indicating whether the animation is loaded.
	/// </summary>
	public bool IsLoaded => Animation is not null;
}
