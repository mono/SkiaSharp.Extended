using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SkiaSharp.Skottie;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Components;

/// <summary>
/// A browser-only Blazor Lottie component that composes <see cref="SKAnimatedSurfaceView"/>
/// with an <see cref="SKLottiePlayer"/>.
/// </summary>
[SupportedOSPlatform("browser")]
public class SKLottieView : ComponentBase, IDisposable
{
	private static readonly TimeSpan ProgressReportInterval = TimeSpan.FromMilliseconds(100);
	private static readonly IReadOnlyDictionary<string, object> EmptyAttributes =
		new Dictionary<string, object>();

	private readonly SKLottiePlayer player = new();
	private CancellationTokenSource? loadCancellation;
	private SKAnimatedSurfaceView? surface;
	private SKLottieImageSource? currentSource;
	private bool invalidateAfterRender;
	private bool progressChangedPending;
	private bool completionPending;
	private bool disposed;
	private TimeSpan progressReportElapsed;

	[Inject]
	private IServiceProvider? services { get; set; }

	/// <summary>Gets or sets the Lottie source.</summary>
	[Parameter]
	public SKLottieImageSource? Source { get; set; }

	/// <summary>How the animation repeats. Defaults to <see cref="SKLottieRepeat.Never"/>.</summary>
	[Parameter]
	public SKLottieRepeat Repeat { get; set; } = SKLottieRepeat.Never;

	/// <summary>Playback speed multiplier. Negative values play in reverse. Defaults to <c>1.0</c>.</summary>
	[Parameter]
	public double AnimationSpeed { get; set; } = 1.0;

	/// <summary>Whether the animation loop is running. Defaults to <see langword="true"/>.</summary>
	[Parameter]
	public bool IsAnimationEnabled { get; set; } = true;

	/// <summary>
	/// Gets or sets the SkiaSharp browser view type used to render the animation.
	/// The type must derive from <see cref="SKCanvasView"/> or <see cref="SKGLView"/>.
	/// </summary>
	[Parameter]
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public Type SurfaceType { get; set; } = typeof(SKCanvasView);

	/// <summary>Gets or sets whether rendering uses CSS pixels instead of physical pixels.</summary>
	[Parameter]
	public bool IgnorePixelScaling { get; set; }

	/// <summary>Fires when the animation is successfully loaded.</summary>
	[Parameter]
	public EventCallback AnimationLoaded { get; set; }

	/// <summary>Fires after all repeats complete.</summary>
	[Parameter]
	public EventCallback AnimationCompleted { get; set; }

	/// <summary>Reports playback progress about ten times per second while the animation is running.</summary>
	[Parameter]
	public EventCallback<TimeSpan> ProgressChanged { get; set; }

	/// <summary>Fires when loading fails.</summary>
	[Parameter]
	public EventCallback<Exception> AnimationFailed { get; set; }

	/// <summary>Additional HTML attributes forwarded to the underlying canvas element.</summary>
	[Parameter(CaptureUnmatchedValues = true)]
	public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

	/// <summary>Gets whether an animation is loaded and ready to play.</summary>
	public bool HasAnimation => player.HasAnimation;

	/// <summary>Gets the total animation duration.</summary>
	public TimeSpan Duration => player.Duration;

	/// <summary>Gets the animation's natural size.</summary>
	public SKSize Size => player.Animation?.Size ?? SKSize.Empty;

	/// <summary>Gets the animation's frame rate.</summary>
	public double Fps => player.Animation?.Fps ?? 0;

	/// <summary>Gets the current playback position.</summary>
	public TimeSpan Progress => player.Progress;

	/// <summary>Gets whether the animation has completed all repeats.</summary>
	public bool IsComplete => player.IsComplete;

	/// <summary>Seeks to an absolute playback position, clamped to the loaded animation's duration.</summary>
	public void Seek(TimeSpan position)
	{
		player.Seek(position);
		QueueProgressChanged();
		_ = RequestRenderAsync(invalidateSurface: true);
	}

