using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKHighContrastFilter : SKFilter
	{
		public static readonly BindableProperty FactorProperty = BindableProperty.Create(
			nameof(Factor),
			typeof(double),
			typeof(SKBlurFilter),
			0.0,
			propertyChanged: OnFilterChanged);

		private SKColorFilter? colorFilter;

		public double Factor
		{
			get => (double)GetValue(FactorProperty);
			set => SetValue(FactorProperty, value);
		}

		private static void OnFilterChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			var filter = (SKHighContrastFilter)bindable;

			// TODO: these formulas

			var factor = (float)filter.Factor + 1.0f;
			if (factor > 1)
				factor = factor * factor * factor * factor;

			filter.colorFilter?.Dispose();
			filter.colorFilter = SKColorFilter.CreateColorMatrix(new float[SKColorFilter.ColorMatrixSize]
			{
					factor, 0.0f, 0.0f, 0.0f, (1.0f - factor) / 2.0f,
					0.0f, factor, 0.0f, 0.0f, (1.0f - factor) / 2.0f,
					0.0f, 0.0f, factor, 0.0f, (1.0f - factor) / 2.0f,
					0.0f, 0.0f, 0.0f, 1.0f, 0.0f
			});

			filter.Paint.ColorFilter = filter.colorFilter;

			filter.OnFilterChanged();
		}
	}
}
