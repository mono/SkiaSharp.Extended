using System.Collections.Generic;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Extended.Iconify;
using Topten.RichTextKit;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			// register the default fonts that we want for Iconify
			SKTextRunLookup.Instance.AddFontAwesome();
			SKTextRunLookup.Instance.AddIonIcons();
			SKTextRunLookup.Instance.AddMaterialDesignIcons();
			SKTextRunLookup.Instance.AddMaterialIcons();
			SKTextRunLookup.Instance.AddMeteocons();
			SKTextRunLookup.Instance.AddSimpleLineIcons();
			SKTextRunLookup.Instance.AddTypicons();
			SKTextRunLookup.Instance.AddWeatherIcons();

			// register all the fonts for RichTextKit
			FontMapper.Default = new DemoFontMapper();

			//MainPage = new NavigationPage(new MainPage());
			MainPage = new NavigationPage(new Demos.PlaygroundPage());
		}

		private class DemoFontMapper : FontMapper
		{
			private readonly Dictionary<string, SKTypeface> loadedFonts = new Dictionary<string, SKTypeface>();

			public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants)
			{
				if (style.FontFamily == "Segoe UI")
				{
					if (style.FontWeight > 400)
						if (style.FontItalic)
							return GetTypeface("SegoeUI-BoldItalic");
						else
							return GetTypeface("SegoeUI-Bold");
					else if (style.FontItalic)
						return GetTypeface("SegoeUI-Italic");
					else
						return GetTypeface("SegoeUI-Regular");
				}
				else if (style.FontFamily == "Segoe Script")
				{
					return GetTypeface("SegoeScript");
				}
				else if (style.FontFamily == "Consolas")
				{
					return GetTypeface("Consolas");
				}

				return base.TypefaceFromStyle(style, ignoreFontVariants);
			}

			private SKTypeface GetTypeface(string name)
			{
				var type = typeof(MainPage).GetTypeInfo();
				var assembly = type.Assembly;

				var stream = assembly.GetManifestResourceStream($"SkiaSharpDemo.fonts.{name}.ttf");

				var tf = SKTypeface.FromStream(stream);
				loadedFonts[name] = tf;

				return tf;
			}
		}
	}
}
