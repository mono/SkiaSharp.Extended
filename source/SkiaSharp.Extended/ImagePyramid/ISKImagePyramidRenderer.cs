#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Renders individual Deep Zoom tiles onto a drawing surface.
/// Implement this interface to provide a custom rendering backend.
/// </summary>
/// <remarks>
/// <para>
/// The rendering pipeline uses a two-pass approach:
/// <list type="number">
///   <item><description>LOD fallback pass: <see cref="DrawFallbackTile"/> is called for tiles that are loading, using a lower-resolution ancestor.</description></item>
///   <item><description>Hi-res pass: <see cref="DrawTile"/> is called for each cached tile at the correct detail level.</description></item>
/// </list>
/// </para>
/// <para>
/// Tile geometry (destination rects, fallback source rects) is pre-calculated by
/// <see cref="SKImagePyramidTileLayout"/> and passed into the draw methods.
/// </para>
/// </remarks>
public interface ISKImagePyramidRenderer : IDisposable
{
    /// <summary>
    /// Called before any tile draw calls for a render frame.
    /// Use this to set up state (e.g., save canvas, clear background).
    /// </summary>
    void BeginRender();

    /// <summary>
    /// Draws a hi-resolution tile at the exact detail level requested.
    /// </summary>
    /// <param name="destRect">Where to draw the tile on screen (in screen pixels).</param>
    /// <param name="tile">The decoded tile image.</param>
    void DrawTile(SKRect destRect, SKImagePyramidTile tile);

    /// <summary>
    /// Draws a lower-resolution fallback tile stretched to fill the destination rect.
    /// Only called when LOD blending is enabled and the hi-res tile is still loading.
    /// </summary>
    /// <param name="destRect">Where to draw the fallback on screen (in screen pixels).</param>
    /// <param name="sourceRect">The sub-region of the fallback tile bitmap to use (in tile pixels).</param>
    /// <param name="tile">The decoded fallback tile.</param>
    void DrawFallbackTile(SKRect destRect, SKRect sourceRect, SKImagePyramidTile tile);

    /// <summary>
    /// Called after all tile draw calls for a render frame.
    /// Use this to flush state (e.g., restore canvas).
    /// </summary>
    void EndRender();
}
