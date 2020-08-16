using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls.Themes
{
	public partial class Generic : ResourceDictionary
	{
		private static bool registered;

		public Generic()
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
					if (dic.GetType() == typeof(Generic))
					{
						registered = true;
						break;
					}
				}

				if (!registered)
				{
					merged.Add(new Generic());
					registered = true;
				}
			}
		}
	}
}
