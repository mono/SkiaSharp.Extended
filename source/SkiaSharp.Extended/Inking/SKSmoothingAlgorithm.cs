namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Specifies the algorithm used for smoothing ink strokes.
/// </summary>
public enum SKSmoothingAlgorithm
{
    /// <summary>
    /// Quadratic Bézier interpolation through midpoints.
    /// Produces smooth approximating curves. Best for stylized/artistic ink.
    /// </summary>
    QuadraticBezier,
    
    /// <summary>
    /// Catmull-Rom spline interpolation.
    /// Passes through all control points, providing more accurate representation
    /// of the original input path. Best for handwriting and signature capture.
    /// </summary>
    CatmullRom
}
