using System.Text;
using System.Runtime.Versioning;
using SkiaSharp.Extended.UI.Blazor.Components;

namespace SkiaSharp.Extended.UI.Blazor.Tests.Components.Lottie;

[SupportedOSPlatform("browser")]
public class SKLottieImageSourceTest
{
	[Fact]
	public async Task Bytes_AreCopiedAndComparedByValue()
	{
		var input = Encoding.UTF8.GetBytes("{\"v\":\"5.7.4\"}");
		var source = SKLottieImageSource.FromBytes(input);
		var equal = SKLottieImageSource.FromBytes(input);
		input[0] = (byte)'x';

		Assert.Equal(equal, source);
		Assert.Equal(
			"{\"v\":\"5.7.4\"}",
			await source.LoadJsonAsync(null, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task StreamFactory_ProvidesFreshStreamsAndDisposesThem()
	{
		var disposed = 0;
		var source = SKLottieImageSource.FromStream(_ =>
			new ValueTask<Stream>(new TrackingStream(Encoding.UTF8.GetBytes("{\"v\":\"5.7.4\"}"), () => disposed++)));

		Assert.Equal("{\"v\":\"5.7.4\"}", await source.LoadJsonAsync(null, TestContext.Current.CancellationToken));
		Assert.Equal("{\"v\":\"5.7.4\"}", await source.LoadJsonAsync(null, TestContext.Current.CancellationToken));
		Assert.Equal(2, disposed);
	}

	[Fact]
	public async Task UriWithoutHttpClient_ReportsConfigurationError()
	{
		var source = SKLottieImageSource.FromUri(new Uri("animations/test.json", UriKind.Relative));

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => source.LoadJsonAsync(null, TestContext.Current.CancellationToken));

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
			() => source.LoadJsonAsync(null, cancellation.Token));
	}

	[Fact]
	public void StringConversion_AlwaysCreatesUriSource()
	{
		SKLottieImageSource? source = "animations/test.json";

		Assert.NotNull(source);
		Assert.NotEqual(SKLottieImageSource.FromJson("animations/test.json"), source);
	}

	[Fact]
	public void StreamEquality_UsesDelegateEquality()
	{
		Func<CancellationToken, ValueTask<Stream>> factory =
			_ => new ValueTask<Stream>(new MemoryStream());
		Func<CancellationToken, ValueTask<Stream>> equalDelegate =
			_ => new ValueTask<Stream>(new MemoryStream());

		Assert.Equal(SKLottieImageSource.FromStream(factory), SKLottieImageSource.FromStream(factory));
		Assert.NotEqual(SKLottieImageSource.FromStream(factory), SKLottieImageSource.FromStream(equalDelegate));
	}

	[Fact]
	public void StreamEquality_MatchesSeparatelyCreatedDelegatesWithTheSameTargetAndMethod()
	{
		var factory = new StreamFactory();
		Func<CancellationToken, ValueTask<Stream>> first = factory.Create;
		Func<CancellationToken, ValueTask<Stream>> second = factory.Create;
		var firstSource = SKLottieImageSource.FromStream(first);
		var secondSource = SKLottieImageSource.FromStream(second);

		Assert.Equal(firstSource, secondSource);
		Assert.Equal(firstSource.GetHashCode(), secondSource.GetHashCode());
	}

	private sealed class StreamFactory
	{
		public ValueTask<Stream> Create(CancellationToken cancellationToken) =>
			new(new MemoryStream());
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
}
