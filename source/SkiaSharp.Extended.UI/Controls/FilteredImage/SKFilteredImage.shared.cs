using System.Collections.Generic;
using SkiaSharp.Extended.UI.Media;
using SkiaSharp.Extended.UI.Media.Extensions;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Controls
{
	public class SKFilteredImage : ContentView
	{
		public static readonly BindableProperty PipelineProperty = BindableProperty.Create(
			nameof(Pipeline),
			typeof(SKFilterPipeline),
			typeof(SKFilteredImage),
			null,
			propertyChanged: OnFiltersChanged);

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(
			nameof(Source),
			typeof(ImageSource),
			typeof(SKFilteredImage),
			null,
			propertyChanged: OnSourceChanged);

		private SKCanvasView canvasView;
		private SKImage? image;

		public SKFilteredImage()
		{
			canvasView = new SKCanvasView();
			canvasView.PaintSurface += OnPaintSurface;

			Content = canvasView;
		}

		public SKFilterPipeline? Pipeline
		{
			get => (SKFilterPipeline?)GetValue(PipelineProperty);
			set => SetValue(PipelineProperty, value);
		}

		public ImageSource? Source
		{
			get => (ImageSource?)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var info = e.Info;
			var canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			if (image != null)
			{
				var rect = info.Rect.AspectFit(new SKSizeI(image.Width, image.Height));

				if (Pipeline?.Count > 0)
				{
					var enabled = new List<SKFilter>(Pipeline.Count);
					foreach (var filter in Pipeline)
					{
						if (filter.IsEnabled)
							enabled.Add(filter);
					}

					if (enabled.Count == 1)
					{
						canvas.DrawImage(image, rect, enabled[0].GetPaint());
					}
					else if (enabled.Count >= 2)
					{
						var newInfo = info.WithSize(rect.Size);
						EnsureTemporarySurface(newInfo, ref temp1);
						EnsureTemporarySurface(newInfo, ref temp2);

						var tempDest = SKRectI.Create(SKPointI.Empty, rect.Size);

						var prevSurface = temp2.Surface;
						var surface = temp1.Surface;

						int i;
						for (i = 0; i < enabled.Count; i++)
						{
							var filter = enabled[i];
							var paint = filter.GetPaint();

							var tempCanvas = surface.Canvas;
							tempCanvas.Clear(SKColors.Transparent);

							if (i == 0)
								tempCanvas.DrawImage(image, tempDest, paint);
							else
								tempCanvas.DrawSurface(prevSurface, 0, 0, paint);

							// swap
							var s = surface;
							surface = prevSurface;
							prevSurface = s;
						}

						canvas.DrawSurface(prevSurface, rect.Location);
					}
				}
				else
				{
					canvas.DrawImage(image, rect);
				}
			}

			void EnsureTemporarySurface(SKImageInfo newInfo, ref (SKImageInfo Info, SKSurface Surface) temp)
			{
				if (temp.Info != newInfo || temp.Surface == null)
				{
					temp.Surface?.Dispose();
					temp = (newInfo, SKSurface.Create(newInfo));
				}
			}
		}

		(SKImageInfo Info, SKSurface Surface) temp1;
		(SKImageInfo Info, SKSurface Surface) temp2;

		private static void OnFiltersChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKFilteredImage view)
			{
				if (oldValue is SKFilterPipeline oldCollection)
					oldCollection.PipelineChanged -= view.OnFilterChanged;
				if (newValue is SKFilterPipeline newCollection)
					newCollection.PipelineChanged += view.OnFilterChanged;

				view.canvasView.InvalidateSurface();
			}
		}

		private static async void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKFilteredImage view)
			{
				if (newValue is ImageSource source)
					view.image = await source.ToSKImageAsync();
				else
					view.image = null;

				view.canvasView.InvalidateSurface();
			}
		}

		private void OnFilterChanged(object sender, SKFilterChangedEventArgs e)
		{
			canvasView.InvalidateSurface();
		}
	}
}
