using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using System;

namespace SkiaSharpDemo;

/// <summary>
/// A renderer decorator that draws tile borders on top of the inner renderer's output.
/// Used in the demo inspector to visually inspect tile boundaries without modifying
/// the core <see cref="SKDeepZoomRenderer"/>.
/// </summary>
public sealed class DebugBorderRenderer : ISKDeepZoomRenderer
{
    private readonly ISKDeepZoomRenderer _inner;
    private readonly SKDeepZoomRenderer _coreRenderer;
    private readonly SKPaint _borderPaint;

    public DebugBorderRenderer(SKDeepZoomRenderer inner)
    {
        _coreRenderer = inner ?? throw new ArgumentNullException(nameof(inner));
        _inner = inner;
        _borderPaint = new SKPaint
        {
            Color = new SKColor(60, 60, 60, 160),
            IsStroke = true,
            StrokeWidth = 1,
            IsAntialias = false,
        };
    }

    /// <summary>
    /// When <see langword="true"/>, a border is drawn around each visible tile.
    /// When <see langword="false"/>, rendering is identical to the inner renderer.
    /// </summary>
    public bool ShowTileBorders { get; set; }

    /// <summary>
    /// Forwarded to the inner <see cref="SKDeepZoomRenderer.EnableLodBlending"/>.
    /// When <see langword="true"/>, lower-resolution fallback tiles fill the canvas
    /// while high-resolution tiles are loading.
    /// </summary>
    public bool EnableLodBlending
    {
        get => _coreRenderer.EnableLodBlending;
        set => _coreRenderer.EnableLodBlending = value;
    }

    /// <inheritdoc />
    public void Render(
        SKCanvas canvas,
        SKDeepZoomImageSource tileSource,
        SKDeepZoomViewport viewport,
        ISKDeepZoomTileCache cache,
        SKDeepZoomTileLayout layout)
    {
        _inner.Render(canvas, tileSource, viewport, cache, layout);

        if (!ShowTileBorders || tileSource == null) return;

        // Overlay borders on every visible tile using layout geometry
        var visibleTiles = layout.GetVisibleTiles(tileSource, viewport);
        foreach (var req in visibleTiles)
        {
            var dest = layout.GetTileDestRect(tileSource, viewport, req.TileId);
            canvas.DrawRect(new SKRect(dest.X, dest.Y, dest.Right, dest.Bottom), _borderPaint);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _inner.Dispose();
        _borderPaint.Dispose();
    }
}
