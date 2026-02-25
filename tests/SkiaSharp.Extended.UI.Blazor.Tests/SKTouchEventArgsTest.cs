namespace SkiaSharp.Extended.UI.Blazor.Tests;

public class SKTouchEventArgsTest
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var location = new SKPoint(100, 200);

        var args = new SKTouchEventArgs(
            id: 42,
            actionType: SKTouchAction.Pressed,
            deviceType: SKTouchDeviceType.Touch,
            location: location,
            inContact: true,
            pressure: 0.8f,
            wheelDelta: 0);

        Assert.Equal(42, args.Id);
        Assert.Equal(SKTouchAction.Pressed, args.ActionType);
        Assert.Equal(SKTouchDeviceType.Touch, args.DeviceType);
        Assert.Equal(location, args.Location);
        Assert.True(args.InContact);
        Assert.Equal(0.8f, args.Pressure);
        Assert.Equal(0, args.WheelDelta);
        Assert.False(args.Handled);
    }

    [Fact]
    public void Handled_CanBeSetToTrue()
    {
        var args = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Mouse,
            new SKPoint(0, 0), false);

        args.Handled = true;

        Assert.True(args.Handled);
    }

    [Fact]
    public void WheelDelta_IsSetForWheelChangedAction()
    {
        var args = new SKTouchEventArgs(
            id: -1,
            actionType: SKTouchAction.WheelChanged,
            deviceType: SKTouchDeviceType.Mouse,
            location: new SKPoint(50, 50),
            inContact: false,
            pressure: 0f,
            wheelDelta: 3);

        Assert.Equal(SKTouchAction.WheelChanged, args.ActionType);
        Assert.Equal(3, args.WheelDelta);
    }

    [Fact]
    public void DefaultPressure_IsOne()
    {
        var args = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            SKPoint.Empty, true);

        Assert.Equal(1f, args.Pressure);
    }

    [Fact]
    public void SKTouchAction_HasCorrectValues()
    {
        // These values must match MAUI's SKTouchAction enum for source sharing
        Assert.Equal(0, (int)SKTouchAction.Cancelled);
        Assert.Equal(1, (int)SKTouchAction.Entered);
        Assert.Equal(2, (int)SKTouchAction.Pressed);
        Assert.Equal(3, (int)SKTouchAction.Moved);
        Assert.Equal(4, (int)SKTouchAction.Released);
        Assert.Equal(5, (int)SKTouchAction.Exited);
        Assert.Equal(6, (int)SKTouchAction.WheelChanged);
    }

    [Fact]
    public void SKTouchDeviceType_HasCorrectValues()
    {
        // These values must match MAUI's SKTouchDeviceType enum for source sharing
        Assert.Equal(0, (int)SKTouchDeviceType.Touch);
        Assert.Equal(1, (int)SKTouchDeviceType.Mouse);
        Assert.Equal(2, (int)SKTouchDeviceType.Stylus);
    }

    [Theory]
    [InlineData(SKTouchAction.Pressed, SKTouchDeviceType.Touch)]
    [InlineData(SKTouchAction.Moved, SKTouchDeviceType.Mouse)]
    [InlineData(SKTouchAction.Released, SKTouchDeviceType.Stylus)]
    [InlineData(SKTouchAction.Cancelled, SKTouchDeviceType.Touch)]
    [InlineData(SKTouchAction.Entered, SKTouchDeviceType.Mouse)]
    [InlineData(SKTouchAction.Exited, SKTouchDeviceType.Mouse)]
    [InlineData(SKTouchAction.WheelChanged, SKTouchDeviceType.Mouse)]
    public void Constructor_AcceptsAllActionAndDeviceTypeCombinations(SKTouchAction action, SKTouchDeviceType device)
    {
        var args = new SKTouchEventArgs(1, action, device, new SKPoint(10, 20), true);

        Assert.Equal(action, args.ActionType);
        Assert.Equal(device, args.DeviceType);
    }

    [Fact]
    public void DefaultWheelDelta_IsZero()
    {
        var args = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            new SKPoint(0, 0), true);

        Assert.Equal(0, args.WheelDelta);
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new SKTouchEventArgs(1, SKTouchAction.Pressed, SKTouchDeviceType.Touch,
            SKPoint.Empty, true);

        Assert.IsAssignableFrom<EventArgs>(args);
    }
}
