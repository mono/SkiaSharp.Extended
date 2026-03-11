using SkiaSharp.Extended;
using SkiaSharp.Extended.DeepZoom;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class SpringAnimatorTest
{
    [Fact]
    public void Initial_CurrentEqualsTarget()
    {
        var spring = new SpringAnimator(5.0);
        Assert.Equal(5.0, spring.Current);
        Assert.Equal(5.0, spring.Target);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void SetTarget_Animates()
    {
        var spring = new SpringAnimator(0.0);
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
        var spring = new SpringAnimator(0.0);
        spring.Target = 10.0;
        spring.Update(1.0 / 60.0);

        Assert.True(spring.Current > 0.0);
        Assert.True(spring.Current < 10.0);
    }

    [Fact]
    public void SnapToTarget_ImmediateSets()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = 5.0;
        spring.SnapToTarget();

        Assert.Equal(5.0, spring.Current);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void Reset_ClearsVelocity()
    {
        var spring = new SpringAnimator(0.0);
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
        var spring = new SpringAnimator(0.0);
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
        var spring = new SpringAnimator(0.0);
        spring.Target = 1.0;
        spring.Update(0.0);

        Assert.Equal(0.0, spring.Current);
    }

    [Fact]
    public void NegativeDeltaTime_DoesNothing()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = 1.0;
        spring.Update(-0.1);

        Assert.Equal(0.0, spring.Current);
    }

    [Fact]
    public void LargeDeltaTime_IsClamped()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = 1.0;

        // Very large delta should not cause instability
        spring.Update(10.0);
        Assert.False(double.IsNaN(spring.Current));
        Assert.False(double.IsInfinity(spring.Current));
    }

    [Fact]
    public void SetCurrent_ClearsVelocity()
    {
        var spring = new SpringAnimator(0.0);
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
    public void ViewportSpring_AllSettled_IsSettled()
    {
        var vpSpring = new ViewportSpring();
        Assert.True(vpSpring.IsSettled);
    }

    [Fact]
    public void ViewportSpring_SetTarget_Animates()
    {
        var vpSpring = new ViewportSpring();
        vpSpring.SetTarget(0.5, 0.3, 0.5);

        Assert.False(vpSpring.IsSettled);

        for (int i = 0; i < 300; i++)
            vpSpring.Update(1.0 / 60.0);

        var state = vpSpring.GetCurrentState();
        Assert.Equal(0.5, state.OriginX, 2);
        Assert.Equal(0.3, state.OriginY, 2);
        Assert.Equal(0.5, state.Width, 2);
    }

    [Fact]
    public void ViewportSpring_SnapToTarget_ImmediateSets()
    {
        var vpSpring = new ViewportSpring();
        vpSpring.SetTarget(0.5, 0.3, 0.5);
        vpSpring.SnapToTarget();

        var state = vpSpring.GetCurrentState();
        Assert.Equal(0.5, state.OriginX);
        Assert.Equal(0.3, state.OriginY);
        Assert.Equal(0.5, state.Width);
        Assert.True(vpSpring.IsSettled);
    }

    [Fact]
    public void ViewportSpring_Reset_ClearsAll()
    {
        var vpSpring = new ViewportSpring();
        vpSpring.SetTarget(0.5, 0.3, 0.5);
        vpSpring.Update(1.0 / 60.0);

        vpSpring.Reset(0.0, 0.0, 1.0);
        Assert.True(vpSpring.IsSettled);
        var state = vpSpring.GetCurrentState();
        Assert.Equal(0.0, state.OriginX);
        Assert.Equal(0.0, state.OriginY);
        Assert.Equal(1.0, state.Width);
    }

    [Fact]
    public void TargetEqualsCurrent_IsSettledImmediately()
    {
        var spring = new SpringAnimator(5.0);
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
        var spring = new SpringAnimator(0.0);
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
        var spring = new SpringAnimator(0.0);
        spring.Target = 10.0;

        for (int i = 0; i < 100; i++)
            spring.Update(0.0);

        Assert.Equal(0.0, spring.Current);
    }

    [Fact]
    public void Stiffness_CanBeChanged()
    {
        var spring = new SpringAnimator(0.0);
        spring.Stiffness = 50.0;
        Assert.Equal(50.0, spring.Stiffness);

        // Very low stiffness is clamped to 0.1
        spring.Stiffness = 0.0;
        Assert.Equal(0.1, spring.Stiffness);
    }

    [Fact]
    public void DampingRatio_CanBeChanged()
    {
        var spring = new SpringAnimator(0.0);
        spring.DampingRatio = 0.5;
        Assert.Equal(0.5, spring.DampingRatio);

        // Negative is clamped to 0.01
        spring.DampingRatio = -1.0;
        Assert.Equal(0.01, spring.DampingRatio);
    }

    [Fact]
    public void SetTargetAfterSettled_RestartsAnimation()
    {
        var spring = new SpringAnimator(0.0);
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
        var slowSpring = new SpringAnimator(0.0);
        slowSpring.Stiffness = 5.0;
        slowSpring.Target = 1.0;

        var fastSpring = new SpringAnimator(0.0);
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
    public void ViewportSpring_Update_ZeroDelta_DoesNotChange()
    {
        var vpSpring = new ViewportSpring();
        vpSpring.SetTarget(0.5, 0.3, 0.5);

        vpSpring.Update(0.0);

        var state = vpSpring.GetCurrentState();
        Assert.Equal(0.0, state.OriginX);
        Assert.Equal(0.0, state.OriginY);
        Assert.Equal(1.0, state.Width);
    }

    [Fact]
    public void DampingRatio_ClampedAt001_NotZero()
    {
        var spring = new SpringAnimator(0.0);
        spring.DampingRatio = 0.0;
        Assert.Equal(0.01, spring.DampingRatio);
    }

    [Fact]
    public void ViewportSpring_Stiffness_PropagatesToAllSprings()
    {
        var vpSpring = new ViewportSpring();
        vpSpring.Stiffness = 42.0;

        Assert.Equal(42.0, vpSpring.OriginX.Stiffness);
        Assert.Equal(42.0, vpSpring.OriginY.Stiffness);
        Assert.Equal(42.0, vpSpring.Width.Stiffness);
    }

    [Fact]
    public void ViewportSpring_DampingRatio_PropagatesToAllSprings()
    {
        var vpSpring = new ViewportSpring();
        vpSpring.DampingRatio = 0.7;

        Assert.Equal(0.7, vpSpring.OriginX.DampingRatio);
        Assert.Equal(0.7, vpSpring.OriginY.DampingRatio);
        Assert.Equal(0.7, vpSpring.Width.DampingRatio);
    }

    [Fact]
    public void ViewportSpring_Stiffness_RoundTrips()
    {
        // SpringStiffness/DampingRatio are on ViewportSpring (view layer), not SKDeepZoomController
        var spring = new ViewportSpring();
        spring.Stiffness = 55.0;
        Assert.Equal(55.0, spring.Stiffness);
    }

    [Fact]
    public void ViewportSpring_DampingRatio_RoundTrips()
    {
        var spring = new ViewportSpring();
        spring.DampingRatio = 0.8;
        Assert.Equal(0.8, spring.DampingRatio);
    }

    [Fact]
    public void Update_ManyCycles_SnapsToTarget()
    {
        // Run enough iterations that the spring should settle and trigger the snap path
        var spring = new SpringAnimator(5.0);
        spring.Target = 10.0;

        // Simulate 10 seconds at 60fps — should be well past convergence
        for (int i = 0; i < 600; i++)
            spring.Update(1.0 / 60.0);

        Assert.Equal(10.0, spring.Current);
        Assert.True(spring.IsSettled);
    }
}
