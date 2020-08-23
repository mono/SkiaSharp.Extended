using System;
using System.Collections.Specialized;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharp.Extended.Controls
{
	public class SKConfettiView : TemplatedView
	{
		private static SKConfettiSystemCollection DefaultSystems =>
			new SKConfettiSystemCollection
			{
				new SKConfettiSystem()
			};

		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
			nameof(IsRunning),
			typeof(bool),
			typeof(SKConfettiView),
			true,
			propertyChanged: OnIsRunningPropertyChanged);

		private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
			nameof(IsComplete),
			typeof(bool),
			typeof(SKConfettiView),
			false);

		public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

		public static readonly BindableProperty SystemsProperty = BindableProperty.Create(
			nameof(Systems),
			typeof(SKConfettiSystemCollection),
			typeof(SKConfettiView),
			DefaultSystems,
			propertyChanged: OnSystemsPropertyChanged);

		private readonly SKFrameCounter frameCounter = new SKFrameCounter();

#if DEBUG
		private readonly SKPaint fpsPaint = new SKPaint { IsAntialias = true };
#endif

		private SKCanvasView? canvasView;

		public SKConfettiView()
		{
			DebugUtils.LogPropertyChanged(this);

			SKConfettiViewResources.EnsureRegistered();

			SizeChanged += OnSizeChanged;

			OnIsRunningPropertyChanged(this, false, IsRunning);
			OnSystemsPropertyChanged(this, null, Systems);
		}

		public bool IsRunning
		{
			get => (bool)GetValue(IsRunningProperty);
			set => SetValue(IsRunningProperty, value);
		}

		public bool IsComplete
		{
			get => (bool)GetValue(IsCompleteProperty);
			private set => SetValue(IsCompletePropertyKey, value);
		}

		public SKConfettiSystemCollection? Systems
		{
			get => (SKConfettiSystemCollection?)GetValue(SystemsProperty);
			set => SetValue(SystemsProperty, value);
		}

		protected override void OnApplyTemplate()
		{
			var templateChild = GetTemplateChild("PART_DrawingSurface");

			if (canvasView != null)
			{
				canvasView.PaintSurface -= OnPaintSurface;
				canvasView = null;
			}

			if (templateChild is SKCanvasView view)
			{
				canvasView = view;
				canvasView.PaintSurface += OnPaintSurface;
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var deltaTime = frameCounter.NextFrame();

			var canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Transparent);
			canvas.Scale(e.Info.Width / (float)Width);

			var particles = 0;
			if (Systems != null)
			{
				for (var i = Systems.Count - 1; i >= 0; i--)
				{
					var system = Systems[i];
					system.Draw(canvas, deltaTime);

					if (system.IsComplete)
						Systems.RemoveAt(i);

					particles += system.ParticleCount;
				}
			}

#if DEBUG
			canvas.DrawText($"{frameCounter.Rate:0.0}", 10f, fpsPaint.TextSize + 10f, fpsPaint);
			canvas.DrawText($"{particles}", 10f, fpsPaint.TextSize * 2 + 20f, fpsPaint);
#endif

			if (Systems != null && Systems.Count == 0)
				frameCounter.Reset();
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			if (Systems != null)
			{
				foreach (var system in Systems)
				{
					system.UpdateEmitterBounds(Width, Height);
				}
			}
		}

		private void Invalidate()
		{
			canvasView?.InvalidateSurface();
		}

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

			UpdateIsComplete();
		}

		private static void OnIsRunningPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			if (bindable is SKConfettiView cv && newValue is bool isRunning)
			{
				if (cv.Systems != null)
				{
					foreach (var system in cv.Systems)
					{
						system.IsRunning = isRunning;
					}
				}

				Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
				{
					cv.Invalidate();

					return cv.IsRunning;
				});
			}
		}

		private static void OnSystemsPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			if (bindable is SKConfettiView cv)
			{
				if (oldValue is SKConfettiSystemCollection oldCollection)
					oldCollection.CollectionChanged -= cv.OnSystemsChanged;

				if (newValue is SKConfettiSystemCollection newCollection)
					newCollection.CollectionChanged += cv.OnSystemsChanged;

				cv.UpdateIsComplete();
			}
		}

		private void UpdateIsComplete()
		{
			if (Systems == null || Systems.Count == 0)
			{
				IsComplete = true;
				return;
			}

			var isComplete = false;
			foreach (var system in Systems)
			{
				if (system.IsComplete)
				{
					isComplete = true;
					break;
				}
			}

			IsComplete = isComplete;
		}
	}
}
