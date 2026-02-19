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

    [Fact]
    public void CompareTo_Null_Returns1()
    {
        var hl = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        Assert.Equal(1, hl.CompareTo((PivotViewerHyperlink?)null));
    }

    [Fact]
    public void CompareTo_IComparable_Null_Returns1()
    {
        var hl = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        Assert.Equal(1, ((IComparable)hl).CompareTo(null));
    }

    [Fact]
    public void CompareTo_SameTextDifferentUri_OrdersByUri()
    {
        var a = new PivotViewerHyperlink("Same", new Uri("https://aaa.com"));
        var b = new PivotViewerHyperlink("Same", new Uri("https://zzz.com"));

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
    }

    [Fact]
    public void GetHashCode_EqualObjects_SameHash()
    {
        var a = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        var b = new PivotViewerHyperlink("Example", new Uri("https://example.com"));

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentObjects_UsuallyDifferent()
    {
        var a = new PivotViewerHyperlink("Alpha", new Uri("https://alpha.com"));
        var b = new PivotViewerHyperlink("Beta", new Uri("https://beta.com"));

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_Operators_BothNull_True()
    {
        PivotViewerHyperlink? a = null;
        PivotViewerHyperlink? b = null;

        Assert.True(a == b);
    }

    [Fact]
    public void Equality_Operators_OneNull_False()
    {
        var hl = new PivotViewerHyperlink("Example", new Uri("https://example.com"));
        PivotViewerHyperlink? n = null;

        Assert.False(hl == n);
        Assert.False(n == hl);
        Assert.True(hl != n);
        Assert.True(n != hl);
    }

    [Fact]
    public void Inequality_Operator_ReturnsCorrect()
    {
        var a = new PivotViewerHyperlink("Alpha", new Uri("https://alpha.com"));
        var b = new PivotViewerHyperlink("Beta", new Uri("https://beta.com"));
        var c = new PivotViewerHyperlink("Alpha", new Uri("https://alpha.com"));

        Assert.True(a != b);
        Assert.False(a != c);
    }
}
