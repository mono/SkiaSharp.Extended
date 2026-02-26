using SkiaSharp;
using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests for histogram layout via GridLayoutEngine and full rendering pipeline.
/// </summary>
public class HistogramLayoutTest
{
    [Fact]
    public void HistogramLayout_WithTextProperty_GroupsCorrectly()
    {
        var controller = CreateControllerWithCategories(30, 3);
        controller.CurrentView = "graph";

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);
        Assert.Equal(3, layout.Columns.Length);

        foreach (var col in layout.Columns)
            Assert.Equal(10, col.Items.Length);
    }

    [Fact]
    public void HistogramLayout_ColumnsHaveLabels()
    {
        var controller = CreateControllerWithCategories(15, 3);
        controller.CurrentView = "graph";

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);
        foreach (var col in layout.Columns)
        {
            Assert.NotNull(col.Label);
            Assert.NotEmpty(col.Label);
        }
    }

    [Fact]
    public void HistogramLayout_PositionsWithinBounds()
    {
        var controller = CreateControllerWithCategories(50, 5);
        controller.CurrentView = "graph";

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);

        foreach (var col in layout.Columns)
        {
            foreach (var pos in col.Items)
            {
                Assert.True(pos.X >= 0, $"X={pos.X} should be >= 0");
                Assert.True(pos.Y >= 0, $"Y={pos.Y} should be >= 0");
                Assert.True(pos.Width > 0, "Width should be > 0");
                Assert.True(pos.Height > 0, "Height should be > 0");
            }
        }
    }

    [Fact]
    public void HistogramLayout_HeadlessRender_HasPixels()
    {
        var controller = CreateControllerWithCategories(30, 3);
        controller.CurrentView = "graph";
        var layout = controller.HistogramLayout!;

        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint { Color = SKColors.CornflowerBlue };
        using var font = new SKFont { Size = 12 };

        foreach (var col in layout.Columns)
        {
            canvas.DrawText(col.Label, (float)(col.X + col.Width / 2), 590,
                SKTextAlign.Center, font, paint);
            foreach (var pos in col.Items)
            {
                canvas.DrawRect(
                    (float)pos.X + 1, (float)pos.Y + 1,
                    (float)pos.Width - 2, (float)pos.Height - 2, paint);
            }
        }

        using var pixmap = surface.PeekPixels();
        bool hasBlue = false;
        for (int y = 0; y < pixmap.Height && !hasBlue; y += 10)
        {
            for (int x = 0; x < pixmap.Width && !hasBlue; x += 10)
            {
                var c = pixmap.GetPixelColor(x, y);
                if (c.Blue > 200 && c.Red < 150) hasBlue = true;
            }
        }
        Assert.True(hasBlue, "Should have blue histogram bars rendered");
    }

    [Fact]
    public void HistogramLayout_AfterFilter_ReducedItems()
    {
        var controller = CreateControllerWithCategories(30, 3);
        controller.FilterEngine.AddStringFilter("Category", "Cat0");
        controller.CurrentView = "graph";

        var layout = controller.HistogramLayout;
        Assert.NotNull(layout);

        int totalItems = layout.Columns.Sum(c => c.Items.Length);
        Assert.Equal(10, totalItems);
    }

    [Fact]
    public void HistogramLayout_HitTest_FindsItem()
    {
        var controller = CreateControllerWithCategories(30, 3);
        controller.CurrentView = "graph";
        var layout = controller.HistogramLayout!;

        var firstPos = layout.Columns[0].Items[0];
        var hit = layout.HitTest(firstPos.X + 1, firstPos.Y + 1);
        Assert.NotNull(hit);
    }

    [Fact]
    public void HistogramLayout_HitTest_OutOfBounds_ReturnsNull()
    {
        var controller = CreateControllerWithCategories(30, 3);
        controller.CurrentView = "graph";
        var layout = controller.HistogramLayout!;

        var hit = layout.HitTest(-100, -100);
        Assert.Null(hit);
    }

    [Fact]
    public void GridToHistogram_SwitchPreservesItems()
    {
        var controller = CreateControllerWithCategories(30, 3);

        var gridPositions = controller.GridLayout!.Positions;
        int gridCount = gridPositions.Length;

        controller.CurrentView = "graph";
        int histCount = controller.HistogramLayout!.Columns.Sum(c => c.Items.Length);

        Assert.Equal(gridCount, histCount);
    }

    [Fact]
    public void HistogramToGrid_SwitchPreservesItems()
    {
        var controller = CreateControllerWithCategories(30, 3);
        controller.CurrentView = "graph";

        int histCount = controller.HistogramLayout!.Columns.Sum(c => c.Items.Length);

        controller.CurrentView = "grid";
        int gridCount = controller.GridLayout!.Positions.Length;

        Assert.Equal(histCount, gridCount);
    }

    private static PivotViewerController CreateControllerWithCategories(int count, int catCount)
    {
        var controller = new PivotViewerController();
        var prop = new PivotViewerStringProperty("Category")
        {
            DisplayName = "Category",
            Options = PivotViewerPropertyOptions.CanFilter
        };

        var items = new List<PivotViewerItem>();
        for (int i = 0; i < count; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(prop, new object[] { $"Cat{i % catCount}" });
            items.Add(item);
        }

        controller.LoadItems(items, new[] { prop });
        controller.SetAvailableSize(800, 600);
        controller.SortProperty = prop;
        return controller;
    }
}
