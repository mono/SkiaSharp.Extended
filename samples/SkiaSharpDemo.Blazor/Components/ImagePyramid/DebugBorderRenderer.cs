using SkiaSharp;
using SkiaSharp.Extended;
using System;
using System.Collections.Generic;

namespace SkiaSharpDemo;

/// <summary>
/// A renderer decorator that draws tile borders on top of the inner renderer's output.
/// Used in the demo inspector to visually inspect tile boundaries without modifying
/// the core <see cref="SKImagePyramidRenderer"/>.
/// </summary>
public sealed class DebugBorderRenderer : ISKImagePyramidRenderer
{
    private readonly SKImagePyramidRenderer _inner;
    private readonly SKPaint _borderPaint;
    private readonly List<SKRect> _frameRects = new();

    public DebugBorderRenderer(SKImagePyramidRenderer inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _borderPaint = new SKPaint
        {
            Color = new SKColor(60, 60, 60, 160),
            IsStroke = true,
            StrokeWidth = 1,
            IsAntialias = false,
        };
    }

    /// <summary>
    /// Gets or sets the canvas. Delegates to the inner <see cref="SKImagePyramidRenderer.Canvas"/>.
    /// The caller must set this before each frame, then pass this renderer to
    /// <see cref="SKImagePyramidController.Render(ISKImagePyramidRenderer)"/>.
    /// </summary>
    public SKCanvas? Canvas
    {
        get => _inner.Canvas;
        set => _inner.Canvas = value;
    }

    /// <summary>
    /// When <see langword="true"/>, a border is drawn around each visible tile.
    /// When <see langword="false"/>, rendering is identical to the inner renderer.
    /// </summary>
    public bool ShowTileBorders { get; set; }

    // ---- ISKImagePyramidRenderer ----

    /// <inheritdoc />
    public void BeginRender()
    {
        _frameRects.Clear();
        _inner.BeginRender();
    }

    /// <inheritdoc />
    public void DrawTile(SKRect destRect, SKImage tile)
    {
        _inner.DrawTile(destRect, tile);
        if (ShowTileBorders)
            _frameRects.Add(destRect);
    }

    /// <inheritdoc />
    public void DrawFallbackTile(SKRect destRect, SKRect sourceRect, SKImage tile)
    {
        _inner.DrawFallbackTile(destRect, sourceRect, tile);
        if (ShowTileBorders)
            _frameRects.Add(destRect);
    }

    /// <inheritdoc />
    public void EndRender()
    {
        // Draw borders on top of all tiles drawn this frame
        if (ShowTileBorders && _inner.Canvas != null)
        {
            foreach (var r in _frameRects)
                _inner.Canvas.DrawRect(r, _borderPaint);
        }
        _inner.EndRender();
        _frameRects.Clear();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _inner.Dispose();
        _borderPaint.Dispose();
    }
}
