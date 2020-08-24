using System;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class ConfettiPage : ContentPage
	{
		public ConfettiPage()
		{
			InitializeComponent();

			BindingContext = this;
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
