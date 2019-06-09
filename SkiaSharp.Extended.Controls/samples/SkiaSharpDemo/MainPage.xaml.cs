using System;
using SkiaSharp;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class MainPage : ContentPage
	{
		private bool useHardware;
		private float elevation;
		private SK3dView rotationView;

		public MainPage()
		{
			InitializeComponent();

			BindingContext = this;

			rotationView = new SK3dView();
			rotationView.RotateYDegrees(30);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			// set up a nice render loop
			Device.StartTimer(TimeSpan.FromSeconds(1.0 / 60.0), () =>
			{
				rotationView.RotateYDegrees(1);
				elevation += 0.05f;

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
		}
	}
}
