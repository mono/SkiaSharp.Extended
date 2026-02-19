using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class LayoutTransitionManagerTest
{
    [Fact]
    public void InitialState_NotAnimating()
    {
        var tm = new LayoutTransitionManager();
        Assert.False(tm.IsAnimating);
        Assert.Equal(0.0, tm.Progress);
    }

    [Fact]
    public void BeginTransition_StartsAnimation()
    {
        var tm = new LayoutTransitionManager();
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var @new = new[] { new ItemPosition(item, 100, 100, 50, 50) };

        tm.BeginTransition(old, @new);
        Assert.True(tm.IsAnimating);
        Assert.Equal(0.0, tm.Progress);
    }

    [Fact]
    public void Update_ProgressesOverTime()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 1.0;
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var @new = new[] { new ItemPosition(item, 100, 0, 50, 50) };

        tm.BeginTransition(old, @new);
        tm.Update(0.5); // 50%

        Assert.True(tm.IsAnimating);
        Assert.InRange(tm.Progress, 0.49, 0.51);

        var positions = tm.GetCurrentPositions();
        Assert.Single(positions);
        // Should be somewhere between 0 and 100
        Assert.True(positions[0].X > 0);
        Assert.True(positions[0].X < 100);
    }

    [Fact]
    public void Update_CompletesTransition()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 0.5;
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var @new = new[] { new ItemPosition(item, 200, 100, 80, 80) };

        tm.BeginTransition(old, @new);
        tm.Update(0.6); // Exceeds duration

        Assert.False(tm.IsAnimating);
        Assert.Equal(1.0, tm.Progress);

        var positions = tm.GetCurrentPositions();
        Assert.Equal(200.0, positions[0].X);
        Assert.Equal(100.0, positions[0].Y);
        Assert.Equal(80.0, positions[0].Width);
        Assert.Equal(80.0, positions[0].Height);
    }

    [Fact]
    public void CancelTransition_SnapsToTarget()
    {
        var tm = new LayoutTransitionManager();
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var @new = new[] { new ItemPosition(item, 100, 100, 50, 50) };

        tm.BeginTransition(old, @new);
        tm.Update(0.1);
        tm.CancelTransition();

        Assert.False(tm.IsAnimating);
        var positions = tm.GetCurrentPositions();
        Assert.Equal(100.0, positions[0].X);
    }

    [Fact]
    public void NewItem_FadesInFromCenter()
    {
        var tm = new LayoutTransitionManager();
        var existingItem = new PivotViewerItem("existing");
        var newItem = new PivotViewerItem("new");

        var old = new[] { new ItemPosition(existingItem, 0, 0, 50, 50) };
        var @new = new[]
        {
            new ItemPosition(existingItem, 0, 0, 50, 50),
            new ItemPosition(newItem, 100, 100, 60, 60)
        };

        tm.BeginTransition(old, @new);
        var positions = tm.GetCurrentPositions();
        Assert.Equal(2, positions.Length);
    }

    [Fact]
    public void MultipleItems_AllTransition()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 1.0;
        var items = Enumerable.Range(0, 5).Select(i => new PivotViewerItem($"item-{i}")).ToArray();

        var old = items.Select((item, i) => new ItemPosition(item, i * 100, 0, 80, 80)).ToArray();
        var @new = items.Select((item, i) => new ItemPosition(item, i * 50, 100, 60, 60)).ToArray();

        tm.BeginTransition(old, @new);
        tm.Update(0.5);

        var positions = tm.GetCurrentPositions();
        Assert.Equal(5, positions.Length);

        // All items should be in intermediate positions
        for (int i = 0; i < 5; i++)
        {
            Assert.True(positions[i].Y > 0 && positions[i].Y < 100);
        }
    }

    [Fact]
    public void Duration_Property()
    {
        var tm = new LayoutTransitionManager();
        Assert.Equal(0.5, tm.Duration); // Default
        tm.Duration = 2.0;
        Assert.Equal(2.0, tm.Duration);
    }

    [Fact]
    public void Update_WithoutBegin_DoesNotThrow()
    {
        var tm = new LayoutTransitionManager();
        tm.Update(0.5);
        Assert.False(tm.IsAnimating);
    }

    [Fact]
    public void GetCurrentPositions_WhenNotAnimating_ReturnsEmpty()
    {
        var tm = new LayoutTransitionManager();
        var positions = tm.GetCurrentPositions();
        Assert.Empty(positions);
    }

    [Fact]
    public void BeginTransition_EmptyArrays_DoesNotThrow()
    {
        var tm = new LayoutTransitionManager();
        tm.BeginTransition(Array.Empty<ItemPosition>(), Array.Empty<ItemPosition>());
        Assert.True(tm.IsAnimating);
        tm.Update(1.0);
        Assert.False(tm.IsAnimating);
    }

    [Fact]
    public void CancelTransition_WhenNotAnimating_DoesNotThrow()
    {
        var tm = new LayoutTransitionManager();
        tm.CancelTransition();
        Assert.False(tm.IsAnimating);
    }

    [Fact]
    public void CancelTransition_ClearsAnimationAndSnapsAllPositions()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 2.0;
        var item1 = new PivotViewerItem("a");
        var item2 = new PivotViewerItem("b");
        var old = new[]
        {
            new ItemPosition(item1, 0, 0, 50, 50),
            new ItemPosition(item2, 0, 0, 50, 50)
        };
        var @new = new[]
        {
            new ItemPosition(item1, 200, 300, 80, 80),
            new ItemPosition(item2, 400, 500, 60, 60)
        };

        tm.BeginTransition(old, @new);
        tm.Update(0.1); // Only 5% through
        Assert.True(tm.IsAnimating);

        tm.CancelTransition();
        Assert.False(tm.IsAnimating);
        Assert.Equal(1.0, tm.Progress);

        var positions = tm.GetCurrentPositions();
        Assert.Equal(2, positions.Length);
        // Both items should be at their target positions
        var p1 = positions.First(p => p.Item.Id == "a");
        var p2 = positions.First(p => p.Item.Id == "b");
        Assert.Equal(200.0, p1.X);
        Assert.Equal(300.0, p1.Y);
        Assert.Equal(80.0, p1.Width);
        Assert.Equal(400.0, p2.X);
        Assert.Equal(500.0, p2.Y);
        Assert.Equal(60.0, p2.Width);
    }

    [Fact]
    public void Duration_AffectsAnimationSpeed()
    {
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var @new = new[] { new ItemPosition(item, 100, 0, 50, 50) };

        // Short duration: 0.5s, update 0.5s → should complete
        var tmFast = new LayoutTransitionManager();
        tmFast.Duration = 0.5;
        tmFast.BeginTransition(old, @new);
        tmFast.Update(0.5);
        Assert.False(tmFast.IsAnimating);

        // Long duration: 2.0s, update 0.5s → should still be animating
        var tmSlow = new LayoutTransitionManager();
        tmSlow.Duration = 2.0;
        tmSlow.BeginTransition(old, @new);
        tmSlow.Update(0.5);
        Assert.True(tmSlow.IsAnimating);
        Assert.InRange(tmSlow.Progress, 0.24, 0.26);
    }

    [Fact]
    public void GetCurrentPositions_InterpolatesCorrectlyAt50Percent()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 1.0;
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 100, 100) };
        var @new = new[] { new ItemPosition(item, 200, 200, 200, 200) };

        tm.BeginTransition(old, @new);
        tm.Update(0.5); // 50% progress

        var positions = tm.GetCurrentPositions();
        Assert.Single(positions);

        // At 50% with EaseOutCubic, t = 1 - (1-0.5)^3 = 1 - 0.125 = 0.875
        // So positions should be about 87.5% of the way
        Assert.True(positions[0].X > 100, $"X should be past midpoint due to easing, got {positions[0].X}");
        Assert.True(positions[0].X < 200, $"X should not have reached target, got {positions[0].X}");
        Assert.True(positions[0].Y > 100, $"Y should be past midpoint due to easing, got {positions[0].Y}");
        Assert.True(positions[0].Width > 100, $"Width should be interpolated, got {positions[0].Width}");
        Assert.True(positions[0].Height > 100, $"Height should be interpolated, got {positions[0].Height}");
    }

    [Fact]
    public void MultipleBeginTransition_ResetsAnimation()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 1.0;
        var item = new PivotViewerItem("1");

        // First transition
        var old1 = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var new1 = new[] { new ItemPosition(item, 100, 100, 50, 50) };
        tm.BeginTransition(old1, new1);
        tm.Update(0.8); // 80% done

        // Second transition resets everything
        var old2 = new[] { new ItemPosition(item, 100, 100, 50, 50) };
        var new2 = new[] { new ItemPosition(item, 300, 300, 50, 50) };
        tm.BeginTransition(old2, new2);

        Assert.True(tm.IsAnimating);
        Assert.Equal(0.0, tm.Progress); // Reset to 0

        var positions = tm.GetCurrentPositions();
        Assert.Single(positions);
        // At progress 0, eased t = 0, so should be at source
        Assert.Equal(100.0, positions[0].X);
        Assert.Equal(100.0, positions[0].Y);
    }

    [Fact]
    public void Update_ReturnsTrue_WhileAnimating()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 1.0;
        var item = new PivotViewerItem("1");
        var old = new[] { new ItemPosition(item, 0, 0, 50, 50) };
        var @new = new[] { new ItemPosition(item, 100, 0, 50, 50) };

        tm.BeginTransition(old, @new);
        Assert.True(tm.Update(0.3));
        Assert.True(tm.Update(0.3));
        Assert.False(tm.Update(0.5)); // completes
    }

    [Fact]
    public void NewItem_StartsFromCenterOfTarget()
    {
        var tm = new LayoutTransitionManager();
        tm.Duration = 1.0;
        var newItem = new PivotViewerItem("brand-new");
        var @new = new[] { new ItemPosition(newItem, 100, 200, 60, 40) };

        tm.BeginTransition(Array.Empty<ItemPosition>(), @new);

        // At progress 0 (start), new items should start from center of target with size 0
        var positions = tm.GetCurrentPositions();
        Assert.Single(positions);
        // Source is center: (100+60/2, 200+40/2) = (130, 220) with size (0,0)
        // At t=0, eased = 0, so position = source = (130, 220, 0, 0)
        Assert.Equal(130.0, positions[0].X, 1);
        Assert.Equal(220.0, positions[0].Y, 1);
        Assert.Equal(0.0, positions[0].Width, 1);
        Assert.Equal(0.0, positions[0].Height, 1);
    }
}
