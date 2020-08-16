using System;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class PlaygroundPage : ContentPage
	{
		public PlaygroundPage()
		{
			InitializeComponent();
		}

		private void OnTapped(object sender, EventArgs e)
		{
			confettiView.Systems.Add(new SKConfettiSystem());
		}
	}
}
