using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class PivotViewerHyperlinkTest
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var link = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        Assert.Equal("Example", link.Text);
        Assert.Equal(new Uri("https://example.com"), link.Uri);
    }

    [Fact]
    public void Constructor_NullText_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PivotViewerHyperlink(null!, new Uri("https://example.com")));
    }

    [Fact]
    public void Constructor_NullUri_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PivotViewerHyperlink("text", null!));
    }

    [Fact]
    public void Equality()
    {
        var a = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        var b = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        var c = new PivotViewerHyperlink("Other", new Uri("https://other.com"));

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a.Equals(c));
        Assert.True(a != c);
    }

    [Fact]
    public void CompareTo_OrdersByText()
    {
        var a = new PivotViewerHyperlink("Alpha", new Uri("https://a.com"));
        var b = new PivotViewerHyperlink("Beta", new Uri("https://b.com"));

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
    }

    [Fact]
    public void ToString_ShowsTextAndUri()
    {
        var link = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        var str = link.ToString();
        Assert.Contains("Example", str);
        Assert.Contains("example.com", str);
    }

    [Fact]
    public void Equality_NullHandling()
    {
        var a = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        PivotViewerHyperlink? b = null;

        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }
}
