using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Describes how a Lottie animation repeats. Use the static factory members
/// <see cref="Never"/>, <see cref="Restart"/>, and <see cref="Reverse"/> to
/// create instances.
/// </summary>
public readonly struct SKLottieRepeat : IEquatable<SKLottieRepeat>
{
	private enum RepeatKind { Never, Restart, Reverse }

	private readonly RepeatKind kind;
	private readonly int count;

	private SKLottieRepeat(RepeatKind kind, int count)
	{
		this.kind = kind;
		this.count = count;
	}

	/// <summary>The animation plays once without repeating.</summary>
	public static SKLottieRepeat Never => new(RepeatKind.Never, 0);

	/// <summary>
	/// The animation repeats by restarting from the beginning.
	/// </summary>
	/// <param name="count">Number of additional plays after the first. Use -1 for infinite.</param>
	public static SKLottieRepeat Restart(int count = -1) => new(RepeatKind.Restart, count);

	/// <summary>
	/// The animation repeats by reversing direction (ping-pong).
	/// </summary>
	/// <param name="count">Number of additional plays after the first. Use -1 for infinite.</param>
	public static SKLottieRepeat Reverse(int count = -1) => new(RepeatKind.Reverse, count);

	/// <summary>Gets whether the animation repeats at all.</summary>
	public bool IsRepeating => kind != RepeatKind.Never;

	/// <summary>Gets whether the animation repeats by restarting from the beginning.</summary>
	public bool IsRestartRepeating => kind == RepeatKind.Restart;

	/// <summary>Gets whether the animation repeats by reversing direction (ping-pong).</summary>
	public bool IsReverseRepeating => kind == RepeatKind.Reverse;

	/// <summary>
	/// Gets the number of additional plays after the first. -1 means infinite.
	/// Returns 0 for <see cref="Never"/>.
	/// </summary>
	public int Count => count;

	/// <inheritdoc />
	public bool Equals(SKLottieRepeat other) => kind == other.kind && count == other.count;

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is SKLottieRepeat other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => (int)kind * 397 ^ count;

	/// <inheritdoc />
	public static bool operator ==(SKLottieRepeat left, SKLottieRepeat right) => left.Equals(right);

	/// <inheritdoc />
	public static bool operator !=(SKLottieRepeat left, SKLottieRepeat right) => !left.Equals(right);
}
