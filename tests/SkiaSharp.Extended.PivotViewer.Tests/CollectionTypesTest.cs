using System.Collections.Specialized;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CollectionTypesTest
{
    // --- BatchObservableCollection ---

    [Fact]
    public void AddRange_AddsAllItems()
    {
        var coll = new BatchObservableCollection<int>();

        coll.AddRange(new[] { 10, 20, 30 });

        Assert.Equal(3, coll.Count);
        Assert.Contains(10, coll);
        Assert.Contains(20, coll);
        Assert.Contains(30, coll);
    }

    [Fact]
    public void AddRange_RaisesSingleCollectionChangedEvent()
    {
        var coll = new BatchObservableCollection<int>();
        int eventCount = 0;
        coll.CollectionChanged += (_, _) => eventCount++;

        coll.AddRange(new[] { 1, 2, 3, 4, 5 });

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void AddRange_EventActionIsReset()
    {
        var coll = new BatchObservableCollection<int>();
        NotifyCollectionChangedAction? action = null;
        coll.CollectionChanged += (_, e) => action = e.Action;

        coll.AddRange(new[] { 1, 2 });

        Assert.Equal(NotifyCollectionChangedAction.Reset, action);
    }

    [Fact]
    public void ReplaceAll_ReplacesExistingItems()
    {
        var coll = new BatchObservableCollection<string>();
        coll.Add("old1");
        coll.Add("old2");

        coll.ReplaceAll(new[] { "new1", "new2", "new3" });

        Assert.Equal(3, coll.Count);
        Assert.DoesNotContain("old1", coll);
        Assert.DoesNotContain("old2", coll);
        Assert.Contains("new1", coll);
        Assert.Contains("new2", coll);
        Assert.Contains("new3", coll);
    }

    [Fact]
    public void ReplaceAll_RaisesSingleCollectionChangedEvent()
    {
        var coll = new BatchObservableCollection<int>();
        coll.Add(1);
        coll.Add(2);

        int eventCount = 0;
        coll.CollectionChanged += (_, _) => eventCount++;

        coll.ReplaceAll(new[] { 10, 20 });

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void RemoveRange_RemovesSpecifiedItems()
    {
        var coll = new BatchObservableCollection<int>();
        coll.AddRange(new[] { 1, 2, 3, 4, 5 });

        // Reset event count after AddRange
        int eventCount = 0;
        coll.CollectionChanged += (_, _) => eventCount++;

        coll.RemoveRange(new[] { 2, 4 });

        Assert.Equal(3, coll.Count);
        Assert.Contains(1, coll);
        Assert.DoesNotContain(2, coll);
        Assert.Contains(3, coll);
        Assert.DoesNotContain(4, coll);
        Assert.Contains(5, coll);
    }

    [Fact]
    public void RemoveRange_RaisesSingleCollectionChangedEvent()
    {
        var coll = new BatchObservableCollection<int>();
        coll.AddRange(new[] { 1, 2, 3 });

        int eventCount = 0;
        coll.CollectionChanged += (_, _) => eventCount++;

        coll.RemoveRange(new[] { 1, 3 });

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void RemoveRange_IgnoresItemsNotInCollection()
    {
        var coll = new BatchObservableCollection<int>();
        coll.AddRange(new[] { 1, 2, 3 });

        coll.RemoveRange(new[] { 99, 100 });

        Assert.Equal(3, coll.Count);
    }

    // --- GradualObservableCollection ---

    [Fact]
    public void EnqueueRange_DoesNotAddItemsImmediately()
    {
        var coll = new GradualObservableCollection<int>();

        coll.EnqueueRange(new[] { 1, 2, 3 });

        Assert.Empty(coll);
        Assert.True(coll.HasPendingItems);
        Assert.Equal(3, coll.PendingCount);
    }

    [Fact]
    public void ProcessCycle_AddsUpToItemsPerCycleItems()
    {
        var coll = new GradualObservableCollection<int> { ItemsPerCycle = 2 };
        coll.EnqueueRange(new[] { 10, 20, 30, 40, 50 });

        bool hasMore = coll.ProcessCycle();

        Assert.True(hasMore);
        Assert.Equal(2, coll.Count);
        Assert.Equal(10, coll[0]);
        Assert.Equal(20, coll[1]);
        Assert.Equal(3, coll.PendingCount);
    }

    [Fact]
    public void ProcessCycle_ReturnsFalseWhenAllItemsProcessed()
    {
        var coll = new GradualObservableCollection<int> { ItemsPerCycle = 10 };
        coll.EnqueueRange(new[] { 1, 2, 3 });

        bool hasMore = coll.ProcessCycle();

        Assert.False(hasMore);
        Assert.Equal(3, coll.Count);
        Assert.False(coll.HasPendingItems);
    }

    [Fact]
    public void ProcessCycle_MultipleCyclesDrainQueue()
    {
        var coll = new GradualObservableCollection<int> { ItemsPerCycle = 2 };
        coll.EnqueueRange(new[] { 1, 2, 3, 4, 5 });

        coll.ProcessCycle(); // adds 1, 2
        coll.ProcessCycle(); // adds 3, 4
        bool hasMore = coll.ProcessCycle(); // adds 5

        Assert.False(hasMore);
        Assert.Equal(5, coll.Count);
        Assert.Equal(0, coll.PendingCount);
    }

    [Fact]
    public void Flush_AddsAllPendingItemsAtOnce()
    {
        var coll = new GradualObservableCollection<int> { ItemsPerCycle = 2 };
        coll.EnqueueRange(new[] { 1, 2, 3, 4, 5 });

        coll.Flush();

        Assert.Equal(5, coll.Count);
        Assert.False(coll.HasPendingItems);
        Assert.Equal(0, coll.PendingCount);
    }

    [Fact]
    public void ItemsPerCycle_DefaultIsTen()
    {
        var coll = new GradualObservableCollection<string>();

        Assert.Equal(10, coll.ItemsPerCycle);
    }

    [Fact]
    public void HasPendingItems_FalseWhenEmpty()
    {
        var coll = new GradualObservableCollection<int>();

        Assert.False(coll.HasPendingItems);
        Assert.Equal(0, coll.PendingCount);
    }
}
