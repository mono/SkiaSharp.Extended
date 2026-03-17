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
    public void DrawTile(SKImagePyramidRectF destRect, ISKImagePyramidTile tile)
    {
        if (Canvas == null) return;
        var image = ((SKImagePyramidImageTile)tile).Image;
        var src = new SKRect(0, 0, image.Width, image.Height);
        var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
        Canvas.DrawImage(image, src, dest, _tilePaint);
    }

    /// <inheritdoc />
    public void DrawFallbackTile(SKImagePyramidRectF destRect, SKImagePyramidRectF sourceRect, ISKImagePyramidTile tile)
    {
        if (Canvas == null) return;
        var image = ((SKImagePyramidImageTile)tile).Image;
        var src  = new SKRect(sourceRect.X, sourceRect.Y, sourceRect.Right, sourceRect.Bottom);
        var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
        Canvas.DrawImage(image, src, dest, _tilePaint);
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
