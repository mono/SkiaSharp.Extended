namespace SkiaSharp.Extended.UI.Controls.Themes;

public partial class SKConfettiViewResources : ResourceDictionary
{
	private static bool registered;

	public SKConfettiViewResources()
	{
		InitializeComponent();
	}

	internal static void EnsureRegistered()
	{
		if (registered)
			return;

		var merged = Application.Current?.Resources?.MergedDictionaries;
		if (merged != null)
		{
			foreach (var dic in merged)
			{
				if (dic.GetType() == typeof(SKConfettiViewResources))
				{
					registered = true;
					break;
				}
			}

			if (!registered)
			{
				merged.Add(new SKConfettiViewResources());
				registered = true;
			}
		}
	}
}
