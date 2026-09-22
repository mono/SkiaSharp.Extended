namespace SkiaSharp.Extended.UI.Blazor.Components;

internal sealed class SKBytesLottieImageSource : SKLottieImageSource
{
	private readonly byte[] bytes;

	internal SKBytesLottieImageSource(byte[] bytes)
	{
		this.bytes = bytes;
	}

	internal override async Task<string> LoadJsonAsync(
		HttpClient? httpClient,
		CancellationToken cancellationToken)
	{
		await using var stream = new MemoryStream(bytes, writable: false);
		return await ReadJsonAsync(stream, cancellationToken).ConfigureAwait(false);
	}

	internal override bool EqualsCore(SKLottieImageSource other) =>
		bytes.AsSpan().SequenceEqual(((SKBytesLottieImageSource)other).bytes);

	internal override int GetHashCodeCore()
	{
		var hash = new HashCode();
		hash.Add(typeof(SKBytesLottieImageSource));
		foreach (var value in bytes)
			hash.Add(value);
		return hash.ToHashCode();
	}
}
