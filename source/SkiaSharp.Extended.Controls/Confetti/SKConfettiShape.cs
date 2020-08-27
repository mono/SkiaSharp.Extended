using System;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public abstract class SKConfettiShape : BindableObject
	{
		public abstract void Draw(SKCanvas canvas, SKPaint paint, float size);
	}

	public class SKConfettiSquare : SKConfettiShape
	{
		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			var offset = -size / 2f;
			var rect = SKRect.Create(offset, offset, size, size);
			canvas.DrawRect(rect, paint);
		}
	}

	public class SKConfettiCircle : SKConfettiShape
	{
		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			canvas.DrawCircle(0, 0, size / 2f, paint);
		}
	}

	public class SKConfettiRect : SKConfettiShape
	{
		public static readonly BindableProperty HeightRatioProperty = BindableProperty.Create(
			nameof(HeightRatio),
			typeof(double),
			typeof(SKConfettiRect),
			0.5,
			coerceValue: OnCoerceHeightRatio);

		public SKConfettiRect()
		{
		}

		public SKConfettiRect(double heightRatio)
		{
			HeightRatio = heightRatio;
		}

		public double HeightRatio
		{
			get => (double)GetValue(HeightRatioProperty);
			set => SetValue(HeightRatioProperty, value);
		}

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			var height = size * (float)HeightRatio;
			var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
			canvas.DrawRect(rect, paint);
		}

		private static object OnCoerceHeightRatio(BindableObject bindable, object value) =>
			value is double ratio
				? Math.Max(0, Math.Min(ratio, 1.0))
				: (object)1.0;
	}

	public class SKConfettiOval : SKConfettiShape
	{
		public static readonly BindableProperty HeightRatioProperty = BindableProperty.Create(
			nameof(HeightRatio),
			typeof(double),
			typeof(SKConfettiOval),
			0.5,
			coerceValue: OnCoerceHeightRatio);

		public SKConfettiOval()
		{
		}

		public SKConfettiOval(double heightRatio)
		{
			HeightRatio = heightRatio;
		}

		public double HeightRatio
		{
			get => (double)GetValue(HeightRatioProperty);
			set => SetValue(HeightRatioProperty, value);
		}

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			var height = size * (float)HeightRatio;
			var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
			canvas.DrawOval(rect, paint);
		}

		private static object OnCoerceHeightRatio(BindableObject bindable, object value) =>
			value is double ratio
				? Math.Max(0, Math.Min(ratio, 1.0))
				: (object)1.0;
	}

	public class SKConfettiPath: SKConfettiShape
	{
		public SKConfettiPath(SKPath path)
		{
			Path = path ?? throw new ArgumentNullException(nameof(path));
			BaseSize = Path.TightBounds.Size;
		}

		public SKPath Path { get; }

		public SKSize BaseSize { get; }

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			if (BaseSize.Width <= 0 || BaseSize.Height <= 0)
				return;

			canvas.Save();
			canvas.Scale(size / BaseSize.Width, size / BaseSize.Height);

			canvas.DrawPath(Path, paint);

			canvas.Restore();
		}
	}
}
