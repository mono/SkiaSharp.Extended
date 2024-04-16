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

	internal SKSurfaceView()
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

	protected virtual void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
	}

	public void Invalidate()
	{
		InvalidateCore();
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface, e.Info.Size);

	private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface, e.BackendRenderTarget.Size);

	internal virtual void OnPaintSurfaceCore(SKSurface surface, SKSize size)
	{
		var canvas = surface.Canvas;
		var scale = size.Width / (float)Width;
		var canvasSize = new SKSize(size.Width / scale, size.Height / scale);

		canvas.Clear(SKColors.Transparent);
		canvas.Scale(scale);

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
