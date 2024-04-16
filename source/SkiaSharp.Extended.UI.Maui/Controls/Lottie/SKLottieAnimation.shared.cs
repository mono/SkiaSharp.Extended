namespace SkiaSharp.Extended.UI.Controls;

public class SKLottieAnimation
{
	public SKLottieAnimation()
	{
	}

	public SKLottieAnimation(Skottie.Animation? animation)
	{
		Animation = animation;
	}

	public Skottie.Animation? Animation { get; }

	public bool IsLoaded => Animation is not null;
}
