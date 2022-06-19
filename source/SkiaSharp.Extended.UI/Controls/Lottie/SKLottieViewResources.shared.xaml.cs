namespace SkiaSharp.Extended.UI.Controls.Themes;

public partial class SKLottieViewResources : ResourceDictionary
{
	private static bool registered;

	public SKLottieViewResources()
	{
		InitializeComponent();
	}

	internal static void EnsureRegistered() =>
		ResourceLoader.EnsureRegistered<SKLottieViewResources>(ref registered);
}
