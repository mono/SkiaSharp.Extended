namespace SkiaSharpDemo;

public partial class MainPage : ContentPage
{
	private bool isNavigating;

	public MainPage()
	{
		InitializeComponent();

		Demos = ExtendedDemos.GetAllDemos();

		BindingContext = this;
	}

	public List<DemoGroup> Demos { get; }

	private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is Demo demo)
		{
			collectionView.SelectedItem = null;
			await NavigateToAsync(demo);
		}
	}

	private async void OnDemoTapped(object sender, TappedEventArgs e)
	{
		if (sender is BindableObject { BindingContext: Demo demo })
			await NavigateToAsync(demo);
	}

	private async Task NavigateToAsync(Demo demo)
	{
		if (isNavigating)
			return;

		isNavigating = true;
		var page = Activator.CreateInstance(demo.PageType) as Page;

		try
		{
			await Navigation.PushAsync(page);
		}
		finally
		{
			isNavigating = false;
		}
	}
}
