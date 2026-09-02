# Deep Zoom for Blazor

Use `SKDeepZoomController` with a plain `SKCanvasView` to render Deep Zoom images in Blazor WebAssembly. There is no custom component — the page wires the services directly to the canvas.

## Quick Start

```razor
@page "/deepzoom"
@implements IDisposable
@inject HttpClient Http
@using SkiaSharp.Extended.DeepZoom

<SKCanvasView @ref="_canvas"
              OnPaintSurface="OnPaintSurface"
              style="width: 100%; height: 600px; border: 1px solid #ccc;" />

@code {
    private SKCanvasView? _canvas;
    private SKDeepZoomController? _controller;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _controller = new SKDeepZoomController();
        _controller.InvalidateRequired += OnInvalidateRequired;

        var xml        = await Http.GetStringAsync("deepzoom/image.dzi");
        var baseUrl    = new Uri(Http.BaseAddress!, "deepzoom/image_files/").ToString();
        var tileSource = SKDeepZoomImageSource.Parse(xml, baseUrl);

        _controller.Load(tileSource, new SKDeepZoomHttpTileFetcher(new HttpClient()));

        await InvokeAsync(StateHasChanged);
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        if (_controller == null) return;

        _controller.SetControlSize(e.Info.Width, e.Info.Height);
        _controller.Update();
        _controller.Render(e.Surface.Canvas);
    }

    private void OnInvalidateRequired(object? sender, EventArgs e)
        => InvokeAsync(() => _canvas?.Invalidate());

    public void Dispose()
    {
        if (_controller != null)
        {
            _controller.InvalidateRequired -= OnInvalidateRequired;
            _controller.Dispose();
        }
    }
}
```

## Serving Tile Assets

Place `.dzi` and tile folder under `wwwroot`. In the project file, reference them as content:

```xml
<ItemGroup>
    <Content Include="wwwroot\deepzoom\image.dzi" />
    <Content Include="wwwroot\deepzoom\image_files\**" />
</ItemGroup>
```

The `SKDeepZoomHttpTileFetcher` fetches each tile URL via `HttpClient`; tile URLs are constructed automatically from the base URL you supply to `SKDeepZoomImageSource.Parse`.

## Rendering Behaviour

- **Fit and center**: On load the controller calls `FitToView()` so the full image is visible and centered in the canvas. Neither cropping nor distortion occurs.
- **Tile resolution**: `Update()` selects the pyramid level whose tile size best matches the physical pixel dimensions of the canvas. Only visible tiles are requested.
- **Tile blending**: While high-resolution tiles are in-flight, parent-level tiles are upscaled and drawn as placeholders.

## Related

- [Deep Zoom overview](deep-zoom.md) — architecture, services, and API reference
- [Deep Zoom for MAUI](deep-zoom-maui.md) — .NET MAUI integration

