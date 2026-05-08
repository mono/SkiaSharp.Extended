using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class GraphicsTests
{
    // --- FromImage ---

    [Fact]
    public void FromImage_ReturnsNonNull()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.NotNull(g);
    }

    [Fact]
    public void FromImage_NullImage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Graphics.FromImage(null!));
    }

    // --- Clear ---

    [Fact]
    public void Clear_Red_AllPixelsRed()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Red);
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                TestHelpers.AssertPixelColor(bmp, x, y, Color.Red);
    }

    [Fact]
    public void Clear_Transparent_AllPixelsTransparent()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Red);
        g.Clear(Color.FromArgb(0, 0, 0, 0));
        var pixel = bmp.GetPixel(5, 5);
        Assert.Equal(0, pixel.A);
    }

    [Fact]
    public void Clear_Blue_AllPixelsBlue()
    {
        using var bmp = new Bitmap(5, 5);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Blue);
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.Blue);
        TestHelpers.AssertPixelColor(bmp, 4, 4, Color.Blue);
    }

    // --- DrawLine ---

    [Fact]
    public void DrawLine_Horizontal_PixelsOnLineAreColored()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Red, 1);
        g.DrawLine(pen, 0, 10, 19, 10);
        // Check pixel on the line
        var pixel = bmp.GetPixel(10, 10);
        Assert.Equal(255, pixel.R);
    }

    [Fact]
    public void DrawLine_Vertical_PixelsOnLineAreColored()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Blue, 1);
        g.DrawLine(pen, 10, 0, 10, 19);
        var pixel = bmp.GetPixel(10, 10);
        Assert.True(pixel.B > 200);
    }

    [Fact]
    public void DrawLine_Diagonal_EndpointsColored()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Green, 1);
        g.DrawLine(pen, 0, 0, 19, 19);
        // Start pixel should have green contribution
        var pixel = bmp.GetPixel(0, 0);
        Assert.True(pixel.G > 100);
    }

    [Fact]
    public void DrawLine_ThickPen_WiderLine()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Red, 5);
        g.DrawLine(pen, 0, 15, 29, 15);
        // Pixel at (15, 15) should be red
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.Red, 10);
        // Pixel at (15, 13) should also be colored (thick line)
        var above = bmp.GetPixel(15, 13);
        Assert.True(above.R > 200);
    }

    [Fact]
    public void DrawLine_NullPen_ThrowsArgumentNullException()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.Throws<ArgumentNullException>(() => g.DrawLine(null!, 0, 0, 5, 5));
    }

    // --- DrawRectangle ---

    [Fact]
    public void DrawRectangle_CornersHavePenColor()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Red, 1);
        g.DrawRectangle(pen, 5, 5, 20, 20);
        // Top-left corner
        TestHelpers.AssertPixelColor(bmp, 5, 5, Color.Red, 10);
        // Center should still be white
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.White, 10);
    }

    [Fact]
    public void DrawRectangle_RectOverload()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Blue, 1);
        g.DrawRectangle(pen, new Rectangle(2, 2, 15, 15));
        var pixel = bmp.GetPixel(2, 2);
        Assert.True(pixel.B > 200);
    }

    // --- FillRectangle ---

    [Fact]
    public void FillRectangle_InteriorPixelsAreBrushColor()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillRectangle(brush, 5, 5, 20, 20);
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.Green, 5);
    }

    [Fact]
    public void FillRectangle_OutsidePixelsUnchanged()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 10, 10, 10, 10);
        // Pixel at (0,0) should still be white
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.White, 5);
    }

    [Fact]
    public void FillRectangle_Various_Colors()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Black);
        using var brush = new SolidBrush(Color.Yellow);
        g.FillRectangle(brush, 0, 0, 10, 10);
        TestHelpers.AssertPixelColor(bmp, 5, 5, Color.Yellow, 5);
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.Black, 5);
    }

    [Fact]
    public void FillRectangle_NullBrush_ThrowsArgumentNullException()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.Throws<ArgumentNullException>(() => g.FillRectangle(null!, 0, 0, 5, 5));
    }

    // --- DrawEllipse ---

    [Fact]
    public void DrawEllipse_TopBottomPixelsColored()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Red, 1);
        g.DrawEllipse(pen, 5, 5, 30, 30);
        // Top center of ellipse should be colored
        var topCenter = bmp.GetPixel(20, 5);
        Assert.True(topCenter.R > 200);
    }

    // --- FillEllipse ---

    [Fact]
    public void FillEllipse_CenterPixelFilled()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 5, 5, 30, 30);
        TestHelpers.AssertPixelColor(bmp, 20, 20, Color.Blue, 10);
    }

    [Fact]
    public void FillEllipse_CornerPixelNotFilled()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 5, 5, 30, 30);
        // Corner pixel should still be white (outside ellipse)
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.White, 5);
    }

    // --- DrawArc ---

    [Fact]
    public void DrawArc_SomePixelsChanged()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 2);
        g.DrawArc(pen, 5, 5, 30, 30, 0, 90);
        // Check that at least some pixels are not white
        bool found = false;
        for (int y = 0; y < 40 && !found; y++)
            for (int x = 0; x < 40 && !found; x++)
            {
                var p = bmp.GetPixel(x, y);
                if (p.R > 200 && p.G < 50) found = true;
            }
        Assert.True(found, "DrawArc should have drawn some red pixels");
    }

    // --- DrawPie ---

    [Fact]
    public void DrawPie_SomePixelsChanged()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Green, 2);
        g.DrawPie(pen, 5, 5, 30, 30, 0, 90);
        // Check that at least some non-white pixels exist
        bool found = false;
        for (int y = 0; y < 40 && !found; y++)
            for (int x = 0; x < 40 && !found; x++)
            {
                var p = bmp.GetPixel(x, y);
                if (p.G > 100 && (p.R < 200 || p.B < 200)) found = true;
            }
        Assert.True(found, "DrawPie should have drawn some non-white pixels");
    }

    // --- FillPie ---

    [Fact]
    public void FillPie_CenterRegionFilled()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillPie(brush, 0, 0, 40, 40, 0, 360);
        // Center should be filled
        var pixel = bmp.GetPixel(20, 20);
        Assert.True(pixel.R > 200);
    }

    // --- DrawPolygon ---

    [Fact]
    public void DrawPolygon_Triangle_SomePixelsDrawn()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 2);
        g.DrawPolygon(pen, new PointF[] { new(20, 0), new(0, 39), new(39, 39) });
        bool found = false;
        for (int y = 0; y < 40 && !found; y++)
            for (int x = 0; x < 40 && !found; x++)
            {
                var p = bmp.GetPixel(x, y);
                if (p.B > 200 && p.R < 50) found = true;
            }
        Assert.True(found, "DrawPolygon should have drawn some blue pixels");
    }

    [Fact]
    public void DrawPolygon_PointOverload()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 1);
        g.DrawPolygon(pen, new Point[] { new(5, 5), new(25, 5), new(15, 25) });
        var pixel = bmp.GetPixel(5, 5);
        Assert.True(pixel.R > 200);
    }

    // --- FillPolygon ---

    [Fact]
    public void FillPolygon_Triangle_InteriorFilled()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Magenta);
        g.FillPolygon(brush, new PointF[] { new(20, 0), new(0, 39), new(39, 39) });
        // Center-ish pixel should be magenta
        var pixel = bmp.GetPixel(20, 25);
        Assert.True(pixel.R > 200 && pixel.B > 200);
    }

    [Fact]
    public void FillPolygon_PointOverload()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillPolygon(brush, new Point[] { new(5, 5), new(25, 5), new(15, 25) });
        // Check center of the triangle (should be well inside)
        var pixel = bmp.GetPixel(15, 12);
        Assert.True(pixel.G > 100, $"Expected green interior pixel, got ({pixel.R},{pixel.G},{pixel.B})");
    }

    // --- DrawImage ---

    [Fact]
    public void DrawImage_SmallOntoLarger_PixelsCopied()
    {
        using var small = TestHelpers.CreateSolidBitmap(5, 5, Color.Red);
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.DrawImage(small, 0, 0);
        TestHelpers.AssertPixelColor(bmp, 2, 2, Color.Red, 10);
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.White, 5);
    }

    [Fact]
    public void DrawImage_WithDestinationRect_Scales()
    {
        using var small = TestHelpers.CreateSolidBitmap(5, 5, Color.Blue);
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.DrawImage(small, 0, 0, 20, 20);
        TestHelpers.AssertPixelColor(bmp, 10, 10, Color.Blue, 10);
    }

    [Fact]
    public void DrawImageUnscaled_ExactCopy()
    {
        using var small = TestHelpers.CreateSolidBitmap(5, 5, Color.Green);
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.DrawImageUnscaled(small, 0, 0);
        TestHelpers.AssertPixelColor(bmp, 2, 2, Color.Green, 10);
    }

    [Fact]
    public void DrawImage_NullImage_ThrowsArgumentNullException()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.Throws<ArgumentNullException>(() => g.DrawImage(null!, 0, 0));
    }

    [Fact]
    public void DrawImage_PointOverload()
    {
        using var small = TestHelpers.CreateSolidBitmap(3, 3, Color.Cyan);
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.DrawImage(small, new Point(2, 2));
        TestHelpers.AssertPixelColor(bmp, 3, 3, Color.Cyan, 10);
    }

    [Fact]
    public void DrawImage_RectangleOverload()
    {
        using var small = TestHelpers.CreateSolidBitmap(5, 5, Color.Orange);
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.DrawImage(small, new Rectangle(5, 5, 10, 10));
        TestHelpers.AssertPixelColor(bmp, 10, 10, Color.Orange, 20);
    }

    [Fact]
    public void DrawImageUnscaledAndClipped_ClipsToRect()
    {
        using var large = TestHelpers.CreateSolidBitmap(20, 20, Color.Red);
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.DrawImageUnscaledAndClipped(large, new Rectangle(5, 5, 10, 10));
        // Inside the rect: red
        TestHelpers.AssertPixelColor(bmp, 10, 10, Color.Red, 10);
        // Outside the rect: white
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.White, 5);
    }

    // --- Property tests ---

    [Fact]
    public void SmoothingMode_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.HighQuality;
        Assert.Equal(SmoothingMode.HighQuality, g.SmoothingMode);
    }

    [Fact]
    public void InterpolationMode_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.Bilinear;
        Assert.Equal(InterpolationMode.Bilinear, g.InterpolationMode);
    }

    [Fact]
    public void CompositingMode_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.CompositingMode = CompositingMode.SourceCopy;
        Assert.Equal(CompositingMode.SourceCopy, g.CompositingMode);
    }

    [Fact]
    public void CompositingQuality_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.CompositingQuality = CompositingQuality.HighQuality;
        Assert.Equal(CompositingQuality.HighQuality, g.CompositingQuality);
    }

    [Fact]
    public void PixelOffsetMode_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        Assert.Equal(PixelOffsetMode.HighQuality, g.PixelOffsetMode);
    }

    [Fact]
    public void PageUnit_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.PageUnit = GraphicsUnit.Inch;
        Assert.Equal(GraphicsUnit.Inch, g.PageUnit);
    }

    [Fact]
    public void PageScale_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.PageScale = 2.0f;
        Assert.Equal(2.0f, g.PageScale);
    }

    [Fact]
    public void TextContrast_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.TextContrast = 8;
        Assert.Equal(8, g.TextContrast);
    }

    [Fact]
    public void RenderingOrigin_GetSet()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.RenderingOrigin = new Point(5, 3);
        Assert.Equal(new Point(5, 3), g.RenderingOrigin);
    }

    [Fact]
    public void DpiX_Default96()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.Equal(96f, g.DpiX);
    }

    [Fact]
    public void DpiY_Default96()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.Equal(96f, g.DpiY);
    }

    [Fact]
    public void ClipBounds_ReturnsNonEmptyRect()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        var bounds = g.ClipBounds;
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    [Fact]
    public void VisibleClipBounds_ReturnsNonEmptyRect()
    {
        using var bmp = new Bitmap(20, 20);
        using var g = Graphics.FromImage(bmp);
        var bounds = g.VisibleClipBounds;
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    [Fact]
    public void IsClipEmpty_NewGraphics_ReturnsFalse()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.False(g.IsClipEmpty);
    }

    [Fact]
    public void IsVisibleClipEmpty_NewGraphics_ReturnsFalse()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        Assert.False(g.IsVisibleClipEmpty);
    }

    // --- Transform methods ---

    [Fact]
    public void TranslateTransform_DrawAtOffset()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.TranslateTransform(10, 10);
        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 0, 0, 5, 5);
        // The rect should be drawn at (10,10) not (0,0)
        TestHelpers.AssertPixelColor(bmp, 12, 12, Color.Red, 10);
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.White, 5);
    }

    [Fact]
    public void ScaleTransform_DrawsScaled()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.ScaleTransform(2, 2);
        using var brush = new SolidBrush(Color.Blue);
        g.FillRectangle(brush, 0, 0, 10, 10);
        // Scaled 2x means a 10x10 fill becomes 20x20
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.Blue, 10);
    }

    [Fact]
    public void RotateTransform_90_DrawsRotated()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.TranslateTransform(20, 20);
        g.RotateTransform(90);
        using var pen = new Pen(Color.Red, 2);
        // Draw a horizontal line — after 90° rotation should appear vertical
        g.DrawLine(pen, 0, 0, 15, 0);
        // After rotating 90° around (20,20), a horizontal line going right
        // should now go downward. Check a pixel below center.
        var pixel = bmp.GetPixel(20, 30);
        Assert.True(pixel.R > 200, "Rotated line should appear in the expected location");
    }

    [Fact]
    public void ResetTransform_RestoresOriginalCoords()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.TranslateTransform(15, 15);
        g.ResetTransform();
        using var brush = new SolidBrush(Color.Green);
        g.FillRectangle(brush, 0, 0, 5, 5);
        // Should be at origin after reset
        TestHelpers.AssertPixelColor(bmp, 2, 2, Color.Green, 10);
    }

    // --- Save / Restore ---

    [Fact]
    public void SaveRestore_RestoresTransformState()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);

        var state = g.Save();
        g.TranslateTransform(15, 15);
        g.Restore(state);

        using var brush = new SolidBrush(Color.Red);
        g.FillRectangle(brush, 0, 0, 5, 5);
        // After restore, should draw at (0,0)
        TestHelpers.AssertPixelColor(bmp, 2, 2, Color.Red, 10);
    }

    // --- Flush ---

    [Fact]
    public void Flush_DoesNotThrow()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.Flush();
    }

    [Fact]
    public void Flush_WithIntention_DoesNotThrow()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.Flush(FlushIntention.Sync);
    }

    // --- GetNearestColor ---

    [Fact]
    public void GetNearestColor_ReturnsSameColor()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        var result = g.GetNearestColor(Color.Coral);
        Assert.Equal(Color.Coral.ToArgb(), result.ToArgb());
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_Clear_ThrowsObjectDisposedException()
    {
        using var bmp = new Bitmap(10, 10);
        var g = Graphics.FromImage(bmp);
        g.Dispose();
        Assert.Throws<ObjectDisposedException>(() => g.Clear(Color.Red));
    }

    [Fact]
    public void Dispose_DrawLine_ThrowsObjectDisposedException()
    {
        using var bmp = new Bitmap(10, 10);
        var g = Graphics.FromImage(bmp);
        g.Dispose();
        Assert.Throws<ObjectDisposedException>(() => g.DrawLine(new Pen(Color.Red), 0, 0, 5, 5));
    }

    [Fact]
    public void Dispose_FillRectangle_ThrowsObjectDisposedException()
    {
        using var bmp = new Bitmap(10, 10);
        var g = Graphics.FromImage(bmp);
        g.Dispose();
        Assert.Throws<ObjectDisposedException>(() => g.FillRectangle(new SolidBrush(Color.Red), 0, 0, 5, 5));
    }

    [Fact]
    public void Dispose_DpiX_ThrowsObjectDisposedException()
    {
        using var bmp = new Bitmap(10, 10);
        var g = Graphics.FromImage(bmp);
        g.Dispose();
        Assert.Throws<ObjectDisposedException>(() => g.DpiX);
    }

    // --- Multiple draw operations ---

    [Fact]
    public void MultipleDraws_LayeringWorks()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var redBrush = new SolidBrush(Color.Red);
        using var blueBrush = new SolidBrush(Color.Blue);
        g.FillRectangle(redBrush, 0, 0, 30, 30);
        g.FillRectangle(blueBrush, 10, 10, 10, 10);
        // Inner area should be blue (drawn last)
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.Blue, 10);
        // Outer area should be red
        TestHelpers.AssertPixelColor(bmp, 0, 0, Color.Red, 10);
    }

    [Fact]
    public void MultipleDraws_LineOverFill()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var greenBrush = new SolidBrush(Color.Green);
        g.FillRectangle(greenBrush, 0, 0, 30, 30);
        using var redPen = new Pen(Color.Red, 3);
        g.DrawLine(redPen, 0, 15, 29, 15);
        // On the line: should be red
        TestHelpers.AssertPixelColor(bmp, 15, 15, Color.Red, 10);
        // Off the line: should be green
        TestHelpers.AssertPixelColor(bmp, 15, 0, Color.Green, 10);
    }

    // --- DrawLines ---

    [Fact]
    public void DrawLines_PointF_DrawsConnectedSegments()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 1);
        g.DrawLines(pen, new PointF[] { new(0, 15), new(15, 15), new(15, 0) });
        var pixel = bmp.GetPixel(10, 15);
        Assert.True(pixel.R > 200);
    }

    [Fact]
    public void DrawLines_Point_DrawsConnectedSegments()
    {
        using var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 1);
        g.DrawLines(pen, new Point[] { new(0, 15), new(15, 15), new(15, 0) });
        var pixel = bmp.GetPixel(10, 15);
        Assert.True(pixel.B > 200);
    }

    // --- DrawRectangles / FillRectangles ---

    [Fact]
    public void DrawRectangles_DrawsMultiple()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(Color.Red, 1);
        g.DrawRectangles(pen, new RectangleF[] { new(2, 2, 10, 10), new(20, 20, 10, 10) });
        TestHelpers.AssertPixelColor(bmp, 2, 2, Color.Red, 10);
        TestHelpers.AssertPixelColor(bmp, 20, 20, Color.Red, 10);
    }

    [Fact]
    public void FillRectangles_FillsMultiple()
    {
        using var bmp = new Bitmap(40, 40);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Green);
        g.FillRectangles(brush, new RectangleF[] { new(0, 0, 10, 10), new(20, 20, 10, 10) });
        TestHelpers.AssertPixelColor(bmp, 5, 5, Color.Green, 10);
        TestHelpers.AssertPixelColor(bmp, 25, 25, Color.Green, 10);
    }

    // --- TranslateTransform with MatrixOrder ---

    [Fact]
    public void TranslateTransform_WithOrder_DoesNotThrow()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.TranslateTransform(5, 5, MatrixOrder.Append);
    }

    [Fact]
    public void ScaleTransform_WithOrder_DoesNotThrow()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.ScaleTransform(2, 2, MatrixOrder.Append);
    }

    [Fact]
    public void RotateTransform_WithOrder_DoesNotThrow()
    {
        using var bmp = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bmp);
        g.RotateTransform(45, MatrixOrder.Append);
    }
}
