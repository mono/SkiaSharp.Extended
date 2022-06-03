using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharpDemo.Demos;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			Demos = new List<DemoGroup>
			{
				new DemoGroup("EXTENDED")
				{
					new Demo
					{
						Title = "Shapes",
						Description = "The rectangle and circle provided by SkiaSharp are very nice, but sometimes you need a bit more.",
						PageType = typeof(ShapesPage),
						Color = Color.CornflowerBlue,
					},
					new Demo
					{
						Title = "Path Interpolation",
						Description = "There is also a few things that might be nice if we could interpolate from one path to another.",
						PageType = typeof(InterpolationPage),
						Color = Color.LightPink,
					},
					new Demo
					{
						Title = "BlurHash",
						Description = "That time you wanted a compact representation of a placeholder for an image, but as a short data string.",
						PageType = typeof(BlurHashPage),
						Color = Color.LightSkyBlue,
					},
				},
				new DemoGroup("UI & CONTROLS")
				{
					new Demo
					{
						Title = "Confetti",
						Description = "Yeaaahhhhh! Throw that confetti! Woooooaaaaahhhh! We are the winners! Celebration words! Congrats! Yay!",
						PageType = typeof(ConfettiPage),
						Color = Color.SteelBlue,
					},
					new Demo
					{
						Title = "ToImage",
						Description = "You have that ImageSource and you really want an actual image... What do you do? Well, you ToSKImageAsync that puppy!",
						PageType = typeof(ImageSourceToImagePage),
						Color = Color.Orange,
					},
				},
				new DemoGroup("TEXT & EMOJI")
				{
					new Demo
					{
						Title = "RichTextKit",
						Description = "Doing text the right way.",
						PageType = typeof(RichTextKitPage),
						Color = Color.YellowGreen,
					},
				},
				new DemoGroup("SVG")
				{
					new Demo
					{
						Title = "SVG (Svg.Skia)",
						Description = "Everyone wants to load SVG files. Literally everyone. So use a great library: Svg.Skia",
						PageType = typeof(SvgPage),
						Color = Color.LightSeaGreen,
					},
				},
				new DemoGroup("PLAYGROUND")
				{
					new Demo
					{
						Title = "Playground",
						Description = "The ground for playing.",
						PageType = typeof(PlaygroundPage),
						Color = Color.Silver,
					},
				},
			};

			BindingContext = this;
		}

		public List<DemoGroup> Demos { get; }

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is Demo demo)
			{
				NavigateTo(demo);
				collectionView.SelectedItem = null;
			}
		}

		private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem is Demo demo)
			{
				NavigateTo(demo);
				listView.SelectedItem = null;
			}
		}

		private void NavigateTo(Demo demo)
		{
			var page = Activator.CreateInstance(demo.PageType) as Page;

			Navigation.PushAsync(page);
		}
	}
}
