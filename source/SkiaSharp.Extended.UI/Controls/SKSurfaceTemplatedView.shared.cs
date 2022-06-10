namespace SkiaSharp.Extended.UI.Controls;

public class SKSurfaceTemplatedView : TemplatedView
{
	internal readonly SKFrameCounter frameCounter = new SKFrameCounter();

#if DEBUG
	private const float DebugStatusMargin = 10f;
	private readonly SKPaint fpsPaint = new SKPaint { IsAntialias = true };
#endif

	private SKCanvasView? canvasView;
	private SKGLView? glView;

#if DEBUG
	private float debugStatusOffset;
	private SKCanvas? debugStatusCanvas;
#endif

	internal SKSurfaceTemplatedView()
	{
		DebugUtils.LogPropertyChanged(this);
	}

	protected override void OnApplyTemplate()
	{
		var templateChild = GetTemplateChild("PART_DrawingSurface");

		if (canvasView is not null)
		{
			canvasView.PaintSurface -= OnPaintSurface;
			canvasView = null;
		}

		if (glView is not null)
		{
			glView.PaintSurface -= OnPaintSurface;
			glView = null;
		}

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

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface, e.Info.Size);

	private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface, e.BackendRenderTarget.Size);

	private void OnPaintSurfaceCore(SKSurface surface, SKSize size)
	{
		var deltaTime = frameCounter.NextFrame();

		var canvas = surface.Canvas;

		canvas.Clear(SKColors.Transparent);
		canvas.Scale(size.Width / (float)Width);

		debugStatusOffset = DebugStatusMargin;
		debugStatusCanvas = canvas;

		OnPaintSurface(canvas, size, deltaTime);

#if DEBUG
		WriteDebugStatus($"FPS: {frameCounter.Rate:0.0}");
#endif
	}

	protected virtual void OnPaintSurface(SKCanvas canvas, SKSize size, TimeSpan deltaTime)
	{
	}

#if DEBUG
	protected internal virtual void WriteDebugStatus(string statusMessage)
	{
		if (debugStatusCanvas is null)
			return;

		debugStatusCanvas.DrawText(statusMessage, DebugStatusMargin, debugStatusOffset, fpsPaint);

		debugStatusOffset += fpsPaint.TextSize;
	}
#endif

	public void Invalidate()
	{
#if !XAMARIN_FORMS
		if (canvasView?.IsLoaded == true)
#endif
			canvasView?.InvalidateSurface();

#if !XAMARIN_FORMS
		if (glView?.IsLoaded == true)
#endif
			glView?.InvalidateSurface();
	}
}
