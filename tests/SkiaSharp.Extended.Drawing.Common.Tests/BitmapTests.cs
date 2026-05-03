using System.Drawing;
using System.Drawing.Imaging;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class BitmapTests
{
    // --- Constructor Tests ---

    [Fact]
    public void Constructor_WidthHeight_CreatesCorrectSize()
    {
        using var bmp = new Bitmap(50, 30);
        Assert.Equal(50, bmp.Width);
        Assert.Equal(30, bmp.Height);
    }

    [Fact]
    public void Constructor_WidthHeight_PixelsAreTransparent()
    {
        using var bmp = new Bitmap(10, 10);
        var pixel = bmp.GetPixel(0, 0);
        Assert.Equal(0, pixel.A);
    }

    [Fact]
    public void Constructor_ZeroWidth_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentException>(() => new Bitmap(0, 10));
    }

    [Fact]
    public void Constructor_NegativeHeight_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentException>(() => new Bitmap(10, -1));
    }

    [Fact]
    public void Constructor_WidthHeightPixelFormat_Format32bppArgb()
    {
        using var bmp = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
        Assert.Equal(20, bmp.Width);
        Assert.Equal(20, bmp.Height);
    }

    [Fact]
    public void Constructor_WidthHeightPixelFormat_Format32bppPArgb()
    {
        using var bmp = new Bitmap(10, 10, PixelFormat.Format32bppPArgb);
        Assert.Equal(10, bmp.Width);
    }

    [Fact]
    public void Constructor_FromFile_LoadsPng()
    {
        var path = TestHelpers.CreateTestImageFile(15, 15);
        try
        {
            using var bmp = new Bitmap(path);
            Assert.Equal(15, bmp.Width);
            Assert.Equal(15, bmp.Height);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Constructor_FromFile_NullFilename_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Bitmap((string)null!));
    }

    [Fact]
    public void Constructor_FromStream_LoadsPng()
    {
        var path = TestHelpers.CreateTestImageFile(12, 8);
        try
        {
            using var stream = File.OpenRead(path);
            using var bmp = new Bitmap(stream);
            Assert.Equal(12, bmp.Width);
            Assert.Equal(8, bmp.Height);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Constructor_FromStream_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Bitmap((Stream)null!));
    }

    [Fact]
    public void Constructor_CopyImage_SameDimensions()
    {
        using var original = new Bitmap(20, 15);
        original.SetPixel(0, 0, Color.Red);
        using var copy = new Bitmap(original);
        Assert.Equal(20, copy.Width);
        Assert.Equal(15, copy.Height);
    }

    [Fact]
    public void Constructor_CopyImage_CopiesPixels()
    {
        using var original = new Bitmap(10, 10);
        original.SetPixel(5, 5, Color.Red);
        using var copy = new Bitmap(original);
        TestHelpers.AssertPixelColor(copy, 5, 5, Color.Red);
    }

    [Fact]
    public void Constructor_CopyImage_IsIndependent()
    {
        using var original = new Bitmap(10, 10);
        original.SetPixel(0, 0, Color.Red);
        using var copy = new Bitmap(original);
        copy.SetPixel(0, 0, Color.Blue);
        TestHelpers.AssertPixelColor(original, 0, 0, Color.Red);
    }

    [Fact]
    public void Constructor_ResizeImage_CorrectDimensions()
    {
        using var original = TestHelpers.CreateSolidBitmap(20, 20, Color.Green);
        using var resized = new Bitmap(original, 10, 10);
        Assert.Equal(10, resized.Width);
        Assert.Equal(10, resized.Height);
    }

    [Fact]
    public void Constructor_ResizeImage_ZeroWidth_ThrowsArgumentOutOfRange()
    {
        using var original = new Bitmap(10, 10);
        Assert.Throws<ArgumentException>(() => new Bitmap(original, 0, 5));
    }

    [Fact]
    public void Constructor_NullImage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Bitmap((Image)null!));
    }

    // --- GetPixel / SetPixel ---

    [Fact]
    public void SetPixel_Red_GetPixel_ReturnsRed()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(0, 0, Color.Red);
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.Red);
    }

    [Fact]
    public void SetPixel_White_GetPixel_ReturnsWhite()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(5, 5, Color.White);
        TestHelpers.AssertPixelColor(bmp, 5, 5, Color.White);
    }

    [Fact]
    public void SetPixel_Black_GetPixel_ReturnsBlack()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(3, 3, Color.Black);
        var pixel = bmp.GetPixel(3, 3);
        Assert.Equal(255, pixel.A);
        Assert.Equal(0, pixel.R);
        Assert.Equal(0, pixel.G);
        Assert.Equal(0, pixel.B);
    }

    [Fact]
    public void SetPixel_Transparent_GetPixel_ReturnsTransparent()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(0, 0, Color.FromArgb(0, 0, 0, 0));
        var pixel = bmp.GetPixel(0, 0);
        Assert.Equal(0, pixel.A);
    }

    [Fact]
    public void SetPixel_SemiTransparent_GetPixel_ReturnsCorrectAlpha()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(0, 0, Color.FromArgb(128, 255, 0, 0));
        var pixel = bmp.GetPixel(0, 0);
        Assert.True(Math.Abs(pixel.A - 128) <= 2);
    }

    [Fact]
    public void GetPixel_OutOfBoundsX_ThrowsArgumentOutOfRange()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Throws<ArgumentException>(() => bmp.GetPixel(10, 0));
    }

    [Fact]
    public void GetPixel_NegativeY_ThrowsArgumentOutOfRange()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Throws<ArgumentException>(() => bmp.GetPixel(0, -1));
    }

    [Fact]
    public void SetPixel_OutOfBoundsX_ThrowsArgumentOutOfRange()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Throws<ArgumentException>(() => bmp.SetPixel(10, 0, Color.Red));
    }

    // --- MakeTransparent ---

    [Fact]
    public void MakeTransparent_SpecificColor_MakesTransparent()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Red);
        bmp.MakeTransparent(Color.Red);
        var pixel = bmp.GetPixel(5, 5);
        Assert.Equal(0, pixel.A);
    }

    [Fact]
    public void MakeTransparent_LeavesOtherColorsIntact()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(0, 0, Color.Blue);
        bmp.SetPixel(1, 0, Color.Red);
        bmp.MakeTransparent(Color.Red);
        // Blue pixel should remain
        var blue = bmp.GetPixel(0, 0);
        Assert.Equal(255, blue.A);
    }

    // --- Clone ---

    [Fact]
    public void Clone_SubRegion_CorrectSize()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Green);
        using var cloned = bmp.Clone(new Rectangle(5, 5, 10, 10), PixelFormat.Format32bppArgb);
        Assert.Equal(10, cloned.Width);
        Assert.Equal(10, cloned.Height);
    }

    [Fact]
    public void Clone_SubRegion_CopiesPixels()
    {
        using var bmp = new Bitmap(20, 20);
        bmp.SetPixel(10, 10, Color.Magenta);
        using var cloned = bmp.Clone(new Rectangle(10, 10, 5, 5), PixelFormat.Format32bppArgb);
        TestHelpers.AssertPixelColor(cloned, 0, 0, Color.Magenta);
    }

    [Fact]
    public void Clone_OutOfBounds_ThrowsArgumentOutOfRange()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => bmp.Clone(new Rectangle(5, 5, 10, 10), PixelFormat.Format32bppArgb));
    }

    // --- SetResolution ---

    [Fact]
    public void SetResolution_UpdatesDpi()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetResolution(150f, 300f);
        Assert.Equal(150f, bmp.HorizontalResolution);
        Assert.Equal(300f, bmp.VerticalResolution);
    }

    // --- Save / Reload ---

    [Fact]
    public void Save_Png_ReloadMatchesDimensions()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid()}.png");
        try
        {
            using var bmp = new Bitmap(25, 30);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Teal);
            bmp.Save(path, ImageFormat.Png);

            using var reloaded = new Bitmap(path);
            Assert.Equal(25, reloaded.Width);
            Assert.Equal(30, reloaded.Height);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_Jpeg_StreamIsNonEmpty()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Yellow);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Jpeg);
        Assert.True(ms.Length > 0);
    }

    [Fact]
    public void Save_Png_StreamRoundtrip()
    {
        using var bmp = new Bitmap(10, 10);
        bmp.SetPixel(0, 0, Color.Red);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        ms.Position = 0;
        using var reloaded = new Bitmap(ms);
        TestHelpers.AssertPixelColor(reloaded, 0, 0, Color.Red);
    }

    // --- Properties ---

    [Fact]
    public void Width_Height_Size_AfterConstruction()
    {
        using var bmp = new Bitmap(33, 44);
        Assert.Equal(33, bmp.Width);
        Assert.Equal(44, bmp.Height);
        Assert.Equal(new Size(33, 44), bmp.Size);
    }

    [Fact]
    public void PixelFormat_Default_Is32bppArgb()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(PixelFormat.Format32bppArgb, bmp.PixelFormat);
    }

    [Fact]
    public void RawFormat_Default_IsMemoryBmp()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(ImageFormat.MemoryBmp.Guid, bmp.RawFormat.Guid);
    }

    [Fact]
    public void HorizontalResolution_Default_Is96()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(96f, bmp.HorizontalResolution);
    }

    [Fact]
    public void VerticalResolution_Default_Is96()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.Equal(96f, bmp.VerticalResolution);
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_GetPixel_ThrowsObjectDisposedException()
    {
        var bmp = new Bitmap(10, 10);
        bmp.Dispose();
        Assert.Throws<ObjectDisposedException>(() => bmp.GetPixel(0, 0));
    }

    [Fact]
    public void Dispose_SetPixel_ThrowsObjectDisposedException()
    {
        var bmp = new Bitmap(10, 10);
        bmp.Dispose();
        Assert.Throws<ObjectDisposedException>(() => bmp.SetPixel(0, 0, Color.Red));
    }

    [Fact]
    public void Dispose_Width_ThrowsObjectDisposedException()
    {
        var bmp = new Bitmap(10, 10);
        bmp.Dispose();
        Assert.Throws<ObjectDisposedException>(() => bmp.Width);
    }

    [Fact]
    public void Dispose_DoubleDispose_DoesNotThrow()
    {
        var bmp = new Bitmap(10, 10);
        bmp.Dispose();
        bmp.Dispose(); // should not throw
    }

    // --- LockBits / UnlockBits ---

    [Fact]
    public void LockBits_ReturnsNonNullBitmapData()
    {
        using var bmp = new Bitmap(10, 10);
        var data = bmp.LockBits(new Rectangle(0, 0, 10, 10), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        Assert.NotNull(data);
        Assert.Equal(10, data.Width);
        Assert.Equal(10, data.Height);
        Assert.NotEqual(IntPtr.Zero, data.Scan0);
        bmp.UnlockBits(data);
    }

    // --- Constructor with Graphics ---

    [Fact]
    public void Constructor_WithGraphics_CorrectSize()
    {
        using var baseBmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(baseBmp);
        using var bmp = new Bitmap(20, 15, g);
        Assert.Equal(20, bmp.Width);
        Assert.Equal(15, bmp.Height);
    }
}
