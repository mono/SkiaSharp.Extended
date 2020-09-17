using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Controls.Themes
{
	public partial class SKFilteredImageResources : ResourceDictionary
	{
		private static bool registered;

		public SKFilteredImageResources()
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
					if (dic.GetType() == typeof(SKFilteredImageResources))
					{
						registered = true;
						break;
					}
				}

				if (!registered)
				{
					merged.Add(new SKFilteredImageResources());
					registered = true;
				}
			}
		}
	}
}
