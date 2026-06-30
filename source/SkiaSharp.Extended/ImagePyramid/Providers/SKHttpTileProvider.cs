#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// An origin provider that fetches encoded tile bytes over HTTP.
/// </summary>
public sealed class SKHttpTileProvider : ISKImagePyramidTileProvider
{
	private readonly HttpClient _http;
	private readonly bool _ownsHttp;

	/// <summary>
	/// Creates an HTTP tile provider.
	/// </summary>
	/// <param name="httpClient">
	/// An optional <see cref="HttpClient"/> to use. When <see langword="null"/>, an internal
	/// client is created and disposed together with this provider.
	/// </param>
	public SKHttpTileProvider(HttpClient? httpClient = null)
	{
		_ownsHttp = httpClient is null;
		_http = httpClient ?? new HttpClient();
	}

	/// <inheritdoc />
	public async Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		try
		{
			using var response = await _http
				.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
				.ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
				return null;

#if NETSTANDARD2_0
			var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#else
			var bytes = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
#endif

			return new SKImagePyramidTileData(bytes);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch
		{
			// Network errors and HTTP timeouts are treated as a missing tile.
			return null;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_ownsHttp)
			_http.Dispose();
	}
}
