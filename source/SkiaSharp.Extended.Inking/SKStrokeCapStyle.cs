namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Specifies the style of the cap at the start and end of strokes.
/// </summary>
public enum SKStrokeCapStyle
{
    /// <summary>
    /// Round cap - a semicircle is added at each end.
    /// </summary>
    Round,

    /// <summary>
    /// Flat cap - the stroke ends abruptly with a flat edge.
    /// </summary>
    Flat,

    /// <summary>
    /// Tapered cap - the stroke narrows to a point at each end,
    /// giving the appearance of lifting the pen off the paper.
    /// </summary>
    Tapered
}
