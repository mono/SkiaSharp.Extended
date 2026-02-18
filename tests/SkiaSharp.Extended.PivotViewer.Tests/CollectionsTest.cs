using SkiaSharp.Extended.PivotViewer;
using System.Collections.Specialized;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CollectionsTest
{
    [Fact]
    public void BatchObservableCollection_AddRange_SingleNotification()
    {
        var coll = new BatchObservableCollection<int>();
        int notifyCount = 0;
        coll.CollectionChanged += (s, e) => notifyCount++;

        coll.AddRange(new[] { 1, 2, 3, 4, 5 });

        Assert.Equal(5, coll.Count);
        Assert.Equal(1, notifyCount); // One Reset notification
    }

    [Fact]
    public void BatchObservableCollection_ReplaceAll_ReplacesAndNotifies()
    {
        var coll = new BatchObservableCollection<string>();
        coll.Add("old1");
        coll.Add("old2");

        int notifyCount = 0;
        coll.CollectionChanged += (s, e) => notifyCount++;

        coll.ReplaceAll(new[] { "new1", "new2", "new3" });

        Assert.Equal(3, coll.Count);
        Assert.Equal("new1", coll[0]);
        Assert.Equal(1, notifyCount);
    }

    [Fact]
    public void BatchObservableCollection_RemoveRange_RemovesAndNotifies()
    {
        var coll = new BatchObservableCollection<int>();
        coll.AddRange(new[] { 1, 2, 3, 4, 5 });

        int notifyCount = 0;
        coll.CollectionChanged += (s, e) => notifyCount++;

        coll.RemoveRange(new[] { 2, 4 });

        Assert.Equal(3, coll.Count);
        Assert.Contains(1, coll);
        Assert.Contains(3, coll);
        Assert.Contains(5, coll);
        Assert.Equal(1, notifyCount);
    }

    [Fact]
    public void BatchObservableCollection_RemoveRange_NonexistentItemsIgnored()
    {
        var coll = new BatchObservableCollection<int>();
        coll.AddRange(new[] { 1, 2, 3 });

        coll.RemoveRange(new[] { 99, 100 });

        Assert.Equal(3, coll.Count);
    }

    [Fact]
    public void GradualObservableCollection_EnqueueAndProcess()
    {
        var coll = new GradualObservableCollection<int>();
        coll.ItemsPerCycle = 3;
        coll.EnqueueRange(new[] { 1, 2, 3, 4, 5, 6, 7 });

        Assert.True(coll.HasPendingItems);
        Assert.Equal(7, coll.PendingCount);

        // First cycle: 3 items
        bool more = coll.ProcessCycle();
        Assert.True(more);
        Assert.Equal(3, coll.Count);
        Assert.Equal(4, coll.PendingCount);

        // Second cycle: 3 more
        more = coll.ProcessCycle();
        Assert.True(more);
        Assert.Equal(6, coll.Count);
        Assert.Equal(1, coll.PendingCount);

        // Third cycle: 1 remaining
        more = coll.ProcessCycle();
        Assert.False(more);
        Assert.Equal(7, coll.Count);
        Assert.Equal(0, coll.PendingCount);
    }

    [Fact]
    public void GradualObservableCollection_Flush_AddsAll()
    {
        var coll = new GradualObservableCollection<string>();
        coll.EnqueueRange(new[] { "a", "b", "c", "d" });

        coll.Flush();

        Assert.Equal(4, coll.Count);
        Assert.False(coll.HasPendingItems);
    }

    [Fact]
    public void GradualObservableCollection_ProcessCycle_EmptyQueue()
    {
        var coll = new GradualObservableCollection<int>();
        bool more = coll.ProcessCycle();
        Assert.False(more);
        Assert.Empty(coll);
    }

    [Fact]
    public void GradualObservableCollection_DefaultItemsPerCycle()
    {
        var coll = new GradualObservableCollection<int>();
        Assert.Equal(10, coll.ItemsPerCycle);
    }

    [Fact]
    public void GradualObservableCollection_FiresCollectionChanged()
    {
        var coll = new GradualObservableCollection<int>();
        coll.ItemsPerCycle = 2;
        coll.EnqueueRange(new[] { 1, 2, 3 });

        int addCount = 0;
        coll.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                addCount++;
        };

        coll.ProcessCycle(); // Adds 2 items individually
        Assert.Equal(2, addCount);
    }
}
