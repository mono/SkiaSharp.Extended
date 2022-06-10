using SkiaSharpDemo.Demos;

namespace SkiaSharpDemo;

public static class ExtendedDemos
{
	public static List<DemoGroup> GetAllDemos() =>
		new()
		{
			new DemoGroup("EXTENDED")
			{
				new Demo
				{
					Title = "Shapes",
					Description = "The rectangle and circle provided by SkiaSharp are very nice, but sometimes you need a bit more.",
					PageType = typeof(ShapesPage),
					Color = Colors.CornflowerBlue,
				},
				new Demo
				{
					Title = "Path Interpolation",
					Description = "There is also a few things that might be nice if we could interpolate from one path to another.",
					PageType = typeof(InterpolationPage),
					Color = Colors.LightPink,
				},
				new Demo
				{
					Title = "BlurHash",
					Description = "That time you wanted a compact representation of a placeholder for an image, but as a short data string.",
					PageType = typeof(BlurHashPage),
					Color = Colors.LightSkyBlue,
				},
			},
			new DemoGroup("UI & CONTROLS")
			{
				new Demo
				{
					Title = "Confetti",
					Description = "Yeaaahhhhh! Throw that confetti! Woooooaaaaahhhh! We are the winners! Celebration words! Congrats! Yay!",
					PageType = typeof(ConfettiPage),
					Color = Colors.SteelBlue,
				},
				//new Demo
				//{
				//	Title = "ToImage",
				//	Description = "You have that ImageSource and you really want an actual image... What do you do? Well, you ToSKImageAsync that puppy!",
				//	//PageType = typeof(ImageSourceToImagePage),
				//	Color = Colors.Orange,
				//},
			},
			new DemoGroup("TEXT & EMOJI")
			{
				new Demo
				{
					Title = "RichTextKit",
					Description = "Doing text the right way.",
					PageType = typeof(RichTextKitPage),
					Color = Colors.YellowGreen,
				},
			},
			new DemoGroup("SVG")
			{
				new Demo
				{
					Title = "SVG (Svg.Skia)",
					Description = "Everyone wants to load SVG files. Literally everyone. So use a great library: Svg.Skia",
					PageType = typeof(SvgPage),
					Color = Colors.LightSeaGreen,
				},
			},
			new DemoGroup("PLAYGROUND")
			{
				new Demo
				{
					Title = "Playground",
					Description = "The ground for playing.",
					PageType = typeof(PlaygroundPage),
					Color = Colors.Silver,
				},
			},
		};
}
