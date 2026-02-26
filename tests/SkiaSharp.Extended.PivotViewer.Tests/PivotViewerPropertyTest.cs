using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerPropertyTest
{
    [Fact]
    public void StringProperty_HasCorrectType()
    {
        var prop = new PivotViewerStringProperty("Name");
        Assert.Equal(PivotViewerPropertyType.Text, prop.PropertyType);
        Assert.Equal("Name", prop.Id);
        Assert.Equal("Name", prop.DisplayName);
    }

    [Fact]
    public void NumericProperty_HasCorrectType()
    {
        var prop = new PivotViewerNumericProperty("Price");
        Assert.Equal(PivotViewerPropertyType.Decimal, prop.PropertyType);
    }

    [Fact]
    public void DateTimeProperty_HasCorrectType()
    {
        var prop = new PivotViewerDateTimeProperty("Birthdate");
        Assert.Equal(PivotViewerPropertyType.DateTime, prop.PropertyType);
    }

    [Fact]
    public void LinkProperty_HasCorrectType()
    {
        var prop = new PivotViewerLinkProperty("Website");
        Assert.Equal(PivotViewerPropertyType.Link, prop.PropertyType);
    }

    [Fact]
    public void Property_SetDisplayName()
    {
        var prop = new PivotViewerStringProperty("body_style");
        prop.DisplayName = "Body Style";
        Assert.Equal("Body Style", prop.DisplayName);
    }

    [Fact]
    public void Property_SetFormat()
    {
        var prop = new PivotViewerNumericProperty("displacement");
        prop.Format = "0.#L";
        Assert.Equal("0.#L", prop.Format);
    }

    [Fact]
    public void Property_Options_CanFilter()
    {
        var prop = new PivotViewerStringProperty("Category");
        prop.Options = PivotViewerPropertyOptions.CanFilter;
        Assert.True(prop.CanFilter);
        Assert.False(prop.CanSearchText);
        Assert.False(prop.IsPrivate);
    }

    [Fact]
    public void Property_Options_Combined()
    {
        var prop = new PivotViewerStringProperty("Category");
        prop.Options = PivotViewerPropertyOptions.CanFilter | PivotViewerPropertyOptions.CanSearchText;
        Assert.True(prop.CanFilter);
        Assert.True(prop.CanSearchText);
    }

    [Fact]
    public void Property_WrappingText()
    {
        var prop = new PivotViewerStringProperty("Description");
        prop.Options = PivotViewerPropertyOptions.WrappingText;
        Assert.True(prop.IsWrappingText);
    }

    [Fact]
    public void Property_Lock_PreventsModification()
    {
        var prop = new PivotViewerStringProperty("Name");
        Assert.False(prop.IsLocked);

        prop.Lock();
        Assert.True(prop.IsLocked);

        Assert.Throws<InvalidOperationException>(() => prop.DisplayName = "New Name");
        Assert.Throws<InvalidOperationException>(() => prop.Format = "new");
        Assert.Throws<InvalidOperationException>(() => prop.Options = PivotViewerPropertyOptions.Private);
    }

    [Fact]
    public void Property_Lock_PreventsIdModification()
    {
        var prop = new PivotViewerStringProperty("Name");
        prop.Lock();

        Assert.Throws<InvalidOperationException>(() => prop.Id = "NewId");
    }

    [Fact]
    public void Property_Lock_IsIdempotent()
    {
        var prop = new PivotViewerStringProperty("Name");
        prop.Lock();
        prop.Lock(); // should not throw

        Assert.True(prop.IsLocked);
    }

    [Fact]
    public void Property_Lock_AllSettersThrow()
    {
        var prop = new PivotViewerNumericProperty("Price");
        prop.Lock();

        Assert.Throws<InvalidOperationException>(() => prop.Id = "X");
        Assert.Throws<InvalidOperationException>(() => prop.DisplayName = "X");
        Assert.Throws<InvalidOperationException>(() => prop.Format = "X");
        Assert.Throws<InvalidOperationException>(() => prop.Options = PivotViewerPropertyOptions.CanFilter);
    }

    [Fact]
    public void Property_Equality_ById()
    {
        var a = new PivotViewerStringProperty("Name");
        var b = new PivotViewerStringProperty("Name");
        var c = new PivotViewerStringProperty("Other");

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Property_CompareTo()
    {
        var a = new PivotViewerStringProperty("Alpha");
        var b = new PivotViewerStringProperty("Beta");

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(new PivotViewerStringProperty("Alpha")));
    }

    [Fact]
    public void Property_ToString()
    {
        var prop = new PivotViewerStringProperty("Category");
        Assert.Equal("Category (Text)", prop.ToString());
    }

    [Fact]
    public void StringProperty_HasSortsCollection()
    {
        var prop = new PivotViewerStringProperty("Category");
        Assert.NotNull(prop.Sorts);
        Assert.Empty(prop.Sorts);
    }

    // --- DateTimeProperty Presets ---

    [Fact]
    public void DateTimeProperty_HasDefaultPresetProvider()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        Assert.NotNull(prop.PresetProvider);
    }

    [Fact]
    public void DateTimeProperty_Presets_EmptyByDefault()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        Assert.Empty(prop.Presets);
    }

    [Fact]
    public void DateTimeProperty_AutogeneratePresets_Years()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = new[]
        {
            new DateTime(2000, 1, 1),
            new DateTime(2005, 6, 15),
            new DateTime(2010, 12, 31)
        };

        var presets = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(null!, prop, values);
        Assert.NotEmpty(presets);
        Assert.True(presets.Count >= 1, "Should generate at least one level of presets");
    }

    [Fact]
    public void DateTimeProperty_AutogeneratePresets_Decades()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = new[]
        {
            new DateTime(1990, 1, 1),
            new DateTime(2000, 6, 15),
            new DateTime(2020, 12, 31)
        };

        var presets = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(null!, prop, values);
        Assert.NotEmpty(presets);
        // Should generate decades + months
        Assert.True(presets.Count >= 2, "Should generate multiple levels for wide range");
    }

    [Fact]
    public void DateTimeProperty_AutogeneratePresets_Empty()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var presets = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(null!, prop, Array.Empty<DateTime>());
        Assert.Empty(presets);
    }

    [Fact]
    public void DateTimeProperty_GetStaticAndAutoPresets_IncludesCustom()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        prop.Presets.Add(new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31)));

        var values = new[] { new DateTime(2020, 3, 15), new DateTime(2020, 7, 20) };
        var presets = PivotViewerDateTimeProperty.GetStaticAndAutoCalendarDateRangePresets(null!, prop, values);

        // First level should be the static presets
        Assert.True(presets.Count >= 1);
        Assert.Contains(presets[0], r => r.Start == new DateTime(2020, 1, 1));
    }

    [Fact]
    public void DateTimeProperty_GetStaticPresets_OnlyStatic()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        prop.Presets.Add(new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31)));

        var values = new[] { new DateTime(2020, 3, 15) };
        var presets = PivotViewerDateTimeProperty.GetStaticCalendarDateRangePresets(null!, prop, values);

        Assert.Single(presets);
        Assert.Single(presets[0]);
    }

    [Fact]
    public void DateRange_ToString_ReturnsRange()
    {
        var range = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        var str = range.ToString();
        Assert.Contains("2020", str);
    }

    [Fact]
    public void NumericProperty_DecimalPlaces_DefaultIsNegativeOne()
    {
        var prop = new PivotViewerNumericProperty("Price");
        Assert.Equal(-1, prop.DecimalPlaces);
    }

    [Fact]
    public void NumericProperty_DecimalPlaces_CanBeSet()
    {
        var prop = new PivotViewerNumericProperty("Price") { DecimalPlaces = 2 };
        Assert.Equal(2, prop.DecimalPlaces);
    }
}
