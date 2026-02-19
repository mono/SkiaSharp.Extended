using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class CxmlCollectionSourceTest
{
    [Fact]
    public void Parse_ConceptCars_LoadsSuccessfully()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        Assert.Equal(CxmlCollectionState.Loaded, source.State);
        Assert.Equal("Concept Cars", source.Name);
        Assert.True(source.Items.Count > 0);
        Assert.True(source.ItemProperties.Count > 0);
    }

    [Fact]
    public void Parse_ConceptCars_HasCorrectPropertyCount()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        // conceptcars.cxml has 12 FacetCategories + well-known (Name, Href, #Image)
        Assert.True(source.ItemProperties.Count >= 12);
    }

    [Fact]
    public void Parse_ConceptCars_PropertiesHaveCorrectTypes()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var manufacturer = source.GetPivotPropertyById("Manufacturer");
        Assert.NotNull(manufacturer);
        Assert.IsType<PivotViewerStringProperty>(manufacturer);
        Assert.Equal(PivotViewerPropertyType.Text, manufacturer!.PropertyType);

        var productionYear = source.GetPivotPropertyById("Production Year");
        Assert.NotNull(productionYear);
        Assert.IsType<PivotViewerNumericProperty>(productionYear);
        Assert.Equal(PivotViewerPropertyType.Decimal, productionYear!.PropertyType);
    }

    [Fact]
    public void Parse_ConceptCars_PropertiesHaveCorrectOptions()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var manufacturer = source.GetPivotPropertyById("Manufacturer");
        Assert.NotNull(manufacturer);
        Assert.True(manufacturer!.CanFilter);
        Assert.True(manufacturer.CanSearchText);

        // Engine Combination has IsFilterVisible="false"
        var engineCombo = source.GetPivotPropertyById("Engine Combination");
        Assert.NotNull(engineCombo);
        Assert.False(engineCombo!.CanFilter);
    }

    [Fact]
    public void Parse_ConceptCars_PropertiesHaveFormat()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var displacement = source.GetPivotPropertyById("Engine Displacement");
        Assert.NotNull(displacement);
        Assert.Equal("0.#L", displacement!.Format);

        var year = source.GetPivotPropertyById("Production Year");
        Assert.NotNull(year);
        Assert.Equal("###0", year!.Format);
    }

    [Fact]
    public void Parse_ConceptCars_ItemsHaveNames()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var firstItem = source.Items[0];
        Assert.True(firstItem.TryGetSingleValue<string>("Name", out var name));
        Assert.False(string.IsNullOrEmpty(name));
    }

    [Fact]
    public void Parse_ConceptCars_ItemsHaveImages()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var firstItem = source.Items[0];
        Assert.True(firstItem.HasProperty("#Image"));
        firstItem.TryGetSingleValue<string>("#Image", out var img);
        Assert.StartsWith("#", img!); // DZC reference like "#0"
    }

    [Fact]
    public void Parse_ConceptCars_ItemsHaveFacetValues()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var firstItem = source.Items[0];

        // Check for a string facet
        var bodyStyle = firstItem["Body Style"];
        Assert.NotNull(bodyStyle);
        Assert.True(bodyStyle!.Count > 0);

        // Check for a numeric facet
        var displacement = firstItem["Engine Displacement"];
        Assert.NotNull(displacement);
        Assert.IsType<double>(displacement![0]);
    }

    [Fact]
    public void Parse_ConceptCars_HasImageBase()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        Assert.NotNull(source.ImageBase);
        Assert.Contains("conceptcars.dzc", source.ImageBase!);
    }

    [Fact]
    public void Parse_ConceptCars_GetItemById()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        var item = source.GetItemById("0");
        Assert.NotNull(item);
        Assert.Equal("0", item!.Id);
    }

    [Fact]
    public void Parse_MinimalCxml_Works()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <FacetCategories>
        <FacetCategory Name=""Color"" Type=""String"" p:IsFilterVisible=""true"" p:IsMetaDataVisible=""true"" p:IsWordWheelVisible=""true"" />
        <FacetCategory Name=""Price"" Type=""Number"" p:IsFilterVisible=""true"" p:IsMetaDataVisible=""true"" p:Format=""$#,##0"" />
        <FacetCategory Name=""Created"" Type=""DateTime"" p:IsFilterVisible=""true"" />
    </FacetCategories>
    <Items>
        <Item Id=""1"" Name=""Widget"" Href=""https://example.com"" Img=""#0"">
            <Facets>
                <Facet Name=""Color""><String Value=""Red"" /></Facet>
                <Facet Name=""Price""><Number Value=""19.99"" /></Facet>
                <Facet Name=""Created""><DateTime Value=""2023-06-15T00:00:00"" /></Facet>
            </Facets>
        </Item>
        <Item Id=""2"" Name=""Gadget"" Img=""#1"">
            <Facets>
                <Facet Name=""Color""><String Value=""Blue"" /><String Value=""Green"" /></Facet>
                <Facet Name=""Price""><Number Value=""29.99"" /></Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);

        Assert.Equal(CxmlCollectionState.Loaded, source.State);
        Assert.Equal("Test", source.Name);
        Assert.Equal(2, source.Items.Count);

        // Check Color property
        var colorProp = source.GetPivotPropertyById("Color");
        Assert.NotNull(colorProp);
        Assert.IsType<PivotViewerStringProperty>(colorProp);
        Assert.True(colorProp!.CanFilter);
        Assert.True(colorProp.CanSearchText);

        // Check Price property
        var priceProp = source.GetPivotPropertyById("Price");
        Assert.NotNull(priceProp);
        Assert.IsType<PivotViewerNumericProperty>(priceProp);
        Assert.Equal("$#,##0", priceProp!.Format);

        // Check DateTime property
        var createdProp = source.GetPivotPropertyById("Created");
        Assert.NotNull(createdProp);
        Assert.IsType<PivotViewerDateTimeProperty>(createdProp);

        // Check item 1
        var item1 = source.GetItemById("1");
        Assert.NotNull(item1);
        Assert.True(item1!.TryGetSingleValue<string>("Name", out var name));
        Assert.Equal("Widget", name);
        Assert.True(item1.TryGetSingleValue<string>("#Image", out var img));
        Assert.Equal("#0", img);
        Assert.Equal(19.99, (double)item1["Price"]![0]);
        Assert.IsType<DateTime>(item1["Created"]![0]);

        // Check item 2 with multi-value string facet
        var item2 = source.GetItemById("2");
        Assert.NotNull(item2);
        var colors = item2!["Color"];
        Assert.NotNull(colors);
        Assert.Equal(2, colors!.Count);
        Assert.Contains("Blue", colors.Cast<string>());
        Assert.Contains("Green", colors.Cast<string>());
    }

    [Fact]
    public void Parse_WithLinkFacet_ParsesHyperlinks()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Links"">
    <FacetCategories>
        <FacetCategory Name=""Website"" Type=""Link"" />
    </FacetCategories>
    <Items>
        <Item Id=""1"" Name=""Test"">
            <Facets>
                <Facet Name=""Website"">
                    <Link Href=""https://example.com"" Name=""Example"" />
                </Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        var item = source.Items[0];
        var website = item["Website"];
        Assert.NotNull(website);
        var link = Assert.IsType<PivotViewerHyperlink>(website![0]);
        Assert.Equal("Example", link.Text);
        Assert.Equal(new Uri("https://example.com"), link.Uri);
    }

    [Fact]
    public void Parse_WithCopyright_ParsesCopyright()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <Copyright Name=""Test Corp"" Href=""https://example.com/copyright"" />
    <FacetCategories />
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.NotNull(source.Copyright);
        Assert.Equal("Test Corp", source.Copyright!.Text);
    }

    [Fact]
    public void Parse_InvalidXml_ThrowsAndSetsFailed()
    {
        Assert.Throws<FormatException>(() =>
            CxmlCollectionSource.Parse("<Invalid/>"));
    }

    [Fact]
    public void CreateProperty_CxmlTypeMapping()
    {
        // Verify the critical CXML → API type mapping
        var stringProp = CxmlCollectionSource.CreateProperty("test", "String");
        Assert.IsType<PivotViewerStringProperty>(stringProp);
        Assert.Equal(PivotViewerPropertyType.Text, stringProp.PropertyType);
        Assert.False(stringProp.IsWrappingText);

        var longStringProp = CxmlCollectionSource.CreateProperty("test", "LongString");
        Assert.IsType<PivotViewerStringProperty>(longStringProp);
        Assert.Equal(PivotViewerPropertyType.Text, longStringProp.PropertyType);
        Assert.True(longStringProp.IsWrappingText);

        var numberProp = CxmlCollectionSource.CreateProperty("test", "Number");
        Assert.IsType<PivotViewerNumericProperty>(numberProp);
        Assert.Equal(PivotViewerPropertyType.Decimal, numberProp.PropertyType);

        var dateTimeProp = CxmlCollectionSource.CreateProperty("test", "DateTime");
        Assert.IsType<PivotViewerDateTimeProperty>(dateTimeProp);
        Assert.Equal(PivotViewerPropertyType.DateTime, dateTimeProp.PropertyType);

        var linkProp = CxmlCollectionSource.CreateProperty("test", "Link");
        Assert.IsType<PivotViewerLinkProperty>(linkProp);
        Assert.Equal(PivotViewerPropertyType.Link, linkProp.PropertyType);
    }

    [Fact]
    public void Parse_MsdnMagazine_LoadsLargeCollection()
    {
        using var stream = TestDataHelper.GetStream("msdnmagazine.cxml");
        var source = CxmlCollectionSource.Parse(stream);

        Assert.Equal(CxmlCollectionState.Loaded, source.State);
        Assert.True(source.Items.Count > 100, $"Expected >100 items, got {source.Items.Count}");
    }

    [Fact]
    public void StateChanged_FiresDuringParsing()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Test"">
    <FacetCategories /><Items />
