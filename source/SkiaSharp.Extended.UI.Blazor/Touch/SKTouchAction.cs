namespace SkiaSharp.Extended.UI.Blazor;

/// <summary>
/// Specifies the action that caused a touch event.
/// Matches the SkiaSharp.Views.Maui SKTouchAction API for source sharing.
/// </summary>
public enum SKTouchAction
{
    /// <summary>The touch was cancelled.</summary>
    Cancelled = 0,
    /// <summary>The pointer entered the element.</summary>
    Entered = 1,
    /// <summary>The touch started (pointer down).</summary>
    Pressed = 2,
    /// <summary>The touch moved.</summary>
    Moved = 3,
    /// <summary>The touch ended (pointer up).</summary>
    Released = 4,
    /// <summary>The pointer exited the element.</summary>
    Exited = 5,
    /// <summary>The mouse wheel changed.</summary>
    WheelChanged = 6,
}
