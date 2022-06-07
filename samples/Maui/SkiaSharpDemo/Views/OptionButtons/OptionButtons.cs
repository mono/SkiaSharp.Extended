using System.Collections;

namespace SkiaSharpDemo.Views;

public static class OptionButtons
{
	// OptionButtonsManager

	private static OptionButtonsManager? GetOptionButtonsManager(BindableObject obj) =>
		(OptionButtonsManager?)obj.GetValue(OptionButtonsManagerProperty);

	private static readonly BindableProperty OptionButtonsManagerProperty = BindableProperty.CreateAttached(
		"OptionButtonsManager",
		typeof(OptionButtonsManager),
		typeof(Layout),
		null,
		defaultValueCreator: _ => new OptionButtonsManager());

	// SelectionMode

	public static SelectionMode GetSelectionMode(BindableObject obj) =>
		(SelectionMode)obj.GetValue(SelectionModeProperty);

	public static void SetSelectionMode(BindableObject obj, SelectionMode value) =>
		obj.SetValue(SelectionModeProperty, value);

	public static readonly BindableProperty SelectionModeProperty = BindableProperty.CreateAttached(
		"SelectionMode",
		typeof(SelectionMode),
		typeof(Layout),
		SelectionMode.None,
		propertyChanged: OnPropertyChanged);

	// AllowNone

	public static bool GetAllowNone(BindableObject obj) =>
		(bool)obj.GetValue(AllowNoneProperty);

	public static void SetAllowNone(BindableObject obj, bool value) =>
		obj.SetValue(AllowNoneProperty, value);

	public static readonly BindableProperty AllowNoneProperty = BindableProperty.CreateAttached(
		"AllowNone",
		typeof(bool),
		typeof(Layout),
		true,
		propertyChanged: OnPropertyChanged);

	// SelectedItems

	public static IList? GetSelectedItems(BindableObject obj) =>
		(IList?)obj.GetValue(SelectedItemsProperty);

	public static void SetSelectedItems(BindableObject obj, IList? value) =>
		obj.SetValue(SelectedItemsProperty, value);

	public static readonly BindableProperty SelectedItemsProperty = BindableProperty.CreateAttached(
		"SelectedItems",
		typeof(IList),
		typeof(Layout),
		null,
		propertyChanged: OnPropertyChanged,
		defaultValueCreator: _ => new List<object>());

	// SelectedItem

	public static object? GetSelectedItem(BindableObject obj) =>
		obj.GetValue(SelectedItemProperty);

	public static void SetSelectedItem(BindableObject obj, object? value) =>
		obj.SetValue(SelectedItemProperty, value);

	public static readonly BindableProperty SelectedItemProperty = BindableProperty.CreateAttached(
		"SelectedItem",
		typeof(object),
		typeof(Layout),
		null,
		propertyChanged: OnPropertyChanged,
		defaultBindingMode: BindingMode.TwoWay);

	//

	private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Layout layout || GetOptionButtonsManager(bindable) is not OptionButtonsManager manager)
			return;

		manager.Update(layout);
	}
}
