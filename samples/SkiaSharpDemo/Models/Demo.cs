using SkiaSharp;

namespace SkiaSharpDemo;

public class Demo
{
	public required string Title { get; set; }

	public required string Description { get; set; }

	public SKPath? ImagePath { get; set; }

	public required Color Color { get; set; }

	public required Type PageType { get; set; }
}
