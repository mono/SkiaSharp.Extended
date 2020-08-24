using System.Collections.Generic;
using Xamarin.Forms;

namespace SkiaSharpDemo.Views
{
	public static class OptionButtons
	{
		// OptionButtonsManager

		private static OptionButtonsManager? GetOptionButtonsManager(BindableObject obj) =>
			(OptionButtonsManager?)obj.GetValue(OptionButtonsManagerProperty);

		private static readonly BindableProperty OptionButtonsManagerProperty = BindableProperty.CreateAttached(
			"OptionButtonsManager",
			typeof(OptionButtonsManager),
			typeof(Layout<View>),
			null,
			defaultValueCreator: b => new OptionButtonsManager());

		// SelectionMode

		public static SelectionMode GetSelectionMode(BindableObject obj) =>
			(SelectionMode)obj.GetValue(SelectionModeProperty);

		public static void SetSelectionMode(BindableObject obj, SelectionMode value) =>
			obj.SetValue(SelectionModeProperty, value);

		public static readonly BindableProperty SelectionModeProperty = BindableProperty.CreateAttached(
			"SelectionMode",
			typeof(SelectionMode),
			typeof(Layout<View>),
			SelectionMode.None,
			propertyChanged: OnSelectionModeChanged);

		// SelectedItems

		public static IList<object>? GetSelectedItems(BindableObject obj) =>
			(IList<object>?)obj.GetValue(SelectedItemsProperty);

		public static void SetSelectedItems(BindableObject obj, IList<object>? value) =>
			obj.SetValue(SelectedItemsProperty, value);

		public static readonly BindableProperty SelectedItemsProperty = BindableProperty.CreateAttached(
			"SelectedItems",
			typeof(IList<object>),
			typeof(Layout<View>),
			null,
			defaultValueCreator: b => new List<object>());

		// SelectedItem

		public static object? GetSelectedItem(BindableObject obj) =>
			obj.GetValue(SelectedItemProperty);

		public static void SetSelectedItem(BindableObject obj, object? value) =>
			obj.SetValue(SelectedItemProperty, value);

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.CreateAttached(
			"SelectedItem",
			typeof(object),
			typeof(Layout<View>),
			null,
			defaultBindingMode: BindingMode.TwoWay);

		//

		private static void OnSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!(bindable is Layout<View> layout) ||
				!(GetOptionButtonsManager(bindable) is OptionButtonsManager manager) ||
				!(newValue is SelectionMode mode))
				return;

			manager.Update(layout, mode);
		}
	}
}
