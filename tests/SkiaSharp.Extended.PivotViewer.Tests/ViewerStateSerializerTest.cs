using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class ViewerStateSerializerTest
{
    [Fact]
    public void Serialize_EmptyState_ReturnsEmpty()
    {
        var state = new ViewerState();
        var result = ViewerStateSerializer.Serialize(state);
        Assert.Equal("", result);
    }

    [Fact]
    public void Serialize_ViewOnly_EncodesView()
    {
        var state = new ViewerState { ViewId = "grid" };
        var result = ViewerStateSerializer.Serialize(state);
        Assert.Contains("$view=grid", result);
    }

    [Fact]
    public void Serialize_SortOnly_EncodesSort()
    {
        var state = new ViewerState { SortPropertyId = "Year" };
        var result = ViewerStateSerializer.Serialize(state);
        Assert.Contains("$sort=Year", result);
    }

    [Fact]
    public void Serialize_SelectOnly_EncodesSelection()
    {
        var state = new ViewerState { SelectedItemId = "42" };
        var result = ViewerStateSerializer.Serialize(state);
        Assert.Contains("$select=42", result);
    }

    [Fact]
    public void Serialize_StringFilters_EncodesPropertyValues()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");

        var state = new ViewerState
        {
            Predicates = new FilterPredicate[] { pred }
        };

        var result = ViewerStateSerializer.Serialize(state);
        Assert.Contains("Category=Sports", result);
    }

    [Fact]
    public void Deserialize_EmptyString_ReturnsDefaultState()
    {
        var state = ViewerStateSerializer.Deserialize("");
        Assert.Null(state.ViewId);
        Assert.Null(state.SortPropertyId);
        Assert.Null(state.SelectedItemId);
    }

    [Fact]
    public void Deserialize_WithView_SetsViewId()
    {
        var state = ViewerStateSerializer.Deserialize("$view=graph");
        Assert.Equal("graph", state.ViewId);
    }

    [Fact]
    public void Deserialize_WithSort_SetsSortProperty()
    {
        var state = ViewerStateSerializer.Deserialize("$sort=Year");
        Assert.Equal("Year", state.SortPropertyId);
    }

    [Fact]
    public void Deserialize_WithSelect_SetsSelectedItem()
    {
        var state = ViewerStateSerializer.Deserialize("$select=42");
        Assert.Equal("42", state.SelectedItemId);
    }

    [Fact]
    public void Deserialize_WithStringFilter_CreatesPredicates()
    {
        var state = ViewerStateSerializer.Deserialize("Category=Sports&Category=Hybrid");
        Assert.Single(state.StringPredicates);
        Assert.Equal(2, state.StringPredicates[0].Values.Count);
    }

    [Fact]
    public void RoundTrip_ViewSortSelect()
    {
        var original = new ViewerState
        {
            ViewId = "graph",
            SortPropertyId = "Year",
            SelectedItemId = "item-7"
        };

        var serialized = ViewerStateSerializer.Serialize(original);
        var restored = ViewerStateSerializer.Deserialize(serialized);

        Assert.Equal("graph", restored.ViewId);
        Assert.Equal("Year", restored.SortPropertyId);
        Assert.Equal("item-7", restored.SelectedItemId);
    }

    [Fact]
    public void RoundTrip_StringFilters()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");
        pred.AddValue("Hybrid");

        var original = new ViewerState
        {
            Predicates = new FilterPredicate[] { pred }
        };

        var serialized = ViewerStateSerializer.Serialize(original);
        var restored = ViewerStateSerializer.Deserialize(serialized);

        Assert.Single(restored.StringPredicates);
        Assert.Equal(2, restored.StringPredicates[0].Values.Count);
    }

    [Fact]
    public void Deserialize_WithHashPrefix_StripsIt()
    {
        var state = ViewerStateSerializer.Deserialize("#$view=grid&$sort=Year");
        Assert.Equal("grid", state.ViewId);
        Assert.Equal("Year", state.SortPropertyId);
    }

    [Fact]
    public void Serialize_SpecialCharacters_UrlEncoded()
    {
        var pred = new StringFilterPredicate("Name");
        pred.AddValue("Sports & Racing");

        var state = new ViewerState
        {
            Predicates = new FilterPredicate[] { pred }
        };

        var serialized = ViewerStateSerializer.Serialize(state);
        Assert.DoesNotContain(" ", serialized); // Spaces should be encoded
        Assert.DoesNotContain("&R", serialized); // & in value should be encoded

        var restored = ViewerStateSerializer.Deserialize(serialized);
        Assert.Single(restored.StringPredicates);
        Assert.Contains("Sports & Racing", restored.StringPredicates[0].Values);
    }
}
