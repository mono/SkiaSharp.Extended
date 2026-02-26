using SkiaSharp.Extended.PivotViewer;
using Xunit;

namespace SkiaSharp.Extended.PivotViewer.Tests;

public class SortDropdownModelTest
{
    private static (PivotViewerController controller, SortDropdownModel model) CreateTestModel()
    {
        var controller = new PivotViewerController();
        var source = TestDataHelper.LoadCxml("conceptcars.cxml");
        controller.LoadCollection(source);
        controller.SetAvailableSize(1024, 768);
        return (controller, controller.SortDropdown);
    }

    [Fact]
    public void Default_IsVisible_False()
    {
        var (_, model) = CreateTestModel();
        Assert.False(model.IsVisible);
    }

    [Fact]
    public void Toggle_SetsVisible()
    {
        var (_, model) = CreateTestModel();

        model.Toggle();
        Assert.True(model.IsVisible);

        model.Toggle();
        Assert.False(model.IsVisible);
    }

    [Fact]
    public void Close_SetsNotVisible()
    {
        var (_, model) = CreateTestModel();

        model.Toggle(); // open
        Assert.True(model.IsVisible);

        model.Close();
        Assert.False(model.IsVisible);
    }

    [Fact]
    public void AvailableProperties_ExcludesPrivate()
    {
        var (controller, model) = CreateTestModel();

        var available = model.AvailableProperties;
        Assert.NotEmpty(available);

        foreach (var prop in available)
        {
            Assert.False(prop.Options.HasFlag(PivotViewerPropertyOptions.Private),
                $"Property '{prop.Id}' should not be private");
        }
    }

    [Fact]
    public void SelectProperty_SetsOnController()
    {
        var (controller, model) = CreateTestModel();

        var prop = model.AvailableProperties[0];
        model.SelectProperty(prop);

        Assert.Equal(prop, controller.SortProperty);
    }

    [Fact]
    public void SelectProperty_ClosesDropdown()
    {
        var (_, model) = CreateTestModel();

        model.Toggle(); // open
        Assert.True(model.IsVisible);

        var prop = model.AvailableProperties[0];
        model.SelectProperty(prop);

        Assert.False(model.IsVisible);
    }

    [Fact]
    public void INPC_Fires_OnVisibilityChange()
    {
        var (_, model) = CreateTestModel();
        var changed = new List<string>();
        model.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

        model.Toggle();

        Assert.Contains("IsVisible", changed);
    }
}
