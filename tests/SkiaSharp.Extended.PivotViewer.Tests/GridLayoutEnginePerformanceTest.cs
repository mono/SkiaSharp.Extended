using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Performance tests for GridLayoutEngine.AddToGroup method.
/// These tests measure execution time to detect O(N²) complexity issues.
/// </summary>
public class GridLayoutEnginePerformanceTest
{

    /// <summary>
    /// Benchmark AddToGroup with increasing numbers of items in the same group.
    /// This test will clearly show O(N²) behavior if the Contains() check isn't optimized.
    /// </summary>
    [Fact]
    public void ComputeHistogramLayout_AddToGroup_O_N_Squared_Detection()
    {
        var engine = new GridLayoutEngine();
        var categoryProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        
        // Test different sizes to detect quadratic growth
        var measurements = new List<(int itemCount, long elapsedMs)>();
        
        // Test cases: 100, 200, 400, 800, 1600 items
        // If O(N²): time ratio should roughly be 4x for 2x items
        // If O(N):  time ratio should be ~2x for 2x items
        var testSizes = new[] { 100, 200, 400, 800, 1600 };
        
        var report = new System.Text.StringBuilder();
        report.AppendLine("\n=== AddToGroup Performance Test (Complexity Detection) ===");
        report.AppendLine("Testing with all items in the same group (worst case for Contains())");
        report.AppendLine("Size   | Time (ms)");
        report.AppendLine("-------|----------");
        
        foreach (var size in testSizes)
        {
            var items = CreateItemsInSameGroup(size, categoryProp);
            
            var sw = Stopwatch.StartNew();
            var layout = engine.ComputeHistogramLayout(items, "Category", 1920, 1080);
            sw.Stop();
            
            measurements.Add((size, sw.ElapsedMilliseconds));
            
            // Verify correctness
            Assert.Single(layout.Columns);
            Assert.Equal(size, layout.AllPositions.Length);
            
            report.AppendLine($"{size:D5}  | {sw.ElapsedMilliseconds:D8}");
        }
        
        // Analyze complexity
        report.AppendLine("\n=== Complexity Analysis ===");
        report.AppendLine("Size Ratio | Time Ratio | Expected O(N) | Expected O(N²) | Best Fit");
        report.AppendLine("-----------|------------|---------------|----------------|----------");
        
        for (int i = 1; i < measurements.Count; i++)
        {
            var prev = measurements[i - 1];
            var curr = measurements[i];
            
            double sizeFactor = (double)curr.itemCount / prev.itemCount;
            double timeFactor = (double)curr.elapsedMs / Math.Max(1, prev.elapsedMs);
            
            double expectedQuadratic = sizeFactor * sizeFactor;
            double errorQuadratic = Math.Abs(timeFactor - expectedQuadratic) / Math.Max(0.1, expectedQuadratic);
            
            double expectedLinear = sizeFactor;
            double errorLinear = Math.Abs(timeFactor - expectedLinear) / Math.Max(0.1, expectedLinear);
            
            string complexity = errorLinear < errorQuadratic ? "O(N)" : "O(N²)";
            
            report.AppendLine(
                $"x{sizeFactor:F1}      | x{timeFactor:F2}      | x{expectedLinear:F2}           | x{expectedQuadratic:F2}            | {complexity}");
        }
        
        // Overall assessment
        report.AppendLine("\n=== ASSESSMENT ===");
        if (measurements[measurements.Count - 1].elapsedMs < 100)
        {
            report.AppendLine("✓ FAST: All operations completed in < 100ms");
            report.AppendLine("  AddToGroup appears to be O(N) or optimized O(N²)");
        }
        else if (measurements[measurements.Count - 1].elapsedMs < 1000)
        {
            report.AppendLine("⚠ MODERATE: Operations taking 100-1000ms");
            report.AppendLine("  AddToGroup may have O(N²) complexity or inefficient implementation");
        }
        else
        {
            report.AppendLine("❌ SLOW: Operations taking > 1000ms");
            report.AppendLine("  AddToGroup likely has O(N²) or worse complexity");
        }
        
        System.Diagnostics.Debug.WriteLine(report.ToString());
    }

