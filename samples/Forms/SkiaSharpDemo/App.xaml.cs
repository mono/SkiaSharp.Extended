using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SkiaSharp;
using Topten.RichTextKit;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class App : Application
	{
		public App()
		{
			Device.SetFlags(new[] { "CarouselView_Experimental" });

			InitializeComponent();

			// register all the fonts for RichTextKit
			FontMapper.Default = new DemoFontMapper();

			MainPage = new NavigationPage(new MainPage());
			//MainPage = new NavigationPage(new Demos.PlaygroundPage());
		}

		public static Stream GetImageResourceStream(string name)
		{
			var assembly = typeof(App).Assembly;

			return assembly.GetManifestResourceStream("SkiaSharpDemo.images." + name);
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
