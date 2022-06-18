namespace SkiaSharp.Extended.UI.Controls.Themes;

public partial class SKConfettiViewResources : ResourceDictionary
{
	private static bool registered;

	public SKConfettiViewResources()
	{
		InitializeComponent();
	}

	internal static void EnsureRegistered() =>
		ResourceLoader.EnsureRegistered<SKConfettiViewResources>(ref registered);
}