    /// <summary>
    /// Test histogram layout with many items distributed across multiple groups.
    /// This tests the worst case for AddToGroup when each group receives multiple items.
    /// </summary>
    [Fact]
    public void ComputeHistogramLayout_ManyGroupsManyItems_Performance()
    {
        var engine = new GridLayoutEngine();
        var categoryProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        
        // Create items distributed across 10 categories, 100 items per category
        var items = new List<PivotViewerItem>();
        for (int category = 0; category < 10; category++)
        {
            for (int itemInCategory = 0; itemInCategory < 100; itemInCategory++)
            {
                var item = new PivotViewerItem($"Item_{category}_{itemInCategory}");
                item.Set(categoryProp, new object[] { $"Category_{category}" });
                items.Add(item);
            }
        }
        
        var sw = Stopwatch.StartNew();
        var layout = engine.ComputeHistogramLayout(items, "Category", 1920, 1080);
        sw.Stop();
        
        // Verify correctness
        Assert.Equal(10, layout.Columns.Length);
        Assert.Equal(1000, layout.AllPositions.Length);
        
        var report = new System.Text.StringBuilder();
        report.AppendLine($"\n=== ManyGroupsManyItems Test ===");
        report.AppendLine($"1000 items in 10 groups (100 per group): {sw.ElapsedMilliseconds} ms");
        System.Diagnostics.Debug.WriteLine(report.ToString());
        
        // This should complete quickly (< 100ms on modern hardware)
        // If AddToGroup is O(N²) per group, total would be O(G*N²) = O(10*100²) = O(100,000)
        Assert.True(sw.ElapsedMilliseconds < 500, 
            $"Performance regression: Expected < 500ms, got {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Direct benchmark of the histogram layout's grouping phase.
    /// Measures time with 5000 items all in the same group (worst case for Contains()).
    /// </summary>
    [Fact]
    public void ComputeHistogramLayout_WorstCase_SingleLargeGroup()
    {
        var engine = new GridLayoutEngine();
        var categoryProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        
        // Create 5000 items, all in one group
        var items = CreateItemsInSameGroup(5000, categoryProp);
        
        var sw = Stopwatch.StartNew();
        var layout = engine.ComputeHistogramLayout(items, "Category", 1920, 1080);
        sw.Stop();
        
        // Verify correctness
        Assert.Single(layout.Columns);
        Assert.Equal(5000, layout.AllPositions.Length);
        
        var report = new System.Text.StringBuilder();
        report.AppendLine($"\n=== WorstCase_SingleLargeGroup Test ===");
        report.AppendLine($"5000 items in 1 group: {sw.ElapsedMilliseconds} ms");
        System.Diagnostics.Debug.WriteLine(report.ToString());
        
        // If using Contains() inefficiently, this could take several seconds
        // Even with O(N²) it should be manageable, but would show the issue clearly
        // We allow up to 10 seconds as a threshold to identify the problem
        Assert.True(sw.ElapsedMilliseconds < 10000,
            $"Performance issue detected: 5000 items took {sw.ElapsedMilliseconds}ms (potential O(N²) complexity)");
    }

    /// <summary>
    /// Baseline test: Items with same property value (same group key) vs unique values.
    /// Demonstrates the performance difference between the two code paths.
    /// </summary>
    [Fact]
    public void ComputeHistogramLayout_SameValueVsUniqueValues_Comparison()
    {
        var engine = new GridLayoutEngine();
        var categoryProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        int itemCount = 1000;
        
        // Case 1: All items have the same category value
        var itemsSameValue = CreateItemsInSameGroup(itemCount, categoryProp);
        
        var sw1 = Stopwatch.StartNew();
        var layout1 = engine.ComputeHistogramLayout(itemsSameValue, "Category", 1920, 1080);
        sw1.Stop();
        
        // Case 2: Each item has a unique category value
        var itemsUniqueValue = new List<PivotViewerItem>();
        for (int i = 0; i < itemCount; i++)
        {
            var item = new PivotViewerItem($"Item_{i}");
            item.Set(categoryProp, new object[] { $"Unique_Category_{i}" });
            itemsUniqueValue.Add(item);
        }
        
        var sw2 = Stopwatch.StartNew();
        var layout2 = engine.ComputeHistogramLayout(itemsUniqueValue, "Category", 1920, 1080);
        sw2.Stop();
        
        var report = new System.Text.StringBuilder();
        report.AppendLine($"\n=== SameValueVsUniqueValues Comparison ===");
        report.AppendLine($"1000 items test:");
        report.AppendLine($"  Same value (1 group):        {sw1.ElapsedMilliseconds} ms");
        report.AppendLine($"  Unique values (1000 groups): {sw2.ElapsedMilliseconds} ms");
        
        double ratio = sw2.ElapsedMilliseconds > 0 ? (double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds : 1.0;
        report.AppendLine($"  Ratio: {ratio:F2}x");
        
        // Case 1 should be MUCH slower if AddToGroup is O(N²)
        // Case 2 creates new groups for each item, so the Contains() is always checking empty lists
        if (sw1.ElapsedMilliseconds > sw2.ElapsedMilliseconds * 2)
        {
            report.AppendLine($"\n  ⚠️  WARNING: Same-value case is significantly slower!");
            report.AppendLine($"      This indicates O(N²) complexity in AddToGroup due to Contains() check.");
        }
        else
        {
            report.AppendLine($"\n  ✓ GOOD: Performance is similar for both cases");
            report.AppendLine($"      AddToGroup does not show obvious O(N²) behavior.");
        }
        
        System.Diagnostics.Debug.WriteLine(report.ToString());
    }

    /// <summary>
    /// Scaling test: Measure AddToGroup performance with exponentially larger datasets.
    /// </summary>
    [Fact]
    public void ComputeHistogramLayout_ScalingTest()
    {
        var engine = new GridLayoutEngine();
        var categoryProp = new PivotViewerStringProperty("Category") { DisplayName = "Category" };
        
        var report = new System.Text.StringBuilder();
        report.AppendLine("\n=== Scaling Test ===");
        report.AppendLine("Size   | Time (ms) | Complexity Indicator");
        report.AppendLine("-------|-----------|---------------------");
        
        long previousTime = 0;
        int previousSize = 0;
        
        foreach (var size in new[] { 100, 200, 500, 1000, 2000 })
        {
            var items = CreateItemsInSameGroup(size, categoryProp);
            
            var sw = Stopwatch.StartNew();
            var layout = engine.ComputeHistogramLayout(items, "Category", 1920, 1080);
            sw.Stop();
            
            string indicator = "";
            if (previousSize > 0)
            {
                double sizeRatio = (double)size / previousSize;
                double timeRatio = (double)sw.ElapsedMilliseconds / Math.Max(1, previousTime);
                double expectedQuadratic = sizeRatio * sizeRatio;
                
                if (timeRatio > expectedQuadratic * 1.5)
                    indicator = "O(N²) ⚠️";
                else if (timeRatio > sizeRatio * 1.5)
                    indicator = "Worse than O(N)";
                else
                    indicator = "Good (O(N))";
            }
            
            report.AppendLine($"{size:D5}   | {sw.ElapsedMilliseconds:D8} | {indicator}");
            
            previousTime = sw.ElapsedMilliseconds;
            previousSize = size;
        }
        
        System.Diagnostics.Debug.WriteLine(report.ToString());
    }

    // ===== Helper Methods =====

    /// <summary>
    /// Creates a list of items, all with the same category value.
    /// This is the worst case for AddToGroup if it uses Contains().
    /// </summary>
    private List<PivotViewerItem> CreateItemsInSameGroup(int count, PivotViewerStringProperty categoryProp)
    {
        var items = new List<PivotViewerItem>(count);
        for (int i = 0; i < count; i++)
        {
            var item = new PivotViewerItem($"Item_{i}");
            item.Set(categoryProp, new object[] { "SameCategory" });
            items.Add(item);
        }
        return items;
    }
}
