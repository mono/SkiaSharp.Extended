using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A collection of standard easing functions for use in animations.
/// Each function takes a normalized time value <c>t</c> in [0, 1] and returns
/// an eased value in [0, 1].
/// </summary>
public static class SKAnimationEasing
{
	/// <summary>Linear easing — no acceleration or deceleration.</summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double Linear(double t) => t;

	/// <summary>
	/// Cubic ease-out — starts fast and decelerates toward the end.
	/// Formula: <c>1 - (1 - t)³</c>
	/// </summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double CubicOut(double t) => 1.0 - Math.Pow(1.0 - t, 3);

	/// <summary>
	/// Quadratic ease-in — starts slow and accelerates.
	/// Formula: <c>t²</c>
	/// </summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double QuadIn(double t) => t * t;

	/// <summary>
	/// Quadratic ease-out — starts fast and decelerates.
	/// Formula: <c>t(2 - t)</c>
	/// </summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double QuadOut(double t) => t * (2.0 - t);

	/// <summary>
	/// Quadratic ease-in-out — accelerates then decelerates.
	/// </summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double QuadInOut(double t) => t < 0.5 ? 2.0 * t * t : -1.0 + (4.0 - 2.0 * t) * t;

	/// <summary>
	/// Cubic ease-in — starts slow and accelerates.
	/// Formula: <c>t³</c>
	/// </summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double CubicIn(double t) => t * t * t;

	/// <summary>
	/// Cubic ease-in-out — accelerates then decelerates with cubic curve.
	/// </summary>
	/// <param name="t">Normalized time in [0, 1].</param>
	public static double CubicInOut(double t) => t < 0.5 ? 4.0 * t * t * t : 1.0 - Math.Pow(-2.0 * t + 2.0, 3) / 2.0;
}
