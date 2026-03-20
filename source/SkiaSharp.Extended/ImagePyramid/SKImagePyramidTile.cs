#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// An opaque tile that holds both the decoded image for rendering and the original
/// encoded bytes for storage. This allows disk and browser caches to persist tiles
/// without re-encoding, and handles forward-only streams by buffering at fetch time.
/// </summary>
public sealed class SKImagePyramidTile : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Creates a new tile from a decoded image and the original encoded bytes.
    /// </summary>
    /// <param name="image">The decoded SkiaSharp image for rendering.</param>
    /// <param name="rawData">The original encoded bytes (JPEG, PNG, etc.) for storage.</param>
    public SKImagePyramidTile(SKImage image, byte[] rawData)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        RawData = rawData ?? throw new ArgumentNullException(nameof(rawData));
    }

    /// <summary>The decoded image for rendering.</summary>
    public SKImage Image { get; }

    /// <summary>
    /// The original encoded bytes (JPEG, PNG, etc.) for disk or browser storage.
    /// Using these avoids re-encoding and preserves the original format and quality.
    /// </summary>
    public byte[] RawData { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Image.Dispose();
    }
}
