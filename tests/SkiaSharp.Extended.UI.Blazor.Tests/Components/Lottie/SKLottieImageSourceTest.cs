using System.Text;
using System.Runtime.Versioning;
using SkiaSharp.Extended.UI.Blazor.Components;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Components.Lottie;

[SupportedOSPlatform("browser")]
public class SKLottieImageSourceTest
{
	private const string MinimalLottieJson =
		"""{"v":"5.7.4","fr":60,"ip":0,"op":60,"w":100,"h":100,"nm":"test","ddd":0,"assets":[],"layers":[]}""";

	[Fact]
	public async Task StreamFactory_ProvidesFreshStreamsAndDisposesThem()
	{
		var disposed = 0;
		var source = SKLottieImageSource.FromStream(_ =>
			new ValueTask<Stream>(new TrackingStream(Encoding.UTF8.GetBytes(MinimalLottieJson), () => disposed++)));

		var first = await source.LoadAnimationAsync(null, TestContext.Current.CancellationToken);
		var second = await source.LoadAnimationAsync(null, TestContext.Current.CancellationToken);
		using var firstAnimation = first.Animation;
		using var secondAnimation = second.Animation;

		Assert.True(first.IsLoaded);
		Assert.True(second.IsLoaded);
		Assert.Equal(TimeSpan.FromSeconds(1), firstAnimation!.Duration);
		Assert.Equal(TimeSpan.FromSeconds(1), secondAnimation!.Duration);
		Assert.Equal(2, disposed);
	}

	[Fact]
	public async Task JsonSource_BuildsAnimation()
	{
		var source = SKLottieImageSource.FromJson(MinimalLottieJson);

		var result = await source.LoadAnimationAsync(null, TestContext.Current.CancellationToken);
		using var animation = result.Animation;

		Assert.True(result.IsLoaded);
		Assert.Equal(TimeSpan.FromSeconds(1), animation!.Duration);
	}

	[Fact]
	public async Task UriWithoutHttpClient_ReportsConfigurationError()
	{
		var source = SKLottieImageSource.FromUri(new Uri("animations/test.json", UriKind.Relative));

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => source.LoadAnimationAsync(null, TestContext.Current.CancellationToken));

		Assert.Contains("HttpClient", exception.Message);
	}

	[Fact]
	public async Task StreamFactory_ReceivesCancellation()
	{
		var source = SKLottieImageSource.FromStream(token =>
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<Stream>(new MemoryStream());
		});
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => source.LoadAnimationAsync(null, cancellation.Token));
	}

	[Fact]
	public void StringConversion_AlwaysCreatesUriSource()
	{
		SKLottieImageSource? source = "animations/test.json";

		Assert.IsType<SKUriLottieImageSource>(source);
	}

	[Fact]
	public void EquivalentBuiltInSources_AreRecognized()
	{
		var uri = SKLottieImageSource.FromUri(new Uri("animations/test.json", UriKind.Relative));
		var sameUri = SKLottieImageSource.FromUri(new Uri("animations/test.json", UriKind.Relative));
		var json = SKLottieImageSource.FromJson(MinimalLottieJson);
		var sameJson = SKLottieImageSource.FromJson(MinimalLottieJson);
		Func<CancellationToken, ValueTask<Stream>> streamFactory =
			_ => new ValueTask<Stream>(new MemoryStream());
		var stream = SKLottieImageSource.FromStream(streamFactory);
		var sameStream = SKLottieImageSource.FromStream(streamFactory);

		Assert.True(uri.IsSameSource(sameUri));
		Assert.True(json.IsSameSource(sameJson));
		Assert.True(stream.IsSameSource(sameStream));
		Assert.False(uri.IsSameSource(json));
	}

	[Fact]
	public async Task CustomSource_CanOverrideLoading()
	{
		var source = new CustomLottieImageSource();

		var result = await source.LoadAnimationAsync(null, TestContext.Current.CancellationToken);
		using var animation = result.Animation;

		Assert.True(result.IsLoaded);
		Assert.Equal(TimeSpan.FromSeconds(1), animation!.Duration);
	}

	private sealed class TrackingStream(byte[] bytes, Action onDispose) : MemoryStream(bytes)
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				onDispose();
			base.Dispose(disposing);
		}
	}

	private sealed class CustomLottieImageSource : SKLottieImageSource
	{
		protected internal override Task<SKLottieAnimation> LoadAnimationAsync(HttpClient? httpClient, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var animation = SkiaSharp.Skottie.Animation.Parse(MinimalLottieJson)
				?? throw new InvalidOperationException("Failed to parse test animation.");
			return Task.FromResult(new SKLottieAnimation(animation));
		}
	}
}
