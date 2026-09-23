using System.Text;

namespace SkiaSharp.Extended.Tests;

public class SKLottieAnimationLoaderTest
{
	private const string MinimalLottieJson =
		"""{"v":"5.7.4","fr":60,"ip":0,"op":60,"w":100,"h":100,"nm":"test","ddd":0,"assets":[],"layers":[]}""";

	[Fact]
	public async Task LoadAsync_LoadsReadableStream()
	{
		await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MinimalLottieJson));
		using var animation = await SKLottieAnimationLoader.LoadAsync(stream, TestContext.Current.CancellationToken);

		Assert.Equal(TimeSpan.FromSeconds(1), animation.Duration);
	}

	[Fact]
	public void Load_RejectsUnreadableStream()
	{
		using var stream = new MemoryStream();
		stream.Close();

		Assert.Throws<ArgumentException>(() => SKLottieAnimationLoader.Load(stream));
	}
}
