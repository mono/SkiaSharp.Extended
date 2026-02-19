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

    [Fact]
    public void Deserialize_NumericRangePredicate_ParsesCorrectly()
    {
        var serialized = "Price=GE(100)AND(LE(500))&$view=grid";
        var state = ViewerStateSerializer.Deserialize(serialized);

        Assert.Single(state.RangePredicates);
        Assert.Equal("Price", state.RangePredicates[0].PropertyId);
        Assert.Contains("GE(100)", state.RangePredicates[0].Expression);
        Assert.Equal("grid", state.ViewId);
    }

    [Fact]
    public void RoundTrip_NumericRange_PreservesValues()
    {
        var state = new ViewerState
        {
            ViewId = "grid",
            Predicates = new FilterPredicate[]
            {
                new NumericRangeFilterPredicate("Price", 100.5, 500.0)
            }
        };

        var serialized = ViewerStateSerializer.Serialize(state);
        Assert.Contains("GE(100.5)", serialized);
        Assert.Contains("LE(500)", serialized);

        var restored = ViewerStateSerializer.Deserialize(serialized);
        Assert.Single(restored.RangePredicates);
        Assert.Equal("Price", restored.RangePredicates[0].PropertyId);
    }

    [Fact]
    public void RoundTrip_DateTimeRange_PreservesValues()
    {
        var min = new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var max = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var state = new ViewerState
        {
            Predicates = new FilterPredicate[]
            {
                new DateTimeRangeFilterPredicate("Created", min, max)
            }
        };

        var serialized = ViewerStateSerializer.Serialize(state);
        Assert.Contains("GE(2020-01-15", serialized);
        Assert.Contains("LE(2023-12-31", serialized);

        var restored = ViewerStateSerializer.Deserialize(serialized);
        Assert.Single(restored.RangePredicates);
        Assert.Equal("Created", restored.RangePredicates[0].PropertyId);
    }

    [Fact]
    public void Deserialize_EmptyString_ReturnsValidState()
    {
        var state = ViewerStateSerializer.Deserialize("");
        Assert.NotNull(state);
        Assert.Null(state.ViewId);
        Assert.Empty(state.StringPredicates);
        Assert.Empty(state.RangePredicates);
    }

    [Fact]
    public void Deserialize_HashOnly_ReturnsValidState()
    {
        var state = ViewerStateSerializer.Deserialize("#");
        Assert.NotNull(state);
        Assert.Null(state.ViewId);
    }

    [Fact]
    public void Deserialize_TripleHash_ReturnsValidState()
    {
        var state = ViewerStateSerializer.Deserialize("###");
        Assert.NotNull(state);
        Assert.Null(state.ViewId);
    }

    [Fact]
    public void Deserialize_TruncatedKeyOnly_ReturnsValidState()
    {
        var state = ViewerStateSerializer.Deserialize("key=");
        Assert.NotNull(state);
        // key= has empty value — should not throw
    }

    [Fact]
    public void Deserialize_TruncatedValueOnly_ReturnsValidState()
    {
        // =value has empty key (eq == 0), should be skipped
        var state = ViewerStateSerializer.Deserialize("=value");
        Assert.NotNull(state);
        Assert.Null(state.ViewId);
    }

    [Fact]
    public void Deserialize_NoEqualsSign_ReturnsValidState()
    {
        var state = ViewerStateSerializer.Deserialize("noequalssign");
        Assert.NotNull(state);
        Assert.Null(state.ViewId);
    }

    [Fact]
    public void Deserialize_DoubleEncodedValues_ReturnsValidState()
    {
        // Double-encoded: %2520 (% → %25, then space → 20)
        var state = ViewerStateSerializer.Deserialize("Name%3DHello%2520World");
        Assert.NotNull(state);
    }

    [Fact]
    public void Deserialize_ExtraAmpersands_ReturnsValidState()
    {
        var state = ViewerStateSerializer.Deserialize("&&filter=val&&");
        Assert.NotNull(state);
        // Should parse filter=val and skip empty segments
        Assert.Single(state.StringPredicates);
        Assert.Equal("val", state.StringPredicates[0].Values.First());
    }

    [Fact]
    public void Serialize_AllStateFields_CombinedOutput()
    {
        var pred1 = new StringFilterPredicate("Color");
        pred1.AddValue("Red");
        pred1.AddValue("Blue");

        var pred2 = new NumericRangeFilterPredicate("Price", 10.0, 99.0);

        var state = new ViewerState
        {
            ViewId = "graph",
            SortPropertyId = "Year",
            SelectedItemId = "item-42",
            Predicates = new FilterPredicate[] { pred1, pred2 }
        };

        var serialized = ViewerStateSerializer.Serialize(state);

        Assert.Contains("$view=graph", serialized);
        Assert.Contains("$sort=Year", serialized);
        Assert.Contains("$select=item-42", serialized);
        Assert.Contains("Color=Red", serialized);
        Assert.Contains("Color=Blue", serialized);
        Assert.Contains("GE(10)", serialized);
        Assert.Contains("LE(99)", serialized);
    }

    [Fact]
    public void RoundTrip_FullState_AllFieldsPreserved()
    {
        var pred = new StringFilterPredicate("Category");
        pred.AddValue("Sports");
        pred.AddValue("Hybrid");

        var rangePred = new NumericRangeFilterPredicate("Price", 50.0, 200.0);

        var original = new ViewerState
        {
            ViewId = "grid",
            SortPropertyId = "Year",
            SelectedItemId = "item-7",
            Predicates = new FilterPredicate[] { pred, rangePred }
        };

        var serialized = ViewerStateSerializer.Serialize(original);
        var restored = ViewerStateSerializer.Deserialize(serialized);

        Assert.Equal("grid", restored.ViewId);
        Assert.Equal("Year", restored.SortPropertyId);
        Assert.Equal("item-7", restored.SelectedItemId);
        Assert.Single(restored.StringPredicates);
        Assert.Equal(2, restored.StringPredicates[0].Values.Count);
        Assert.Single(restored.RangePredicates);
        Assert.Equal("Price", restored.RangePredicates[0].PropertyId);
    }

    [Fact]
    public void Deserialize_NullString_ReturnsDefaultState()
    {
        var state = ViewerStateSerializer.Deserialize(null!);
        Assert.NotNull(state);
        Assert.Null(state.ViewId);
        Assert.Null(state.SortPropertyId);
        Assert.Null(state.SelectedItemId);
    }

    [Fact]
    public void Serialize_NullPredicates_DoesNotThrow()
    {
        var state = new ViewerState
        {
            ViewId = "grid",
            Predicates = null
        };

        var result = ViewerStateSerializer.Serialize(state);
        Assert.Contains("$view=grid", result);
    }

    [Fact]
    public void Serialize_EmptyStringFilterPredicate_Skipped()
    {
        var pred = new StringFilterPredicate("Color");
        // No values added
        var state = new ViewerState
        {
            Predicates = new FilterPredicate[] { pred }
        };

        var result = ViewerStateSerializer.Serialize(state);
        Assert.Equal("", result);
    }

    [Fact]
    public void Deserialize_MultipleStringFiltersOnDifferentProperties()
    {
        var state = ViewerStateSerializer.Deserialize("Color=Red&Size=Large&Color=Blue");
        Assert.Equal(2, state.StringPredicates.Count);

        var colorPred = state.StringPredicates.First(p => p.PropertyId == "Color");
        var sizePred = state.StringPredicates.First(p => p.PropertyId == "Size");
        Assert.Equal(2, colorPred.Values.Count);
        Assert.Single(sizePred.Values);
    }

    [Fact]
    public void RoundTrip_SpecialCharsInPropertyId()
    {
        var pred = new StringFilterPredicate("Make & Model");
        pred.AddValue("Toyota Camry");

        var state = new ViewerState
        {
            Predicates = new FilterPredicate[] { pred }
        };

        var serialized = ViewerStateSerializer.Serialize(state);
        var restored = ViewerStateSerializer.Deserialize(serialized);

        Assert.Single(restored.StringPredicates);
        Assert.Equal("Make & Model", restored.StringPredicates[0].PropertyId);
        Assert.Contains("Toyota Camry", restored.StringPredicates[0].Values);
    }

    [Fact]
    public void ViewerState_DefaultProperties()
    {
        var state = new ViewerState();
        Assert.Null(state.ViewId);
        Assert.Null(state.SortPropertyId);
        Assert.Null(state.SelectedItemId);
        Assert.Null(state.Predicates);
        Assert.NotNull(state.StringPredicates);
        Assert.NotNull(state.RangePredicates);
        Assert.Empty(state.StringPredicates);
        Assert.Empty(state.RangePredicates);
    }
}
