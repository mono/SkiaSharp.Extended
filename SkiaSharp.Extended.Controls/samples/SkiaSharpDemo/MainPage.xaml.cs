using System;
using SkiaSharp;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class MainPage : ContentPage
	{
		private bool useHardware;

		public MainPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			// set up a nice render loop
			Device.StartTimer(TimeSpan.FromSeconds(1.0 / 60.0), () =>
			{
				dynamicSurface?.InvalidateSurface();
				return true;
			});
		}

		public bool UseHardware
		{
			get => useHardware;
			set
			{
				useHardware = value;
				OnPropertyChanged();
			}
		}

		SKPaint paint = new SKPaint { TextSize = 40 };

		private void OnPainting(object sender, SKPaintDynamicSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			canvas.DrawText($"ticks: {Environment.TickCount}", 200, 200, paint);
			canvas.DrawText($"time:  {DateTime.Now}", 200, 250, paint);
			canvas.DrawText($"is hw: {e.IsHardwareAccelerated}", 200, 300, paint);
		}
	}
}
