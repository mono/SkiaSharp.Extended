#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// SkiaSharp implementation of <see cref="ISKImagePyramidRenderer"/>.
/// Draws Deep Zoom tiles onto an <see cref="SKCanvas"/>.
/// </summary>
/// <remarks>
/// Set <see cref="Canvas"/> before calling <see cref="SKImagePyramidController.Render(ISKImagePyramidRenderer)"/>.
/// The canvas reference is only used during the render call and should not be stored
/// beyond the scope of the paint callback.
/// The two-pass LOD blending logic is controlled by
/// <see cref="SKImagePyramidController.EnableLodBlending"/>.
/// </remarks>
public class SKImagePyramidRenderer : ISKImagePyramidRenderer
{
    private readonly SKPaint _tilePaint;

    public SKImagePyramidRenderer()
    {
        _tilePaint = new SKPaint { IsAntialias = true };
    }

    /// <summary>
    /// The canvas to draw onto. Set this before each frame by the caller,
    /// then pass this renderer to <see cref="SKImagePyramidController.Render(ISKImagePyramidRenderer)"/>.
    /// </summary>
    public SKCanvas? Canvas { get; set; }

    // ---- ISKImagePyramidRenderer ----

    /// <inheritdoc />
    public void BeginRender()
    {
        Canvas?.Save();
    }

    /// <inheritdoc />
    public void DrawTile(SKRect destRect, SKImagePyramidTile tile)
    {
        if (Canvas == null) return;
        var src = SKRect.Create(tile.Image.Width, tile.Image.Height);
        Canvas.DrawImage(tile.Image, src, destRect, _tilePaint);
    }

    /// <inheritdoc />
    public void DrawFallbackTile(SKRect destRect, SKRect sourceRect, SKImagePyramidTile tile)
    {
        if (Canvas == null) return;
        Canvas.DrawImage(tile.Image, sourceRect, destRect, _tilePaint);
    }

    /// <inheritdoc />
    public void EndRender()
    {
        Canvas?.Restore();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _tilePaint.Dispose();
    }
}
