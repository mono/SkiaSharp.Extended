#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// An opaque tile that holds the decoded image for rendering and optionally
/// the original encoded bytes for storage.
/// </summary>
public sealed class SKImagePyramidTile : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Creates a new tile from a decoded image and optional raw bytes.
    /// </summary>
    /// <param name="image">The decoded SkiaSharp image for rendering.</param>
    /// <param name="rawData">The original encoded bytes (JPEG, PNG, etc.) for storage. Null if not needed.</param>
    public SKImagePyramidTile(SKImage image, byte[]? rawData = null)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        RawData = rawData;
    }

    /// <summary>The decoded image for rendering.</summary>
    public SKImage Image { get; }

    /// <summary>
    /// The original encoded bytes (JPEG, PNG, etc.) for disk or browser storage.
    /// Null when the tile was created for render-only use without persistence.
    /// </summary>
    public byte[]? RawData { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Image.Dispose();
    }
}
