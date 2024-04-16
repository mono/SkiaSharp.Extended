namespace SkiaSharpDemo;

public class DemoGroup : List<Demo>
{
	public DemoGroup(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
}
