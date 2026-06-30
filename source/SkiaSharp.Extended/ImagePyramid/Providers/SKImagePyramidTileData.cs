#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Encoded tile data that flows through the fetch/cache pipeline.
/// Wraps the raw encoded bytes (JPEG, PNG, etc.) with optional metadata.
/// </summary>
public sealed class SKImagePyramidTileData
{
    /// <summary>
    /// Creates tile data from encoded bytes.
    /// </summary>
    /// <param name="data">The raw encoded image bytes (JPEG, PNG, etc.).</param>
    public SKImagePyramidTileData(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <summary>The raw encoded image bytes (JPEG, PNG, etc.).</summary>
    public byte[] Data { get; }
}
