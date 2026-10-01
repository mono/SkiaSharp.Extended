namespace SkiaSharp.Extended.UI.Controls.Tests;

public class SKStreamLottieImageSourceTest : SKLottieImageSourceTest<SKStreamLottieImageSource>
{
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";
	private const string LoloJson = "TestAssets/Lottie/lolo.json";

	private static Task<Stream?> Load(string file) =>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		Task.Run(async () => (Stream?)File.OpenRead(file));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

	protected override SKStreamLottieImageSource CreateEmptyImageSource() =>
		new SKStreamLottieImageSource { };

	protected override SKStreamLottieImageSource CreateCompleteImageSource() =>
		new SKStreamLottieImageSource { Stream = token => Load(TrophyJson) };

	protected override void UpdateImageSource(SKStreamLottieImageSource imageSource, bool first) =>
		imageSource.Stream = token => Load(first ? TrophyJson : LoloJson);

	protected override void ResetImageSource(SKStreamLottieImageSource imageSource) =>
		imageSource.Stream = null;

	[Fact]
	public async Task LegacyStreamFactory_CreatesReplayableSource()
	{
		await using var input = File.OpenRead(TrophyJson);
		var source = Assert.IsType<SKStreamLottieImageSource>(SKLottieImageSource.FromStream(input));

		Assert.Equal(input.Length, input.Position);

		var first = await source.LoadAnimationAsync(TestContext.Current.CancellationToken);
		var second = await source.LoadAnimationAsync(TestContext.Current.CancellationToken);
		using var firstAnimation = first.Animation;
		using var secondAnimation = second.Animation;

		Assert.True(first.IsLoaded);
		Assert.True(second.IsLoaded);
	}

	[Fact]
	public async Task LegacyStreamFactory_DoesNotDependOnInputStreamAfterSnapshot()
	{
		SKLottieImageSource source;
		using (var input = File.OpenRead(TrophyJson))
		{
			source = Assert.IsType<SKStreamLottieImageSource>(SKLottieImageSource.FromStream(input));
		}

		var first = await source.LoadAnimationAsync(TestContext.Current.CancellationToken);
		var second = await source.LoadAnimationAsync(TestContext.Current.CancellationToken);
		using var firstAnimation = first.Animation;
		using var secondAnimation = second.Animation;

		Assert.True(first.IsLoaded);
		Assert.True(second.IsLoaded);
		Assert.NotSame(firstAnimation, secondAnimation);
	}
}
