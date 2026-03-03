using Microsoft.AspNetCore.Components;
using SkiaSharp.Skottie;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A Blazor component that plays Lottie animations, mirroring the MAUI
/// <c>SKLottieView</c> API. Wraps <see cref="SKAnimatedCanvasView"/> with
/// an <see cref="SKLottiePlayer"/> to handle loading, playback, and rendering.
/// </summary>
/// <remarks>
/// <para>
/// Point <see cref="Source"/> at a Lottie JSON URL, configure repeat and speed,
/// and the component handles the rest—loading, frame updates, and rendering.
/// </para>
/// <para>
/// Access read-only state (<see cref="IsLoading"/>, <see cref="Progress"/>, etc.)
/// via an <c>@ref</c> to the component instance.
/// </para>
/// </remarks>
public partial class SKLottieView : ComponentBase, IAsyncDisposable
{
	private readonly SKLottiePlayer _player = new();
	private CancellationTokenSource? _loadCts;
	private Animation? _loadedAnimation;
	private bool _isLoading;
	private string? _currentSource;

	[Inject]
	private HttpClient Http { get; set; } = default!;

	/// <summary>URL of the Lottie JSON file to load.</summary>
	[Parameter]
	public string? Source { get; set; }

	/// <summary>How the animation repeats. Defaults to <see cref="SKLottieRepeatMode.Restart"/>.</summary>
	[Parameter]
	public SKLottieRepeatMode RepeatMode { get; set; } = SKLottieRepeatMode.Restart;

	/// <summary>
	/// Number of additional plays after the first.
	/// Use <c>-1</c> for infinite, <c>0</c> for no repeat. Defaults to <c>-1</c>.
	/// </summary>
	[Parameter]
	public int RepeatCount { get; set; } = 0;

	/// <summary>Playback speed multiplier. Negative values play in reverse. Defaults to <c>1.0</c>.</summary>
	[Parameter]
	public double AnimationSpeed { get; set; } = 1.0;

	/// <summary>Whether the animation loop is running. Defaults to <see langword="true"/>.</summary>
	[Parameter]
	public bool IsAnimationEnabled { get; set; } = true;

	/// <summary>Fires when the animation is successfully loaded.</summary>
	[Parameter]
	public EventCallback AnimationLoaded { get; set; }

	/// <summary>Fires when all repeats complete.</summary>
	[Parameter]
	public EventCallback AnimationCompleted { get; set; }

	/// <summary>Fires when loading fails.</summary>
	[Parameter]
	public EventCallback<Exception?> AnimationFailed { get; set; }

	/// <summary>Fires after each frame update, allowing the host to refresh displayed state.</summary>
	[Parameter]
	public EventCallback AnimationUpdated { get; set; }

	/// <summary>Additional HTML attributes forwarded to the underlying canvas element.</summary>
	[Parameter(CaptureUnmatchedValues = true)]
	public IDictionary<string, object>? AdditionalAttributes { get; set; }

	/// <summary>Gets whether the animation is currently loading.</summary>
	public bool IsLoading => _isLoading;

	/// <summary>Gets whether an animation is loaded and ready to play.</summary>
	public bool HasAnimation => _player.HasAnimation;

	/// <summary>Gets the total animation duration.</summary>
	public TimeSpan Duration => _player.Duration;

	/// <summary>Gets the current playback position.</summary>
	public TimeSpan Progress => _player.Progress;

	/// <summary>Gets whether the animation has completed all repeats.</summary>
	public bool IsComplete => _player.IsComplete;

	/// <summary>Restarts the animation from the beginning with current settings.</summary>
	public void Restart()
	{
		ApplySettings();
		_player.SetAnimation(_loadedAnimation);
	}

	/// <inheritdoc />
	protected override async Task OnParametersSetAsync()
	{
		ApplySettings();

		if (Source != _currentSource)
		{
			_currentSource = Source;
			await LoadAnimationAsync();
		}
	}

	private void ApplySettings()
	{
		_player.Repeat = RepeatCount == 0
			? SKLottieRepeat.Never
			: RepeatMode == SKLottieRepeatMode.Reverse
				? SKLottieRepeat.Reverse(RepeatCount)
				: SKLottieRepeat.Restart(RepeatCount);
		_player.AnimationSpeed = AnimationSpeed;
	}

	private async Task LoadAnimationAsync()
	{
		// Cancel any in-flight load before starting a new one
		_loadCts?.Cancel();
		_loadCts?.Dispose();
		_loadCts = new CancellationTokenSource();
		var ct = _loadCts.Token;

		if (string.IsNullOrEmpty(Source))
		{
			_loadedAnimation?.Dispose();
			_loadedAnimation = null;
			_player.SetAnimation(null);
			return;
		}

		_isLoading = true;
		StateHasChanged();

		Exception? exception = null;
		try
		{
			var json = await Http.GetStringAsync(Source, ct);

			if (ct.IsCancellationRequested)
				return;

			_player.SetAnimation(null);
			_loadedAnimation?.Dispose();
			_loadedAnimation = Animation.Parse(json);
			_player.SetAnimation(_loadedAnimation);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			return;
		}
		catch (Exception ex)
		{
			if (ct.IsCancellationRequested)
				return;

			exception = ex;
			_loadedAnimation?.Dispose();
			_loadedAnimation = null;
			_player.SetAnimation(null);
		}
		finally
		{
			_isLoading = false;
		}

		if (_player.HasAnimation)
			await AnimationLoaded.InvokeAsync();
		else
		{
			exception ??= new InvalidOperationException("The Lottie animation source could not be parsed.");
			await AnimationFailed.InvokeAsync(exception);
		}

		StateHasChanged();
	}

	private async Task HandleUpdate(TimeSpan delta)
	{
		var wasComplete = _player.IsComplete;
		_player.Update(delta);

		if (_player.IsComplete && !wasComplete)
			await AnimationCompleted.InvokeAsync();

		if (AnimationUpdated.HasDelegate)
			await AnimationUpdated.InvokeAsync();
	}

	private void HandlePaintSurface(SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		if (_player.HasAnimation)
			_player.Render(canvas, SKRect.Create(0, 0, e.Info.Width, e.Info.Height));
	}

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		_loadCts?.Cancel();
		_loadCts?.Dispose();
		_player.SetAnimation(null);
		_loadedAnimation?.Dispose();
		_loadedAnimation = null;
		return ValueTask.CompletedTask;
	}
}
