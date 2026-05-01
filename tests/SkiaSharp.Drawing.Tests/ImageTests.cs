using System.Drawing;
using System.Drawing.Imaging;

namespace SkiaSharp.Drawing.Tests;

public class ImageTests
{
    // --- FromFile ---

    [Fact]
    public void FromFile_ReturnsImage_WithCorrectDimensions()
    {
        var path = TestHelpers.CreateTestImageFile(20, 15);
        try
        {
            using var img = Image.FromFile(path);
            Assert.Equal(20, img.Width);
            Assert.Equal(15, img.Height);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FromFile_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Image.FromFile(null!));
    }

    [Fact]
    public void FromFile_WithEmbeddedColorManagement()
    {
        var path = TestHelpers.CreateTestImageFile(10, 10);
        try
        {
            using var img = Image.FromFile(path, true);
            Assert.Equal(10, img.Width);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // --- FromStream ---

    [Fact]
    public void FromStream_ReturnsImage_WithCorrectDimensions()
    {
        var path = TestHelpers.CreateTestImageFile(12, 8);
        try
        {
            using var stream = File.OpenRead(path);
            using var img = Image.FromStream(stream);
            Assert.Equal(12, img.Width);
            Assert.Equal(8, img.Height);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FromStream_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Image.FromStream(null!));
    }

    [Fact]
    public void FromStream_WithUseEmbeddedColorManagement()
    {
        var path = TestHelpers.CreateTestImageFile(10, 10);
        try
        {
            using var stream = File.OpenRead(path);
            using var img = Image.FromStream(stream, true);
            Assert.Equal(10, img.Width);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FromStream_ThreeParam_Overload()
    {
        var path = TestHelpers.CreateTestImageFile(10, 10);
        try
        {
            using var stream = File.OpenRead(path);
            using var img = Image.FromStream(stream, true, true);
            Assert.Equal(10, img.Width);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // --- Save / Reload Roundtrip ---

    [Fact]
    public void Save_Reload_DimensionsMatch()
    {
        var path = Path.Combine(Path.GetTempPath(), $"img_test_{Guid.NewGuid()}.png");
        try
        {
            using var bmp = TestHelpers.CreateSolidBitmap(30, 25, Color.Purple);
            bmp.Save(path);

            using var reloaded = Image.FromFile(path);
            Assert.Equal(30, reloaded.Width);
            Assert.Equal(25, reloaded.Height);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_ToStream_WithImageFormat()
    {
        using var bmp = TestHelpers.CreateSolidBitmap(10, 10, Color.Coral);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        Assert.True(ms.Length > 0);
    }

    // --- Properties ---

    [Fact]
    public void PixelFormat_ReturnsExpectedFormat()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(PixelFormat.Format32bppArgb, bmp.PixelFormat);
    }

    [Fact]
    public void RawFormat_FromFile_MatchesPng()
    {
        var path = TestHelpers.CreateTestImageFile(10, 10);
        try
        {
            using var img = Image.FromFile(path);
            Assert.Equal(ImageFormat.Png.Guid, img.RawFormat.Guid);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void HorizontalResolution_Default96()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(96f, ((Image)bmp).HorizontalResolution);
    }

    [Fact]
    public void VerticalResolution_Default96()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(96f, ((Image)bmp).VerticalResolution);
    }

    [Fact]
    public void Size_ReturnsCorrectValue()
    {
        using var bmp = new Bitmap(40, 50);
        Assert.Equal(new Size(40, 50), ((Image)bmp).Size);
    }

    [Fact]
    public void PhysicalDimension_ReturnsCorrectValue()
    {
        using var bmp = new Bitmap(40, 50);
        var dim = ((Image)bmp).PhysicalDimension;
        Assert.Equal(40f, dim.Width);
        Assert.Equal(50f, dim.Height);
    }

    [Fact]
    public void Tag_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        Image img = bmp;
        img.Tag = "test-tag";
        Assert.Equal("test-tag", img.Tag);
    }

    [Fact]
    public void Flags_HasExpectedFlags()
    {
        using var bmp = new Bitmap(10, 10);
        var flags = ((Image)bmp).Flags;
        Assert.NotEqual(0, flags);
    }

    [Fact]
    public void FrameDimensionsList_ReturnsNonEmpty()
    {
        using var bmp = new Bitmap(10, 10);
        var dims = ((Image)bmp).FrameDimensionsList;
        Assert.NotEmpty(dims);
    }

    [Fact]
    public void GetFrameCount_Returns1()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(1, ((Image)bmp).GetFrameCount(FrameDimension.Page));
    }

    [Fact]
    public void PropertyIdList_IsEmpty()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Empty(((Image)bmp).PropertyIdList);
    }

    [Fact]
    public void PropertyItems_IsEmpty()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Empty(((Image)bmp).PropertyItems);
    }

    [Fact]
    public void Palette_GetReturnsNonNull()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.NotNull(((Image)bmp).Palette);
    }

    // --- Clone ---

    [Fact]
    public void Clone_CreatesExactCopy()
    {
        using var bmp = new Bitmap(15, 15);
        bmp.SetPixel(7, 7, Color.Red);
        using var clone = (Image)((Image)bmp).Clone();
        Assert.Equal(15, clone.Width);
        Assert.Equal(15, clone.Height);
    }

    [Fact]
    public void Clone_IsIndependent()
    {
        using var original = new Bitmap(10, 10);
        original.SetPixel(0, 0, Color.Green);
        using var clone = (Bitmap)((Image)original).Clone();
        clone.SetPixel(0, 0, Color.Red);
        TestHelpers.AssertPixelColor(original, 0, 0, Color.Green);
    }

    // --- RotateFlip ---

    [Fact]
    public void RotateFlip_Rotate90_SwapsDimensions()
    {
        using var bmp = new Bitmap(20, 10);
        ((Image)bmp).RotateFlip(RotateFlipType.Rotate90FlipNone);
        Assert.Equal(10, bmp.Width);
        Assert.Equal(20, bmp.Height);
    }

    [Fact]
    public void RotateFlip_Rotate180_KeepsDimensions()
    {
        using var bmp = new Bitmap(20, 10);
        ((Image)bmp).RotateFlip(RotateFlipType.Rotate180FlipNone);
        Assert.Equal(20, bmp.Width);
        Assert.Equal(10, bmp.Height);
    }

    [Fact]
    public void RotateFlip_Rotate270_SwapsDimensions()
    {
        using var bmp = new Bitmap(30, 10);
        ((Image)bmp).RotateFlip(RotateFlipType.Rotate270FlipNone);
        Assert.Equal(10, bmp.Width);
        Assert.Equal(30, bmp.Height);
    }

    [Fact]
    public void RotateFlip_FlipX_KeepsDimensions()
    {
        using var bmp = new Bitmap(20, 10);
        ((Image)bmp).RotateFlip(RotateFlipType.RotateNoneFlipX);
        Assert.Equal(20, bmp.Width);
        Assert.Equal(10, bmp.Height);
    }

    // --- GetThumbnailImage ---

    [Fact]
    public void GetThumbnailImage_ReturnsSmallerImage()
    {
        using var bmp = TestHelpers.CreateSolidBitmap(100, 100, Color.Blue);
        using var thumb = bmp.GetThumbnailImage(20, 20, null, IntPtr.Zero);
        Assert.Equal(20, thumb.Width);
        Assert.Equal(20, thumb.Height);
    }

    [Fact]
    public void GetThumbnailImage_ZeroWidth_ThrowsArgumentOutOfRange()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => bmp.GetThumbnailImage(0, 10, null, IntPtr.Zero));
    }

