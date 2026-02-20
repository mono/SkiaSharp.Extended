namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Defines how pen velocity affects stroke appearance.
/// </summary>
public enum SKVelocityMode
{
    /// <summary>
    /// Velocity has no effect on stroke appearance.
    /// </summary>
    None,

    /// <summary>
    /// Ballpoint pen mode: faster velocity = thinner stroke.
    /// Simulates ink flow from a ballpoint pen.
    /// </summary>
    BallpointPen,

    /// <summary>
    /// Pencil mode: faster velocity = lighter/thinner stroke.
    /// Simulates graphite deposit from a pencil.
    /// </summary>
    Pencil
}