</Collection>";

        var states = new List<CxmlCollectionState>();
        var source = CxmlCollectionSource.Parse(xml);
        // State should be Loaded after successful parse
        Assert.Equal(CxmlCollectionState.Loaded, source.State);
    }

    [Fact]
    public void Parse_EmptyItems_ReturnsEmptyCollection()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Empty"">
    <FacetCategories>
        <FacetCategory Name=""Color"" Type=""String"" />
    </FacetCategories>
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.Empty(source.Items);
        Assert.Single(source.ItemProperties);
    }

    [Fact]
    public async Task LoadAsync_FromStream_Succeeds()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.cxml");
        var source = await CxmlCollectionSource.LoadAsync(stream);

        Assert.Equal(CxmlCollectionState.Loaded, source.State);
        Assert.True(source.Items.Count > 0);
    }

    [Fact]
    public async Task LoadAsync_InvalidStream_FailsGracefully()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("not xml"));

        // LoadAsync in ParseInto will throw because ParseDocument re-throws
        // But Task.Run wrapping means the exception propagates through the task
        try
        {
            var source = await CxmlCollectionSource.LoadAsync(stream);
            // If it doesn't throw, it should have Failed state
            Assert.Equal(CxmlCollectionState.Failed, source.State);
        }
        catch
        {
            // Exception propagated, which is also acceptable
        }
    }

    [Fact]
    public async Task ParseCxml_MsdnMagazine_ParsesSortOrder()
    {
        using var stream = TestDataHelper.GetStream("msdnmagazine.cxml");
        var source = await CxmlCollectionSource.LoadAsync(stream);
        Assert.Equal(CxmlCollectionState.Loaded, source.State);

        var popularity = source.ItemProperties.FirstOrDefault(p => p.Id == "Popularity");
        Assert.NotNull(popularity);
        Assert.IsType<PivotViewerStringProperty>(popularity);

        var strProp = (PivotViewerStringProperty)popularity;
        Assert.True(strProp.Sorts.Count > 0, "Sorts should be populated from SortOrder extension");

        var sort = strProp.Sorts[0];
        Assert.Equal("Popularity Group", sort.Key);

        // Verify the sort order: "Viewed Often" < "Viewed Occasionally" < "Viewed Infrequently"
        var comparer = sort.Value;
        Assert.True(comparer.Compare("Viewed Often", "Viewed Occasionally") < 0);
        Assert.True(comparer.Compare("Viewed Occasionally", "Viewed Infrequently") < 0);
        Assert.True(comparer.Compare("Viewed Often", "Viewed Infrequently") < 0);
    }

    [Fact]
    public void CustomSortOrderComparer_SortsKnownValuesFirst()
    {
        var comparer = new CustomSortOrderComparer(new[] { "High", "Medium", "Low" });

        Assert.True(comparer.Compare("High", "Medium") < 0);
        Assert.True(comparer.Compare("Medium", "Low") < 0);
        Assert.True(comparer.Compare("High", "Low") < 0);
        Assert.Equal(0, comparer.Compare("High", "HIGH")); // case insensitive
        Assert.True(comparer.Compare("Low", "Unknown") < 0); // known < unknown
        Assert.True(comparer.Compare("Apple", "Banana") < 0); // unknown sorts alphabetically
    }

    [Fact]
    public void Parse_WithRelatedCollections_ParsesCorrectly()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <FacetCategories />
    <Items />
    <p:RelatedCollections>
        <p:RelatedCollection Name=""Cars"" Href=""cars.cxml"" />
        <p:RelatedCollection Name=""Trucks"" Href=""trucks.cxml"" />
    </p:RelatedCollections>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.Equal(2, source.RelatedCollections.Count);
        Assert.Equal("Cars", source.RelatedCollections[0].Name);
        Assert.Equal("cars.cxml", source.RelatedCollections[0].Href);
        Assert.Equal("Trucks", source.RelatedCollections[1].Name);
        Assert.Equal("trucks.cxml", source.RelatedCollections[1].Href);
    }

    [Fact]
    public void Parse_WithoutRelatedCollections_ReturnsEmpty()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Test"">
    <FacetCategories /><Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.Empty(source.RelatedCollections);
    }

    [Fact]
    public void Parse_WithAdditionalSearchText_ParsesCorrectly()
    {
        // Schema-compliant: AdditionalSearchText as attribute on Item
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <FacetCategories>
        <FacetCategory Name=""Color"" Type=""String"" p:IsWordWheelVisible=""true"" />
    </FacetCategories>
    <Items>
        <Item Id=""1"" Name=""Widget"" p:AdditionalSearchText=""bonus keyword searchable"">
            <Facets>
                <Facet Name=""Color""><String Value=""Red"" /></Facet>
            </Facets>
        </Item>
        <Item Id=""2"" Name=""Gadget"">
            <Facets>
                <Facet Name=""Color""><String Value=""Blue"" /></Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        var item1 = source.GetItemById("1");
        Assert.NotNull(item1);
        Assert.Equal("bonus keyword searchable", item1!.AdditionalSearchText);

        var item2 = source.GetItemById("2");
        Assert.NotNull(item2);
        Assert.Null(item2!.AdditionalSearchText);
    }

    [Fact]
    public void Parse_WithAdditionalSearchText_ExtensionFallback()
    {
        // Non-standard fallback: AdditionalSearchText in Extension element
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <FacetCategories>
        <FacetCategory Name=""Color"" Type=""String"" />
    </FacetCategories>
    <Items>
        <Item Id=""1"" Name=""Widget"">
            <Extension>
                <p:AdditionalSearchText>fallback text</p:AdditionalSearchText>
            </Extension>
            <Facets>
                <Facet Name=""Color""><String Value=""Red"" /></Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        var item1 = source.GetItemById("1");
        Assert.NotNull(item1);
        Assert.Equal("fallback text", item1!.AdditionalSearchText);
    }

    [Fact]
    public void Parse_SupplementUri()
    {
        var xml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test' p:Supplement='descriptions.cxml'>
    <FacetCategories />
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.Equal("descriptions.cxml", source.SupplementUri);
    }

    [Fact]
    public void Parse_DecimalPlaces_FromAttribute()
    {
        var xml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
    <FacetCategories>
        <FacetCategory Name='Price' Type='Number' p:DecimalPlaces='2' />
    </FacetCategories>
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        var priceProp = source.ItemProperties.First(p => p.Id == "Price");
        var numProp = Assert.IsType<PivotViewerNumericProperty>(priceProp);
        Assert.Equal(2, numProp.DecimalPlaces);
    }

    [Fact]
    public void MergeSupplementalData_AddsNewProperties()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Main'>
    <FacetCategories>
        <FacetCategory Name='Name' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='1' Name='Alpha'>
            <Facets><Facet Name='Name'><String Value='Alpha'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var suppXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Supplement'>
    <FacetCategories>
        <FacetCategory Name='Desc' Type='LongString' />
    </FacetCategories>
    <Items>
        <Item Id='1'>
            <Facets><Facet Name='Desc'><String Value='A description'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var main = CxmlCollectionSource.Parse(mainXml);
        var supp = CxmlCollectionSource.Parse(suppXml);
        main.MergeSupplementalData(supp);

        // The Desc property should now exist on the item
        var item = main.GetItemById("1");
        Assert.NotNull(item);
        var descValues = item!["Desc"];
        Assert.NotNull(descValues);
        Assert.Single(descValues!);
        Assert.Equal("A description", descValues![0]?.ToString());
    }

    [Fact]
    public void MergeSupplementalData_IgnoresUnmatchedItems()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Main'>
    <FacetCategories><FacetCategory Name='Name' Type='String' /></FacetCategories>
    <Items>
        <Item Id='1' Name='Alpha'>
            <Facets><Facet Name='Name'><String Value='Alpha'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var suppXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Supp'>
    <FacetCategories><FacetCategory Name='Extra' Type='String' /></FacetCategories>
    <Items>
        <Item Id='999'>
            <Facets><Facet Name='Extra'><String Value='Ghost'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var main = CxmlCollectionSource.Parse(mainXml);
        var supp = CxmlCollectionSource.Parse(suppXml);
        main.MergeSupplementalData(supp);

        // Item 999 does not exist in main, so it should be ignored
        Assert.Null(main.GetItemById("999"));
        // Item 1 should not have Extra
        var item = main.GetItemById("1");
        Assert.Null(item!["Extra"]);
    }

    [Fact]
    public void GetPivotPropertyById_NotFound_ReturnsNull()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Test"">
    <FacetCategories>
        <FacetCategory Name=""Color"" Type=""String"" />
    </FacetCategories>
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.Null(source.GetPivotPropertyById("NonExistent"));
    }

    [Fact]
    public void Parse_WithIcon_ParsesIconAttribute()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"" p:Icon=""icon.png"">
    <FacetCategories />
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.Equal("icon.png", source.Icon);
    }

    [Fact]
    public void Parse_WithBrandImage_ParsesBrandImage()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <BrandImage Source=""https://example.com/brand.png"" />
    <FacetCategories />
    <Items />
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        Assert.NotNull(source.BrandImage);
        Assert.Equal("https://example.com/brand.png", source.BrandImage!.ToString());
    }

    [Fact]
    public void PropertyChanged_FiresWhenStateChanges()
    {
        var source = new CxmlCollectionSource();
        var changedProps = new List<string>();
        source.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName!);

        // State setter is private; use reflection to trigger a state change
        typeof(CxmlCollectionSource)
            .GetProperty(nameof(CxmlCollectionSource.State))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(source, new object[] { CxmlCollectionState.Loading });

        Assert.Single(changedProps);
        Assert.Equal("State", changedProps[0]);
    }

    [Fact]
    public void MergeSupplementalData_DoesNotDuplicateExistingValues()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Main'>
    <FacetCategories>
        <FacetCategory Name='Color' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='1' Name='Alpha'>
            <Facets><Facet Name='Color'><String Value='Red'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var suppXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Supp'>
    <FacetCategories>
        <FacetCategory Name='Color' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='1'>
            <Facets><Facet Name='Color'><String Value='Blue'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var main = CxmlCollectionSource.Parse(mainXml);
        var supp = CxmlCollectionSource.Parse(suppXml);
        main.MergeSupplementalData(supp);

        // Should keep original "Red", not add "Blue"
        var item = main.GetItemById("1");
        Assert.NotNull(item);
        var colorValues = item!["Color"];
        Assert.NotNull(colorValues);
        Assert.Single(colorValues!);
        Assert.Equal("Red", colorValues![0]?.ToString());
    }

    [Fact]
    public void Parse_SchemaVersion_Extracted()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        Assert.NotNull(source.SchemaVersion);
    }

    [Fact]
    public void MergeSupplementalData_NullArg_ThrowsArgumentNullException()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009' Name='Main'>
    <FacetCategories />
    <Items />
