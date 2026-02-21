namespace SkiaSharp.Extended.UI.Blazor;

/// <summary>
/// Provides data for the <see cref="SKTouchCanvasView.Touch"/> event.
/// Uses the same API as SkiaSharp.Views.Maui SKTouchEventArgs for source sharing.
/// </summary>
public class SKTouchEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="SKTouchEventArgs"/>.
    /// </summary>
    public SKTouchEventArgs(
        long id,
        SKTouchAction actionType,
        SKTouchDeviceType deviceType,
        SKPoint location,
        bool inContact,
        float pressure = 1f,
        int wheelDelta = 0)
    {
        Id = id;
        ActionType = actionType;
        DeviceType = deviceType;
        Location = location;
        InContact = inContact;
        Pressure = pressure;
        WheelDelta = wheelDelta;
    }

    /// <summary>Gets the unique identifier for this touch point.</summary>
    public long Id { get; }

    /// <summary>Gets the action type that caused this event.</summary>
    public SKTouchAction ActionType { get; }

    /// <summary>Gets the type of input device.</summary>
    public SKTouchDeviceType DeviceType { get; }

    /// <summary>Gets the location of the touch point in canvas coordinates (points, not pixels).</summary>
    public SKPoint Location { get; }

    /// <summary>Gets a value indicating whether the touch point is in contact with the surface.</summary>
    public bool InContact { get; }

    /// <summary>Gets the pressure of the touch (0.0–1.0).</summary>
    public float Pressure { get; }

    /// <summary>Gets the mouse wheel delta (only valid for <see cref="SKTouchAction.WheelChanged"/>).</summary>
    public int WheelDelta { get; }

    /// <summary>
    /// Gets or sets whether this event has been handled.
    /// Set to <c>true</c> to prevent further processing.
    /// </summary>
    public bool Handled { get; set; }
}
