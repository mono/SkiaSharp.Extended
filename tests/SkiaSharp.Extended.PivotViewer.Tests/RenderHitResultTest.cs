using System;
using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class RenderHitResultTest
{
    [Fact]
    public void None_Factory_ReturnsNoneType()
    {
        var result = RenderHitResult.None;

        Assert.Equal(RenderHitType.None, result.Type);
        Assert.Null(result.Item);
        Assert.Null(result.FilterPropertyId);
        Assert.Null(result.FilterValue);
        Assert.Null(result.RangeMin);
        Assert.Null(result.RangeMax);
        Assert.Null(result.DateRangeMin);
        Assert.Null(result.DateRangeMax);
        Assert.Null(result.LinkUri);
        Assert.Equal(-1, result.SortRowIndex);
        Assert.Null(result.CategoryName);
    }

    [Fact]
    public void Default_HasNoneType()
    {
        var result = new RenderHitResult();
        Assert.Equal(RenderHitType.None, result.Type);
    }

    [Fact]
    public void Default_SortRowIndex_IsMinusOne()
    {
        var result = new RenderHitResult();
        Assert.Equal(-1, result.SortRowIndex);
    }

    [Fact]
    public void Item_Property_Roundtrip()
    {
        var item = new PivotViewerItem("test-item");
        var result = new RenderHitResult { Item = item };
        Assert.Same(item, result.Item);
    }

    [Fact]
    public void FilterCheckbox_Properties_Roundtrip()
    {
        var result = new RenderHitResult
        {
            Type = RenderHitType.FilterCheckbox,
            FilterPropertyId = "color",
            FilterValue = "red",
        };

        Assert.Equal(RenderHitType.FilterCheckbox, result.Type);
        Assert.Equal("color", result.FilterPropertyId);
        Assert.Equal("red", result.FilterValue);
    }

    [Fact]
    public void NumericHistogram_Properties_Roundtrip()
    {
        var result = new RenderHitResult
        {
            Type = RenderHitType.FilterNumericHistogramBar,
            RangeMin = 1.5,
            RangeMax = 9.9,
        };

        Assert.Equal(1.5, result.RangeMin);
        Assert.Equal(9.9, result.RangeMax);
    }

    [Fact]
    public void DateTimeHistogram_Properties_Roundtrip()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2023, 12, 31);
        var result = new RenderHitResult
        {
            Type = RenderHitType.FilterDateTimeHistogramBar,
            DateRangeMin = min,
            DateRangeMax = max,
        };

        Assert.Equal(min, result.DateRangeMin);
        Assert.Equal(max, result.DateRangeMax);
    }

    [Fact]
    public void DetailLink_Properties_Roundtrip()
    {
        var result = new RenderHitResult
        {
            Type = RenderHitType.DetailLink,
            LinkUri = "https://example.com",
        };

        Assert.Equal("https://example.com", result.LinkUri);
    }

    [Fact]
    public void SortDropdownRow_Properties_Roundtrip()
    {
        var result = new RenderHitResult
        {
            Type = RenderHitType.SortDropdownRow,
            SortRowIndex = 3,
        };

        Assert.Equal(3, result.SortRowIndex);
    }

    [Fact]
    public void CategoryToggle_Properties_Roundtrip()
    {
        var result = new RenderHitResult
        {
            Type = RenderHitType.FilterCategoryToggle,
            CategoryName = "Color",
        };

        Assert.Equal("Color", result.CategoryName);
    }

    [Fact]
    public void RenderHitType_HasAllExpectedValues()
    {
        var values = Enum.GetValues<RenderHitType>();
        Assert.Equal(16, values.Length);
    }

    [Fact]
    public void RenderHitType_NoneIsZero()
    {
        Assert.Equal(0, (int)RenderHitType.None);
    }

    [Fact]
    public void None_Factory_ReturnsNewInstanceEachTime()
    {
        var a = RenderHitResult.None;
        var b = RenderHitResult.None;
        Assert.NotSame(a, b);
    }

    [Fact]
    public void FilterCategoryClear_Properties_Roundtrip()
    {
        var result = new RenderHitResult
        {
            Type = RenderHitType.FilterCategoryClear,
            CategoryName = "test",
        };

        Assert.Equal(RenderHitType.FilterCategoryClear, result.Type);
        Assert.Equal("test", result.CategoryName);
    }
}