    // --- GetBounds ---

    [Fact]
    public void GetBounds_ReturnsCorrectRect()
    {
        using var bmp = new Bitmap(30, 20);
        var unit = GraphicsUnit.Pixel;
        var bounds = ((Image)bmp).GetBounds(ref unit);
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(30, bounds.Width);
        Assert.Equal(20, bounds.Height);
        Assert.Equal(GraphicsUnit.Pixel, unit);
    }

    // --- Static helpers ---

    [Fact]
    public void GetPixelFormatSize_32bppArgb_Returns32()
    {
        Assert.Equal(32, Image.GetPixelFormatSize(PixelFormat.Format32bppArgb));
    }

    [Fact]
    public void GetPixelFormatSize_24bppRgb_Returns24()
    {
        Assert.Equal(24, Image.GetPixelFormatSize(PixelFormat.Format24bppRgb));
    }

    [Fact]
    public void IsAlphaPixelFormat_32bppArgb_ReturnsTrue()
    {
        Assert.True(Image.IsAlphaPixelFormat(PixelFormat.Format32bppArgb));
    }

    [Fact]
    public void IsAlphaPixelFormat_24bppRgb_ReturnsFalse()
    {
        Assert.False(Image.IsAlphaPixelFormat(PixelFormat.Format24bppRgb));
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_AccessWidth_ThrowsObjectDisposedException()
    {
        var bmp = new Bitmap(10, 10);
        ((Image)bmp).Dispose();
        Assert.Throws<ObjectDisposedException>(() => bmp.Width);
    }

    [Fact]
    public void Dispose_Clone_ThrowsObjectDisposedException()
    {
        var bmp = new Bitmap(10, 10);
        ((Image)bmp).Dispose();
        Assert.Throws<ObjectDisposedException>(() => ((Image)bmp).Clone());
    }

    // --- SelectActiveFrame ---

    [Fact]
    public void SelectActiveFrame_Frame0_Returns0()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(0, ((Image)bmp).SelectActiveFrame(FrameDimension.Page, 0));
    }

    [Fact]
    public void SelectActiveFrame_NonZero_ThrowsArgument()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Throws<ArgumentException>(() => ((Image)bmp).SelectActiveFrame(FrameDimension.Page, 1));
    }
}
