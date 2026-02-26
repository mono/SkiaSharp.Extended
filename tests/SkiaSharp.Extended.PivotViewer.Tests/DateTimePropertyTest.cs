using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class DateTimePropertyTest
{
    [Fact]
    public void Constructor_SetsPropertyTypeToDateTime()
    {
        var prop = new PivotViewerDateTimeProperty("Released");

        Assert.Equal(PivotViewerPropertyType.DateTime, prop.PropertyType);
        Assert.Equal("Released", prop.Id);
    }

    [Fact]
    public void Presets_DefaultIsEmpty()
    {
        var prop = new PivotViewerDateTimeProperty("Date");

        Assert.NotNull(prop.Presets);
        Assert.Empty(prop.Presets);
    }

    [Fact]
    public void PresetProvider_DefaultIsGetStaticAndAutoCalendarDateRangePresets()
    {
        var prop = new PivotViewerDateTimeProperty("Date");

        Assert.NotNull(prop.PresetProvider);
        Assert.Equal(
            (PivotViewerDateTimePropertyRangesProvider)PivotViewerDateTimeProperty.GetStaticAndAutoCalendarDateRangePresets,
            prop.PresetProvider);
    }

    [Fact]
    public void AutogenerateCalendarDateRangePresets_EmptyValues_ReturnsEmpty()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = Enumerable.Empty<DateTime>();

        var result = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(
            new object(), prop, values);

        Assert.Empty(result);
    }

    [Fact]
    public void AutogenerateCalendarDateRangePresets_SpanningDecades_GeneratesDecadeAndMonthRanges()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = new[]
        {
            new DateTime(1990, 3, 15),
            new DateTime(2000, 6, 1),
            new DateTime(2020, 9, 30),
        };

        var result = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(
            new object(), prop, values);

        // Spanning >5 years produces decades (level 1) + months (level 2)
        Assert.Equal(2, result.Count);

        // Level 1: decades
        var decades = result[0];
        Assert.True(decades.Count > 0);
        Assert.Equal(new DateTime(1990, 1, 1), decades[0].Start);
        Assert.Equal(new DateTime(2000, 1, 1), decades[0].End);

        // Level 2: months
        var months = result[1];
        Assert.True(months.Count > 0);
        Assert.Equal(new DateTime(1990, 3, 1), months[0].Start);
        Assert.Equal(new DateTime(1990, 4, 1), months[0].End);
    }

    [Fact]
    public void AutogenerateCalendarDateRangePresets_SpanningYears_GeneratesYearAndMonthRanges()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = new[]
        {
            new DateTime(2022, 1, 10),
            new DateTime(2023, 6, 15),
            new DateTime(2024, 3, 20),
        };

        var result = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(
            new object(), prop, values);

        // Spanning >1 year but <=5 years produces years (level 1) + months (level 2)
        Assert.Equal(2, result.Count);

        // Level 1: years
        var years = result[0];
        Assert.True(years.Count > 0);
        Assert.Equal(new DateTime(2022, 1, 1), years[0].Start);
        Assert.Equal(new DateTime(2023, 1, 1), years[0].End);

        // Level 2: months
        var months = result[1];
        Assert.True(months.Count > 0);
        Assert.Equal(new DateTime(2022, 1, 1), months[0].Start);
        Assert.Equal(new DateTime(2022, 2, 1), months[0].End);
    }

    [Fact]
    public void AutogenerateCalendarDateRangePresets_Within60Days_GeneratesNoRanges()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = new[]
        {
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 15),
        };

        var result = PivotViewerDateTimeProperty.AutogenerateCalendarDateRangePresets(
            new object(), prop, values);

        // 46 days span is <=60, so no coarse or fine ranges
        Assert.Empty(result);
    }

    [Fact]
    public void GetStaticCalendarDateRangePresets_NoPresets_ReturnsEmpty()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var values = new[] { DateTime.Now };

        var result = PivotViewerDateTimeProperty.GetStaticCalendarDateRangePresets(
            new object(), prop, values);

        Assert.Empty(result);
    }

    [Fact]
    public void GetStaticCalendarDateRangePresets_WithPresets_ReturnsPresetsOnly()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var preset = new DateRange(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1));
        prop.Presets.Add(preset);

        var values = new[]
        {
            new DateTime(2010, 1, 1),
            new DateTime(2025, 1, 1),
        };

        var result = PivotViewerDateTimeProperty.GetStaticCalendarDateRangePresets(
            new object(), prop, values);

        Assert.Single(result);
        Assert.Single(result[0]);
        Assert.Equal(preset.Start, result[0][0].Start);
        Assert.Equal(preset.End, result[0][0].End);
    }

    [Fact]
    public void GetStaticAndAutoCalendarDateRangePresets_CombinesStaticAndAuto()
    {
        var prop = new PivotViewerDateTimeProperty("Date");
        var preset = new DateRange(new DateTime(2020, 1, 1), new DateTime(2021, 1, 1));
        prop.Presets.Add(preset);

        var values = new[]
        {
            new DateTime(2010, 3, 1),
            new DateTime(2025, 6, 1),
        };

        var result = PivotViewerDateTimeProperty.GetStaticAndAutoCalendarDateRangePresets(
            new object(), prop, values);

        // First group: static presets
        Assert.True(result.Count >= 2);
        Assert.Contains(preset, result[0]);

        // Remaining groups: auto-generated (decades + months for >5 year span)
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void DateRange_StartAndEnd_ReturnConstructorValues()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        var range = new DateRange(start, end);

        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Fact]
    public void DateRange_ToString_FormatsWithShortDates()
    {
        var range = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));

        var str = range.ToString();

        // Format is "{Start:d}–{End:d}" using short date format
        Assert.Contains("–", str);
        Assert.Contains(new DateTime(2024, 1, 1).ToString("d"), str);
        Assert.Contains(new DateTime(2024, 12, 31).ToString("d"), str);
    }
}
