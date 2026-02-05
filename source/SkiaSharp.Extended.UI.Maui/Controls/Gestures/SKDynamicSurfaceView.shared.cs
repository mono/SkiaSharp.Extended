namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A SkiaSharp view that can dynamically switch between hardware (OpenGL) and software (Canvas) rendering.
/// </summary>
/// <remarks>
/// This view provides the flexibility to toggle between GPU-accelerated rendering using OpenGL/Metal
/// and CPU-based software rendering at runtime. This is useful when:
/// - You want to give users the option to choose based on their device capabilities
/// - You need to switch rendering modes based on content being drawn
/// - You want to compare performance between rendering modes
/// </remarks>
public class SKDynamicSurfaceView : TemplatedView
{
	/// <summary>
	/// Identifies the IsHardwareAccelerated bindable property.
	/// </summary>
	public static readonly BindableProperty IsHardwareAcceleratedProperty = BindableProperty.Create(
		nameof(IsHardwareAccelerated),
		typeof(bool),
		typeof(SKDynamicSurfaceView),
		false,
		propertyChanged: OnIsHardwareAcceleratedChanged);

	/// <summary>
	/// Identifies the EnableTouchEvents bindable property.
	/// </summary>
	public static readonly BindableProperty EnableTouchEventsProperty = BindableProperty.Create(
		nameof(EnableTouchEvents),
		typeof(bool),
		typeof(SKDynamicSurfaceView),
		false,
		propertyChanged: OnEnableTouchEventsChanged);

	private SKCanvasView? canvasView;
	private SKGLView? glView;

	/// <summary>
	/// Creates a new instance of SKDynamicSurfaceView.
	/// </summary>
	public SKDynamicSurfaceView()
	{
		ResourceLoader<Themes.SKDynamicSurfaceViewResources>.EnsureRegistered(this);

		DebugUtils.LogPropertyChanged(this);
	}

	/// <summary>
	/// Gets or sets whether hardware acceleration is enabled.
	/// </summary>
	/// <remarks>
	/// When true, rendering uses OpenGL/Metal for GPU acceleration.
	/// When false, rendering uses the CPU-based canvas renderer.
	/// </remarks>
	public bool IsHardwareAccelerated
	{
		get => (bool)GetValue(IsHardwareAcceleratedProperty);
		set => SetValue(IsHardwareAcceleratedProperty, value);
	}

	/// <summary>
	/// Gets or sets whether touch events are enabled.
	/// </summary>
	public bool EnableTouchEvents
	{
		get => (bool)GetValue(EnableTouchEventsProperty);
		set => SetValue(EnableTouchEventsProperty, value);
	}

	/// <summary>
	/// Occurs when the surface needs to be painted.
	/// </summary>
	public event EventHandler<SKPaintDynamicSurfaceEventArgs>? PaintSurface;

	/// <summary>
	/// Occurs when a touch event is detected.
	/// </summary>
	public event EventHandler<SKTouchEventArgs>? Touch;

	/// <summary>
	/// Gets the current canvas size.
	/// </summary>
	public SKSize CanvasSize => canvasView?.CanvasSize ?? glView?.CanvasSize ?? SKSize.Empty;

	/// <summary>
	/// Invalidates the surface and requests a redraw.
	/// </summary>
	public void InvalidateSurface()
	{
		if (canvasView?.IsLoadedEx() == true)
			canvasView?.InvalidateSurface();

		if (glView?.IsLoadedEx() == true)
			glView?.InvalidateSurface();
	}

	/// <inheritdoc/>
	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		// Unsubscribe from old views
		if (canvasView is not null)
		{
			canvasView.PaintSurface -= OnCanvasPaintSurface;
			canvasView.Touch -= OnTouch;
			canvasView = null;
		}

		if (glView is not null)
		{
			glView.PaintSurface -= OnGLPaintSurface;
			glView.Touch -= OnTouch;
			glView = null;
		}

		// Get the new canvas view from template
		var canvasChild = GetTemplateChild("PART_CanvasSurface");
		if (canvasChild is SKCanvasView cv)
		{
			canvasView = cv;
			canvasView.PaintSurface += OnCanvasPaintSurface;
			canvasView.Touch += OnTouch;
			canvasView.EnableTouchEvents = EnableTouchEvents;
			canvasView.IsVisible = !IsHardwareAccelerated;
		}

		// Get the new GL view from template
		var glChild = GetTemplateChild("PART_GLSurface");
		if (glChild is SKGLView gv)
		{
			glView = gv;
			glView.PaintSurface += OnGLPaintSurface;
			glView.Touch += OnTouch;
			glView.EnableTouchEvents = EnableTouchEvents;
			glView.IsVisible = IsHardwareAccelerated;
		}
	}

	/// <summary>
	/// Called when the surface needs to be painted.
	/// </summary>
	protected virtual void OnPaintSurface(SKPaintDynamicSurfaceEventArgs e) =>
		PaintSurface?.Invoke(this, e);

	/// <summary>
	/// Called when a touch event occurs.
	/// </summary>
	protected virtual void OnTouch(SKTouchEventArgs e) =>
		Touch?.Invoke(this, e);

	private void OnCanvasPaintSurface(object? sender, SKPaintSurfaceEventArgs e) =>
		OnPaintSurface(new SKPaintDynamicSurfaceEventArgs(e));

	private void OnGLPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e) =>
		OnPaintSurface(new SKPaintDynamicSurfaceEventArgs(e));

	private void OnTouch(object? sender, SKTouchEventArgs e) =>
		OnTouch(e);

	private void UpdateSurfaceVisibility()
	{
		if (canvasView is not null)
			canvasView.IsVisible = !IsHardwareAccelerated;

		if (glView is not null)
			glView.IsVisible = IsHardwareAccelerated;

		InvalidateSurface();
	}

	private void UpdateTouchEvents()
	{
		if (canvasView is not null)
			canvasView.EnableTouchEvents = EnableTouchEvents;

		if (glView is not null)
			glView.EnableTouchEvents = EnableTouchEvents;
	}

	private static void OnIsHardwareAcceleratedChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is SKDynamicSurfaceView view)
			view.UpdateSurfaceVisibility();
	}

	private static void OnEnableTouchEventsChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is SKDynamicSurfaceView view)
			view.UpdateTouchEvents();
	}
}
