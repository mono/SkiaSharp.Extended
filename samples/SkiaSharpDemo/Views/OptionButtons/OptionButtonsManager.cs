using System;
using Xamarin.Forms;

namespace SkiaSharpDemo.Views
{
	internal class OptionButtonsManager
	{
		private bool isSubscribed;

		public void Update(Layout<View> layout, SelectionMode mode)
		{
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
		}

		private static void OnChildAdded(object sender, ElementEventArgs e)
		{
			if (e.Element is Button button)
				button.Clicked += OnButtonClicked;
		}

		private static void OnChildRemoved(object sender, ElementEventArgs e)
		{
			if (e.Element is Button button)
				button.Clicked -= OnButtonClicked;
		}

		private static void OnButtonClicked(object sender, EventArgs e)
		{
			const string Selected = "Selected";
			const string Unselected = "Unselected";

			if (!(sender is Button button) ||
				!(button?.Parent is Layout<View> parent) ||
				!(button.BindingContext is object item))
				return;

			var style = OptionButtons.GetSelectionMode(parent);
			if (style == SelectionMode.None)
				return;

			var selectedItems = OptionButtons.GetSelectedItems(parent);

			// clear the list if we are not a multi-select list
			if (style != SelectionMode.Multiple)
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

				foreach (var btn in parent.Children)
				{
					if (btn != button)
						VisualStateManager.GoToState(btn, Unselected);
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

			VisualStateManager.GoToState(button, shouldSelect ? Selected : Unselected);
		}
	}
}
