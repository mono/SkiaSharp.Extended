using System;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKDynamicSurfaceView : Layout<View>
	{
		public static readonly BindableProperty IsHardwareAcceleratedProperty = BindableProperty.Create(
			nameof(IsHardwareAccelerated),
			typeof(bool),
			typeof(SKDynamicSurfaceView),
			false,
			propertyChanged: OnIsHardwareAcceleratedChanged);

		private SKCanvasView canvasView;
		private SKGLView glView;

		public SKDynamicSurfaceView()
		{
			SwapSurface(false);
		}

		public bool IsHardwareAccelerated
		{
			get => (bool)GetValue(IsHardwareAcceleratedProperty);
			set => SetValue(IsHardwareAcceleratedProperty, value);
		}

		public event EventHandler<SKPaintDynamicSurfaceEventArgs> PaintSurface;

		public void InvalidateSurface()
		{
			if (canvasView?.IsVisible == true)
				canvasView?.InvalidateSurface();

			if (glView?.IsVisible == true)
				glView?.InvalidateSurface();
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			foreach (var child in Children)
			{
				if (!child.IsVisible)
					continue;

				if (child == canvasView || child == glView)
					child.Layout(new Rectangle(0, 0, Width, Height));
				else
					child.Layout(new Rectangle(x, y, width, height));
			}
		}

		protected virtual void OnPaintSurface(SKPaintDynamicSurfaceEventArgs e) =>
			PaintSurface?.Invoke(this, e);

		private void SwapSurface(bool hardware)
		{
			if (hardware)
			{
				glView = new SKGLView();
				glView.PaintSurface += OnPaintSurfaceHandler;
				Children.Insert(0, glView);

				if (canvasView != null)
				{
					canvasView.PaintSurface -= OnPaintSurfaceHandler;
					canvasView.IsVisible = false;
					Children.Remove(canvasView);
					canvasView = null;
				}
			}
			else
			{
				canvasView = new SKCanvasView();
				canvasView.PaintSurface += OnPaintSurfaceHandler;
				Children.Insert(0, canvasView);

				if (glView != null)
				{
					glView.PaintSurface -= OnPaintSurfaceHandler;
					glView.IsVisible = false;
					Children.Remove(glView);
					glView = null;
				}
			}
		}

		private void OnPaintSurfaceHandler(object sender, SKPaintSurfaceEventArgs e) =>
			OnPaintSurface(new SKPaintDynamicSurfaceEventArgs(e));

		private void OnPaintSurfaceHandler(object sender, SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(new SKPaintDynamicSurfaceEventArgs(e));

		private static void OnIsHardwareAcceleratedChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SKDynamicSurfaceView view && newValue is bool hardware)
				view.SwapSurface(hardware);
		}
	}
}
