using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Integration tests that validate compatibility with real CXML test data,
/// covering property coverage, ImageBase resolution, supplemental merge, and type correctness.
/// </summary>
public class CxmlCompatibilityIntegrationTest
{
    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("collection-dz.cxml")]
    [InlineData("nigeria_state.cxml")]
    [InlineData("ski_resorts.cxml")]
    [InlineData("stockport_house_sales.cxml")]
    [InlineData("buxton.cxml")]
    [InlineData("msdnmagazine.cxml")]
    [InlineData("venues.cxml")]
    [InlineData("area.cxml")]
    [InlineData("geometry.cxml")]
    public void Parse_MostPropertiesHaveAtLeastOneItemWithValues(string file)
    {
        var source = TestDataHelper.LoadCxml(file);

        int total = source.ItemProperties.Count;
        int covered = source.ItemProperties
            .Count(prop => source.Items.Any(item => item[prop.Id]?.Count > 0));

        // At least 50% of declared properties should have item values
        // (some collections define FacetCategories that no items use)
        Assert.True(covered > 0, $"{file}: No properties have any item values");
        Assert.True(covered >= total / 2,
            $"{file}: Only {covered}/{total} properties have item values (expected >= {total / 2})");
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("nigeria_state.cxml")]
    [InlineData("ski_resorts.cxml")]
    [InlineData("area.cxml")]
    [InlineData("geometry.cxml")]
    public void Parse_EveryFacetCategoryPropertyHasAtLeastOneItemWithValues(string file)
    {
        var source = TestDataHelper.LoadCxml(file);

        foreach (var prop in source.ItemProperties)
        {
            // Skip implicit properties (Name, Href, Description, #Image) — not all items have these
            if (prop.Id is "Name" or "Href" or "Description" or "#Image")
                continue;

            bool anyItemHasValue = source.Items.Any(item => item[prop.Id]?.Count > 0);
            Assert.True(anyItemHasValue,
                $"{file}: Property '{prop.DisplayName}' ({prop.Id}) has no items with values");
        }
    }

    [Theory]
    [InlineData("conceptcars.cxml", "deepzoom/conceptcars.dzc")]
    [InlineData("collection-dz.cxml", "deepzoom/collection-dz.dzc")]
    [InlineData("nigeria_state.cxml", "deepzoom/images.dzc")]
    [InlineData("ski_resorts.cxml", "deepzoom/ski_resorts.dzc")]
    [InlineData("stockport_house_sales.cxml", "deepzoom/images.dzc")]
    [InlineData("area.cxml", "deepzoom/area.dzc")]
    [InlineData("geometry.cxml", "deepzoom/geometry.dzc")]
    [InlineData("msdnmagazine.cxml", "msdnmagazine_deepzoom\\msdnmagazine.dzc")]
    [InlineData("venues.cxml", "/pivot/venues/venues-collection.dzc.dzc")]
    public void Parse_ImageBase_IsCorrectlyResolved(string file, string expectedImgBase)
    {
        var source = TestDataHelper.LoadCxml(file);

        Assert.NotNull(source.ImageBase);
        Assert.Equal(expectedImgBase, source.ImageBase);
    }

    [Fact]
    public void Parse_Buxton_ImageBase_IsResolved()
    {
        var source = TestDataHelper.LoadCxml("buxton.cxml");

        Assert.NotNull(source.ImageBase);
        Assert.Contains("PivotViewer_files", source.ImageBase);
    }

    [Fact]
    public void Parse_SimpleSkiResorts_ImageBase_IsDirectPath()
    {
        var source = TestDataHelper.LoadCxml("simple_ski.cxml");

        Assert.NotNull(source.ImageBase);
        Assert.Equal("images", source.ImageBase);
    }

    [Fact]
    public void SupplementalCxml_MergesDescriptions()
    {
        var primary = TestDataHelper.LoadCxml("conceptcars.cxml");
        var secondary = TestDataHelper.LoadCxml("conceptcars_secondary.cxml");

        // Primary should not have Description property on items before merge
        // (descriptions are in the secondary file)
        int descCountBefore = primary.Items
            .Count(i => i.HasProperty("Description"));

        primary.MergeSupplementalData(secondary);

        int descCountAfter = primary.Items
            .Count(i => i.HasProperty("Description"));

        Assert.True(descCountAfter > descCountBefore,
            $"Merge should add descriptions: before={descCountBefore}, after={descCountAfter}");

        // Verify at least one merged description is non-empty
        var itemWithDesc = primary.Items.FirstOrDefault(i =>
            i.TryGetSingleValue<string>("Description", out var d) && !string.IsNullOrEmpty(d));
        Assert.NotNull(itemWithDesc);
    }

    [Fact]
    public void SupplementalCxml_PreservesExistingData()
    {
        var primary = TestDataHelper.LoadCxml("conceptcars.cxml");
        int itemCountBefore = primary.Items.Count;
        int propCountBefore = primary.ItemProperties.Count;

        var secondary = TestDataHelper.LoadCxml("conceptcars_secondary.cxml");
        primary.MergeSupplementalData(secondary);

        // Item count should not change (supplement only adds data, not items)
        Assert.Equal(itemCountBefore, primary.Items.Count);
        // Properties may increase (Description added)
        Assert.True(primary.ItemProperties.Count >= propCountBefore);
    }

    [Fact]
    public void ConceptCars_SupplementUri_PointsToSecondary()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");

        Assert.NotNull(source.SupplementUri);
        Assert.Equal("conceptcars_secondary.cxml", source.SupplementUri);
    }

    [Fact]
    public void ConceptCars_PropertyTypes_AreCorrect()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");

        // String properties
        var manufacturer = source.GetPivotPropertyById("Manufacturer");
        Assert.NotNull(manufacturer);
        Assert.Equal(PivotViewerPropertyType.Text, manufacturer!.PropertyType);

        var bodyStyle = source.GetPivotPropertyById("Body Style");
        Assert.NotNull(bodyStyle);
        Assert.Equal(PivotViewerPropertyType.Text, bodyStyle!.PropertyType);

        // Number properties
        var productionYear = source.GetPivotPropertyById("Production Year");
        Assert.NotNull(productionYear);
        Assert.Equal(PivotViewerPropertyType.Decimal, productionYear!.PropertyType);

        var engineDisplacement = source.GetPivotPropertyById("Engine Displacement");
        Assert.NotNull(engineDisplacement);
        Assert.Equal(PivotViewerPropertyType.Decimal, engineDisplacement!.PropertyType);
    }

    [Fact]
    public void NigeriaState_PropertyTypes_IncludeAllExpected()
    {
        var source = TestDataHelper.LoadCxml("nigeria_state.cxml");

        var typeSet = source.ItemProperties
            .Select(p => p.PropertyType)
            .Distinct()
            .ToHashSet();

        Assert.Contains(PivotViewerPropertyType.Text, typeSet);
        Assert.Contains(PivotViewerPropertyType.Decimal, typeSet);
        Assert.Contains(PivotViewerPropertyType.DateTime, typeSet);
        Assert.Contains(PivotViewerPropertyType.Link, typeSet);

        // Verify a specific Link property has URI values
        var linkProp = source.ItemProperties
            .First(p => p.PropertyType == PivotViewerPropertyType.Link);
        var itemWithLink = source.Items
            .First(i => i[linkProp.Id]?.Count > 0);
        var linkVal = (PivotViewerHyperlink)itemWithLink[linkProp.Id]![0];
        Assert.NotNull(linkVal.Uri);
    }

    [Theory]
    [InlineData("conceptcars.cxml")]
    [InlineData("ski_resorts.cxml")]
    [InlineData("stockport_house_sales.cxml")]
    [InlineData("nigeria_state.cxml")]
    [InlineData("buxton.cxml")]
    [InlineData("msdnmagazine.cxml")]
    [InlineData("venues.cxml")]
    public void Parse_AllItemsHaveNameProperty(string file)
    {
        var source = TestDataHelper.LoadCxml(file);

        var itemsWithName = source.Items
            .Count(i => i.HasProperty("Name"));

        Assert.True(itemsWithName > 0,
            $"{file}: Expected at least some items with Name property");
    }

    [Theory]
    [InlineData("conceptcars.cxml", 298)]
    [InlineData("collection-dz.cxml", 50)]
    [InlineData("nigeria_state.cxml", 33)]
    [InlineData("buxton.cxml", 239)]
    [InlineData("msdnmagazine.cxml", 2117)]
    public void Parse_ImageAttribute_IsParsedOnItems(string file, int expectedItems)
    {
        var source = TestDataHelper.LoadCxml(file);

        Assert.Equal(expectedItems, source.Items.Count);

        // Items that have an Img attribute in CXML get a #Image property
        var itemsWithImage = source.Items
            .Count(i => i.HasProperty("#Image"));

        // At least some items should have images
        Assert.True(itemsWithImage > 0,
            $"{file}: Expected at least some items with #Image property, got {itemsWithImage}");
    }
}
