namespace SkiaSharpDemo;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		Demos = ExtendedDemos.GetAllDemos();

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

	private void NavigateTo(Demo demo)
	{
		var page = Activator.CreateInstance(demo.PageType) as Page;

		Navigation.PushAsync(page);
	}
}
