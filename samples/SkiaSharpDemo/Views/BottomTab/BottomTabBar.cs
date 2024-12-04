namespace SkiaSharpDemo.Views;

[ContentProperty(nameof(Tabs))]
public class BottomTabBar : TemplatedView
{
	public static readonly BindableProperty TabsProperty = BindableProperty.Create(
		nameof(Tabs),
		typeof(BottomTabCollection),
		typeof(BottomTabBar),
		defaultValueCreator: _ => new BottomTabCollection());

	public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
		nameof(SelectedIndex),
		typeof(int),
		typeof(BottomTabBar),
		-1,
		defaultBindingMode: BindingMode.TwoWay,
		propertyChanged: OnSelectedIndexChanged);

	public static readonly BindableProperty HorizontalContentAlignmentProperty = BindableProperty.Create(
		nameof(HorizontalContentAlignment),
		typeof(LayoutOptions),
		typeof(BottomTabBar),
		LayoutOptions.Fill);

	public static readonly BindableProperty VerticalContentAlignmentProperty = BindableProperty.Create(
		nameof(VerticalContentAlignment),
		typeof(LayoutOptions),
		typeof(BottomTabBar),
		LayoutOptions.Fill);

	public static readonly BindableProperty HeaderContentTemplateProperty = BindableProperty.Create(
		nameof(HeaderContentTemplate),
		typeof(DataTemplate),
		typeof(BottomTabBar),
		null);

	public static readonly BindableProperty PagePaddingProperty = BindableProperty.Create(
		nameof(PagePadding),
		typeof(Thickness),
		typeof(BottomTabBar),
		Thickness.Zero);

	private Layout? tabBar;
	private Layout? pages;
	private View? selector;

	public BottomTabBar()
	{
		TabTappedCommand = new Command<View>(OnTabTapped);
	}

	public BottomTabCollection? Tabs
	{
		get => (BottomTabCollection?)GetValue(TabsProperty);
		set => SetValue(TabsProperty, value);
	}

	public int SelectedIndex
	{
		get => (int)GetValue(SelectedIndexProperty);
		set => SetValue(SelectedIndexProperty, value);
	}

	public LayoutOptions HorizontalContentAlignment
	{
		get => (LayoutOptions)GetValue(HorizontalContentAlignmentProperty);
		set => SetValue(HorizontalContentAlignmentProperty, value);
	}

	public LayoutOptions VerticalContentAlignment
	{
		get => (LayoutOptions)GetValue(VerticalContentAlignmentProperty);
		set => SetValue(VerticalContentAlignmentProperty, value);
	}

	public DataTemplate? HeaderContentTemplate
	{
		get => (DataTemplate?)GetValue(HeaderContentTemplateProperty);
		set => SetValue(HeaderContentTemplateProperty, value);
	}

	public Thickness PagePadding
	{
		get => (Thickness)GetValue(PagePaddingProperty);
		set => SetValue(PagePaddingProperty, value);
	}

	// TODO: this probably should not be a public property...
	public Command<View> TabTappedCommand { get; }

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		tabBar = (Layout?)GetTemplateChild("PART_TabBar");
		pages = (Layout?)GetTemplateChild("PART_PagesContainer");
		selector = (View?)GetTemplateChild("PART_Selector");
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);

		if (Tabs?.Count > 0 && SelectedIndex == -1)
			SelectedIndex = 0;
		else
			UpdateSelectedTab();
	}

	private void OnTabTapped(View tab) =>
		SelectedIndex = tabBar?.Children?.IndexOf(tab) ?? -1;

	private void UpdateSelectedTab()
	{
		if (!(tabBar?.Children?.Count > 0 && pages?.Children?.Count > 0))
			return;

		var index = SelectedIndex;

		if (index < 0)
			index = 0;
		if (index >= tabBar.Children.Count)
			index = tabBar.Children.Count - 1;

		var tab = tabBar.Children[index];
		var newPage = pages.Children[index];

		if (selector != null)
		{
			selector.TranslateTo(tab.Frame.X, 0);
			selector.ScaleXTo(tab.Frame.Width / selector.Width);
		}

		foreach (VisualElement page in pages.Children)
		{
			Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(page);
			if (page != newPage)
				_ = HideTab(page);
			else
				_ = ShowTab(page);
		}

		static async Task HideTab(VisualElement page)
		{
			await page.FadeTo(0, 100);
			page.IsVisible = false;
		}

		static async Task ShowTab(VisualElement page)
		{
			page.Opacity = 0;
			page.IsVisible = true;
			await page.FadeTo(1, 100);
		}
	}

	private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is BottomTabBar control)
			control.UpdateSelectedTab();
	}
}
