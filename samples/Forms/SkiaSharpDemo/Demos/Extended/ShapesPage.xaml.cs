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
		private static readonly Random random = new Random();
		private readonly List<float> sectors = GeneratePieData();
		private readonly SKColor[] colors =
		{
			0xff959edf,
			0xff2de513,
			0xffdbcc93,
			0xffaa6b7f,
			0xff3c5061,
			0xffa31359,
			0xff29ef1b,
			0xff3d0fad,
			0xffd365f1,
			0xff3cd7d5,
		};

		public ShapesPage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			canvasView.InvalidateSurface();
		}

		private static List<float> GeneratePieData()
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
			return data.Select(d => d / sum).ToList();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			// clear the surface
			canvas.Clear(SKColors.Transparent);

			// create a paint for text
			var textPaint = new SKPaint
			{
				IsAntialias = true,
				TextSize = 12,
			};

			// create the paint for the shapes
			var shapePaint = new SKPaint
			{
				IsAntialias = true,
				Color = SKColors.CadetBlue,
			};

			var bigSpace = 12f;
			var smallSpace = 6f;
			var shapeSize = 100f;

			var offsetX = bigSpace;
			var offsetY = bigSpace;

			// Square
			offsetY += textPaint.TextSize;
			canvas.DrawText("Square (from center)", offsetX, offsetY, textPaint);
			offsetY += smallSpace;
			canvas.DrawSquare(offsetX + 50, offsetY + 50, 75, shapePaint);
			offsetY += shapeSize;

			// Triangle
			offsetY += textPaint.TextSize;
			canvas.DrawText("Triangle (using radius)", offsetX, offsetY, textPaint);
			offsetY += smallSpace;
			canvas.DrawTriangle(offsetX + 50, offsetY + 50, 40, shapePaint);
			offsetY += shapeSize;

			// Triangle
			offsetY += textPaint.TextSize;
			canvas.DrawText("Triangle (using radius)", offsetX, offsetY, textPaint);
			offsetY += smallSpace;
			canvas.DrawTriangle(offsetX + 75, offsetY + 50, 75, 40, shapePaint);
			offsetY += shapeSize;

			// Triangle
			offsetY += textPaint.TextSize;
			canvas.DrawText("Triangle (using rect)", offsetX, offsetY, textPaint);
			offsetY += smallSpace;
			canvas.DrawTriangle(SKRect.Create(offsetX, offsetY + 15, 150, 75), shapePaint);
			offsetY += shapeSize;

			// Regular Polygons
			offsetY += textPaint.TextSize;
			canvas.DrawText("Regular Polygons", offsetX, offsetY, textPaint);
			offsetY += smallSpace;
			for (var i = 5; i < 12; i++)
			{
				var x = (i - 5) * 60;
				canvas.DrawRegularPolygon(offsetX + 50 + x, offsetY + 50, 30, i, shapePaint);
			}
			offsetY += shapeSize;

			// Regular Stars
			offsetY += textPaint.TextSize;
			canvas.DrawText("Regular Stars", offsetX, offsetY, textPaint);
			offsetY += smallSpace;
			for (var i = 5; i < 12; i++)
			{
				var x = (i - 5) * 60;
				canvas.DrawStar(offsetX + 50 + x, offsetY + 50, 30, 15, i, shapePaint);
			}
		}
	}
}
