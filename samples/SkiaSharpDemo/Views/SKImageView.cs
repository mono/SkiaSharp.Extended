using System;
using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Extended.UI;
using SkiaSharp.Extended.UI.Extensions;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Views
{
	public class SKImageView : SKCanvasView
	{
		private SKImage? image;

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(
			nameof(Source),
			typeof(ImageSource),
			typeof(SKImageView),
			null,
			propertyChanged: OnSourceChanged);

		public ImageSource? Source
		{
			get => (ImageSource?)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			base.OnPaintSurface(e);

			var info = e.Info;
			var canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			if (image != null)
			{
				var rect = info.Rect.AspectFit(new SKSizeI(image.Width, image.Height));

				canvas.DrawImage(image, rect);
			}
		}

		private static async void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKImageView view)
			{
				var source = newValue as ImageSource;

				if (source == null)
					view.image = null;
				else
				{
					try
					{
						var img = await source.ToSKImageAsync();
						view.image = img;
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Unable to load image: " + ex);

						view.image = null;
					}
				}

				view.InvalidateSurface();
			}
		}
	}
}
