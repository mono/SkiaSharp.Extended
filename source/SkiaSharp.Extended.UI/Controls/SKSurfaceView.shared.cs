namespace SkiaSharp.Extended.UI.Controls;

public class SKSurfaceView : TemplatedView
{
#if DEBUG
	private const float DebugStatusMargin = 12f;
	private readonly SKPaint debugStatusPaint =
		new SKPaint
		{
			IsAntialias = true,
			TextSize = 12
		};
	private float debugStatusOffset;
	private SKCanvas? debugStatusCanvas;
#endif

	private SKCanvasView? canvasView;
	private SKGLView? glView;
	private bool enableTouchEvents;
	private float lastScale = 1f;

	internal SKSurfaceView()
	{
		DebugUtils.LogPropertyChanged(this);

#if XAMARIN_FORMS
		this.RegisterLoadedUnloaded(
			() => Loaded?.Invoke(this, EventArgs.Empty),
			() => Unloaded?.Invoke(this, EventArgs.Empty));
#endif
	}

#if XAMARIN_FORMS
	internal event EventHandler? Loaded;
	internal event EventHandler? Unloaded;
#endif

	protected bool EnableTouchEvents
	{
		get => enableTouchEvents;
		set
		{
			enableTouchEvents = value;
			if (canvasView is not null)
				canvasView.EnableTouchEvents = value;
			if (glView is not null)
				glView.EnableTouchEvents = value;
		}
	}

	protected override void OnApplyTemplate()
	{
		var templateChild = GetTemplateChild("PART_DrawingSurface");

		if (canvasView is not null)
		{
			canvasView.PaintSurface -= OnPaintSurface;
			canvasView.Touch -= OnTouch;
			canvasView = null;
		}

		if (glView is not null)
		{
			glView.PaintSurface -= OnPaintSurface;
			glView.Touch -= OnTouch;
			glView = null;
		}

		if (templateChild is SKCanvasView view)
		{
			canvasView = view;
			canvasView.PaintSurface += OnPaintSurface;
			canvasView.Touch += OnTouch;
		}

		if (templateChild is SKGLView gl)
		{
			glView = gl;
			glView.PaintSurface += OnPaintSurface;
			glView.Touch += OnTouch;
		}

		// re-apply the property
		EnableTouchEvents = enableTouchEvents;
	}

	protected virtual void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
	}

	protected virtual void OnTouch(SKTouchEventArgs e)
	{
	}

	public void Invalidate()
	{
		InvalidateCore();
	}

	private void OnTouch(object? sender, SKTouchEventArgs e) =>
		OnTouch(new SKTouchEventArgs(
			e.Id,
			e.ActionType,
			e.MouseButton,
			e.DeviceType,
			new SKPoint(
				e.Location.X / lastScale,
				e.Location.Y / lastScale),
			e.InContact,
			e.WheelDelta,
			e.Pressure));

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface, e.Info.Size);

	private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface, e.BackendRenderTarget.Size);

	internal virtual void OnPaintSurfaceCore(SKSurface surface, SKSize size)
	{
		var canvas = surface.Canvas;
		lastScale = size.Width / (float)Width;
		var canvasSize = new SKSize(size.Width / lastScale, size.Height / lastScale);

		canvas.Clear(SKColors.Transparent);
		canvas.Scale(lastScale);

#if DEBUG
		debugStatusOffset = DebugStatusMargin;
		debugStatusCanvas = canvas;
#endif

		OnPaintSurface(canvas, canvasSize);
	}

#if DEBUG
	internal void WriteDebugStatus(string statusMessage)
	{
		if (debugStatusCanvas is null)
			return;

		debugStatusOffset += debugStatusPaint.TextSize;

		debugStatusCanvas.DrawText(statusMessage, DebugStatusMargin, debugStatusOffset, debugStatusPaint);
	}
#endif

	internal virtual void InvalidateCore()
	{
		if (canvasView?.IsLoadedEx() == true)
			canvasView?.InvalidateSurface();

		if (glView?.IsLoadedEx() == true)
			glView?.InvalidateSurface();
	}
}
