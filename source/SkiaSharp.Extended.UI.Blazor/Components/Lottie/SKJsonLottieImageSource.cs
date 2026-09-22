using System.Text;

namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKJsonLottieImageSource : SKLottieImageSource
{
	private readonly string json;

	internal SKJsonLottieImageSource(string json)
	{
		this.json = json;
	}

	protected internal override Task<SKLottieAnimation> LoadAnimationAsync(HttpClient? httpClient, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json), writable: false);
		return Task.FromResult(new SKLottieAnimation(SKLottieAnimationLoader.Load(stream)));
	}

	internal override bool IsSameSource(SKLottieImageSource? other) =>
		other is SKJsonLottieImageSource source && StringComparer.Ordinal.Equals(json, source.json);
}
