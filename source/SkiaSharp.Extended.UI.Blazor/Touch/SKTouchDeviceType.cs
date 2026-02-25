namespace SkiaSharp.Extended.UI.Blazor;

/// <summary>
/// Specifies the type of device that caused a touch event.
/// Matches the SkiaSharp.Views.Maui SKTouchDeviceType API for source sharing.
/// </summary>
public enum SKTouchDeviceType
{
    /// <summary>A touch input device (finger).</summary>
    Touch = 0,
    /// <summary>A mouse input device.</summary>
    Mouse = 1,
    /// <summary>A stylus/pen input device.</summary>
    Stylus = 2,
}