	/// <inheritdoc />
	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenComponent<SKAnimatedSurfaceView>(0);
		builder.AddAttribute(1, nameof(SKAnimatedSurfaceView.AdditionalAttributes), AdditionalAttributes ?? EmptyAttributes);
		builder.AddAttribute(2, nameof(SKAnimatedSurfaceView.SurfaceType), SurfaceType);
		builder.AddAttribute(
			3,
			nameof(SKAnimatedSurfaceView.IsAnimationEnabled),
			IsAnimationEnabled && player.AnimationSpeed != 0 && player.HasAnimation && !player.IsComplete);
		builder.AddAttribute(4, nameof(SKAnimatedSurfaceView.IgnorePixelScaling), IgnorePixelScaling);
		builder.AddAttribute(5, nameof(SKAnimatedSurfaceView.OnUpdate), (Action<TimeSpan>)HandleUpdate);
		builder.AddAttribute(6, nameof(SKAnimatedSurfaceView.OnPaintSurface), (Action<SKCanvas, SKSize>)HandlePaintSurface);
		builder.AddComponentReferenceCapture(7, component => surface = (SKAnimatedSurfaceView)component);
		builder.CloseComponent();
	}

	/// <inheritdoc />
	protected override void OnParametersSet()
	{
		player.Repeat = Repeat;
		player.AnimationSpeed = AnimationSpeed;
	}

	/// <inheritdoc />
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (invalidateAfterRender)
		{
			invalidateAfterRender = false;
#pragma warning disable CA1416 // The component is browser-only.
			surface?.Invalidate();
#pragma warning restore CA1416
		}

		if (!IsSameSource(Source, currentSource))
		{
			currentSource = Source;
			CancelLoad();
			await LoadAnimationAsync(currentSource);
		}

		if (progressChangedPending)
		{
			try
			{
				await ProgressChanged.InvokeAsync(player.Progress);
			}
			finally
			{
				progressChangedPending = false;
			}
		}

		if (completionPending)
		{
			completionPending = false;
			await AnimationCompleted.InvokeAsync();
		}
	}

	internal void HandleUpdate(TimeSpan delta)
	{
		var wasComplete = player.IsComplete;
		player.Update(delta);
		var completed = !wasComplete && player.IsComplete;

		if (ProgressChanged.HasDelegate)
		{
			progressReportElapsed += delta;
			if (completed || progressReportElapsed >= ProgressReportInterval)
			{
				if (QueueProgressChanged() && !completed)
					_ = RequestRenderAsync();
			}
		}

		if (completed)
		{
			completionPending = true;
			_ = RequestRenderAsync(invalidateSurface: true);
		}
	}

	internal void ReplaceAnimation(Animation? animation)
	{
		var previousAnimation = player.Animation;
		player.Animation = animation;
		QueueProgressChanged();
		if (!ReferenceEquals(previousAnimation, animation))
			previousAnimation?.Dispose();
	}

	private void HandlePaintSurface(SKCanvas canvas, SKSize size)
	{
		canvas.Clear(SKColors.Transparent);
		player.Render(canvas, SKRect.Create(0, 0, size.Width, size.Height));
	}

	private async Task LoadAnimationAsync(SKLottieImageSource? source)
	{
		if (source is null)
		{
			ReplaceAnimation(null);
			await RequestRenderAsync(invalidateSurface: true);
			return;
		}

		var currentCancellation = new CancellationTokenSource();
		loadCancellation = currentCancellation;
		SKLottieAnimation? result = null;
		var cancellationToken = currentCancellation.Token;
		try
		{
			try
			{
				if (!IsCurrentLoad(currentCancellation, source))
					return;

				var httpClient = services?.GetService(typeof(HttpClient)) as HttpClient;
				result = source is SKUriLottieImageSource uriSource
					? await uriSource.LoadUriAnimationAsync(httpClient, cancellationToken)
					: await source.LoadAnimationAsync(cancellationToken);
				if (result is null || !result.IsLoaded)
					throw new InvalidOperationException("The Lottie animation source could not be parsed.");
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				if (!IsCurrentLoad(currentCancellation, source))
					return;

				ReplaceAnimation(null);
				await RequestRenderAsync(invalidateSurface: true);
				if (!IsCurrentLoad(currentCancellation, source))
					return;

				await AnimationFailed.InvokeAsync(ex);
				return;
			}

			if (!IsCurrentLoad(currentCancellation, source))
				return;

			ReplaceAnimation(result.Animation);
			result = null;
			await RequestRenderAsync(invalidateSurface: true);
			if (!IsCurrentLoad(currentCancellation, source))
				return;

			await AnimationLoaded.InvokeAsync();
		}
		finally
		{
			result?.Animation?.Dispose();
			if (ReferenceEquals(loadCancellation, currentCancellation))
				loadCancellation = null;

			currentCancellation.Dispose();
		}
	}

	private void CancelLoad()
	{
		var cancellation = loadCancellation;
		loadCancellation = null;
		cancellation?.Cancel();
	}

	private bool IsCurrentLoad(CancellationTokenSource cancellation, SKLottieImageSource? source) =>
		!disposed &&
		ReferenceEquals(loadCancellation, cancellation) &&
		IsSameSource(Source, source) &&
		!cancellation.IsCancellationRequested;

	private Task RequestRenderAsync(bool invalidateSurface = false)
	{
		if (invalidateSurface)
			invalidateAfterRender = true;

		return surface is not null ? InvokeAsync(StateHasChanged) : Task.CompletedTask;
	}

	private bool QueueProgressChanged()
	{
		progressReportElapsed = TimeSpan.Zero;
		if (disposed || !ProgressChanged.HasDelegate || progressChangedPending)
			return false;

		progressChangedPending = true;
		return true;
	}

	private static bool IsSameSource(SKLottieImageSource? left, SKLottieImageSource? right) =>
		ReferenceEquals(left, right) ||
		(left is not null && left.IsSameSource(right));

	internal SKLottiePlayer Player => player;

	/// <inheritdoc />
	public void Dispose()
	{
		disposed = true;
		CancelLoad();
		ReplaceAnimation(null);
	}
}
