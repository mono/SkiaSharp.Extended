#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// A lightweight, axis-aligned rectangle defined by position and size.
/// Replaces the former <c>SKImagePyramidRectI</c> (int) and <c>SKImagePyramidRectF</c> (float)
/// domain-specific types and the <c>(double X, double Y, double Width, double Height)</c> tuples
/// that previously appeared throughout the ImagePyramid pipeline.
/// </summary>
/// <typeparam name="T">Numeric element type (e.g. <see cref="int"/>, <see cref="float"/>, <see cref="double"/>).</typeparam>
/// <remarks>
/// Computed edges (<c>Right</c> / <c>Bottom</c>) are intentionally absent from this struct to
/// maintain <c>netstandard2.0</c> compatibility without requiring the .NET 7+
/// <c>System.Numerics.INumber&lt;T&gt;</c> constraint. Compute them inline:
/// <code>
/// float right  = rect.X + rect.Width;
/// float bottom = rect.Y + rect.Height;
/// </code>
/// </remarks>
public readonly record struct Rect<T>(T X, T Y, T Width, T Height) where T : struct;
