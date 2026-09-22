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
public class SKLottieView : ComponentBase, IAsyncDisposable
{
	private static readonly IReadOnlyDictionary<string, object> EmptyAttributes =
		new Dictionary<string, object>();

	private sealed class LoadRequest : IDisposable
	{
		public LoadRequest(
			long generation,
			SKLottieImageSource? source,
			CancellationToken cancellationToken,
			TaskCompletionSource<bool>? completion,
			bool reportFailure)
		{
			Generation = generation;
			Source = source;
			Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			Completion = completion;
			ReportFailure = reportFailure;
			CancellationRegistration = Cancellation.Token.Register(() => Completion?.TrySetCanceled(Cancellation.Token));
		}

		public CancellationTokenSource Cancellation { get; }

		public CancellationTokenRegistration CancellationRegistration { get; }

		public TaskCompletionSource<bool>? Completion { get; }

		public long Generation { get; }

		public bool ReportFailure { get; }

		public SKLottieImageSource? Source { get; }

		public bool Started { get; set; }

		public void Cancel()
		{
			Cancellation.Cancel();
			Completion?.TrySetCanceled(Cancellation.Token);
		}

		public void Dispose()
		{
			CancellationRegistration.Dispose();
			Cancellation.Dispose();
		}
	}

	private readonly SKLottiePlayer player = new();
	private Animation? loadedAnimation;
	private SKAnimatedSurfaceView? surface;
	private SKLottieImageSource? currentSource;
	private LoadRequest? loadRequest;
	private long nextLoadGeneration;
	private long currentLoadGeneration;
	private bool hasRendered;
	private bool invalidateAfterRender;
	private bool loadPending;
	private bool isLoading;
	private bool completionPending;
	private bool disposed;

	[Inject]
	private IServiceProvider? services { get; set; }

	/// <summary>
	/// Gets or sets the HTTP client used for URI sources. When omitted, the component resolves
	/// a registered default <see cref="HttpClient"/> after interactivity begins.
	/// </summary>
	[Parameter]
	public HttpClient? HttpClient { get; set; }

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

	/// <summary>Fires when loading fails.</summary>
	[Parameter]
	public EventCallback<Exception?> AnimationFailed { get; set; }

	/// <summary>Additional HTML attributes forwarded to the underlying canvas element.</summary>
	[Parameter(CaptureUnmatchedValues = true)]
	public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

	/// <summary>Gets whether the animation is currently loading.</summary>
	public bool IsLoading => isLoading;

	/// <summary>Gets whether an animation is loaded and ready to play.</summary>
	public bool HasAnimation => player.HasAnimation;

	/// <summary>Gets the total animation duration.</summary>
	public TimeSpan Duration => player.Duration;

	/// <summary>Gets the current playback position.</summary>
	public TimeSpan Progress => player.Progress;

	/// <summary>Gets whether the animation has completed all repeats.</summary>
	public bool IsComplete => player.IsComplete;

	internal bool ShouldRenderContinuously =>
		IsAnimationEnabled && player.HasAnimation && !player.IsComplete;

	/// <summary>
	/// Reloads the current source, including an equal source. Before interactivity, the request
	/// is queued and completes after the component's first interactive render.
	/// </summary>
	public Task ReloadAsync(CancellationToken cancellationToken = default)
	{
		if (disposed)
			return Task.FromCanceled(new CancellationToken(canceled: true));

		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);

