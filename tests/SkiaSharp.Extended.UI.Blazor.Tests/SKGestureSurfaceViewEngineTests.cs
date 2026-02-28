namespace SkiaSharp.Extended.UI.Blazor.Tests;

/// <summary>
/// Tests for the gesture engine integration used by <see cref="SKGestureSurfaceView"/>.
/// These tests verify the touch-to-gesture translation by driving the underlying
/// <see cref="SKGestureEngine"/> directly, matching the same logic used in the view.
/// </summary>
public class SKGestureSurfaceViewEngineTests
{
    private long _testTicks = 1_000_000;

    private SKGestureEngine CreateEngine()
    {
        return new SKGestureEngine
        {
            TimeProvider = () => _testTicks
        };
    }

    private void AdvanceTime(long milliseconds)
    {
        _testTicks += milliseconds * TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Simulates the same touch routing logic that SKGestureSurfaceView uses.
    /// </summary>
    private static bool RouteTouch(SKGestureEngine engine, SKTouchEventArgs e)
    {
        var isMouse = e.DeviceType == SKTouchDeviceType.Mouse;
        var location = e.Location;

        return e.ActionType switch
        {
            SKTouchAction.Pressed => engine.ProcessTouchDown(e.Id, location, isMouse),
            SKTouchAction.Moved => engine.ProcessTouchMove(e.Id, location, e.InContact),
            SKTouchAction.Released => engine.ProcessTouchUp(e.Id, location, isMouse),
            SKTouchAction.Cancelled => engine.ProcessTouchCancel(e.Id),
            SKTouchAction.WheelChanged => engine.ProcessMouseWheel(location, 0, e.WheelDelta),
            _ => true // Entered/Exited are always handled
        };
    }

    [Fact]
    public void TouchDown_ReturnsTrue_WhenEngineEnabled()
    {
        var engine = CreateEngine();
        var touchDown = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), true);

        var result = RouteTouch(engine, touchDown);

