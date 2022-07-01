using System;
using System.Collections.Generic;
using SkiaSharp.Extended.UI.Media;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Media
{
	//public class SKFilterPipelineImageSource : IMageSource

}


namespace SkiaSharp.Extended.UI.Controls
{
	[ContentProperty(nameof(Pipeline))]
	public class SKFilteredImage : TemplatedView
	{
		public static readonly BindableProperty PipelineProperty = BindableProperty.Create(
			nameof(Pipeline),
			typeof(SKFilterPipeline),
			typeof(SKFilteredImage),
			null,
			propertyChanged: OnPipelineChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(
			nameof(Aspect),
			typeof(Aspect),
			typeof(SKFilteredImage),
			Aspect.AspectFit,
			propertyChanged: OnAspectChanged);

		private (SKImageInfo Info, SKSurface? Surface) tempSurface1;
		private (SKImageInfo Info, SKSurface? Surface) tempSurface2;

		private SKCanvasView? canvasView;
		private SKGLView? glView;

		public SKFilteredImage()
		{
			DebugUtils.LogPropertyChanged(this);

			Themes.SKFilteredImageResources.EnsureRegistered();
		}

		public SKFilterPipeline? Pipeline
		{
			get => (SKFilterPipeline?)GetValue(PipelineProperty);
			set => SetValue(PipelineProperty, value);
		}

		public Aspect Aspect
		{
			get => (Aspect)GetValue(AspectProperty);
			set => SetValue(AspectProperty, value);
		}

		protected override void OnApplyTemplate()
		{
			var templateChild = GetTemplateChild("PART_DrawingSurface");

			if (canvasView != null)
			{
				canvasView.PaintSurface -= OnPaintSurface;
				canvasView = null;
			}

			if (glView != null)
			{
				glView.PaintSurface -= OnPaintSurface;
				glView = null;
			}

			tempSurface1.Surface?.Dispose();
			tempSurface1 = default;

			tempSurface2.Surface?.Dispose();
			tempSurface2 = default;

			if (templateChild is SKCanvasView view)
			{
				canvasView = view;
				canvasView.PaintSurface += OnPaintSurface;
			}

			if (templateChild is SKGLView gl)
			{
				glView = gl;
				glView.PaintSurface += OnPaintSurface;
			}
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			Pipeline?.SetInheritedBindingContext(BindingContext);
		}

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			var view = (SKGLView)sender;
			var brt = e.BackendRenderTarget;
			var canvas = e.Surface.Canvas;

			OnPaintSurface(canvas, brt.Rect, (newSize) =>
			{
				var newInfo = new SKImageInfo(newSize.Width, newSize.Height, e.ColorType);

				EnsureTemporarySurface(view.GRContext, newInfo, ref tempSurface1);
				EnsureTemporarySurface(view.GRContext, newInfo, ref tempSurface2);
			});

			void EnsureTemporarySurface(GRContext context, SKImageInfo newInfo, ref (SKImageInfo Info, SKSurface? Surface) temp)
			{
				if (temp.Info != newInfo || temp.Surface == null)
				{
					temp.Surface?.Dispose();
					temp = (newInfo, SKSurface.Create(context, true, newInfo, brt!.SampleCount, e.Origin));
				}
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var view = (SKCanvasView)sender;
			var info = e.Info;
			var canvas = e.Surface.Canvas;

			OnPaintSurface(canvas, info.Rect, (newSize) =>
			{
				var newInfo = info.WithSize(newSize);

				EnsureTemporarySurface(newInfo, ref tempSurface1);
				EnsureTemporarySurface(newInfo, ref tempSurface2);
			});

			void EnsureTemporarySurface(SKImageInfo newInfo, ref (SKImageInfo Info, SKSurface? Surface) temp)
			{
				if (temp.Info != newInfo || temp.Surface == null)
				{
					temp.Surface?.Dispose();
					temp = (newInfo, SKSurface.Create(newInfo));
				}
			}
		}

		private void OnPaintSurface(SKCanvas canvas, SKRectI viewRect, Action<SKSizeI> EnsureTemporarySurfaces)
		{
			canvas.Clear(SKColors.Transparent);

			if (Pipeline?.Image is SKImage image)
			{
				var imageSize = new SKSizeI(image.Width, image.Height);
				var rect = Aspect switch
				{
					Aspect.AspectFit => viewRect.AspectFit(imageSize),
					Aspect.AspectFill => viewRect.AspectFill(imageSize),
					Aspect.Fill => viewRect,
					_ => throw new ArgumentOutOfRangeException(nameof(Aspect)),
				};

				var enabledFilters = GetEnabledFilters();
				if (enabledFilters?.Count > 0)
				{
					if (enabledFilters.Count == 1)
					{
						canvas.DrawImage(image, rect, enabledFilters[0].GetPaint());
					}
					else if (enabledFilters.Count >= 2)
					{
						EnsureTemporarySurfaces(rect.Size);

						var tempDest = SKRectI.Create(SKPointI.Empty, rect.Size);

						var prevSurface = tempSurface2.Surface!;
						var surface = tempSurface1.Surface!;

						for (var i = 0; i < enabledFilters.Count; i++)
						{
							var filter = enabledFilters[i];
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
		}

		private List<SKFilter>? GetEnabledFilters()
		{
			if (Pipeline?.Filters == null || Pipeline?.IsEnabled != true)
				return null;

			List<SKFilter>? enabled = null;

			var count = Pipeline.Filters.Count;
			if (count > 0)
			{
				enabled = new List<SKFilter>(count);
				foreach (var filter in Pipeline.Filters)
				{
					if (filter.IsEnabled)
						enabled.Add(filter);
				}
			}

			return enabled;
		}

		private void InvalidateSurface()
		{
			canvasView?.InvalidateSurface();
			glView?.InvalidateSurface();
		}

		private static void OnPipelineChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			var view = (SKFilteredImage)bindable;

			if (oldValue is SKFilterPipeline oldPipeline)
			{
				oldPipeline.PipelineChanged -= OnPipelineChanged;
				oldPipeline.FilterChanged -= OnFilterChanged;
				oldPipeline.SetInheritedBindingContext(null);
			}

			if (newValue is SKFilterPipeline newPipeline)
			{
				newPipeline.SetInheritedBindingContext(view.BindingContext);
				newPipeline.PipelineChanged += OnPipelineChanged;
				newPipeline.FilterChanged += OnFilterChanged;
			}

			view.InvalidateSurface();

			void OnPipelineChanged(object sender, EventArgs e) =>
				view.InvalidateSurface();

			void OnFilterChanged(object sender, SKFilterChangedEventArgs e) =>
				view.InvalidateSurface();
		}

		private static void OnAspectChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var view = (SKFilteredImage)bindable;

			view.InvalidateSurface();
		}
	}
}
