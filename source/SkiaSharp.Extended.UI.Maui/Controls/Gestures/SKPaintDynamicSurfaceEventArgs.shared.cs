namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Event arguments for paint surface events that can switch between hardware and software rendering.
/// </summary>
public class SKPaintDynamicSurfaceEventArgs : EventArgs
{
	private readonly SKPaintSurfaceEventArgs? canvasEvent;
	private readonly SKPaintGLSurfaceEventArgs? glEvent;

	/// <summary>
	/// Creates a new instance from a canvas (software) paint event.
	/// </summary>
	public SKPaintDynamicSurfaceEventArgs(SKPaintSurfaceEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		canvasEvent = e;
		IsHardwareAccelerated = false;
	}

	/// <summary>
	/// Creates a new instance from a GL (hardware) paint event.
	/// </summary>
	public SKPaintDynamicSurfaceEventArgs(SKPaintGLSurfaceEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		glEvent = e;
		IsHardwareAccelerated = true;
	}

	/// <summary>
	/// Gets whether this paint event is using hardware acceleration.
	/// </summary>
	public bool IsHardwareAccelerated { get; }

	/// <summary>
	/// Gets the surface to draw on.
	/// </summary>
	public SKSurface Surface => canvasEvent?.Surface ?? glEvent!.Surface;

	/// <summary>
	/// Gets the backend render target (only available for hardware accelerated rendering).
	/// </summary>
	public GRBackendRenderTarget? BackendRenderTarget => glEvent?.BackendRenderTarget;

	/// <summary>
	/// Gets the image info for the surface.
	/// </summary>
	public SKImageInfo Info => canvasEvent?.Info ?? 
		new SKImageInfo(glEvent!.BackendRenderTarget.Width, glEvent.BackendRenderTarget.Height, glEvent.ColorType);

	/// <summary>
	/// Gets the color type of the surface.
	/// </summary>
	public SKColorType ColorType => canvasEvent?.Info.ColorType ?? glEvent?.ColorType ?? SKColorType.Unknown;

	/// <summary>
	/// Gets the surface origin (only relevant for GL surfaces).
	/// </summary>
	public GRSurfaceOrigin Origin => glEvent?.Origin ?? GRSurfaceOrigin.TopLeft;
}