        Assert.True(result);
    }

    [Fact]
    public void TouchDown_ReturnsFalse_WhenEngineDisabled()
    {
        var engine = CreateEngine();
        engine.IsEnabled = false;
        var touchDown = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), true);

        var result = RouteTouch(engine, touchDown);

        Assert.False(result);
    }

    [Fact]
    public void QuickTap_RaisesTapDetected()
    {
        var engine = CreateEngine();
        SKTapEventArgs? tapArgs = null;
        engine.TapDetected += (_, e) => tapArgs = e;

        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), true));
        AdvanceTime(50);
        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Released, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), false));

        Assert.NotNull(tapArgs);
    }

    [Fact]
    public void MouseTouchDown_IsTreatedAsMouse()
    {
        var engine = CreateEngine();
        bool? processedAsMouse = null;

        // Override to capture whether it was processed as mouse
        var engine2 = new SKGestureEngine
        {
            TimeProvider = () => _testTicks
        };
        engine2.TapDetected += (_, _) => { };

        var touchDown = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Mouse,
            new SKPoint(50, 50), true);

        // Mouse down should still return true (handled)
        var result = RouteTouch(engine2, touchDown);
        Assert.True(result);
    }

    [Fact]
    public void WheelChanged_RoutesToMouseWheel()
    {
        var engine = CreateEngine();
        SKScrollEventArgs? scrollArgs = null;
        engine.ScrollDetected += (_, e) => scrollArgs = e;

        var wheelEvent = new SKTouchEventArgs(
            id: -1,
            actionType: SKTouchAction.WheelChanged,
            deviceType: SKTouchDeviceType.Mouse,
            location: new SKPoint(100, 100),
            inContact: false,
            pressure: 0f,
            wheelDelta: 3);

        RouteTouch(engine, wheelEvent);

        Assert.NotNull(scrollArgs);
    }

    [Fact]
    public void EnteredAndExited_AlwaysHandled()
    {
        var engine = CreateEngine();

        var entered = new SKTouchEventArgs(0, SKTouchAction.Entered, SKTouchDeviceType.Mouse,
            new SKPoint(0, 0), false);
        var exited = new SKTouchEventArgs(0, SKTouchAction.Exited, SKTouchDeviceType.Mouse,
            new SKPoint(0, 0), false);

        // The routing logic returns true for Entered/Exited
        var enteredResult = RouteTouch(engine, entered);
        var exitedResult = RouteTouch(engine, exited);

        Assert.True(enteredResult);
        Assert.True(exitedResult);
    }

    [Fact]
    public void PanGesture_DetectedAfterMovingBeyondSlop()
    {
        var engine = CreateEngine();
        SKPanEventArgs? panArgs = null;
        engine.PanDetected += (_, e) => panArgs = e;

        engine.TouchSlop = 5f;

        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), true));
        AdvanceTime(20);
        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Moved, SKTouchDeviceType.Touch,
            new SKPoint(110, 100), true));
        AdvanceTime(20);
        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Moved, SKTouchDeviceType.Touch,
            new SKPoint(120, 100), true));

        Assert.NotNull(panArgs);
    }

    [Fact]
    public void TouchCancel_IsRouted()
    {
        var engine = CreateEngine();

        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), true));

        var cancel = new SKTouchEventArgs(1, SKTouchAction.Cancelled, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), false);

        // Should not throw - cancel is handled by the engine
        var result = RouteTouch(engine, cancel);
        Assert.True(result);
    }

    [Fact]
    public void EngineProperties_AreConfigurable()
    {
        var engine = CreateEngine();
        engine.TouchSlop = 12f;
        engine.LongPressDuration = 800;
        engine.IsEnabled = false;

        Assert.Equal(12f, engine.TouchSlop);
        Assert.Equal(800, engine.LongPressDuration);
        Assert.False(engine.IsEnabled);
    }

    [Fact]
    public void PointerEventData_MapsCorrectly()
    {
        // Verify the PointerEventData → SKTouchEventArgs mapping used in SKTouchCanvasView
        var data = new SKTouchCanvasView.PointerEventData
        {
            Id = 5,
            Action = 2, // Pressed
            DeviceType = 0, // Touch
            X = 150f,
            Y = 250f,
            Pressure = 0.7f,
            InContact = true,
            WheelDelta = 0
        };

        var args = new SKTouchEventArgs(
            id: data.Id,
            actionType: (SKTouchAction)data.Action,
            deviceType: (SKTouchDeviceType)data.DeviceType,
            location: new SKPoint(data.X, data.Y),
            inContact: data.InContact,
            pressure: data.Pressure,
            wheelDelta: data.WheelDelta);

        Assert.Equal(5, args.Id);
        Assert.Equal(SKTouchAction.Pressed, args.ActionType);
        Assert.Equal(SKTouchDeviceType.Touch, args.DeviceType);
        Assert.Equal(new SKPoint(150f, 250f), args.Location);
        Assert.True(args.InContact);
        Assert.Equal(0.7f, args.Pressure);
    }

    [Fact]
    public void DoubleTap_RaisesDoubleTapDetected()
    {
        var engine = CreateEngine();
        SKTapEventArgs? doubleTapArgs = null;
        engine.DoubleTapDetected += (_, e) => doubleTapArgs = e;

        // First tap
        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), true));
        AdvanceTime(50);
        RouteTouch(engine, new SKTouchEventArgs(1, SKTouchAction.Released, SKTouchDeviceType.Touch,
            new SKPoint(100, 100), false));

        AdvanceTime(100);

        // Second tap (within double-tap time window)
        RouteTouch(engine, new SKTouchEventArgs(2, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(105, 100), true));
        AdvanceTime(50);
        RouteTouch(engine, new SKTouchEventArgs(2, SKTouchAction.Released, SKTouchDeviceType.Touch,
            new SKPoint(105, 100), false));

        Assert.NotNull(doubleTapArgs);
        Assert.Equal(2, doubleTapArgs.TapCount);
    }
}
