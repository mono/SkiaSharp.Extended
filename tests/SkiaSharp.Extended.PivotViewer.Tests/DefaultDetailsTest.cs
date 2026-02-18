using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests for PivotViewerDefaultDetails — Silverlight detail pane configuration.
/// </summary>
public class DefaultDetailsTest
{
    [Fact]
    public void AllProperties_DefaultToFalse()
    {
        var details = new PivotViewerDefaultDetails();

        Assert.False(details.IsNameHidden);
        Assert.False(details.IsDescriptionHidden);
        Assert.False(details.IsFacetCategoriesHidden);
        Assert.False(details.IsRelatedCollectionsHidden);
        Assert.False(details.IsCopyrightHidden);
    }

    [Fact]
    public void IsNameHidden_NotifiesPropertyChanged()
    {
        var details = new PivotViewerDefaultDetails();
        string? changed = null;
        details.PropertyChanged += (s, e) => changed = e.PropertyName;

        details.IsNameHidden = true;

        Assert.True(details.IsNameHidden);
        Assert.Equal(nameof(PivotViewerDefaultDetails.IsNameHidden), changed);
    }

    [Fact]
    public void IsDescriptionHidden_NotifiesPropertyChanged()
    {
        var details = new PivotViewerDefaultDetails();
        string? changed = null;
        details.PropertyChanged += (s, e) => changed = e.PropertyName;

        details.IsDescriptionHidden = true;

        Assert.True(details.IsDescriptionHidden);
        Assert.Equal(nameof(PivotViewerDefaultDetails.IsDescriptionHidden), changed);
    }

    [Fact]
    public void IsFacetCategoriesHidden_NotifiesPropertyChanged()
    {
        var details = new PivotViewerDefaultDetails();
        string? changed = null;
        details.PropertyChanged += (s, e) => changed = e.PropertyName;

        details.IsFacetCategoriesHidden = true;

        Assert.True(details.IsFacetCategoriesHidden);
        Assert.Equal(nameof(PivotViewerDefaultDetails.IsFacetCategoriesHidden), changed);
    }

    [Fact]
    public void IsRelatedCollectionsHidden_NotifiesPropertyChanged()
    {
        var details = new PivotViewerDefaultDetails();
        string? changed = null;
        details.PropertyChanged += (s, e) => changed = e.PropertyName;

        details.IsRelatedCollectionsHidden = true;

        Assert.True(details.IsRelatedCollectionsHidden);
        Assert.Equal(nameof(PivotViewerDefaultDetails.IsRelatedCollectionsHidden), changed);
    }

    [Fact]
    public void IsCopyrightHidden_NotifiesPropertyChanged()
    {
        var details = new PivotViewerDefaultDetails();
        string? changed = null;
        details.PropertyChanged += (s, e) => changed = e.PropertyName;

        details.IsCopyrightHidden = true;

        Assert.True(details.IsCopyrightHidden);
        Assert.Equal(nameof(PivotViewerDefaultDetails.IsCopyrightHidden), changed);
    }

    [Fact]
    public void LinkClicked_EventCanBeSubscribed()
    {
        var details = new PivotViewerDefaultDetails();
        bool fired = false;
        details.LinkClicked += (s, e) => fired = true;

        // Event is wired but only fires through internal trigger
        Assert.False(fired);
    }

    [Fact]
    public void ApplyFilter_EventCanBeSubscribed()
    {
        var details = new PivotViewerDefaultDetails();
        bool fired = false;
        details.ApplyFilter += (s, e) => fired = true;

        // Event is wired but only fires through internal trigger
        Assert.False(fired);
    }

    [Fact]
    public void Controller_DefaultDetails_IsAccessible()
    {
        var controller = new PivotViewerController();

        Assert.NotNull(controller.DefaultDetails);
        Assert.False(controller.DefaultDetails.IsNameHidden);
    }

    [Fact]
    public void Controller_DefaultDetails_IsSameInstance()
    {
        var controller = new PivotViewerController();

        var d1 = controller.DefaultDetails;
        var d2 = controller.DefaultDetails;
        Assert.Same(d1, d2);
    }
}
