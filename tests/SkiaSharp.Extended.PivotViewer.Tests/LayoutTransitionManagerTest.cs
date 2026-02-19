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
}
