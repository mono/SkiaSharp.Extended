using System;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public abstract class SKConfettiShape : BindableObject
	{
		public abstract void Draw(SKCanvas canvas, SKPaint paint, float size);
	}

	public class SKConfettiSquareShape : SKConfettiShape
	{
		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			var offset = -size / 2f;
			var rect = SKRect.Create(offset, offset, size, size);
			canvas.DrawRect(rect, paint);
		}
	}

	public class SKConfettiCircleShape : SKConfettiShape
	{
		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			canvas.DrawCircle(0, 0, size / 2f, paint);
		}
	}

	public class SKConfettiRectShape : SKConfettiShape
	{
		public static readonly BindableProperty HeightRatioProperty = BindableProperty.Create(
			nameof(HeightRatio),
			typeof(double),
			typeof(SKConfettiRectShape),
			0.5,
			coerceValue: OnCoerceHeightRatio);

		public SKConfettiRectShape()
		{
		}

		public SKConfettiRectShape(double heightRatio)
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
			if (size <= 0 || height <= 0)
				return;

			var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
			canvas.DrawRect(rect, paint);
		}

		private static object OnCoerceHeightRatio(BindableObject bindable, object value) =>
			value is double ratio
				? Math.Max(0, Math.Min(ratio, 1.0))
				: (object)1.0;
	}

	public class SKConfettiOvalShape : SKConfettiShape
	{
		public static readonly BindableProperty HeightRatioProperty = BindableProperty.Create(
			nameof(HeightRatio),
			typeof(double),
			typeof(SKConfettiOvalShape),
			0.5,
			coerceValue: OnCoerceHeightRatio);

		public SKConfettiOvalShape()
		{
		}

		public SKConfettiOvalShape(double heightRatio)
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
			if (size <= 0 || height <= 0)
				return;

			var rect = SKRect.Create(-size / 2f, -height / 2f, size, height);
			canvas.DrawOval(rect, paint);
		}

		private static object OnCoerceHeightRatio(BindableObject bindable, object value) =>
			value is double ratio
				? Math.Max(0, Math.Min(ratio, 1.0))
				: (object)1.0;
	}

	public class SKConfettiPathShape : SKConfettiShape
	{
		public static readonly BindableProperty PathProperty = BindableProperty.Create(
			nameof(Path),
			typeof(SKPath),
			typeof(SKConfettiPathShape),
			null,
			propertyChanged: OnPathChanged);

		private SKSize baseSize;

		public SKConfettiPathShape(SKPath path)
		{
			Path = path ?? throw new ArgumentNullException(nameof(path));
		}

		public SKPath? Path
		{
			get => (SKPath?)GetValue(PathProperty);
			set => SetValue(PathProperty, value);
		}

		public override void Draw(SKCanvas canvas, SKPaint paint, float size)
		{
			if (baseSize.Width <= 0 || baseSize.Height <= 0 || Path == null)
				return;

			canvas.Save();
			canvas.Scale(size / baseSize.Width, size / baseSize.Height);

			canvas.DrawPath(Path, paint);

			canvas.Restore();
		}

		private static void OnPathChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKConfettiPathShape shape && newValue is SKPath path)
				shape.baseSize = path.TightBounds.Size;
		}
	}
}
