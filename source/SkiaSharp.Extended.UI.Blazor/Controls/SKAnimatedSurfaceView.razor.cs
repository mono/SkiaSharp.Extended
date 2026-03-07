using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A Blazor component that drives a frame-update loop and renders onto a
/// software-rendered <see cref="SKCanvasView"/>.
/// </summary>
/// <remarks>
/// <para>
/// The animation loop runs at approximately 60 fps while <see cref="IsAnimationEnabled"/> is
/// <see langword="true"/>. Setting it to <see langword="false"/> stops the loop; setting it back
/// to <see langword="true"/> restarts it.
/// </para>
/// <para>
/// Subscribe to <see cref="OnPaintSurface"/> to render content, and <see cref="OnUpdate"/> to
/// update animation state each frame. Subclasses can override <see cref="UpdateAsync"/> instead.
/// </para>
/// </remarks>
public partial class SKAnimatedSurfaceView : ComponentBase, IAsyncDisposable
{
    private bool _isAnimationEnabled = true;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private SKCanvasView? _canvasView;

    /// <summary>
    /// Gets or sets whether the animation loop is running.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool IsAnimationEnabled { get; set; } = true;

    /// <summary>
    /// Callback invoked on each frame tick with the elapsed time since the previous frame.
    /// Use this to update animation state.
    /// </summary>
    [Parameter]
    public Action<TimeSpan>? OnUpdate { get; set; }

    /// <summary>
    /// Callback invoked each time the canvas needs to be redrawn.
    /// Subscribe here to render content onto the <see cref="SKCanvas"/>.
    /// </summary>
    [Parameter]
    public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

    /// <summary>
    /// Additional HTML attributes forwarded to the underlying canvas element
    /// (e.g., <c>style</c>, <c>class</c>).
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        var enabledChanged = _isAnimationEnabled != IsAnimationEnabled;
        _isAnimationEnabled = IsAnimationEnabled;

        if (enabledChanged)
        {
            if (_isAnimationEnabled)
                await StartLoopAsync();
            else
                await StopLoopAsync();
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && IsAnimationEnabled)
            await StartLoopAsync();
    }

    /// <summary>
    /// Called once per frame before the canvas is invalidated. Override this in a subclass
    /// to update animation state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the previous frame.</param>
    protected virtual Task UpdateAsync(TimeSpan deltaTime)
    {
        OnUpdate?.Invoke(deltaTime);
        return Task.CompletedTask;
    }

    /// <summary>Forces the underlying canvas to repaint on the next frame.</summary>
    public void Invalidate()
    {
#pragma warning disable CA1416 // Blazor canvas views are browser-only
        _canvasView?.Invalidate();
#pragma warning restore CA1416
    }

    private void HandlePaintSurface(SKPaintSurfaceEventArgs e) =>
        OnPaintSurface?.Invoke(e);

    private async Task StartLoopAsync()
    {
        await StopLoopAsync();
        _cts = new CancellationTokenSource();
        _loopTask = RunLoopAsync(_cts.Token);
    }

    private async Task StopLoopAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        var loopTask = _loopTask;
        _loopTask = null;
        if (loopTask is not null)
        {
            try { await loopTask; }
            catch (Exception) { }
        }
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000.0 / 60));
        var lastTick = DateTime.UtcNow;

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                var now = DateTime.UtcNow;
                var delta = now - lastTick;
                lastTick = now;

                await InvokeAsync(async () =>
                {
                    await UpdateAsync(delta);
                    Invalidate();
                    StateHasChanged();
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — ignore.
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopLoopAsync();
    }
}
