using SkiaSharpDemo.Views;

namespace SkiaSharpDemo;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		Resources.MergedDictionaries.Add(new OptionButtonsResources());
		Resources.MergedDictionaries.Add(new BottomTabBarResources());
	}

	protected override Window CreateWindow(IActivationState? activationState) =>
		new Window(new AppShell());
}
