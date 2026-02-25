using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor;

namespace SkiaSharp.Extended.UI.Blazor.Controls;

/// <summary>
/// A SkiaSharp canvas view for Blazor that adds touch/pointer event support
/// using the same <see cref="SKTouchEventArgs"/> API as the MAUI SKCanvasView.
/// </summary>
/// <remarks>
/// <para>This view translates browser pointer events (mouse, touch, stylus) into
/// <see cref="SKTouchEventArgs"/> and raises the <see cref="Touch"/> event,
/// matching the MAUI touch API for shared source compatibility.</para>
/// <para>Pointer events are captured using JavaScript interop. The canvas is wrapped
/// in a container div that receives pointer events.</para>
/// </remarks>
public partial class SKTouchCanvasView : ComponentBase, IAsyncDisposable
{
    private SkiaSharp.Views.Blazor.SKCanvasView? _skCanvasView;
    private ElementReference _containerRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<SKTouchCanvasView>? _dotNetRef;
    private bool _touchInitialized;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>Gets or sets the paint surface callback.</summary>
    [Parameter]
    public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

    /// <summary>
    /// Gets or sets the touch event callback.
    /// This event is raised for all pointer interactions (mouse, touch, stylus).
    /// </summary>
    [Parameter]
    public EventCallback<SKTouchEventArgs> Touch { get; set; }

    /// <summary>Gets or sets whether touch events are enabled.</summary>
    [Parameter]
    public bool EnableTouchEvents { get; set; } = true;

    /// <summary>Gets or sets whether continuous rendering is enabled.</summary>
    [Parameter]
    public bool EnableRenderLoop { get; set; }

    /// <summary>Gets or sets whether pixel scaling is ignored.</summary>
    [Parameter]
    public bool IgnorePixelScaling { get; set; }

    /// <summary>Gets or sets the CSS style string for the container.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Gets or sets the CSS class string for the container.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Gets or sets additional HTML attributes passed to the inner canvas.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Gets the current DPI of the display.</summary>
    public float Dpi => _skCanvasView is not null ? (float)_skCanvasView.Dpi : 1f;

    /// <summary>Requests a redraw of the canvas.</summary>
    public void Invalidate() => _skCanvasView?.Invalidate();

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && EnableTouchEvents)
        {
            await InitializeTouchAsync();
        }
    }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (_touchInitialized && !EnableTouchEvents)
        {
            await DisposeTouchAsync();
        }
    }

    private async Task InitializeTouchAsync()
    {
        if (_touchInitialized)
            return;

        try
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SkiaSharp.Extended.UI.Blazor/SKTouchInterop.js");

            _dotNetRef = DotNetObjectReference.Create(this);

            await _jsModule.InvokeVoidAsync("initializeTouchEvents", _containerRef, _dotNetRef);

            _touchInitialized = true;
        }
        catch (Exception ex) when (ex is JSException || ex is InvalidOperationException)
        {
            // JS interop may not be available in pre-rendering scenarios
        }
    }

    private async Task DisposeTouchAsync()
    {
        _touchInitialized = false;
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("disposeTouchEvents", _containerRef);
                await _jsModule.DisposeAsync();
                _jsModule = null;
            }
            catch (JSDisconnectedException) { }
            catch (JSException) { }
        }
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    /// <summary>
    /// Invoked from JavaScript when a pointer event occurs.
    /// </summary>
    [JSInvokable]
    public async Task OnPointerEvent(PointerEventData data)
    {
        if (!EnableTouchEvents || !Touch.HasDelegate)
            return;

        var args = new SKTouchEventArgs(
            id: data.Id,
            actionType: (SKTouchAction)data.Action,
            deviceType: (SKTouchDeviceType)data.DeviceType,
            location: new SKPoint(data.X, data.Y),
            inContact: data.InContact,
            pressure: data.Pressure,
            wheelDelta: data.WheelDelta);

        await Touch.InvokeAsync(args);
    }

    private void OnPaintSurfaceInternal(SKPaintSurfaceEventArgs e)
    {
        OnPaintSurface?.Invoke(e);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeTouchAsync();
        _skCanvasView?.Dispose();
    }

    /// <summary>Data transfer object for pointer events from JavaScript.</summary>
    public sealed class PointerEventData
    {
        /// <summary>Gets or sets the pointer ID.</summary>
        public long Id { get; set; }
        /// <summary>Gets or sets the action type (maps to <see cref="SKTouchAction"/>).</summary>
        public int Action { get; set; }
        /// <summary>Gets or sets the device type (maps to <see cref="SKTouchDeviceType"/>).</summary>
        public int DeviceType { get; set; }
        /// <summary>Gets or sets the X coordinate in CSS pixels.</summary>
        public float X { get; set; }
        /// <summary>Gets or sets the Y coordinate in CSS pixels.</summary>
        public float Y { get; set; }
        /// <summary>Gets or sets the pressure (0.0–1.0).</summary>
        public float Pressure { get; set; }
        /// <summary>Gets or sets whether the pointer is in contact.</summary>
        public bool InContact { get; set; }
        /// <summary>Gets or sets the wheel delta.</summary>
        public int WheelDelta { get; set; }
    }
}
