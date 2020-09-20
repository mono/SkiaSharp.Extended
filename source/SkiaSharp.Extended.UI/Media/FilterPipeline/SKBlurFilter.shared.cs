using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKBlurFilter : SKFilter
	{
		public static readonly BindableProperty SigmaXProperty = BindableProperty.Create(
			nameof(SigmaX),
			typeof(double),
			typeof(SKBlurFilter),
			0.0,
			propertyChanged: OnFilterChanged);

		public static readonly BindableProperty SigmaYProperty = BindableProperty.Create(
			nameof(SigmaY),
			typeof(double),
			typeof(SKBlurFilter),
			0.0,
			propertyChanged: OnFilterChanged);

		private SKImageFilter? imageFilter;

		public double SigmaX
		{
			get => (double)GetValue(SigmaXProperty);
			set => SetValue(SigmaXProperty, value);
		}

		public double SigmaY
		{
			get => (double)GetValue(SigmaYProperty);
			set => SetValue(SigmaYProperty, value);
		}

		private static void OnFilterChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			var filter = (SKBlurFilter)bindable;

			var sigmaX = (float)filter.SigmaX;
			var sigmaY = (float)filter.SigmaY;
			if (sigmaX <= 0)
				sigmaX = 0;
			if (sigmaY <= 0)
				sigmaY = 0;

			filter.imageFilter?.Dispose();
			filter.imageFilter = (sigmaX >= 0 && sigmaY >= 0)
				? SKImageFilter.CreateBlur(sigmaX, sigmaY)
				: null;

			filter.Paint.ImageFilter = filter.imageFilter;

			filter.OnFilterChanged();
		}
	}
}
