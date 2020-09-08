using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Extended.Controls;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class GestureSurfacePage : ContentPage
	{
		private bool useHardware;

		private SKPaint paint = new SKPaint
		{
			TextSize = 40,
			IsAntialias = true,
			StrokeWidth = 6,
		};

		private SKMatrix transformMatrix = SKMatrix.MakeIdentity();
		private SKMatrix finalMatrix = SKMatrix.MakeIdentity();
		private float totalScale = 1f;
		private float totalRotation = 0f;
		private SKPoint totalTranslate = SKPoint.Empty;

		private SKRect viewportBounds;

		private const float MaxScale = 1f;
		private const float MinScale = 1f;
		private const float MaxRotation = 180f;
		private const float MinRotation = 0f;

		private const float MinPanX = 200f;
		private const float MaxPanX = 1500f;
		private const float MinPanY = 150f;
		private const float MaxPanY = 1000f;

		public GestureSurfacePage()
		{
			InitializeComponent();

			BindingContext = this;

			gestureSurface.FlingDetected += (sender, e) =>
			{
				Debug.WriteLine($"Flinging: ({e.VelocityX}, {e.VelocityY})");

				var easing = Easing.SinOut;

				var ratio = e.VelocityX / e.VelocityY;

				gestureSurface.AbortAnimation("Fling");
				var animation = new Animation(v => Transform(new SKPoint((float)(v * ratio), (float)v), SKPoint.Empty, 1, 0), e.VelocityY * 0.01f, 0, easing);
				animation.Commit(gestureSurface, "Fling", 16, 1000);
			};

			gestureSurface.TransformDetected += (sender, e) =>
			{
				gestureSurface.AbortAnimation("Fling");

				Transform(e.Center, e.PreviousCenter, e.ScaleDelta, e.RotationDelta);
			};

			gestureSurface.SingleTapDetected += (sender, e) =>
			{
				Debug.WriteLine($"Single tap: ({e.Location.X}, {e.Location.Y})");
			};

			gestureSurface.DoubleTapDetected += (sender, e) =>
			{
				Debug.WriteLine($"Multi-tap: [{e.TapCount}] ({e.Location.X}, {e.Location.Y})");

				gestureSurface.AbortAnimation("Fling");

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
				SKMatrix.Concat(ref transformMatrix, ref m, ref transformMatrix);
			}

			if (scaleDelta != 1)
			{
				if (totalScale * scaleDelta > MaxScale)
					scaleDelta = MaxScale / totalScale;
				if (totalScale * scaleDelta < MinScale)
					scaleDelta = MinScale / totalScale;

				totalScale *= scaleDelta;
				var m = SKMatrix.MakeScale(scaleDelta, scaleDelta, positionScreen.X, positionScreen.Y);
				SKMatrix.Concat(ref transformMatrix, ref m, ref transformMatrix);
			}

			if (rotationDelta != 0)
			{
				if (totalRotation + rotationDelta > MaxRotation)
					rotationDelta = MaxRotation - totalRotation;
				if (totalRotation + rotationDelta < MinRotation)
					rotationDelta = MinRotation - totalRotation;

				totalRotation += rotationDelta;
				var m = SKMatrix.MakeRotationDegrees(rotationDelta, positionScreen.X, positionScreen.Y);
				SKMatrix.Concat(ref transformMatrix, ref m, ref transformMatrix);
			}

			//var values = transformMatrix.Values;
			//var sb = new StringBuilder();
			//for (var i = 0; i < 9; i++)
			//{
			//	if (i % 3 == 0)
			//		sb.AppendLine();
			//	sb.Append(values[i].ToString("000.000"));
			//	sb.Append(", ");
			//}
			//sb.AppendLine();
			//Debug.WriteLine(sb.ToString());



			if (!viewportBounds.IsEmpty)
			{
				var translatedViewport = transformMatrix.MapRect(viewportBounds);

				transformMatrix.TryInvert(out var invMatrix);
				var invertedViewport = invMatrix.MapRect(viewportBounds);

				var bo = new SKRect(MinPanX, MinPanY, MaxPanX, MaxPanY);
				var boundaries = transformMatrix.MapRect(bo);


				#region
				Debug.WriteLine(
					$"vp-o: " +
					$"x={viewportBounds.Left};" +
					$"y={viewportBounds.Top};" +
					$"w={viewportBounds.Width};" +
					$"h={viewportBounds.Height};" +
					$"r={viewportBounds.Right};" +
					$"b={viewportBounds.Bottom}");
				Debug.WriteLine(
					$"vp-s: " +
					$"x={translatedViewport.Left};" +
					$"y={translatedViewport.Top};" +
					$"w={translatedViewport.Width};" +
					$"h={translatedViewport.Height};" +
					$"r={translatedViewport.Right};" +
					$"b={translatedViewport.Bottom}");
				Debug.WriteLine(
					$"b -o: " +
					$"x={bo.Left};" +
					$"y={bo.Top};" +
					$"w={bo.Width};" +
					$"h={bo.Height};" +
					$"r={bo.Right};" +
					$"b={bo.Bottom}");
				Debug.WriteLine(
					$"b -s: " +
					$"x={boundaries.Left};" +
					$"y={boundaries.Top};" +
					$"w={boundaries.Width};" +
					$"h={boundaries.Height};" +
					$"r={boundaries.Right};" +
					$"b={boundaries.Bottom}");
				Debug.WriteLine(
					$"inv : " +
					$"x={invertedViewport.Left};" +
					$"y={invertedViewport.Top};" +
					$"w={invertedViewport.Width};" +
					$"h={invertedViewport.Height};" +
					$"r={invertedViewport.Right};" +
					$"b={invertedViewport.Bottom}");
				#endregion


				var adjustX = 0f;
				var adjustY = 0f;

				//if (boundaries.Left > viewportBounds.Right)
				//	adjustX = viewportBounds.Right - boundaries.Left;
				//if (boundaries.Top > viewportBounds.Bottom)
				//	adjustY = viewportBounds.Bottom - boundaries.Top;
				//if (boundaries.Right < viewportBounds.Left)
				//	adjustX = viewportBounds.Left - boundaries.Right;
				//if (boundaries.Bottom < viewportBounds.Top)
				//	adjustY = viewportBounds.Top - boundaries.Bottom;

				if (invertedViewport.Right < viewportBounds.Left)
					adjustX -= viewportBounds.Left - invertedViewport.Right;
				if (invertedViewport.Bottom < viewportBounds.Top)
					adjustY -= viewportBounds.Top - invertedViewport.Bottom;
				if (invertedViewport.Left > viewportBounds.Right)
					adjustX -= viewportBounds.Right - invertedViewport.Left;
				if (invertedViewport.Top > viewportBounds.Bottom)
					adjustY -= viewportBounds.Bottom - invertedViewport.Top;

				Debug.WriteLine($"{adjustX}, {adjustY}");

				//var m = SKMatrix.MakeTranslation(adjustX, adjustY);
				//SKMatrix.Concat(ref finalMatrix, ref m, ref transformMatrix);
				//transformMatrix = finalMatrix;

				finalMatrix = transformMatrix;

				//if (totalTranslate.X + positionDelta.X < MinPanX)
				//	positionDelta.X = MinPanX - totalTranslate.X;
				//if (totalTranslate.Y + positionDelta.Y < MinPanY)
				//	positionDelta.Y = MinPanY - totalTranslate.Y;
			}
			else
			{
				finalMatrix = transformMatrix;
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

			viewportBounds = SKRect.Create(width, height);

			var boundary = new SKRect(MinPanX, MinPanY, MaxPanX, MaxPanY);

			canvas.Clear(SKColors.CornflowerBlue);

			paint.Color = SKColors.Black;
			paint.Style = SKPaintStyle.Fill;
			for (int x = -20; x <= 20; x++)
			{
				for (int y = -20; y <= 20; y++)
				{
					canvas.DrawText($"{x}x{y}", x * 100, y * 100, paint);
				}
			}

			canvas.SetMatrix(finalMatrix);

			paint.Color = SKColors.Red;
			paint.Style = SKPaintStyle.Fill;
			canvas.DrawRect(300, 300, 200, 200, paint);
			canvas.DrawRect(700, 200, 200, 200, paint);
			canvas.DrawRect(200, 600, 200, 200, paint);

			paint.Color = SKColors.Green;
			paint.Style = SKPaintStyle.Stroke;
			canvas.DrawRect(viewportBounds, paint);
			paint.Color = SKColors.Orange;
			canvas.DrawRect(boundary, paint);

			canvas.ResetMatrix();

			var r = (float)height / (float)width;
			var w = 250.0f;
			var h = w * r;

			var previewMatrix = SKMatrix.MakeIdentity();
			var s = SKMatrix.MakeScale(w / width, w / width);
			SKMatrix.Concat(ref previewMatrix, ref s, ref previewMatrix);
			var t = SKMatrix.MakeTranslation(width - w, height - h);
			SKMatrix.Concat(ref previewMatrix, ref t, ref previewMatrix);

			canvas.SetMatrix(previewMatrix);

			canvas.ClipRect(viewportBounds);

			paint.Color = SKColors.White.WithAlpha(128);
			paint.Style = SKPaintStyle.Fill;
			canvas.DrawRect(viewportBounds, paint);

			paint.Color = SKColors.Green;
			paint.Style = SKPaintStyle.Stroke;
			canvas.DrawRect(viewportBounds, paint);
			paint.Color = SKColors.Orange;
			canvas.DrawRect(boundary, paint);

			if (finalMatrix.TryInvert(out var inv))
			{
				SKMatrix.Concat(ref inv, ref previewMatrix, ref inv);
				canvas.SetMatrix(inv);
			}

			paint.Color = SKColors.Black.WithAlpha(128);
			paint.Style = SKPaintStyle.Fill;
			canvas.DrawRect(viewportBounds, paint);

			canvas.ResetMatrix();
			canvas.ClipRect(viewportBounds);


			paint.Color = SKColors.Red;
			paint.Style = SKPaintStyle.Stroke;
			var v = inv.MapRect(viewportBounds);
			canvas.DrawRect(v, paint);
		}
	}
}
