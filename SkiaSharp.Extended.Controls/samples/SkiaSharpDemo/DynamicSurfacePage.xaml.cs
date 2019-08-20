using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Extended.Controls;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class DynamicSurfacePage : ContentPage
	{
		private bool useHardware;
		private float elevation;
		private bool isCurrent;
		private SK3dView rotationView = new SK3dView();
		private Dictionary<long, SKPath> points = new Dictionary<long, SKPath>();

		public DynamicSurfacePage()
		{
			InitializeComponent();

			BindingContext = this;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			isCurrent = true;

			rotationView.RotateYDegrees(30);

			// set up a nice render loop
			Device.StartTimer(TimeSpan.FromSeconds(1.0 / 60.0), () =>
			{
				rotationView.RotateYDegrees(1);
				elevation += 0.05f;

				dynamicSurface?.InvalidateSurface();
				return isCurrent;
			});
		}

		protected override void OnDisappearing()
		{
			isCurrent = false;

			base.OnDisappearing();
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

		private void OnPainting(object sender, SKPaintDynamicSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;
			var sine = (float)Math.Abs(Math.Sin(elevation)) * 100;

			// get the 2D equivalent of the 3D matrix
			var rotationMatrix = rotationView.Matrix;

			// get the properties of the rectangle
			var length = Math.Min(width / 6, height / 6);
			var rect = new SKRect(-length, -length - sine, length, length - sine);
			var side = rotationMatrix.MapPoint(new SKPoint(1, 0)).X > 0;

			canvas.Clear(SKColors.CornflowerBlue);

			// first do 2D translation to the center of the screen
			canvas.Translate(width / 2, height / 2);

			// then apply the 3D rotation
			canvas.Concat(ref rotationMatrix);

			// draw main block
			var paint = new SKPaint
			{
				Color = side ? SKColors.Purple : SKColors.Green,
				Style = SKPaintStyle.Fill,
				IsAntialias = true
			};
			canvas.DrawRoundRect(rect, 30, 30, paint);

			// draw shadow
			var shadow = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(0, length * 2),
				new[] { paint.Color.WithAlpha(127), paint.Color.WithAlpha(0) },
				null,
				SKShaderTileMode.Clamp);
			paint = new SKPaint
			{
				Shader = shadow,
				Style = SKPaintStyle.Fill,
				IsAntialias = true
			};
			rect.Offset(0, length * 2 + 5 + sine + sine);
			canvas.DrawRoundRect(rect, 30, 30, paint);

			// prepare to draw the touch lines
			canvas.ResetMatrix();

			// draw the lines
			paint = new SKPaint
			{
				Color = SKColors.Orange,
				Style = SKPaintStyle.Stroke,
				IsAntialias = true,
				StrokeCap = SKStrokeCap.Round,
				StrokeJoin = SKStrokeJoin.Round,
				StrokeWidth = 20
			};
			foreach (var line in points)
			{
				canvas.DrawPath(line.Value, paint);
			}
		}

		private void OnTouching(object sender, SKTouchEventArgs e)
		{
			switch (e.ActionType)
			{
				case SKTouchAction.Pressed:
					points[e.Id] = new SKPath();
					points[e.Id].MoveTo(e.Location);
					break;
				case SKTouchAction.Moved:
					if (points.TryGetValue(e.Id, out var path))
						path.LineTo(e.Location);
					break;
				case SKTouchAction.Released:
				case SKTouchAction.Cancelled:
					points.Remove(e.Id);
					break;
			}

			e.Handled = true;
		}
	}
}
