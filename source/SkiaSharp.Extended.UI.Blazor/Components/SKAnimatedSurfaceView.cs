using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SkiaSharp.Extended.Internal;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Components;

/// <summary>
/// Renders static or animated SkiaSharp content through a browser canvas or OpenGL surface.
/// </summary>
/// <remarks>
/// When animation is enabled, <see cref="OnUpdate"/> runs immediately before <see cref="OnPaintSurface"/> on each rendered frame.
/// Disable animation and call <see cref="Invalidate"/> for on-demand rendering.
/// </remarks>
[SupportedOSPlatform("browser")]
public class SKAnimatedSurfaceView : ComponentBase
{
	private static readonly IReadOnlyDictionary<string, object> EmptyAttributes = new Dictionary<string, object>();

	private readonly SKFrameCounter frameCounter = new();
	private DynamicComponent? dynamicSurface;
	private object? surface;
	private Action? invalidate;
	private IDictionary<string, object> surfaceParameters = new Dictionary<string, object>();
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	private Type surfaceType = typeof(SKCanvasView);
	private bool wasAnimationEnabled;

#if DEBUG
	private const float DebugStatusMargin = 12f;
#endif

	/// <summary>
	/// Gets or sets the SkiaSharp browser view type used for rendering.
	/// The type must derive from <see cref="SKCanvasView"/> or <see cref="SKGLView"/>.
	/// </summary>
	[Parameter]
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public Type SurfaceType { get; set; } = typeof(SKCanvasView);

	/// <summary>
	/// Gets or sets whether browser animation frames update and repaint the view.
	/// Defaults to <see langword="true"/>.
	/// </summary>
	[Parameter]
	public bool IsAnimationEnabled { get; set; } = true;

	/// <summary>
	/// Gets or sets whether rendering uses CSS pixels instead of physical pixels.
	/// </summary>
	[Parameter]
	public bool IgnorePixelScaling { get; set; }

	/// <summary>
	/// Gets or sets the callback invoked immediately before each animated paint.
	/// </summary>
	[Parameter]
	public Action<TimeSpan>? OnUpdate { get; set; }

	/// <summary>
	/// Gets or sets the callback invoked to paint the current surface.
	/// </summary>
	[Parameter]
	public Action<SKCanvas, SKSize>? OnPaintSurface { get; set; }

	/// <summary>
	/// Gets or sets additional HTML attributes forwarded to the underlying canvas.
	/// </summary>
	[Parameter(CaptureUnmatchedValues = true)]
	public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

	/// <inheritdoc />
	protected override void OnParametersSet()
	{
		ValidateSurfaceType(SurfaceType);
		surfaceParameters = CreateSurfaceParameters();

		if (surfaceType != SurfaceType)
		{
			dynamicSurface = null;
			surface = null;
			invalidate = null;
			frameCounter.Reset();
		}

		if (!wasAnimationEnabled && IsAnimationEnabled)
			frameCounter.Reset();

		surfaceType = SurfaceType;
		wasAnimationEnabled = IsAnimationEnabled;
	}

	/// <inheritdoc />
	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenComponent<DynamicComponent>(0);
		builder.SetKey(SurfaceType);
		builder.AddAttribute(1, nameof(DynamicComponent.Type), SurfaceType);
		builder.AddAttribute(2, nameof(DynamicComponent.Parameters), surfaceParameters);
		builder.AddComponentReferenceCapture(3, value => dynamicSurface = (DynamicComponent)value);
		builder.CloseComponent();
	}

	/// <inheritdoc />
	protected override void OnAfterRender(bool firstRender)
	{
		var instance = dynamicSurface?.Instance;
		if (!ReferenceEquals(surface, instance) && instance is not null)
		{
			surface = instance;
			invalidate = GetInvalidateCallback(instance);
			frameCounter.Reset();
		}
	}

	/// <summary>
	/// Schedules an on-demand repaint, including while animation is disabled.
	/// </summary>
	[SupportedOSPlatform("browser")]
	public void Invalidate() => invalidate?.Invoke();

	/// <summary>
	/// Invoked when the native surface needs to be painted.
	/// </summary>
	protected virtual void OnPaintSurfaceCore(SKCanvas canvas, SKSize size)
	{
		if (IsAnimationEnabled)
			OnUpdate?.Invoke(frameCounter.NextFrame());

		OnPaintSurface?.Invoke(canvas, size);

#if DEBUG
		WriteDebugStatus(canvas);
#endif
	}

	private IDictionary<string, object> CreateSurfaceParameters() =>
		new Dictionary<string, object>
		{
			[nameof(SKCanvasView.AdditionalAttributes)] = AdditionalAttributes ?? EmptyAttributes,
			[nameof(SKCanvasView.EnableRenderLoop)] = IsAnimationEnabled,
			[nameof(SKCanvasView.IgnorePixelScaling)] = IgnorePixelScaling,
			[nameof(SKCanvasView.OnPaintSurface)] = GetPaintCallback(),
		};

	private object GetPaintCallback() =>
		typeof(SKCanvasView).IsAssignableFrom(SurfaceType)
			? (Action<SKPaintSurfaceEventArgs>)HandleCanvasPaintSurface
			: (Action<SKPaintGLSurfaceEventArgs>)HandleGlPaintSurface;

	private void HandleCanvasPaintSurface(SKPaintSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface.Canvas, new SKSize(e.Info.Width, e.Info.Height));

	private void HandleGlPaintSurface(SKPaintGLSurfaceEventArgs e) =>
		OnPaintSurfaceCore(e.Surface.Canvas, new SKSize(e.Info.Width, e.Info.Height));

	private static Action GetInvalidateCallback(object instance)
	{
#pragma warning disable CA1416 // Supported surface components are browser-only.
		return instance switch
		{
			SKCanvasView canvasView => canvasView.Invalidate,
			SKGLView glView => glView.Invalidate,
			_ => throw new InvalidOperationException($"The rendered surface type '{instance.GetType()}' is not supported."),
		};
#pragma warning restore CA1416
	}

	private static void ValidateSurfaceType(Type? value)
	{
		if (value is null || (!typeof(SKCanvasView).IsAssignableFrom(value) && !typeof(SKGLView).IsAssignableFrom(value)))
		{
			throw new ArgumentException(
				$"SurfaceType must derive from {nameof(SKCanvasView)} or {nameof(SKGLView)}.",
				nameof(SurfaceType));
		}
	}

#if DEBUG
	private void WriteDebugStatus(SKCanvas canvas)
	{
		using var font = new SKFont { Size = 12 };
		using var paint = new SKPaint { IsAntialias = true };
		canvas.DrawText(
			$"FPS: {frameCounter.Rate:0.0}",
			DebugStatusMargin,
			DebugStatusMargin + font.Size,
			SKTextAlign.Left,
			font,
			paint);
	}
#endif
}
