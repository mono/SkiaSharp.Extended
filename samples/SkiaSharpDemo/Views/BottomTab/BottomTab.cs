namespace SkiaSharpDemo.Views;

public class BottomTab : ContentView
{
	public static readonly BindableProperty TitleProperty = BindableProperty.Create(
		nameof(Title),
		typeof(string),
		typeof(BottomTab),
		null);

	public string? Title
	{
		get => (string?)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}
}
