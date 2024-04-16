namespace SkiaSharpDemo.Views;

internal class OptionButtonsManager
{
	private const string SelectedState = "Selected";
	private const string UnselectedState = "Unselected";

	private bool isSubscribed;

	public void Update(Layout layout)
	{
		var mode = OptionButtons.GetSelectionMode(layout);
		if (mode == SelectionMode.None)
		{
			foreach (var child in layout.Children)
			{
				if (child is Button button)
					button.Clicked -= OnButtonClicked;
			}

			layout.ChildAdded -= OnChildAdded;
			layout.ChildRemoved -= OnChildRemoved;

			isSubscribed = false;
		}
		else if (!isSubscribed)
		{
			foreach (var child in layout.Children)
			{
				if (child is Button button)
					button.Clicked += OnButtonClicked;
			}

			layout.ChildAdded += OnChildAdded;
			layout.ChildRemoved += OnChildRemoved;

			isSubscribed = true;
		}

		UpdateSelection(layout);
	}

	private static void UpdateSelection(Layout layout)
	{
		var mode = OptionButtons.GetSelectionMode(layout);
		var allowNone = OptionButtons.GetAllowNone(layout);
		var selectedItems = OptionButtons.GetSelectedItems(layout);
		var selectedItem = OptionButtons.GetSelectedItem(layout);

		for (var i = 0; i < layout.Children.Count; i++)
		{
			var child = (View?)layout.Children[i];
			if (child is Button button && child.BindingContext is object item)
			{
				if (mode == SelectionMode.Multiple)
				{
					if (selectedItems?.Contains(item) == true)
						VisualStateManager.GoToState(button, SelectedState);
					else
						VisualStateManager.GoToState(button, UnselectedState);
				}
				else if (mode == SelectionMode.Single)
				{
					if (selectedItem == item)
						VisualStateManager.GoToState(button, SelectedState);
					else
						VisualStateManager.GoToState(button, UnselectedState);
				}
			}
		}
	}

	private static void OnChildAdded(object? sender, ElementEventArgs e)
	{
		if (e.Element is Button button)
			button.Clicked += OnButtonClicked;
	}

	private static void OnChildRemoved(object? sender, ElementEventArgs e)
	{
		if (e.Element is Button button)
			button.Clicked -= OnButtonClicked;
	}

	private static void OnButtonClicked(object? sender, EventArgs e)
	{
		if (!(sender is Button button) ||
			!(button?.Parent is Layout parent) ||
			!(button.BindingContext is object item))
			return;

		var mode = OptionButtons.GetSelectionMode(parent);
		if (mode == SelectionMode.None)
			return;

		var allowNone = OptionButtons.GetAllowNone(parent);
		var selectedItems = OptionButtons.GetSelectedItems(parent);

		if (!allowNone)
		{
			if (selectedItems != null)
			{
				// do not unselect a single item if we are not allowed
				if (selectedItems.Count == 1 && item.Equals(selectedItems[0]))
					return;
			}
		}

		// clear the list if we are not a multi-select list
		if (mode != SelectionMode.Multiple)
		{
			if (selectedItems != null)
			{
				for (var i = selectedItems.Count - 1; i >= 0; i--)
				{
					if (selectedItems[i] == item)
						i--;
					else
						selectedItems.RemoveAt(i);
				}
			}

			foreach (VisualElement btn in parent.Children)
			{
				if (btn != button)
					VisualStateManager.GoToState(btn, UnselectedState);
			}
		}

		var shouldSelect = false;

		// update the selected items list
		if (selectedItems != null)
		{
			shouldSelect = !selectedItems.Contains(item);
			if (shouldSelect)
				selectedItems.Add(item);
			else
				selectedItems.Remove(item);
		}

		// updated the selected item
		OptionButtons.SetSelectedItem(parent, item);

		VisualStateManager.GoToState(button, shouldSelect ? SelectedState : UnselectedState);

#if WINDOWS
		(button.Handler.PlatformView as Microsoft.UI.Xaml.Controls.Button).RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
		(button.Handler.PlatformView as Microsoft.UI.Xaml.Controls.Button).RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Default;
#endif
	}
}
