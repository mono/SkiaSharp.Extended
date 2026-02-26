using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Edge case and corner case tests for the CXML parser.
/// </summary>
public class CxmlEdgeCaseTest
{
    [Fact]
    public void Parse_EmptyFacets_ItemHasNoValues()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Color' Type='String' p:IsFilterVisible='true'/>
  </FacetCategories>
  <Items>
    <Item Id='1' Name='No Facets'>
      <Facets/>
    </Item>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        Assert.Single(source.Items);
        var item = source.Items.First();
        Assert.Null(item["Color"]);
    }

    [Fact]
    public void Parse_MultiValueFacet_AllValuesPresent()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Tags' Type='String' p:IsFilterVisible='true'/>
  </FacetCategories>
  <Items>
    <Item Id='1' Name='Multi'>
      <Facets>
        <Facet Name='Tags'>
          <String Value='red'/>
          <String Value='fast'/>
          <String Value='expensive'/>
        </Facet>
      </Facets>
    </Item>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        var item = source.Items.First();
        var tags = item["Tags"];
        Assert.NotNull(tags);
        Assert.Equal(3, tags!.Count);
    }

    [Fact]
    public void Parse_LongString_HasWrappingTextFlag()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Description' Type='LongString' p:IsMetaDataVisible='true'/>
  </FacetCategories>
  <Items>
    <Item Id='1' Name='With Description'>
      <Facets>
        <Facet Name='Description'>
          <String Value='This is a long description that should have wrapping text.'/>
        </Facet>
      </Facets>
    </Item>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        var prop = source.ItemProperties.First(p => p.Id == "Description");
        Assert.Equal(PivotViewerPropertyType.Text, prop.PropertyType);
        Assert.True(prop.Options.HasFlag(PivotViewerPropertyOptions.WrappingText));
    }

    [Fact]
    public void Parse_NumberFormat_Preserved()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Weight' Type='Number' Format='#.0 lbs' p:IsFilterVisible='true'/>
  </FacetCategories>
  <Items>
    <Item Id='1' Name='Item'>
      <Facets>
        <Facet Name='Weight'><Number Value='3.14'/></Facet>
      </Facets>
    </Item>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        var prop = source.ItemProperties.First(p => p.Id == "Weight");
        Assert.Equal("#.0 lbs", prop.Format);
    }

    [Fact]
    public void Parse_DateTimeFacet_ParsesCorrectly()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Born' Type='DateTime' p:IsFilterVisible='true'/>
  </FacetCategories>
  <Items>
    <Item Id='1' Name='Person'>
      <Facets>
        <Facet Name='Born'><DateTime Value='2010-06-15T00:00:00'/></Facet>
      </Facets>
    </Item>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        var item = source.Items.First();
        var born = item["Born"];
        Assert.NotNull(born);
        Assert.IsType<DateTime>(born![0]);
        Assert.Equal(2010, ((DateTime)born[0]!).Year);
    }

    [Fact]
    public void Parse_LinkFacet_ParsesUriAndText()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Website' Type='Link'/>
  </FacetCategories>
  <Items>
    <Item Id='1' Name='Item'>
      <Facets>
        <Facet Name='Website'><Link Href='https://example.com' Name='Example'/></Facet>
      </Facets>
    </Item>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        var item = source.Items.First();
        var link = item["Website"];
        Assert.NotNull(link);
        Assert.IsType<PivotViewerHyperlink>(link![0]);
        var hyperlink = (PivotViewerHyperlink)link[0]!;
        Assert.Equal("Example", hyperlink.Text);
        Assert.Equal("https://example.com", hyperlink.Uri.ToString().TrimEnd('/'));
    }

    [Fact]
    public void Parse_Copyright_Extracted()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <p:Copyright Name='Test Corp' Href='https://test.com'/>
  <FacetCategories/>
  <Items/>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        Assert.NotNull(source.Copyright);
        Assert.Equal("Test Corp", source.Copyright!.Text);
    }

    [Fact]
    public void Parse_NoFacetCategories_EmptyProperties()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Empty'>
  <FacetCategories/>
  <Items/>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        Assert.Empty(source.ItemProperties);
        Assert.Empty(source.Items);
    }

    [Fact]
    public void Parse_ItemWithImg_AttributePreserved()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories/>
  <Items ImgBase='collection.dzc'>
    <Item Id='1' Img='#5' Name='Item 5'/>
    <Item Id='2' Img='#10' Name='Item 10'/>
  </Items>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        Assert.Equal(2, source.Items.Count());
    }

    [Fact]
    public void Parse_CollectionName_Extracted()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='My Collection'>
  <FacetCategories/>
  <Items/>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        Assert.Equal("My Collection", source.Name);
    }

    [Fact]
    public void Parse_FilterOptions_MappedCorrectly()
    {
        var cxml = @"<?xml version='1.0'?>
<Collection xmlns='http://schemas.microsoft.com/collection/metadata/2009'
            xmlns:p='http://schemas.microsoft.com/livelabs/pivot/collection/2009'
            Name='Test'>
  <FacetCategories>
    <FacetCategory Name='Visible' Type='String' p:IsFilterVisible='true' p:IsWordWheelVisible='true'/>
    <FacetCategory Name='Hidden' Type='String' p:IsFilterVisible='false'/>
  </FacetCategories>
  <Items/>
</Collection>";

        var source = CxmlCollectionSource.Parse(cxml);
        var visible = source.ItemProperties.First(p => p.Id == "Visible");
        Assert.True(visible.Options.HasFlag(PivotViewerPropertyOptions.CanFilter));
        Assert.True(visible.Options.HasFlag(PivotViewerPropertyOptions.CanSearchText));

        var hidden = source.ItemProperties.First(p => p.Id == "Hidden");
        Assert.False(hidden.Options.HasFlag(PivotViewerPropertyOptions.CanFilter));
    }
}
