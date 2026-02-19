using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class WordWheelIndexTest
{
    private static (WordWheelIndex index, List<PivotViewerItem> items) CreateTestIndex()
    {
        var nameP = new PivotViewerStringProperty("Name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var catP = new PivotViewerStringProperty("Category")
        {
            DisplayName = "Category",
            Options = PivotViewerPropertyOptions.CanSearchText
        };

        var items = new List<PivotViewerItem>();

        var i1 = new PivotViewerItem("1");
        i1.Set(nameP, new object[] { "Ford GT" });
        i1.Set(catP, new object[] { "Sports Car" });
        items.Add(i1);

        var i2 = new PivotViewerItem("2");
        i2.Set(nameP, new object[] { "Ferrari Enzo" });
        i2.Set(catP, new object[] { "Sports Car" });
        items.Add(i2);

        var i3 = new PivotViewerItem("3");
        i3.Set(nameP, new object[] { "Toyota Prius" });
        i3.Set(catP, new object[] { "Hybrid" });
        items.Add(i3);

        var i4 = new PivotViewerItem("4");
        i4.Set(nameP, new object[] { "Tesla Model S" });
        i4.Set(catP, new object[] { "Electric" });
        items.Add(i4);

        var index = new WordWheelIndex();
        index.Build(items, new PivotViewerProperty[] { nameP, catP });

        return (index, items);
    }

    [Fact]
    public void Search_EmptyPrefix_ReturnsEmpty()
    {
        var (index, _) = CreateTestIndex();
        var results = index.Search("");
        Assert.Empty(results);
    }

    [Fact]
    public void Search_MatchingPrefix_ReturnsResults()
    {
        var (index, _) = CreateTestIndex();
        var results = index.Search("F");

        Assert.True(results.Count >= 2, "Should find Ford and Ferrari");
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
    {
        var (index, _) = CreateTestIndex();
        var results = index.Search("Zzzz");
        Assert.Empty(results);
    }

    [Fact]
    public void Search_CaseInsensitive()
    {
        var (index, _) = CreateTestIndex();
        var results = index.Search("ford");
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Search_PartialWord_Matches()
    {
        var (index, _) = CreateTestIndex();
        var results = index.Search("Fer");
        Assert.NotEmpty(results);
        Assert.True(results.Any(r => r.Text.StartsWith("Ferrari", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void GetMatchingItems_ReturnsCorrectItems()
    {
        var (index, items) = CreateTestIndex();
        var matching = index.GetMatchingItems("Sports");
        Assert.Equal(2, matching.Count);
    }

    [Fact]
    public void GetMatchingItems_EmptyText_ReturnsEmpty()
    {
        var (index, _) = CreateTestIndex();
        var matching = index.GetMatchingItems("");
        Assert.Empty(matching);
    }

    [Fact]
    public void GetCharBuckets_EmptyPrefix_ShowsAllFirstChars()
    {
        var (index, _) = CreateTestIndex();
        var buckets = index.GetCharBuckets();

        Assert.NotEmpty(buckets);
        // Should have buckets for various first chars (c, e, f, g, h, m, p, s, t...)
        var chars = buckets.Select(b => b.Character).ToHashSet();
        Assert.Contains('f', chars); // Ford, Ferrari
        Assert.Contains('t', chars); // Toyota, Tesla
    }

    [Fact]
    public void GetCharBuckets_WithPrefix_NarrowsResults()
    {
        var (index, _) = CreateTestIndex();
        var buckets = index.GetCharBuckets("f");

        Assert.NotEmpty(buckets);
        // After 'f', should show next chars (e for Ferrari, o for Ford)
        var chars = buckets.Select(b => b.Character).ToHashSet();
        Assert.Contains('e', chars); // Ferrari
        Assert.Contains('o', chars); // Ford
    }

    [Fact]
    public void Build_SkipsNonSearchableProperties()
    {
        var nameP = new PivotViewerStringProperty("Name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };
        var privateP = new PivotViewerStringProperty("Secret")
        {
            DisplayName = "Secret",
            Options = PivotViewerPropertyOptions.Private // Not searchable
        };

        var item = new PivotViewerItem("1");
        item.Set(nameP, new object[] { "Hello" });
        item.Set(privateP, new object[] { "Hidden" });

        var index = new WordWheelIndex();
        index.Build(new[] { item }, new PivotViewerProperty[] { nameP, privateP });

        Assert.NotEmpty(index.Search("Hello"));
        Assert.Empty(index.Search("Hidden"));
    }

    [Fact]
    public void ResultLimit_LimitsSearchResults()
    {
        var nameP = new PivotViewerStringProperty("Name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };

        var items = new List<PivotViewerItem>();
        for (int i = 0; i < 100; i++)
        {
            var item = new PivotViewerItem(i.ToString());
            item.Set(nameP, new object[] { $"Item_{i:D3}" });
            items.Add(item);
        }

        var index = new WordWheelIndex();
        index.ResultLimit = 10;
        index.Build(items, new[] { nameP });

        var results = index.Search("Item");
        Assert.True(results.Count <= 10);
    }

    [Fact]
    public void Search_MultiWordValues_IndexesIndividualWords()
    {
        var (index, _) = CreateTestIndex();
        // "Sports Car" should be indexed as both "Sports Car" and "Car"
        var results = index.Search("Car");
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Build_IndexesAdditionalSearchText()
    {
        var nameP = new PivotViewerStringProperty("Name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };

        var item = new PivotViewerItem("1");
        item.Set(nameP, new object[] { "Widget" });
        item.AdditionalSearchText = "bonus keyword";

        var index = new WordWheelIndex();
        index.Build(new[] { item }, new PivotViewerProperty[] { nameP });

        // Should find "Widget" from property
        Assert.NotEmpty(index.Search("Widget"));

        // Should find "bonus" from AdditionalSearchText
        var bonusResults = index.Search("bonus");
        Assert.NotEmpty(bonusResults);

        // Should find "keyword" from AdditionalSearchText
        var keywordResults = index.Search("keyword");
        Assert.NotEmpty(keywordResults);

        // Matching items should include the item
        var matching = index.GetMatchingItems("bonus");
        Assert.Single(matching);
        Assert.Equal("1", matching[0].Id);
    }

    [Fact]
    public void Search_SpecialCharacters_DoesNotThrow()
    {
        var nameP = new PivotViewerStringProperty("Name")
        {
            DisplayName = "Name",
            Options = PivotViewerPropertyOptions.CanSearchText
        };

        var item = new PivotViewerItem("1");
        item.Set(nameP, new object[] { "Café résumé naïve" });

        var index = new WordWheelIndex();
        index.Build(new[] { item }, new PivotViewerProperty[] { nameP });

        var results = index.Search("café");
        Assert.NotNull(results);
    }

    [Fact]
    public void GetMatchingItems_PartialMatch_ReturnsCorrectItems()
    {
        var (index, items) = CreateTestIndex();
        var matching = index.GetMatchingItems("Ford");
        Assert.Single(matching);
        Assert.Equal("1", matching[0].Id);
    }

    [Fact]
    public void Search_SingleCharPrefix_ReturnsResults()
    {
        var (index, _) = CreateTestIndex();
        var results = index.Search("T");
        Assert.True(results.Count >= 2, "Should find Toyota and Tesla");
    }
}
