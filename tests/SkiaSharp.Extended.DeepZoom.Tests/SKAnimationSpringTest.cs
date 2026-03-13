using SkiaSharp.Extended;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class SKAnimationSpringTest
{
    [Fact]
    public void Initial_CurrentEqualsTarget()
    {
        var spring = new SKAnimationSpring(5.0);
        Assert.Equal(5.0, spring.Current);
        Assert.Equal(5.0, spring.Target);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void SetTarget_Animates()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 1.0;

        Assert.False(spring.IsSettled);

        // Simulate several frames (5 seconds at 60fps)
        for (int i = 0; i < 600; i++)
            spring.Update(1.0 / 60.0);

        // After 10 seconds at 60fps, should be very close to target
        Assert.Equal(1.0, spring.Current, 2);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void Update_MovesTowardTarget()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 10.0;
        spring.Update(1.0 / 60.0);

        Assert.True(spring.Current > 0.0);
        Assert.True(spring.Current < 10.0);
    }

    [Fact]
    public void SnapToTarget_ImmediateSets()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 5.0;
        spring.SnapToTarget();

        Assert.Equal(5.0, spring.Current);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void Reset_ClearsVelocity()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 10.0;
        spring.Update(1.0 / 60.0);
        Assert.True(spring.Velocity != 0);

        spring.Reset(5.0);
        Assert.Equal(5.0, spring.Current);
        Assert.Equal(5.0, spring.Target);
        Assert.Equal(0.0, spring.Velocity);
    }

    [Fact]
    public void CriticallyDamped_NoOscillation()
    {
        // Critically damped spring should not overshoot
        var spring = new SKAnimationSpring(0.0);
        spring.DampingRatio = 1.0;
        spring.Target = 1.0;

        double maxValue = 0;
        for (int i = 0; i < 600; i++)
        {
            spring.Update(1.0 / 60.0);
            if (spring.Current > maxValue) maxValue = spring.Current;
        }

        // Should not significantly overshoot 1.0
        Assert.True(maxValue < 1.1, $"Overshot to {maxValue}");
    }

    [Fact]
    public void ZeroDeltaTime_DoesNothing()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 1.0;
        spring.Update(0.0);

        Assert.Equal(0.0, spring.Current);
    }

    [Fact]
    public void NegativeDeltaTime_DoesNothing()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 1.0;
        spring.Update(-0.1);

        Assert.Equal(0.0, spring.Current);
    }

    [Fact]
    public void LargeDeltaTime_IsClamped()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 1.0;

        // Very large delta should not cause instability
        spring.Update(10.0);
        Assert.False(double.IsNaN(spring.Current));
        Assert.False(double.IsInfinity(spring.Current));
    }

    [Fact]
    public void SetCurrent_ClearsVelocity()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 10.0;

        // Build up velocity
        for (int i = 0; i < 10; i++)
            spring.Update(1.0 / 60.0);

        Assert.True(spring.Velocity != 0.0);

        // Setting Current should clear velocity
        spring.Current = 5.0;
        Assert.Equal(5.0, spring.Current);
        Assert.Equal(0.0, spring.Velocity);
    }

    [Fact]
    public void TargetEqualsCurrent_IsSettledImmediately()
    {
        var spring = new SKAnimationSpring(5.0);
        // Explicitly set target to same value
        spring.Target = 5.0;
        Assert.True(spring.IsSettled);

        // Update should be a no-op
        spring.Update(1.0 / 60.0);
        Assert.Equal(5.0, spring.Current);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void LargeDeltaTime_ConvergesToTarget()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 100.0;

        // Simulate a very large time step (should converge, not diverge)
        for (int i = 0; i < 100; i++)
            spring.Update(1.0);

        Assert.Equal(100.0, spring.Current, 2);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void MultiplZeroDeltaUpdates_DoNotChangeState()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 10.0;

        for (int i = 0; i < 100; i++)
            spring.Update(0.0);

        Assert.Equal(0.0, spring.Current);
    }

    [Fact]
    public void Stiffness_CanBeChanged()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Stiffness = 50.0;
        Assert.Equal(50.0, spring.Stiffness);

        // Very low stiffness is clamped to 0.1
        spring.Stiffness = 0.0;
        Assert.Equal(0.1, spring.Stiffness);
    }

    [Fact]
    public void DampingRatio_CanBeChanged()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.DampingRatio = 0.5;
        Assert.Equal(0.5, spring.DampingRatio);

        // Negative is clamped to 0.01
        spring.DampingRatio = -1.0;
        Assert.Equal(0.01, spring.DampingRatio);
    }

    [Fact]
    public void SetTargetAfterSettled_RestartsAnimation()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.Target = 1.0;

        // Converge fully
        for (int i = 0; i < 600; i++)
            spring.Update(1.0 / 60.0);
        Assert.True(spring.IsSettled);

        // Set new target — should no longer be settled
        spring.Target = 2.0;
        Assert.False(spring.IsSettled);

        // Converge again
        for (int i = 0; i < 600; i++)
            spring.Update(1.0 / 60.0);
        Assert.Equal(2.0, spring.Current, 2);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void HighStiffness_ConvergesFaster()
    {
        var slowSpring = new SKAnimationSpring(0.0);
        slowSpring.Stiffness = 5.0;
        slowSpring.Target = 1.0;

        var fastSpring = new SKAnimationSpring(0.0);
        fastSpring.Stiffness = 50.0;
        fastSpring.Target = 1.0;

        // After a few frames, the stiffer spring should be closer to target
        for (int i = 0; i < 30; i++)
        {
            slowSpring.Update(1.0 / 60.0);
            fastSpring.Update(1.0 / 60.0);
        }

        Assert.True(fastSpring.Current > slowSpring.Current,
            $"Stiffer spring should converge faster: fast={fastSpring.Current:F4}, slow={slowSpring.Current:F4}");
    }

    [Fact]
    public void DampingRatio_ClampedAt001_NotZero()
    {
        var spring = new SKAnimationSpring(0.0);
        spring.DampingRatio = 0.0;
        Assert.Equal(0.01, spring.DampingRatio);
    }

    [Fact]
    public void Update_ManyCycles_SnapsToTarget()
    {
        // Run enough iterations that the spring should settle and trigger the snap path
        var spring = new SKAnimationSpring(5.0);
        spring.Target = 10.0;

        // Simulate 10 seconds at 60fps — should be well past convergence
        for (int i = 0; i < 600; i++)
            spring.Update(1.0 / 60.0);

        Assert.Equal(10.0, spring.Current);
        Assert.True(spring.IsSettled);
    }
}
