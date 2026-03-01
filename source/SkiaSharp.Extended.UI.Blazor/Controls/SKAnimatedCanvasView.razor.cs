using Microsoft.AspNetCore.Components;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A Blazor component that wraps <see cref="SKCanvasView"/> and drives a
/// frame-update loop using a <see cref="PeriodicTimer"/>, mirroring the
/// MAUI <c>SKAnimatedSurfaceView</c> pattern for Blazor applications.
/// </summary>
/// <remarks>
/// <para>
/// Subclass this component and override <see cref="UpdateAsync"/> to update
/// your animation state on each frame, and subscribe to <see cref="OnPaintSurface"/>
/// to render your content.
/// </para>
/// <para>
/// The animation loop runs at approximately 60 fps while
/// <see cref="IsAnimationEnabled"/> is <see langword="true"/>. Setting it to
/// <see langword="false"/> stops the loop; setting it back to
/// <see langword="true"/> restarts it.
/// </para>
/// </remarks>
public partial class SKAnimatedCanvasView : ComponentBase, IAsyncDisposable
{
    private bool _isAnimationEnabled;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    /// <summary>
    /// Gets or sets whether the animation loop is running.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool IsAnimationEnabled { get; set; } = true;

    /// <summary>
    /// Callback invoked each time the canvas needs to be redrawn.
    /// Subscribe here to render your content onto the <see cref="SKCanvas"/>.
    /// </summary>
    [Parameter]
    public EventCallback<SKPaintSurfaceEventArgs> OnPaintSurface { get; set; }

    /// <summary>
    /// Additional HTML attributes to be applied to the underlying
    /// <see cref="SKCanvasView"/> element (e.g., <c>style</c>, <c>class</c>).
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && IsAnimationEnabled)
            StartLoop();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (_isAnimationEnabled == IsAnimationEnabled)
            return;

        _isAnimationEnabled = IsAnimationEnabled;
        if (_isAnimationEnabled)
            StartLoop();
        else
            StopLoop();
    }

    /// <summary>
    /// Called once per frame before the canvas is invalidated. Override this
    /// method in a subclass to update animation state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the previous frame.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous update work.</returns>
    protected virtual Task UpdateAsync(TimeSpan deltaTime) => Task.CompletedTask;

    private void HandlePaintSurface(SKPaintSurfaceEventArgs e)
    {
        if (OnPaintSurface.HasDelegate)
            _ = OnPaintSurface.InvokeAsync(e);
    }

    private void StartLoop()
    {
        StopLoop();
        _cts = new CancellationTokenSource();
        _loopTask = RunLoopAsync(_cts.Token);
    }

    private void StopLoop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
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

                await UpdateAsync(delta);
                await InvokeAsync(StateHasChanged);
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
        StopLoop();
        if (_loopTask is not null)
        {
            try { await _loopTask; }
            catch (OperationCanceledException) { }
        }
    }
}
