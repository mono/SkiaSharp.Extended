namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKJsonLottieImageSource : SKLottieImageSource
{
	private readonly string json;

	internal SKJsonLottieImageSource(string json)
	{
		this.json = json;
	}

	internal override Task<string> LoadJsonAsync(
		HttpClient? httpClient,
		CancellationToken cancellationToken) =>
		Task.FromResult(json);

	internal override bool EqualsCore(SKLottieImageSource other) =>
		StringComparer.Ordinal.Equals(json, ((SKJsonLottieImageSource)other).json);

	internal override int GetHashCodeCore() =>
		HashCode.Combine(typeof(SKJsonLottieImageSource), StringComparer.Ordinal.GetHashCode(json));
}
