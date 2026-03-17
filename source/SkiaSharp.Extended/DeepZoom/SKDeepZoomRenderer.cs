#nullable enable

namespace SkiaSharp.Extended.DeepZoom;

/// <summary>
/// SkiaSharp implementation of <see cref="ISKDeepZoomRenderer"/>.
/// Draws Deep Zoom tiles onto an <see cref="SKCanvas"/>.
/// </summary>
/// <remarks>
/// Set <see cref="Canvas"/> before calling <see cref="SKDeepZoomController.Render(ISKDeepZoomRenderer)"/>.
/// The canvas reference is only used during the render call and should not be stored
/// beyond the scope of the paint callback.
/// The two-pass LOD blending logic is controlled by
/// <see cref="SKDeepZoomController.EnableLodBlending"/>.
/// </remarks>
public class SKDeepZoomRenderer : ISKDeepZoomRenderer
{
    private readonly SKPaint _tilePaint;

    public SKDeepZoomRenderer()
    {
        _tilePaint = new SKPaint { IsAntialias = true };
    }

    /// <summary>
    /// The canvas to draw onto. Set this before each frame by the caller,
    /// then pass this renderer to <see cref="SKDeepZoomController.Render(ISKDeepZoomRenderer)"/>.
    /// </summary>
    public SKCanvas? Canvas { get; set; }

    // ---- ISKDeepZoomRenderer ----

    /// <inheritdoc />
    public void BeginRender()
    {
        Canvas?.Save();
    }

    /// <inheritdoc />
    public void DrawTile(SKDeepZoomRectF destRect, ISKDeepZoomTile tile)
    {
        if (Canvas == null) return;
        var bitmap = ((SKDeepZoomBitmapTile)tile).Bitmap;
        var src = new SKRect(0, 0, bitmap.Width, bitmap.Height);
        var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
        Canvas.DrawBitmap(bitmap, src, dest, _tilePaint);
    }

    /// <inheritdoc />
    public void DrawFallbackTile(SKDeepZoomRectF destRect, SKDeepZoomRectF sourceRect, ISKDeepZoomTile tile)
    {
        if (Canvas == null) return;
        var bitmap = ((SKDeepZoomBitmapTile)tile).Bitmap;
        var src  = new SKRect(sourceRect.X, sourceRect.Y, sourceRect.Right, sourceRect.Bottom);
        var dest = new SKRect(destRect.X, destRect.Y, destRect.Right, destRect.Bottom);
        Canvas.DrawBitmap(bitmap, src, dest, _tilePaint);
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
