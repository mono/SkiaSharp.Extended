using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A Blazor component that drives a frame-update loop and renders to either a
/// software-rendered <see cref="SKCanvasView"/> or a GPU-accelerated <see cref="SKGLView"/>.
/// </summary>
/// <remarks>
/// <para>
/// Set <see cref="UseGL"/> to <see langword="true"/> to switch to GPU-accelerated rendering
/// using <see cref="SKGLView"/> (WebGL). The default is software rendering via <see cref="SKCanvasView"/>.
/// </para>
/// <para>
/// The animation loop runs at approximately 60 fps while <see cref="IsAnimationEnabled"/> is
/// <see langword="true"/>. Setting it to <see langword="false"/> stops the loop; setting it back
/// to <see langword="true"/> restarts it.
/// </para>
/// <para>
/// Subscribe to <see cref="OnPaintSurface"/> to render content, and <see cref="OnUpdate"/> to
/// update animation state each frame. Both callbacks receive the same types regardless of the
/// underlying rendering backend.
/// </para>
/// </remarks>
public partial class SKAnimatedSurfaceView : ComponentBase, IAsyncDisposable
{
    private bool _isAnimationEnabled = true;
    private bool _useGL = false;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private SKCanvasView? _canvasView;
    private SKGLView? _glView;

    /// <summary>
    /// Gets or sets whether the animation loop is running.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool IsAnimationEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use GPU-accelerated rendering via <see cref="SKGLView"/> (WebGL).
    /// When <see langword="false"/> (default), uses software rendering via <see cref="SKCanvasView"/>.
    /// </summary>
    /// <remarks>
    /// Changing this value at runtime causes the underlying view to be swapped. The animation
    /// loop is restarted automatically. This is the Blazor equivalent of the MAUI
    /// <c>PART_DrawingSurface</c> control-template pattern, where the consumer decides the
    /// rendering backend.
    /// </remarks>
    [Parameter]
    public bool UseGL { get; set; } = false;

    /// <summary>
    /// Callback invoked on each frame tick with the elapsed time since the previous frame.
    /// Use this to update animation state.
    /// </summary>
    [Parameter]
    public Action<TimeSpan>? OnUpdate { get; set; }

    /// <summary>
    /// Callback invoked each time the canvas needs to be redrawn. The <see cref="SKSurface"/>
    /// and logical size (in DIPs) are provided regardless of the underlying rendering backend.
    /// </summary>
    [Parameter]
    public Action<SKSurface, SKSize>? OnPaintSurface { get; set; }

    /// <summary>
    /// Additional HTML attributes forwarded to the underlying canvas element
    /// (e.g., <c>style</c>, <c>class</c>).
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        // Restart the loop when UseGL changes so the new view reference is used.
        var useGLChanged = _useGL != UseGL;
        _useGL = UseGL;

        var enabledChanged = _isAnimationEnabled != IsAnimationEnabled;
        _isAnimationEnabled = IsAnimationEnabled;

        if (useGLChanged)
        {
            // Stop the loop; the new view is not yet rendered. OnAfterRenderAsync
            // will restart it once the new view element is in the DOM.
            await StopLoopAsync();
        }
        else if (enabledChanged)
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
        // Start the loop on first render, or after a UseGL toggle swaps the canvas element.
        if (IsAnimationEnabled && _loopTask is null)
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
        _glView?.Invalidate();
#pragma warning restore CA1416
    }

    private void HandleCanvasPaintSurface(SKPaintSurfaceEventArgs e) =>
        OnPaintSurface?.Invoke(e.Surface, e.Info.Size);

    private void HandleGLPaintSurface(SKPaintGLSurfaceEventArgs e) =>
        OnPaintSurface?.Invoke(e.Surface, e.BackendRenderTarget.Size);

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