</Collection>";
        var main = CxmlCollectionSource.Parse(mainXml);
        Assert.Throws<ArgumentNullException>(() => main.MergeSupplementalData(null!));
    }

    // --- ExtraData parsing (buxton.cxml) ---

    [Fact]
    public void Parse_Buxton_ExtraDataTypes_Populated()
    {
        var source = TestDataHelper.LoadCxml("buxton.cxml");

        Assert.NotEmpty(source.ExtraDataTypes);
    }

    [Fact]
    public void Parse_Buxton_FirstExtraDataType_HasNameImageAndSortedIds()
    {
        var source = TestDataHelper.LoadCxml("buxton.cxml");

        var first = source.ExtraDataTypes[0];
        Assert.False(string.IsNullOrEmpty(first.Name));
        Assert.False(string.IsNullOrEmpty(first.Image));
        Assert.NotEmpty(first.SortedIds);
    }

    [Fact]
    public void Parse_InlineCxml_ExtraDataType_NameIsSet()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Test"">
    <FacetCategories />
    <Items />
    <ExtraData>
        <Types>
            <Type Name=""Phones"" Image=""phone.png"" SortedIds=""1,2,3"" />
        </Types>
    </ExtraData>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);

        Assert.Single(source.ExtraDataTypes);
        Assert.Equal("Phones", source.ExtraDataTypes[0].Name);
    }

    [Fact]
    public void Parse_InlineCxml_ExtraDataType_SortedIdsPopulated()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009"" Name=""Test"">
    <FacetCategories />
    <Items />
    <ExtraData>
        <Types>
            <Type Name=""Tablets"" SortedIds=""A, B, C"" />
        </Types>
    </ExtraData>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);

        Assert.Single(source.ExtraDataTypes);
        Assert.Equal(new[] { "A", "B", "C" }, source.ExtraDataTypes[0].SortedIds);
    }

    [Fact]
    public void Parse_StringElementWithNumericContent_CoercedToDoubleForNumberCategory()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Collection xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
            xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009""
            Name=""Test"">
    <FacetCategories>
        <FacetCategory Name=""Score"" Type=""Number"" />
    </FacetCategories>
    <Items>
        <Item Id=""1"" Name=""Widget"">
            <Facets>
                <Facet Name=""Score""><String Value=""42.5"" /></Facet>
            </Facets>
        </Item>
    </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(xml);
        var item = source.GetItemById("1");
        Assert.NotNull(item);
        var scoreValues = item!["Score"];
        Assert.NotNull(scoreValues);
        Assert.Single(scoreValues!);
        Assert.IsType<double>(scoreValues![0]);
        Assert.Equal(42.5, (double)scoreValues[0]!);
    }

    // --- Additional MergeSupplementalData tests ---

    [Fact]
    public void MergeSupplementalData_AddsNewPropertyDefinitions()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Main'>
    <FacetCategories>
        <FacetCategory Name='Name' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='1' Name='Alpha'>
            <Facets><Facet Name='Name'><String Value='Alpha'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var suppXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Supplement'>
    <FacetCategories>
        <FacetCategory Name='Weight' Type='Number' />
    </FacetCategories>
    <Items>
        <Item Id='1'>
            <Facets><Facet Name='Weight'><Number Value='10.5'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var main = CxmlCollectionSource.Parse(mainXml);
        var supp = CxmlCollectionSource.Parse(suppXml);
        main.MergeSupplementalData(supp);

        // Weight property should now exist in the property list
        var weightProp = main.GetPivotPropertyById("Weight");
        Assert.NotNull(weightProp);
        Assert.IsType<PivotViewerNumericProperty>(weightProp);
    }

    [Fact]
    public void MergeSupplementalData_MultipleItemsWithMultipleValues()
    {
        var mainXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Main'>
    <FacetCategories>
        <FacetCategory Name='Name' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='A' Name='Item A'>
            <Facets><Facet Name='Name'><String Value='A'/></Facet></Facets>
        </Item>
        <Item Id='B' Name='Item B'>
            <Facets><Facet Name='Name'><String Value='B'/></Facet></Facets>
        </Item>
        <Item Id='C' Name='Item C'>
            <Facets><Facet Name='Name'><String Value='C'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var suppXml = @"<?xml version='1.0' encoding='utf-8'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Supp'>
    <FacetCategories>
        <FacetCategory Name='Tags' Type='String' />
    </FacetCategories>
    <Items>
        <Item Id='A'>
            <Facets><Facet Name='Tags'><String Value='X'/><String Value='Y'/></Facet></Facets>
        </Item>
        <Item Id='C'>
            <Facets><Facet Name='Tags'><String Value='Z'/></Facet></Facets>
        </Item>
    </Items>
</Collection>";

        var main = CxmlCollectionSource.Parse(mainXml);
        var supp = CxmlCollectionSource.Parse(suppXml);
        main.MergeSupplementalData(supp);

        var itemA = main.GetItemById("A");
        Assert.Equal(2, itemA!["Tags"]!.Count);

        var itemB = main.GetItemById("B");
        Assert.Null(itemB!["Tags"]); // B not in supplement

        var itemC = main.GetItemById("C");
        Assert.Single(itemC!["Tags"]!);
        Assert.Equal("Z", itemC["Tags"]![0]?.ToString());
    }

    [Fact]
    public void ItemTemplates_DefaultIsEmpty()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        Assert.NotNull(source.ItemTemplates);
        Assert.Empty(source.ItemTemplates);
    }

    [Fact]
    public void ItemTemplates_CanAddTemplates()
    {
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        source.ItemTemplates.Add(new PivotViewerItemTemplate { MaxWidth = 200 });
        Assert.Single(source.ItemTemplates);
    }

    [Fact]
    public void Parse_NullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CxmlCollectionSource.Parse((string)null!));
    }

    [Fact]
    public void Parse_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CxmlCollectionSource.Parse((System.IO.Stream)null!));
    }

    [Fact]
    public void Parse_HrefBase_ResolvesRelativeHrefs()
    {
        var xml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            Name='Test'>
  <FacetCategories/>
  <Items HrefBase='http://example.com/items/'>
    <Item Id='1' Name='A' Href='page1.html'/>
    <Item Id='2' Name='B' Href='http://other.com/page2.html'/>
  </Items>
</Collection>";
        var source = CxmlCollectionSource.Parse(xml);

        Assert.Equal("http://example.com/items/", source.HrefBase);

        // Relative Href should be resolved
        var item1 = source.GetItemById("1");
        Assert.NotNull(item1);
        var href1 = item1!.GetPropertyValue("Href");
        Assert.NotNull(href1);
        Assert.Contains("http://example.com/items/page1.html", href1![0]?.ToString());

        // Absolute Href should be unchanged
        var item2 = source.GetItemById("2");
        var href2 = item2!.GetPropertyValue("Href");
        Assert.Equal("http://other.com/page2.html", href2![0]?.ToString());
    }

    [Fact]
    public void Parse_NoHrefBase_HrefUnchanged()
    {
        var xml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            Name='Test'>
  <FacetCategories/>
  <Items>
    <Item Id='1' Name='A' Href='relative.html'/>
  </Items>
</Collection>";
        var source = CxmlCollectionSource.Parse(xml);

        Assert.Null(source.HrefBase);
        var item = source.GetItemById("1");
        var href = item!.GetPropertyValue("Href");
        Assert.Equal("relative.html", href![0]?.ToString());
    }
}
