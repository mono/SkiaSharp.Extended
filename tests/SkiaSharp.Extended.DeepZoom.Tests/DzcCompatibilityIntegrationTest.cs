using SkiaSharp.Extended;

namespace SkiaSharp.Extended.DeepZoom.Tests;

/// <summary>
/// Integration tests that cross-validate DZC collections against known CXML item counts,
/// verify sub-image structure, and ensure DZC/DZI consistency.
/// </summary>
public class DzcCompatibilityIntegrationTest
{
    /// <summary>
    /// Verifies that DZC sub-image counts match the expected CXML item counts.
    /// This cross-validates that the DZC and CXML files are in sync.
    /// </summary>
    [Theory]
    [InlineData("conceptcars.dzc", 298)]
    [InlineData("collection-dz.dzc", 50)]
    [InlineData("nigeria_images.dzc", 31)]
    [InlineData("ski_resorts.dzc", 5)]
    [InlineData("stockport_images.dzc", 1)]
    [InlineData("area.dzc", 1)]
    [InlineData("geometry.dzc", 1)]
    public void Parse_SubImageCount_MatchesExpectedCxmlItemCount(string file, int expectedCount)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        Assert.Equal(expectedCount, dzc.Items.Count);
    }

    /// <summary>
    /// Verifies sub-image IDs form a valid zero-based sequence matching the DZC item order.
    /// </summary>
    [Theory]
    [InlineData("conceptcars.dzc")]
    [InlineData("collection-dz.dzc")]
    [InlineData("nigeria_images.dzc")]
    [InlineData("ski_resorts.dzc")]
    public void Parse_SubImageIds_AreZeroBasedSequential(string file)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        var ids = dzc.Items.Select(i => i.Id).OrderBy(i => i).ToList();
        for (int i = 0; i < ids.Count; i++)
        {
            Assert.Equal(i, ids[i]);
        }
    }

    /// <summary>
    /// Verifies that NextItemId is consistent with the actual item count.
    /// </summary>
    [Theory]
    [InlineData("conceptcars.dzc", 298)]
    [InlineData("collection-dz.dzc", 50)]
    public void Parse_NextItemId_IsConsistentWithCount(string file, int expectedNextId)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        Assert.Equal(expectedNextId, dzc.NextItemId);
    }

    /// <summary>
    /// Verifies DZI source paths in DZC items point to valid .dzi file paths.
    /// </summary>
    [Theory]
    [InlineData("conceptcars.dzc")]
    [InlineData("collection-dz.dzc")]
    public void Parse_SubImageSources_AreValidDziPaths(string file)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        foreach (var item in dzc.Items)
        {
            Assert.NotNull(item.Source);
            Assert.EndsWith(".dzi", item.Source);
            // Source should not contain directory separators at root level
            Assert.DoesNotContain("/", item.Source);
            Assert.DoesNotContain("\\", item.Source);
        }
    }

    /// <summary>
    /// Verifies that Morton indices (N values) are unique within each DZC.
    /// </summary>
    [Theory]
    [InlineData("conceptcars.dzc")]
    [InlineData("collection-dz.dzc")]
    [InlineData("nigeria_images.dzc")]
    public void Parse_MortonIndices_AreUnique(string file)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        var mortons = dzc.Items.Select(i => i.MortonIndex).ToList();
        Assert.Equal(mortons.Count, mortons.Distinct().Count());
    }

    /// <summary>
    /// Verifies composite tile URL generation for a real DZC.
    /// </summary>
    [Fact]
    public void ConceptCars_CompositeTileUrl_FormatsCorrectly()
    {
        using var stream = TestDataHelper.GetStream("conceptcars.dzc");
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        Assert.Equal("jpg", dzc.Format);
        Assert.Equal("5/2_3.jpg", dzc.GetCompositeTileUrl(5, 2, 3));
        Assert.Equal("0/0_0.jpg", dzc.GetCompositeTileUrl(0, 0, 0));
    }

    /// <summary>
    /// Verifies the Morton grid size is reasonable for each collection.
    /// </summary>
    [Theory]
    [InlineData("conceptcars.dzc", 298)]
    [InlineData("collection-dz.dzc", 50)]
    [InlineData("nigeria_images.dzc", 31)]
    public void Parse_MortonGridSize_IsReasonable(string file, int itemCount)
    {
        using var stream = TestDataHelper.GetStream(file);
        var dzc = SKDeepZoomCollectionSource.Parse(stream);

        int gridSize = dzc.GetMortonGridSize();
        // Grid must be large enough to contain all items (gridSize^2 >= itemCount)
        Assert.True(gridSize * gridSize >= itemCount,
            $"{file}: Grid {gridSize}x{gridSize}={gridSize * gridSize} too small for {itemCount} items");
        // But not excessively large (should be next power of 2)
        Assert.True(gridSize * gridSize < itemCount * 4 + 4,
            $"{file}: Grid {gridSize}x{gridSize} is excessively large for {itemCount} items");
    }
}
