using System.Drawing.Imaging;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class ImageFormatTests
{
    [Fact]
    public void Png_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Png.Guid);
    }

    [Fact]
    public void Jpeg_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Jpeg.Guid);
    }

    [Fact]
    public void Bmp_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Bmp.Guid);
    }

    [Fact]
    public void Gif_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Gif.Guid);
    }

    [Fact]
    public void Icon_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cb5-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Icon.Guid);
    }

    [Fact]
    public void Tiff_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Tiff.Guid);
    }

    [Fact]
    public void Emf_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Emf.Guid);
    }

    [Fact]
    public void Wmf_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Wmf.Guid);
    }

    [Fact]
    public void Exif_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3cb2-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.Exif.Guid);
    }

    [Fact]
    public void MemoryBmp_HasCorrectGuid()
    {
        Assert.Equal(new Guid("{b96b3caa-0728-11d3-9d7b-0000f81ef32e}"), ImageFormat.MemoryBmp.Guid);
    }

    [Fact]
    public void AllStaticProperties_ReturnNonNull()
    {
        Assert.NotNull(ImageFormat.Png);
        Assert.NotNull(ImageFormat.Jpeg);
        Assert.NotNull(ImageFormat.Bmp);
        Assert.NotNull(ImageFormat.Gif);
        Assert.NotNull(ImageFormat.Icon);
        Assert.NotNull(ImageFormat.Tiff);
        Assert.NotNull(ImageFormat.Emf);
        Assert.NotNull(ImageFormat.Wmf);
        Assert.NotNull(ImageFormat.Exif);
        Assert.NotNull(ImageFormat.MemoryBmp);
    }

    [Fact]
    public void Equals_SameFormat_ReturnsTrue()
    {
        var a = ImageFormat.Png;
        var b = new ImageFormat(ImageFormat.Png.Guid);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentFormat_ReturnsFalse()
    {
        Assert.False(ImageFormat.Png.Equals(ImageFormat.Jpeg));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(ImageFormat.Png.Equals(null));
    }

    [Fact]
    public void Equals_NonImageFormat_ReturnsFalse()
    {
        Assert.False(ImageFormat.Png.Equals("not an image format"));
    }

    [Fact]
    public void GetHashCode_SameFormat_SameHash()
    {
        var a = ImageFormat.Png;
        var b = new ImageFormat(ImageFormat.Png.Guid);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentFormat_DifferentHash()
    {
        Assert.NotEqual(ImageFormat.Png.GetHashCode(), ImageFormat.Jpeg.GetHashCode());
    }

    [Fact]
    public void ToString_Png_ContainsPng()
    {
        Assert.Contains("Png", ImageFormat.Png.ToString());
    }

    [Fact]
    public void ToString_Jpeg_ContainsJpeg()
    {
        Assert.Contains("Jpeg", ImageFormat.Jpeg.ToString());
    }

    [Fact]
    public void ToString_Bmp_ContainsBmp()
    {
        Assert.Contains("Bmp", ImageFormat.Bmp.ToString());
    }

    [Fact]
    public void ToString_CustomGuid_ContainsGuid()
    {
        var guid = Guid.NewGuid();
        var format = new ImageFormat(guid);
        Assert.Contains(guid.ToString(), format.ToString());
    }

    [Fact]
    public void Constructor_StoresGuid()
    {
        var guid = Guid.NewGuid();
        var format = new ImageFormat(guid);
        Assert.Equal(guid, format.Guid);
    }
}
