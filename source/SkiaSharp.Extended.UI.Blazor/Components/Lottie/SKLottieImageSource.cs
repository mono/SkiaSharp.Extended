using System.Text;

namespace SkiaSharp.Extended.UI.Blazor.Components;

/// <summary>
/// An immutable, browser-specific source for Lottie JSON animation data.
/// </summary>
public abstract class SKLottieImageSource : IEquatable<SKLottieImageSource>
{
	internal SKLottieImageSource()
	{
	}

	/// <summary>Creates a source that downloads Lottie JSON from a URI.</summary>
	public static SKLottieImageSource FromUri(Uri uri) =>
		new SKUriLottieImageSource(uri ?? throw new ArgumentNullException(nameof(uri)));

	/// <summary>Creates a source from raw Lottie JSON.</summary>
	public static SKLottieImageSource FromJson(string json) =>
		new SKJsonLottieImageSource(json ?? throw new ArgumentNullException(nameof(json)));

	/// <summary>Creates a source from a private copy of UTF-encoded Lottie JSON bytes.</summary>
	public static SKLottieImageSource FromBytes(ReadOnlyMemory<byte> bytes) =>
		new SKBytesLottieImageSource(bytes.ToArray());

	/// <summary>
	/// Creates a source that obtains a fresh, readable stream for every load.
	/// The component disposes each returned stream after reading it.
	/// </summary>
	public static SKLottieImageSource FromStream(
		Func<CancellationToken, ValueTask<Stream>> streamFactory) =>
		new SKStreamLottieImageSource(streamFactory ?? throw new ArgumentNullException(nameof(streamFactory)));

	/// <summary>
	/// Converts a URI string to a source. Strings always represent relative or absolute URIs,
	/// never raw JSON.
	/// </summary>
	public static implicit operator SKLottieImageSource?(string? value) =>
		value is null ? null : FromUri(new Uri(value, UriKind.RelativeOrAbsolute));

	/// <inheritdoc />
	public bool Equals(SKLottieImageSource? other) =>
		ReferenceEquals(this, other) ||
		(other is not null &&
		 GetType() == other.GetType() &&
		 EqualsCore(other));

	/// <inheritdoc />
	public override bool Equals(object? obj) => Equals(obj as SKLottieImageSource);

	/// <inheritdoc />
	public override int GetHashCode() => GetHashCodeCore();

	internal abstract Task<string> LoadJsonAsync(
		HttpClient? httpClient,
		CancellationToken cancellationToken);

	internal abstract bool EqualsCore(SKLottieImageSource other);

	internal abstract int GetHashCodeCore();

	internal static async Task<string> ReadJsonAsync(
		Stream stream,
		CancellationToken cancellationToken)
	{
		if (!stream.CanRead)
			throw new InvalidOperationException("The Lottie stream must be readable.");

		using var reader = new StreamReader(
			stream,
			Encoding.UTF8,
			detectEncodingFromByteOrderMarks: true,
			bufferSize: 1024,
			leaveOpen: true);
		return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
	}

}
