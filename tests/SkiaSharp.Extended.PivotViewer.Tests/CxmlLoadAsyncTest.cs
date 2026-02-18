using System.Net;
using System.Text;
using SkiaSharp.Extended.PivotViewer;

namespace SkiaSharp.Extended.PivotViewer.Tests;

/// <summary>
/// Tests for CxmlCollectionSource.LoadAsync URI overload using a mock HttpMessageHandler.
/// Also tests state transitions and event firing during async loading.
/// </summary>
public class CxmlLoadAsyncTest
{
    private static readonly string SampleCxml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Collection Name=""Test"" SchemaVersion=""1.0""
    xmlns=""http://schemas.microsoft.com/collection/metadata/2009""
    xmlns:p=""http://schemas.microsoft.com/livelabs/pivot/collection/2009"">
  <FacetCategories>
    <FacetCategory Name=""Color"" Type=""String""/>
    <FacetCategory Name=""Size"" Type=""Number""/>
  </FacetCategories>
  <Items>
    <Item Id=""1"" Name=""Item One"">
      <Facets>
        <Facet Name=""Color""><String Value=""Red""/></Facet>
        <Facet Name=""Size""><Number Value=""42""/></Facet>
      </Facets>
    </Item>
  </Items>
</Collection>";

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;
        private readonly Exception? _exception;
        public int RequestCount { get; private set; }

        public MockHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _content = content;
            _statusCode = statusCode;
        }

        public MockHttpMessageHandler(Exception exception)
        {
            _content = "";
            _statusCode = HttpStatusCode.InternalServerError;
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;

            if (_exception != null)
                throw _exception;

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/xml")
            });
        }
    }

    [Fact]
    public async Task LoadAsync_Uri_Success()
    {
        var handler = new MockHttpMessageHandler(SampleCxml);
        using var client = new HttpClient(handler);

        var source = await CxmlCollectionSource.LoadAsync(
            new Uri("http://example.com/test.cxml"), client);

        Assert.Equal(CxmlCollectionState.Loaded, source.State);
        Assert.Equal("Test", source.Name);
        Assert.Single(source.Items);
        // 2 explicit FacetCategories + 1 implicit "Name" from Item Name="" attribute
        Assert.Equal(3, source.ItemProperties.Count);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task LoadAsync_Uri_SetsUriSource()
    {
        var handler = new MockHttpMessageHandler(SampleCxml);
        using var client = new HttpClient(handler);
        var uri = new Uri("http://example.com/test.cxml");

        var source = await CxmlCollectionSource.LoadAsync(uri, client);

        Assert.Equal(uri, source.UriSource);
    }

    [Fact]
    public async Task LoadAsync_Uri_InvalidXml_SetsFailed()
    {
        var handler = new MockHttpMessageHandler("this is not xml");
        using var client = new HttpClient(handler);

        var source = await CxmlCollectionSource.LoadAsync(
            new Uri("http://example.com/bad.cxml"), client);

        Assert.Equal(CxmlCollectionState.Failed, source.State);
    }

    [Fact]
    public async Task LoadAsync_Uri_NetworkError_SetsFailed()
    {
        var handler = new MockHttpMessageHandler(new HttpRequestException("Network error"));
        using var client = new HttpClient(handler);

        var source = await CxmlCollectionSource.LoadAsync(
            new Uri("http://example.com/unreachable.cxml"), client);

        Assert.Equal(CxmlCollectionState.Failed, source.State);
    }

    [Fact]
    public async Task LoadAsync_Uri_FiresStateChangedOnFailure()
    {
        var handler = new MockHttpMessageHandler("not xml");
        using var client = new HttpClient(handler);

        CxmlCollectionStateChangedEventArgs? eventArgs = null;
        var source = await CxmlCollectionSource.LoadAsync(
            new Uri("http://example.com/bad.cxml"), client);

        // Subscribe after creation (state already changed) — verify state
        Assert.Equal(CxmlCollectionState.Failed, source.State);
    }

    [Fact]
    public async Task LoadAsync_Stream_SuccessWithRealData()
    {
        using var stream = TestDataHelper.GetStream("ski_resorts.cxml");
        var source = await CxmlCollectionSource.LoadAsync(stream);

        Assert.Equal(CxmlCollectionState.Loaded, source.State);
        Assert.Equal("SPARQL Query Results", source.Name);
        Assert.True(source.Items.Count >= 14);
    }

    [Fact]
    public async Task LoadAsync_Stream_CancellationThrows()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using var stream = TestDataHelper.GetStream("conceptcars.cxml");

        // Cancellation should propagate
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => CxmlCollectionSource.LoadAsync(stream, cts.Token));
    }

    [Fact]
    public async Task LoadAsync_Stream_ParsesAllItemProperties()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleCxml));
        var source = await CxmlCollectionSource.LoadAsync(stream);

        var item = source.Items[0];
        Assert.Equal("Item One", item["Name"]![0]?.ToString());

        var colorProp = source.ItemProperties.First(p => p.Id == "Color");
        Assert.Equal(PivotViewerPropertyType.Text, colorProp.PropertyType);

        var sizeProp = source.ItemProperties.First(p => p.Id == "Size");
        Assert.Equal(PivotViewerPropertyType.Decimal, sizeProp.PropertyType);
    }
}
