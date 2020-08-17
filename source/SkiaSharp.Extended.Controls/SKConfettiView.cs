using System;
using System.Collections.Specialized;
using SkiaSharp.Extended.Controls.Themes;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiView : TemplatedView
	{
		private TimeSpan lastTime;
		private SKCanvasView? canvasView;
		private bool isRunning;

		public SKConfettiView()
		{
			Generic.EnsureRegistered();

			Systems.CollectionChanged += OnSystemsChanged;
			SizeChanged += OnSizeChanged;

			IsRunning = true;
		}

		public SKConfettiSystemCollection Systems { get; } = new SKConfettiSystemCollection();

		public bool IsRunning
		{
			get => isRunning;
			set
			{
				isRunning = value;

				Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
				{
					Invalidate();

					return IsRunning;
				});
			}
		}

		public void Invalidate()
		{
			canvasView?.InvalidateSurface();
		}

		protected override void OnApplyTemplate()
		{
			var templateChild = GetTemplateChild("PART_DrawingSurface");
			if (templateChild is SKCanvasView view)
			{
				canvasView = view;
				canvasView.PaintSurface += OnPaintSurface;
			}
		}

		//protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		//{
		//	base.OnPropertyChanged(propertyName);
		//
		//	if (propertyName == IsRunningProperty.PropertyName)
		//	{
		//		foreach (var system in Systems)
		//			system.IsRunning = IsRunning;
		//	}
		//}

		private void OnSystemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (SKConfettiSystem system in e.NewItems)
				{
					system.UpdateEmitterBounds(Width, Height);
					system.IsRunning = IsRunning;
				}

				Invalidate();
			}
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			foreach (var system in Systems)
			{
				system.UpdateEmitterBounds(Width, Height);
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			if (lastTime == TimeSpan.Zero)
				lastTime = TimeSpan.FromMilliseconds(Environment.TickCount);

			var currentTime = TimeSpan.FromMilliseconds(Environment.TickCount);
			var deltaTime = currentTime - lastTime;
			lastTime = currentTime;

			var canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Transparent);
			canvas.Scale(e.Info.Width / (float)Width);

			for (var i = Systems.Count - 1; i >= 0; i--)
			{
				var system = Systems[i];
				system.Draw(canvas, deltaTime);

				if (system.IsComplete)
					Systems.RemoveAt(i);
			}

			if (Systems.Count == 0)
				lastTime = TimeSpan.Zero;
		}
	}
}