		var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		QueueLoad(Source, cancellationToken, completion, reportFailure: true);
		RequestRenderAndInvalidate();
		return completion.Task;
	}

	/// <summary>Restarts the animation from the beginning with current settings.</summary>
	public void Restart()
	{
		ApplySettings();
		player.SetAnimation(loadedAnimation);
		RequestRenderAndInvalidate();
	}

	/// <inheritdoc />
	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenComponent<SKAnimatedSurfaceView>(0);
		builder.AddAttribute(
			1,
			nameof(SKAnimatedSurfaceView.AdditionalAttributes),
			AdditionalAttributes ?? EmptyAttributes);
		builder.AddAttribute(2, nameof(SKAnimatedSurfaceView.SurfaceType), SurfaceType);
		builder.AddAttribute(3, nameof(SKAnimatedSurfaceView.IsAnimationEnabled), ShouldRenderContinuously);
		builder.AddAttribute(4, nameof(SKAnimatedSurfaceView.IgnorePixelScaling), IgnorePixelScaling);
		builder.AddAttribute(5, nameof(SKAnimatedSurfaceView.OnUpdate), (Action<TimeSpan>)HandleUpdate);
		builder.AddAttribute(6, nameof(SKAnimatedSurfaceView.OnPaintSurface), (Action<SKCanvas, SKSize>)HandlePaintSurface);
		builder.AddComponentReferenceCapture(7, component => surface = (SKAnimatedSurfaceView)component);
		builder.CloseComponent();
	}

	/// <inheritdoc />
	protected override Task OnParametersSetAsync()
	{
		ApplySettings();
		if (!Equals(Source, currentSource))
		{
			currentSource = Source;
			QueueLoad(Source, CancellationToken.None, completion: null, reportFailure: true);
		}

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		hasRendered = true;
		if (invalidateAfterRender)
		{
			invalidateAfterRender = false;
#pragma warning disable CA1416 // The component is browser-only.
			surface?.Invalidate();
#pragma warning restore CA1416
		}

		if (loadPending)
		{
			var request = loadRequest;
			loadPending = false;
			if (request is not null && IsCurrent(request))
				await LoadAnimationAsync(request);
			else if (request is not null &&
				ReferenceEquals(loadRequest, request) &&
				currentLoadGeneration == request.Generation &&
				!request.Started)
			{
				loadRequest = null;
				currentLoadGeneration = 0;
				request.Dispose();
			}
		}

		if (completionPending)
		{
			completionPending = false;
			await AnimationCompleted.InvokeAsync();
		}
	}

	internal void ApplySettings()
	{
		player.Repeat = Repeat;
		player.AnimationSpeed = AnimationSpeed;
	}

	internal void HandleUpdate(TimeSpan delta)
	{
		var wasComplete = player.IsComplete;
		player.Update(delta);
		if (!wasComplete && player.IsComplete)
		{
			completionPending = true;
			RequestRenderAndInvalidate();
		}
	}

	internal void SetAnimation(Animation? animation)
	{
		player.SetAnimation(null);
		loadedAnimation?.Dispose();
		loadedAnimation = animation;
		player.SetAnimation(animation);
	}

	private void HandlePaintSurface(SKCanvas canvas, SKSize size)
	{
		canvas.Clear(SKColors.Transparent);
		player.Render(canvas, SKRect.Create(0, 0, size.Width, size.Height));
	}

	private void QueueLoad(
		SKLottieImageSource? source,
		CancellationToken cancellationToken,
		TaskCompletionSource<bool>? completion,
		bool reportFailure)
	{
		CancelLoad();
		var request = new LoadRequest(
			++nextLoadGeneration,
			source,
			cancellationToken,
			completion,
			reportFailure);
		loadRequest = request;
		currentLoadGeneration = request.Generation;
		loadPending = true;
	}

	private async Task LoadAnimationAsync(LoadRequest request)
	{
		Animation? animation = null;
		var cancellationToken = request.Cancellation.Token;
		try
		{
			request.Started = true;
			if (!IsCurrent(request))
				return;

			if (request.Source is null)
			{
				SetAnimation(null);
				isLoading = false;
				await RenderAndInvalidateAsync();
				if (!IsCurrent(request))
					return;

				request.Completion?.TrySetResult(true);
				return;
			}

			try
			{
				isLoading = true;
				SetAnimation(null);
				await InvokeAsync(StateHasChanged);
				if (!IsCurrent(request))
					return;

				var httpClient = HttpClient ?? services?.GetService(typeof(HttpClient)) as HttpClient;
				var json = await request.Source.LoadJsonAsync(httpClient, cancellationToken);
				if (!IsCurrent(request))
					return;

				animation = Animation.Parse(json);
				if (animation is null)
					throw new InvalidOperationException("The Lottie animation source could not be parsed.");

				if (!IsCurrent(request))
					return;

				SetAnimation(animation);
				animation = null;
				isLoading = false;
				await RenderAndInvalidateAsync();
				if (!IsCurrent(request))
					return;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				request.Completion?.TrySetCanceled(cancellationToken);
				if (IsLatestGeneration(request))
				{
					isLoading = false;
					await RenderAndInvalidateAsync();
				}
				return;
			}
			catch (Exception ex)
			{
				if (!IsCurrent(request))
					return;

				SetAnimation(null);
				isLoading = false;
				await RenderAndInvalidateAsync();
				if (!IsCurrent(request))
					return;

				request.Completion?.TrySetException(ex);
				if (request.ReportFailure)
					await AnimationFailed.InvokeAsync(ex);
				return;
			}

			try
			{
				await AnimationLoaded.InvokeAsync();
			}
			catch (Exception ex)
			{
				request.Completion?.TrySetException(ex);
				throw;
			}

			if (!IsCurrent(request))
				return;

			request.Completion?.TrySetResult(true);
		}
		finally
		{
			animation?.Dispose();
			if (ReferenceEquals(loadRequest, request) && currentLoadGeneration == request.Generation)
			{
				loadRequest = null;
				currentLoadGeneration = 0;
				isLoading = false;
			}

			request.Dispose();
		}
	}

	private void CancelLoad()
	{
		var request = loadRequest;
		if (request is not null)
		{
			loadRequest = null;
			currentLoadGeneration = 0;
			loadPending = false;
			isLoading = false;
			request.Cancel();
			if (!request.Started)
				request.Dispose();
		}
	}

	private bool IsCurrent(LoadRequest request) =>
		!disposed &&
		IsLatestGeneration(request) &&
		!request.Cancellation.IsCancellationRequested;

	private bool IsLatestGeneration(LoadRequest request) =>
		ReferenceEquals(loadRequest, request) &&
		currentLoadGeneration == request.Generation;

	private Task RenderAndInvalidateAsync()
	{
		invalidateAfterRender = true;
		return hasRendered ? InvokeAsync(StateHasChanged) : Task.CompletedTask;
	}

	private void RequestRenderAndInvalidate()
	{
		invalidateAfterRender = true;
		if (hasRendered)
			_ = InvokeAsync(StateHasChanged);
	}

	internal SKLottiePlayer Player => player;

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		disposed = true;
		CancelLoad();
		SetAnimation(null);
		return ValueTask.CompletedTask;
	}
}
