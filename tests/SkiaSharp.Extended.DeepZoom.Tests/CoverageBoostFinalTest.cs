using SkiaSharp;
using SkiaSharp.Extended.DeepZoom;
using System.Net;
using System.Net.Http;
using Xunit;

namespace SkiaSharp.Extended.DeepZoom.Tests;

public class CoverageBoostFinalTest
{
    // --- SpringAnimator: snap-to-target branch (lines 101-105) ---

    [Fact]
    public void SpringAnimator_SnapToTarget_WhenNearlySettled()
    {
        // Run the spring until nearly settled, then call Update once more
        // to trigger the IsSettled snap path at lines 78-83.
        var spring = new SpringAnimator(0.0);
        spring.Target = 1.0;

        for (int i = 0; i < 10000; i++)
            spring.Update(1.0 / 60.0);

        // One more Update triggers the IsSettled check and snaps to target
        spring.Update(1.0 / 60.0);

        Assert.Equal(1.0, spring.Current);
        Assert.Equal(0.0, spring.Velocity);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void SpringAnimator_SnapToTarget_SmallDisplacement()
    {
        // Start very close to target so the inner snap check (lines 101-105)
        // triggers within the physics step, before the outer IsSettled check.
        var spring = new SpringAnimator(1.0 - 1e-9);
        spring.Target = 1.0;

        // With displacement ~1e-9 and velocity 0, the first update should
        // compute tiny displacement/velocity that triggers the 1e-8 snap.
        spring.Update(1.0 / 60.0);

        Assert.Equal(1.0, spring.Current);
        Assert.Equal(0.0, spring.Velocity);
    }

    // --- FileTileFetcher: OperationCanceledException rethrown (lines 96-98) ---

    [Fact]
    public async Task FileTileFetcher_CancelledTokenDuringFetch_ThrowsOperationCanceledException()
    {
        var fetcher = new FileTileFetcher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Cancellation at the start should throw OperationCanceledException
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.FetchTileAsync("/some/path.png", cts.Token));
    }

    // --- FileTileFetcher: Generic exception returns null (lines 100-102) ---

    [Fact]
    public async Task FileTileFetcher_CorruptedFilePath_ReturnsNull()
    {
        var fetcher = new FileTileFetcher();

        // A file URI with invalid characters causes an exception in Uri parsing
        // that should be caught by the generic catch and return null.
        // Use a path with embedded null bytes or illegal URI chars.
        var result = await fetcher.FetchTileAsync("file://\0invalid\0path");
        Assert.Null(result);
    }

    // --- HttpTileFetcher: non-success status code (lines 43-44) ---

    [Fact]
    public async Task HttpTileFetcher_NonSuccessStatusCode_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var client = new HttpClient(handler);
        var fetcher = new HttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    [Fact]
    public async Task HttpTileFetcher_InternalServerError_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "error");
        var client = new HttpClient(handler);
        var fetcher = new HttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    // --- HttpTileFetcher: exception path (HttpRequestException) ---

    [Fact]
    public async Task HttpTileFetcher_HttpRequestException_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Network error"));
        var client = new HttpClient(handler);
        var fetcher = new HttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    // --- HttpTileFetcher: TaskCanceledException path ---

    [Fact]
    public async Task HttpTileFetcher_TaskCanceledException_ReturnsNull()
    {
        var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("Timeout"));
        var client = new HttpClient(handler);
        var fetcher = new HttpTileFetcher(client);

        var result = await fetcher.FetchTileAsync("http://example.com/tile.png");
        Assert.Null(result);

        fetcher.Dispose();
        client.Dispose();
    }

    // --- SpringAnimator snap: verify exact snap after many iterations ---

    [Fact]
    public void SpringAnimator_LargeTarget_SnapsExactly()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = 100.0;

        // Run many frames, then one more to trigger the snap via IsSettled
        for (int i = 0; i < 20000; i++)
            spring.Update(1.0 / 60.0);
        spring.Update(1.0 / 60.0);

        Assert.Equal(100.0, spring.Current);
        Assert.Equal(0.0, spring.Velocity);
        Assert.True(spring.IsSettled);
    }

    [Fact]
    public void SpringAnimator_NegativeTarget_SnapsExactly()
    {
        var spring = new SpringAnimator(0.0);
        spring.Target = -50.0;

        for (int i = 0; i < 20000; i++)
            spring.Update(1.0 / 60.0);
        spring.Update(1.0 / 60.0);

        Assert.Equal(-50.0, spring.Current);
        Assert.True(spring.IsSettled);
    }

    // --- Mock handlers for HttpTileFetcher testing ---

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }

    private class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw _exception;
        }
    }
}
