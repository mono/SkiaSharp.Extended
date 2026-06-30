#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Extended;

/// <summary>
/// An origin provider that reads encoded tile bytes from the local filesystem. Accepts
/// plain paths or <c>file://</c> URIs.
/// </summary>
public sealed class SKFileTileProvider : ISKImagePyramidTileProvider
{
	/// <inheritdoc />
	public Task<SKImagePyramidTileData?> GetTileAsync(string url, CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		string path;
		if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				path = new Uri(url).LocalPath;
			}
			catch
			{
				return Task.FromResult<SKImagePyramidTileData?>(null);
			}
		}
		else
		{
			path = url;
		}

		if (!File.Exists(path))
			return Task.FromResult<SKImagePyramidTileData?>(null);

		try
		{
			var bytes = File.ReadAllBytes(path);
			return Task.FromResult<SKImagePyramidTileData?>(new SKImagePyramidTileData(bytes));
		}
		catch
		{
			return Task.FromResult<SKImagePyramidTileData?>(null);
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
	}
}
