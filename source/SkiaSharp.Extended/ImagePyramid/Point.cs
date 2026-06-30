#nullable enable

namespace SkiaSharp.Extended;

/// <summary>
/// A lightweight 2-D point.
/// Replaces the <c>(double X, double Y)</c> tuples that previously appeared in viewport
/// and sub-image coordinate-conversion methods of the ImagePyramid pipeline.
/// </summary>
/// <typeparam name="T">Numeric element type (e.g. <see cref="double"/>, <see cref="float"/>).</typeparam>
public readonly record struct Point<T>(T X, T Y) where T : struct;
