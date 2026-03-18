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
    public void DrawTile(Rect<float> destRect, ISKImagePyramidTile tile)
    {
        if (Canvas == null) return;
        // SKImagePyramidRenderer requires SKImagePyramidImageTile; skip gracefully for custom decoders
        if (tile is not SKImagePyramidImageTile imageTile) return;
        var src = new SKRect(0, 0, imageTile.Image.Width, imageTile.Image.Height);
        var dest = new SKRect(destRect.X, destRect.Y, destRect.X + destRect.Width, destRect.Y + destRect.Height);
        Canvas.DrawImage(imageTile.Image, src, dest, _tilePaint);
    }

    /// <inheritdoc />
    public void DrawFallbackTile(Rect<float> destRect, Rect<float> sourceRect, ISKImagePyramidTile tile)
    {
        if (Canvas == null) return;
        // SKImagePyramidRenderer requires SKImagePyramidImageTile; skip gracefully for custom decoders
        if (tile is not SKImagePyramidImageTile imageTile) return;
        var src  = new SKRect(sourceRect.X, sourceRect.Y, sourceRect.X + sourceRect.Width, sourceRect.Y + sourceRect.Height);
        var dest = new SKRect(destRect.X, destRect.Y, destRect.X + destRect.Width, destRect.Y + destRect.Height);
        Canvas.DrawImage(imageTile.Image, src, dest, _tilePaint);
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
