using System;
using SkiaSharp;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public partial class GestureSurfacePage : ContentPage
	{
		private bool useHardware;

		private SKPaint paint = new SKPaint
		{
			TextSize = 40,
			IsAntialias = true
		};

		private SKMatrix totalMatrix = SKMatrix.MakeIdentity();
		private float totalScale = 1f;
		private float totalRotation = 0f;
		private SKPoint totalTranslate = SKPoint.Empty;

		private const float MaxScale = 3f;
		private const float MinScale = 0.5f;
		private const float MaxRotation = 30f;
		private const float MinRotation = -60f;

		public GestureSurfacePage()
		{
			InitializeComponent();

			BindingContext = this;

			//gestureSurface.FlingDetected += (sender, e) =>
			//{
			//	gestureSurface.InvalidateSurface();
			//};
			gestureSurface.TransformDetected += (sender, e) =>
			{
				Transform(e.Center, e.PreviousCenter, e.ScaleDelta, e.RotationDelta);
			};

			gestureSurface.DoubleTapDetected += (sender, e) =>
			{
				Transform(e.Location, e.Location, 1.5f, 0);
			};
		}

		private void Transform(SKPoint positionScreen, SKPoint previousPositionScreen, float scaleDelta, float rotationDelta)
		{
			var positionDelta = positionScreen - previousPositionScreen;
			if (!positionDelta.IsEmpty)
			{
				totalTranslate += positionDelta;
				var m = SKMatrix.MakeTranslation(positionDelta.X, positionDelta.Y);
				SKMatrix.Concat(ref totalMatrix, ref m, ref totalMatrix);
			}

			if (scaleDelta != 1)
			{
				if (totalScale * scaleDelta > MaxScale)
					scaleDelta = MaxScale / totalScale;
				if (totalScale * scaleDelta < MinScale)
					scaleDelta = MinScale / totalScale;

				totalScale *= scaleDelta;
				var m = SKMatrix.MakeScale(scaleDelta, scaleDelta, positionScreen.X, positionScreen.Y);
				SKMatrix.Concat(ref totalMatrix, ref m, ref totalMatrix);
			}

			if (rotationDelta != 0)
			{
				if (totalRotation + rotationDelta > MaxRotation)
					rotationDelta = MaxRotation - totalRotation;
				if (totalRotation + rotationDelta < MinRotation)
					rotationDelta = MinRotation - totalRotation;

				totalRotation += rotationDelta;
				var m = SKMatrix.MakeRotationDegrees(rotationDelta, positionScreen.X, positionScreen.Y);
				SKMatrix.Concat(ref totalMatrix, ref m, ref totalMatrix);
			}

			gestureSurface.InvalidateSurface();
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

			canvas.Clear(SKColors.CornflowerBlue);

			paint.Color = SKColors.Black;
			for (int x = -20; x <= 20; x++)
			{
				for (int y = -20; y <= 20; y++)
				{
					canvas.DrawText($"{x}x{y}", x * 100, y * 100, paint);
				}
			}

			canvas.SetMatrix(totalMatrix);

			paint.Color = SKColors.Red;
			canvas.DrawRect(300, 300, 200, 200, paint);
			canvas.DrawRect(700, 200, 200, 200, paint);
			canvas.DrawRect(200, 600, 200, 200, paint);
		}
	}
}
