using SkiaSharp.Extended;
using Xunit;

namespace SkiaSharp.Extended.ImagePyramid.Tests;

/// <summary>Tests targeting specific uncovered branches for higher branch coverage.</summary>
public class BranchCoverageTest
{
    // --- SKImagePyramidTileId branch coverage ---

    [Fact]
    public void TileId_Equals_WithNull_ReturnsFalse()
    {
        var id = new SKImagePyramidTileId(1, 2, 3);
        Assert.False(id.Equals(null));
    }

    [Fact]
    public void TileId_Equals_WithDifferentType_ReturnsFalse()
    {
        var id = new SKImagePyramidTileId(1, 2, 3);
        Assert.False(id.Equals("not a tile id"));
    }

    [Fact]
    public void TileId_Equals_WithBoxedEqual_ReturnsTrue()
    {
        var id1 = new SKImagePyramidTileId(1, 2, 3);
        object id2 = new SKImagePyramidTileId(1, 2, 3);
        Assert.True(id1.Equals(id2));
    }

    [Fact]
    public void TileId_Equals_DifferentLevel_ReturnsFalse()
    {
        var id1 = new SKImagePyramidTileId(1, 2, 3);
        var id2 = new SKImagePyramidTileId(2, 2, 3);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TileId_Equals_DifferentCol_ReturnsFalse()
    {
        var id1 = new SKImagePyramidTileId(1, 2, 3);
        var id2 = new SKImagePyramidTileId(1, 3, 3);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TileId_Equals_DifferentRow_ReturnsFalse()
    {
        var id1 = new SKImagePyramidTileId(1, 2, 3);
        var id2 = new SKImagePyramidTileId(1, 2, 4);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TileId_Operators()
    {
        var id1 = new SKImagePyramidTileId(1, 2, 3);
        var id2 = new SKImagePyramidTileId(1, 2, 3);
        var id3 = new SKImagePyramidTileId(4, 5, 6);

        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.True(id1 != id3);
        Assert.False(id1 == id3);
    }

    // --- SKImagePyramidDziCollectionSource branch coverage ---

    [Fact]
    public void DzcTileSource_SmallCollection_MortonGrid()
    {
        // Test with 1 item (smallest possible)
        var xml = @"<?xml version='1.0' encoding='UTF-8'?>
<Collection xmlns='http://schemas.microsoft.com/deepzoom/2009'
    MaxLevel='8' TileSize='256' Format='jpg'>
    <Items>
        <I Id='0' N='0' Source='items/0.dzi'>
            <Size Width='800' Height='600'/>
            <Viewport Width='1.0' X='0' Y='0'/>
        </I>
    </Items>
</Collection>";
        var dzc = SKImagePyramidDziCollectionSource.Parse(xml);
        Assert.Equal(1, dzc.Items.Count);
    }

    [Fact]
    public void DzcTileSource_LargeCollection_MortonGrid()
    {
        // Test with 5+ items to exercise larger morton grid
        var items = "";
        for (int i = 0; i < 10; i++)
            items += $"<I Id='{i}' N='{i}' Source='items/{i}.dzi'><Size Width='800' Height='600'/><Viewport Width='0.5' X='{-i * 0.5}' Y='0'/></I>\n";

        var xml = $@"<?xml version='1.0' encoding='UTF-8'?>
<Collection xmlns='http://schemas.microsoft.com/deepzoom/2009'
    MaxLevel='8' TileSize='256' Format='jpg'>
    <Items>{items}</Items>
</Collection>";
        var dzc = SKImagePyramidDziCollectionSource.Parse(xml);
        Assert.Equal(10, dzc.Items.Count);
    }

    // --- SKImagePyramidTileRequest branch coverage ---

    [Fact]
    public void TileRequest_Equality()
    {
        var req1 = new SKImagePyramidTileRequest(new SKImagePyramidTileId(1, 2, 3), 1.0);
        var req2 = new SKImagePyramidTileRequest(new SKImagePyramidTileId(1, 2, 3), 1.0);
        var req3 = new SKImagePyramidTileRequest(new SKImagePyramidTileId(4, 5, 6), 2.0);

        Assert.Equal(req1, req2);
        Assert.NotEqual(req1, req3);
        Assert.Equal(req1.GetHashCode(), req2.GetHashCode());
    }

    [Fact]
    public void TileRequest_Equals_WithNull_ReturnsFalse()
    {
        var req = new SKImagePyramidTileRequest(new SKImagePyramidTileId(1, 2, 3), 1.0);
        Assert.False(req.Equals(null));
    }

    [Fact]
    public void TileRequest_Equals_WithDifferentType_ReturnsFalse()
    {
        var req = new SKImagePyramidTileRequest(new SKImagePyramidTileId(1, 2, 3), 1.0);
        Assert.False(req.Equals("not a request"));
    }

    // --- SKImagePyramidViewport edge cases ---

    [Fact]
    public void Viewport_SetControlSize_Zero()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 0;
        vp.ControlHeight = 0;
        // Min is clamped to 1
        Assert.Equal(1, vp.ControlWidth);
        Assert.Equal(1, vp.ControlHeight);
    }

    [Fact]
    public void Viewport_ZoomAboutLogicalPoint_AtOrigin()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 1.0;

        vp.ZoomAboutLogicalPoint(2.0, 0.0, 0.0);
        Assert.True(vp.ViewportWidth < 1.0, "Should be zoomed in");
    }

    [Fact]
    public void Viewport_ZoomAboutLogicalPoint_AtCenter()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 1.0;

        double centerX = 0.5;
        double centerY = 0.5;
        vp.ZoomAboutLogicalPoint(4.0, centerX, centerY);
        Assert.True(vp.ViewportWidth < 1.0);
    }

