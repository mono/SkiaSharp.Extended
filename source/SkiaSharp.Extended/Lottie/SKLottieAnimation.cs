using SkiaSharp.Skottie;

namespace SkiaSharp.Extended;

/// <summary>
/// Wraps a loaded Skottie animation returned by a platform image source.
/// </summary>
/// <remarks>Image sources should return a new result and native animation for each successful load.</remarks>
public class SKLottieAnimation
{
	/// <summary>
	/// Initializes an empty animation result.
	/// </summary>
	public SKLottieAnimation()
	{
	}

	/// <summary>
	/// Initializes an animation result.
	/// </summary>
	/// <param name="animation">The loaded Skottie animation, or <see langword="null"/>.</param>
	public SKLottieAnimation(Animation? animation)
	{
		Animation = animation;
	}

	/// <summary>
	/// Gets the loaded Skottie animation, or <see langword="null"/> when empty.
	/// </summary>
	public Animation? Animation { get; }

	/// <summary>
	/// Gets whether an animation is loaded.
	/// </summary>
	public bool IsLoaded => Animation is not null;
}
