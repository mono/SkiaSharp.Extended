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

			OptionTappedCommand = new Command<View>(OnOptionTapped);

			BindingContext = this;
		}

		public Command<View> OptionTappedCommand { get; }

		private void OnOptionTapped(View button)
		{
			var vsg = VisualStateManager.GetVisualStateGroups(button);
			var common = vsg.FirstOrDefault(g => g.Name == "SelectedStates");
			if (common == null)
				return;

			var isSingle = button.Parent.ClassId == "SingleSelect";
			if (isSingle)
			{
				foreach (var btn in ((Layout<View>)button.Parent).Children)
				{
					if (btn != button)
						VisualStateManager.GoToState(btn, "Unselected");
				}
			}

			var newState = common.CurrentState?.Name == "Selected"
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
