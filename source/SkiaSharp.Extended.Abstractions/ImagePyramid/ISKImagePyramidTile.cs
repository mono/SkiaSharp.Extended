#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Opaque handle representing a decoded tile image.
/// Concrete implementations (e.g. <c>SKImagePyramidBitmapTile</c> in SkiaSharp.Extended) wrap
/// the actual decoded image in a rendering-backend-specific type.
/// </summary>
public interface ISKImagePyramidTile : IDisposable { }
