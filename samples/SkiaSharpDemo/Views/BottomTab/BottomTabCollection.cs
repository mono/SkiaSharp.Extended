using System.Collections.ObjectModel;

namespace SkiaSharpDemo.Views;

public class BottomTabCollection : ObservableCollection<BottomTab>
{
	public BottomTabCollection()
	{
	}

	public BottomTabCollection(IEnumerable<BottomTab> collection)
		: base(collection)
	{
	}

	public BottomTabCollection(List<BottomTab> list)
		: base(list)
	{
	}
}
