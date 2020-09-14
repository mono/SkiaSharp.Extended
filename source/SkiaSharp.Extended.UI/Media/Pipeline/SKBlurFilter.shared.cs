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

		private static void OnFilterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKBlurFilter blur)
			{
				var sigmaX = (float)blur.SigmaX;
				var sigmaY = (float)blur.SigmaY;
				if (sigmaX <= 0)
					sigmaX = 0;
				if (sigmaY <= 0)
					sigmaY = 0;

				blur.imageFilter?.Dispose();
				blur.imageFilter = (sigmaX >= 0 && sigmaY >= 0)
					? SKImageFilter.CreateBlur(sigmaX, sigmaY)
					: null;

				blur.Paint.ImageFilter = blur.imageFilter;

				blur.OnFilterChanged();
			}
		}
	}
}
