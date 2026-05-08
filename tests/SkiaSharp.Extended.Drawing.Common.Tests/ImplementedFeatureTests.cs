using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class ImplementedFeatureTests
{
    // --- SystemIcons ---

    [Fact]
    public void SystemIcons_Application_IsNotNull()
    {
        var icon = SystemIcons.Application;
        Assert.NotNull(icon);
        Assert.Equal(32, icon.Width);
        Assert.Equal(32, icon.Height);
    }

    [Fact]
    public void SystemIcons_Error_IsNotNull()
    {
        var icon = SystemIcons.Error;
        Assert.NotNull(icon);
    }

    [Fact]
    public void SystemIcons_AllProperties_AreNotNull()
    {
        Assert.NotNull(SystemIcons.Asterisk);
        Assert.NotNull(SystemIcons.Exclamation);
        Assert.NotNull(SystemIcons.Hand);
        Assert.NotNull(SystemIcons.Information);
        Assert.NotNull(SystemIcons.Question);
        Assert.NotNull(SystemIcons.Shield);
        Assert.NotNull(SystemIcons.Warning);
        Assert.NotNull(SystemIcons.WinLogo);
    }

    // --- ImageAnimator ---

    [Fact]
    public void CanAnimate_NullImage_ReturnsFalse()
    {
        Assert.False(ImageAnimator.CanAnimate(null));
    }

    [Fact]
    public void CanAnimate_SingleFrameImage_ReturnsFalse()
    {
        using var bmp = new Bitmap(10, 10);
        Assert.False(ImageAnimator.CanAnimate(bmp));
    }

    [Fact]
    public void StopAnimate_NonAnimatedImage_DoesNotThrow()
    {
        using var bmp = new Bitmap(10, 10);
        ImageAnimator.StopAnimate(bmp, (s, e) => { });
    }

    [Fact]
    public void UpdateFrames_DoesNotThrow()
    {
        ImageAnimator.UpdateFrames();
    }

    [Fact]
    public void UpdateFrames_WithNull_DoesNotThrow()
    {
        ImageAnimator.UpdateFrames(null);
    }

    // --- FontCollection ---

    [Fact]
    public void InstalledFontCollection_Constructor_DoesNotThrow()
    {
        using var ifc = new InstalledFontCollection();
        Assert.NotNull(ifc);
    }

    [Fact]
    public void InstalledFontCollection_Families_ReturnsNonEmpty()
    {
        using var ifc = new InstalledFontCollection();
        var families = ifc.Families;
        Assert.True(families.Length > 0);
    }

    [Fact]
    public void PrivateFontCollection_Constructor_DoesNotThrow()
    {
        using var pfc = new PrivateFontCollection();
        Assert.NotNull(pfc);
    }

    [Fact]
    public void FontFamily_Families_ReturnsNonEmpty()
    {
        var families = FontFamily.Families;
        Assert.True(families.Length > 0);
    }

    // --- CategoryNameCollection ---

    [Fact]
    public void CategoryNameCollection_FromArray()
    {
        var names = new[] { "Alpha", "Beta", "Gamma" };
        var collection = new CategoryNameCollection(names);
        Assert.Equal(3, collection.Count);
        Assert.Equal("Alpha", collection[0]);
        Assert.True(collection.Contains("Beta"));
        Assert.Equal(2, collection.IndexOf("Gamma"));
    }

    [Fact]
    public void CategoryNameCollection_CopyTo()
    {
        var names = new[] { "A", "B" };
        var collection = new CategoryNameCollection(names);
        var target = new string[2];
        collection.CopyTo(target, 0);
        Assert.Equal("A", target[0]);
        Assert.Equal("B", target[1]);
    }

    [Fact]
    public void CategoryNameCollection_CopyConstructor()
    {
        var original = new CategoryNameCollection(new[] { "X", "Y" });
        var copy = new CategoryNameCollection(original);
        Assert.Equal(2, copy.Count);
        Assert.Equal("X", copy[0]);
    }

    // --- WmfPlaceableFileHeader ---

    [Fact]
    public void WmfPlaceableFileHeader_Properties()
    {
        var header = new WmfPlaceableFileHeader();
        header.Key = unchecked((int)0x9AC6CDD7u);
        header.BboxLeft = 10;
        header.BboxTop = 20;
        header.BboxRight = 100;
        header.BboxBottom = 200;
        header.Inch = 1440;
        header.Hmf = 1;
        header.Checksum = 42;
        header.Reserved = 0;

        Assert.Equal(unchecked((int)0x9AC6CDD7u), header.Key);
        Assert.Equal(10, header.BboxLeft);
        Assert.Equal(20, header.BboxTop);
        Assert.Equal(100, header.BboxRight);
        Assert.Equal(200, header.BboxBottom);
        Assert.Equal(1440, header.Inch);
        Assert.Equal(1, header.Hmf);
        Assert.Equal(42, header.Checksum);
        Assert.Equal(0, header.Reserved);
    }

    // --- MetaHeader ---

    [Fact]
    public void MetaHeader_Properties()
    {
        var header = new MetaHeader();
        header.Type = 1;
        header.HeaderSize = 9;
        header.Version = 0x0300;
        header.Size = 1024;
        header.NoObjects = 5;
        header.MaxRecord = 256;
        header.NoParameters = 0;

        Assert.Equal(1, header.Type);
        Assert.Equal(9, header.HeaderSize);
        Assert.Equal(0x0300, header.Version);
        Assert.Equal(1024, header.Size);
        Assert.Equal(5, header.NoObjects);
        Assert.Equal(256, header.MaxRecord);
        Assert.Equal(0, header.NoParameters);
    }

    // --- MetafileHeader ---

    [Fact]
    public void MetafileHeader_TypeCheckMethods_DoNotThrow()
    {
        // MetafileHeader has internal constructor - test via Activator with non-public access
        var ctor = typeof(MetafileHeader).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, Type.EmptyTypes, null);
        Assert.NotNull(ctor);
        var header = (MetafileHeader)ctor!.Invoke(null);
        // The default type is Wmf, so IsWmf should be true
        Assert.True(header.IsWmf());
        Assert.NotNull(header.WmfHeader);
    }

    // --- ToolboxBitmapAttribute ---

    [Fact]
    public void ToolboxBitmapAttribute_Default_IsNotNull()
    {
        Assert.NotNull(ToolboxBitmapAttribute.Default);
    }

    [Fact]
    public void ToolboxBitmapAttribute_GetImage_ReturnsNull()
    {
        var attr = new ToolboxBitmapAttribute(typeof(Bitmap));
        Assert.Null(attr.GetImage(typeof(Bitmap)));
    }

    [Fact]
    public void ToolboxBitmapAttribute_Equals_SameType()
    {
        var a1 = new ToolboxBitmapAttribute(typeof(Bitmap));
        var a2 = new ToolboxBitmapAttribute(typeof(Bitmap));
        Assert.Equal(a1, a2);
    }

    // --- BitmapSuffix Attributes ---

    [Fact]
    public void BitmapSuffixInSameAssemblyAttribute_Constructor()
    {
        var attr = new BitmapSuffixInSameAssemblyAttribute();
        Assert.NotNull(attr);
    }

    [Fact]
    public void BitmapSuffixInSatelliteAssemblyAttribute_Constructor()
    {
        var attr = new BitmapSuffixInSatelliteAssemblyAttribute();
        Assert.NotNull(attr);
    }

    // --- Graphics.IsVisible ---

    [Fact]
    public void Graphics_IsVisible_PointInBounds_ReturnsTrue()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.True(g.IsVisible(50, 50));
    }

    [Fact]
    public void Graphics_IsVisible_PointOutOfBounds_ReturnsFalse()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.False(g.IsVisible(200, 200));
    }

    [Fact]
    public void Graphics_IsVisible_RectangleInBounds()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.True(g.IsVisible(new Rectangle(10, 10, 20, 20)));
    }

    [Fact]
    public void Graphics_IsVisible_RectangleFOutOfBounds()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.False(g.IsVisible(new RectangleF(200, 200, 10, 10)));
    }

    [Fact]
    public void Graphics_IsVisible_PointOverloads()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        Assert.True(g.IsVisible(new Point(50, 50)));
        Assert.True(g.IsVisible(new PointF(50, 50)));
        Assert.True(g.IsVisible(50f, 50f));
    }

    // --- Graphics.TransformPoints ---

    [Fact]
    public void Graphics_TransformPoints_WorldToDevice()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        g.TranslateTransform(50, 50);
        var pts = new PointF[] { new PointF(0, 0) };
        g.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, pts);
        Assert.True(Math.Abs(pts[0].X - 50) < 1f);
        Assert.True(Math.Abs(pts[0].Y - 50) < 1f);
    }

    [Fact]
    public void Graphics_TransformPoints_IntOverload()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        g.TranslateTransform(10, 20);
        var pts = new Point[] { new Point(0, 0) };
        g.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, pts);
        Assert.True(Math.Abs(pts[0].X - 10) <= 1);
        Assert.True(Math.Abs(pts[0].Y - 20) <= 1);
    }

    // --- Graphics.SetClip with Graphics ---

    [Fact]
    public void Graphics_SetClip_FromGraphics_DoesNotThrow()
    {
        using var bmp1 = new Bitmap(100, 100);
        using var g1 = Graphics.FromImage(bmp1);
        using var bmp2 = new Bitmap(100, 100);
        using var g2 = Graphics.FromImage(bmp2);
        g2.SetClip(g1);
    }

    // --- Graphics.DrawIcon ---

    [Fact]
    public void Graphics_DrawIcon_DoesNotThrow()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        var icon = SystemIcons.Application;
        g.DrawIcon(icon, new Rectangle(0, 0, 32, 32));
    }

    [Fact]
    public void Graphics_DrawIcon_XY_DoesNotThrow()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        g.DrawIcon(SystemIcons.Error, 10, 10);
    }

    [Fact]
    public void Graphics_DrawIconUnstretched_DoesNotThrow()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        g.DrawIconUnstretched(SystemIcons.Warning, new Rectangle(0, 0, 64, 64));
    }

    // --- Graphics.MeasureCharacterRanges ---

    [Fact]
    public void Graphics_MeasureCharacterRanges_ReturnsRegions()
    {
        using var bmp = new Bitmap(200, 50);
        using var g = Graphics.FromImage(bmp);
        using var font = new Font("Arial", 12);
        using var sf = new StringFormat();
        sf.SetMeasurableCharacterRanges(new[]
        {
            new CharacterRange(0, 5),
            new CharacterRange(5, 5)
        });
        var regions = g.MeasureCharacterRanges("HelloWorld", font, new RectangleF(0, 0, 200, 50), sf);
        Assert.Equal(2, regions.Length);
    }

    // --- FontConverter ---

    [Fact]
    public void FontConverter_CanConvertFromString()
    {
        var converter = new FontConverter();
        Assert.True(converter.CanConvertFrom(typeof(string)));
    }

    [Fact]
    public void FontConverter_ConvertFromString()
    {
        var converter = new FontConverter();
        var result = converter.ConvertFromString("Arial, 12pt");
        Assert.IsType<Font>(result);
        var font = (Font)result!;
        Assert.Equal(12f, font.Size);
        font.Dispose();
    }

    [Fact]
    public void FontConverter_ConvertTo_String()
    {
        var converter = new FontConverter();
        using var font = new Font("Arial", 14, FontStyle.Bold);
        var str = converter.ConvertToString(font);
        Assert.NotNull(str);
        Assert.Contains("14", str!);
    }

    [Fact]
    public void FontConverter_GetCreateInstanceSupported()
    {
        var converter = new FontConverter();
        Assert.True(converter.GetCreateInstanceSupported());
    }

    // --- ImageConverter ---

    [Fact]
    public void ImageConverter_CanConvertFromBytes()
    {
        var converter = new ImageConverter();
        Assert.True(converter.CanConvertFrom(typeof(byte[])));
    }

    [Fact]
    public void ImageConverter_CanConvertToString()
    {
        var converter = new ImageConverter();
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Fact]
    public void ImageConverter_RoundTrip()
    {
        var converter = new ImageConverter();
        using var bmp = new Bitmap(10, 10);
        var bytes = (byte[])converter.ConvertTo(bmp, typeof(byte[]))!;
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
        using var img = (Image)converter.ConvertFrom(bytes)!;
        Assert.Equal(10, img.Width);
    }

    // --- IconConverter ---

    [Fact]
    public void IconConverter_Constructor()
    {
        var converter = new IconConverter();
        Assert.NotNull(converter);
    }

    [Fact]
    public void IconConverter_CanConvertFromBytes()
    {
        var converter = new IconConverter();
        Assert.True(converter.CanConvertFrom(typeof(byte[])));
    }

    // --- ImageFormatConverter ---

    [Fact]
    public void ImageFormatConverter_ConvertFromString_Png()
    {
        var converter = new ImageFormatConverter();
        var result = converter.ConvertFromString("Png");
        Assert.Equal(ImageFormat.Png, result);
    }

    [Fact]
    public void ImageFormatConverter_ConvertToString()
    {
        var converter = new ImageFormatConverter();
        var result = converter.ConvertToString(ImageFormat.Jpeg);
        Assert.NotNull(result);
        Assert.Contains("Jpeg", result!);
    }

    [Fact]
    public void ImageFormatConverter_StandardValues()
    {
        var converter = new ImageFormatConverter();
        Assert.True(converter.GetStandardValuesSupported());
        var values = converter.GetStandardValues();
        Assert.True(values!.Count > 0);
    }

    // --- BufferedGraphics ---

    [Fact]
    public void BufferedGraphicsContext_Allocate_ReturnsBufferedGraphics()
    {
        using var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        using var ctx = new BufferedGraphicsContext();
        using var bg = ctx.Allocate(g, new Rectangle(0, 0, 100, 100));
        Assert.NotNull(bg);
        Assert.NotNull(bg.Graphics);
    }

    [Fact]
    public void BufferedGraphicsContext_MaximumBuffer_GetSet()
    {
        using var ctx = new BufferedGraphicsContext();
        ctx.MaximumBuffer = new Size(500, 500);
        Assert.Equal(new Size(500, 500), ctx.MaximumBuffer);
    }
}
