#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// A decorator that tries several inner providers in order and returns the first non-null
/// result. Use it for hybrid origins, for example an app-packaged source with an HTTP
/// fallback.
/// </summary>
public sealed class SKCompositeTileProvider : ISKImagePyramidTileProvider
{
	private readonly ISKImagePyramidTileProvider[] _providers;

	/// <summary>
	/// Creates a composite provider that queries each inner provider in order.
	/// </summary>
	/// <param name="providers">The providers to try, in priority order.</param>
	public SKCompositeTileProvider(params ISKImagePyramidTileProvider[] providers)
	{
		if (providers is null || providers.Length == 0)
			throw new ArgumentException("At least one provider is required.", nameof(providers));

		_providers = providers;
	}

	/// <inheritdoc />
	public async Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		foreach (var provider in _providers)
		{
			var result = await provider.GetTileAsync(url, ct).ConfigureAwait(false);
			if (result is not null)
				return result;
		}

		return null;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		foreach (var provider in _providers)
			provider.Dispose();
	}
}
