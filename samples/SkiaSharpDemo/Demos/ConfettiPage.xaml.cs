using System;
using System.Linq;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class ConfettiPage : ContentPage
	{
		public ConfettiPage()
		{
			InitializeComponent();

			TabTappedCommand = new Command<View>(OnTabTapped);
			ColorTappedCommand = new Command<View>(OnColorTapped);

			BindingContext = this;
		}

		public Command<View> TabTappedCommand { get; }

		public Command<View> ColorTappedCommand { get; }

		protected override void OnAppearing()
		{
			base.OnAppearing();

			OnTabTapped(tabBar.Children[0]);
		}

		private void OnTabTapped(View tab)
		{
			var idx = tabBar.Children.IndexOf(tab);
			var newPage = pages.Children[idx];

			selector.LayoutTo(new Rectangle(tab.X, tab.Height, tab.Width, 3));

			foreach (var page in pages.Children)
			{
				var p = page;
				if (p != newPage && p.IsVisible)
					p.FadeTo(0, 100).ContinueWith(t => p.IsVisible = false);
			}

			if (!newPage.IsVisible)
			{
				newPage.IsVisible = true;
				newPage.FadeTo(1, 100);
			}
		}

		private void OnColorTapped(View button)
		{
			var vsg = VisualStateManager.GetVisualStateGroups(button);
			var common = vsg.FirstOrDefault(g => g.Name == "SelectedStates");
			var newState = common?.CurrentState?.Name == "Selected"
				? "Unselected"
				: "Selected";

			VisualStateManager.GoToState(button, newState);
		}

		private void OnTapped(object sender, EventArgs e)
		{
			confettiView.Systems!.Add(new SKConfettiSystem
			{
				Emitter = SKConfettiEmitter.Burst(200)
			});
		}
	}
}
