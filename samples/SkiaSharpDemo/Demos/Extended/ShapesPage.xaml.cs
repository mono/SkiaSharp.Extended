using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class ShapesPage : ContentPage
	{
		private readonly Random random = new Random();

		private List<float> sectors;

		public ShapesPage()
		{
			InitializeComponent();

			GenerateData();

			BindingContext = this;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			GenerateData();
		}

		private void GenerateData()
		{
			// generate some random data
			var count = random.Next(5, 10);
			var data = new List<float>();
			while (count-- > 0)
			{
				data.Add(random.Next(10, 100));
			}
			var sum = data.Sum();

			// get the percentages
			sectors?.Clear();
			sectors = data.Select(d => d / sum).ToList();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			// clear the surface
			canvas.Clear(SKColors.Transparent);

			// get the radius of the screen
			var radius = Math.Min(e.Info.Width / 2f, e.Info.Height / 2f);

			// create the paint for a star
			var starPaint = new SKPaint
			{
				IsAntialias = true,
				Color = SKColors.Gold
			};

			// draw the star
			canvas.DrawStar(e.Info.Width / 2f, e.Info.Height / 2f, radius * 0.4f, radius * 0.2f, 5, starPaint);

			// create the paint for the pie
			var piePaint = new SKPaint
			{
				IsAntialias = true,
				Color = SKColors.Green
			};

			// calculate the pie
			var piePath = SKGeometry.CreatePiePath(sectors, radius * 0.9f, radius * 0.5f, 6f);

			// draw the pie
			canvas.Translate(e.Info.Width / 2f, e.Info.Height / 2f);
			canvas.DrawPath(piePath, piePaint);
		}
	}
}
