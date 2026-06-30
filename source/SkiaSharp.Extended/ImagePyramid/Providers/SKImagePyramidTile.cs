#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A decoded tile held in the controller's render buffer. Tiles live above the decode
/// gate and carry only the decoded image — encoded bytes never travel past the controller.
/// </summary>
public sealed class SKImagePyramidTile : IDisposable
{
	private bool _disposed;

	/// <summary>
	/// Creates a tile from a decoded image.
	/// </summary>
	/// <param name="image">The decoded image used for rendering.</param>
	public SKImagePyramidTile(SKImage image)
	{
		Image = image ?? throw new ArgumentNullException(nameof(image));
	}

	/// <summary>
	/// The decoded image used for rendering.
	/// </summary>
	public SKImage Image { get; }

	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;
		Image.Dispose();
	}
}