    [Fact]
    public void Viewport_PanByScreenDelta()
    {
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 800;
        vp.ControlHeight = 600;
        vp.AspectRatio = 1.0;

        double origX = vp.ViewportOriginX;
        vp.PanByScreenDelta(100, 50);
        Assert.NotEqual(origX, vp.ViewportOriginX);
    }

    // --- SKImagePyramidTileLayout edge cases ---

    [Fact]
    public void TileLayout_SmallViewport_FewTiles()
    {
        var layout = new SKImagePyramidTileLayout();
        var vp = new SKImagePyramidViewport();
        vp.ControlWidth = 100;
        vp.ControlHeight = 100;
        vp.AspectRatio = 1.0;

        var dziXml = @"<?xml version='1.0' encoding='UTF-8'?>
<Image xmlns='http://schemas.microsoft.com/deepzoom/2008' Format='jpg' Overlap='1' TileSize='256'>
    <Size Width='1000' Height='1000'/>
</Image>";
        var dzi = SKImagePyramidDziSource.Parse(dziXml);

        var requests = layout.GetVisibleTiles(dzi, vp);
        Assert.NotEmpty(requests);
    }

    // --- SKImagePyramidSubImage ---

    [Fact]
    public void ImagePyramidSubImage_ViewportOrigin_InvertedCoords()
    {
        var sub = new SKImagePyramidSubImage(0, 0, 4.0 / 3.0, "items/0.dzi");
        sub.ViewportOriginX = -0.5;
        sub.ViewportOriginY = -0.25;
        sub.ViewportWidth = 0.5;

        Assert.Equal(-0.5, sub.ViewportOriginX);
        Assert.Equal(-0.25, sub.ViewportOriginY);
        Assert.Equal(0.5, sub.ViewportWidth);
    }

    [Fact]
    public void ImagePyramidSubImage_Opacity_DefaultsToOne()
    {
        var sub = new SKImagePyramidSubImage(0, 0, 1.0, null);
        Assert.Equal(1.0, sub.Opacity);
    }

    [Fact]
    public void ImagePyramidSubImage_Opacity_Clamped()
    {
        var sub = new SKImagePyramidSubImage(0, 0, 1.0, null);
        sub.Opacity = 1.5;
        Assert.Equal(1.0, sub.Opacity);

        sub.Opacity = -0.5;
        Assert.Equal(0.0, sub.Opacity);
    }

    [Fact]
    public void ImagePyramidSubImage_ZIndex_DefaultsToZero()
    {
        var sub = new SKImagePyramidSubImage(0, 0, 1.0, null);
        Assert.Equal(0, sub.ZIndex);
    }

    [Fact]
    public void ImagePyramidSubImage_GetMosaicBounds()
    {
        var sub = new SKImagePyramidSubImage(0, 0, 2.0, null);
        sub.ViewportOriginX = -1.0;
        sub.ViewportOriginY = -0.5;
        sub.ViewportWidth = 2.0;

        var (x, y, w, h) = sub.GetMosaicBounds();
        Assert.Equal(0.5, x);
        Assert.Equal(0.25, y);
        Assert.Equal(0.5, w);
        Assert.Equal(0.25, h);
    }

    [Fact]
    public void ImagePyramidSubImage_ParentToLocal_RoundTrips()
    {
        var sub = new SKImagePyramidSubImage(0, 0, 1.0, null);
        sub.ViewportOriginX = -0.5;
        sub.ViewportOriginY = -0.25;
        sub.ViewportWidth = 0.5;

        var (localX, localY) = sub.ParentToLocal(1.0, 0.5);
        var (parentX, parentY) = sub.LocalToParent(localX, localY);

        Assert.InRange(parentX, 0.999, 1.001);
        Assert.InRange(parentY, 0.499, 0.501);
    }
}
