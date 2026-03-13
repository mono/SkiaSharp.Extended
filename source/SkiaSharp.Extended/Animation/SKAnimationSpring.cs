using System;

namespace SkiaSharp.Extended;

/// <summary>
/// A spring-physics animator that smoothly transitions between values.
/// Uses spring physics to animate toward a target value, matching the feel of
/// natural spring motion (as seen in Silverlight's VisualStateKeySpline).
/// </summary>
/// <remarks>
/// The spring can be configured as critically damped (no overshoot), underdamped (bouncy),
/// or overdamped (sluggish) via the <see cref="DampingRatio"/> property.
/// For critical damping use an exact analytical solution; for other ratios it uses
/// a sub-stepped semi-implicit Euler integration for stability.
/// </remarks>
public class SKAnimationSpring
{
	private const double DefaultSpringStiffness = 100.0;
	private const double DefaultDampingRatio = 1.0; // critically damped

	private double _current;
	private double _target;
	private double _velocity;
	private double _stiffness;
	private double _dampingRatio;

	/// <summary>
	/// Initializes a new <see cref="SKAnimationSpring"/> starting at <paramref name="initialValue"/>.
	/// </summary>
	/// <param name="initialValue">The starting and initial target value.</param>
	public SKAnimationSpring(double initialValue = 0.0)
	{
		_current = initialValue;
		_target = initialValue;
		_velocity = 0;
		_stiffness = DefaultSpringStiffness;
		_dampingRatio = DefaultDampingRatio;
	}

	/// <summary>
	/// Gets or sets the current animated value.
	/// Setting this resets the velocity to zero.
	/// </summary>
	public double Current
	{
		get => _current;
		set
		{
			_current = value;
			_velocity = 0;
		}
	}

	/// <summary>Gets or sets the target value the spring is animating toward.</summary>
	public double Target
	{
		get => _target;
		set => _target = value;
	}

	/// <summary>Gets the current velocity of the spring.</summary>
	public double Velocity => _velocity;

	/// <summary>
	/// Gets or sets the spring stiffness. Higher values produce a faster snap.
	/// Minimum value is <c>0.1</c>.
	/// </summary>
	public double Stiffness
	{
		get => _stiffness;
		set => _stiffness = Math.Max(0.1, value);
	}

	/// <summary>
	/// Gets or sets the damping ratio.
	/// <list type="bullet">
	///   <item><term>1.0</term><description>Critically damped — smooth with no overshoot (default).</description></item>
	///   <item><term>&lt;1.0</term><description>Underdamped — bouncy, oscillates around target.</description></item>
	///   <item><term>&gt;1.0</term><description>Overdamped — slow to settle.</description></item>
	/// </list>
	/// Minimum value is <c>0.01</c>.
	/// </summary>
	public double DampingRatio
	{
		get => _dampingRatio;
		set => _dampingRatio = Math.Max(0.01, value);
	}

	/// <summary>
	/// Gets a value indicating whether the spring has settled (within epsilon of target with near-zero velocity).
	/// </summary>
	public bool IsSettled => Math.Abs(_current - _target) < 1e-6 && Math.Abs(_velocity) < 1e-6;

	/// <summary>
	/// Updates the spring by a time step. Uses the exact critically-damped spring solution
	/// for unconditional stability at any time step.
	/// </summary>
	/// <param name="deltaTime">Time step in seconds.</param>
	public void Update(double deltaTime)
	{
		if (deltaTime <= 0 || IsSettled)
		{
			if (IsSettled)
			{
				_current = _target;
				_velocity = 0;
			}
			return;
		}

		// Clamp delta to prevent instability after app pause or large gaps
		deltaTime = Math.Min(deltaTime, 0.1);

		double omega = Math.Sqrt(_stiffness); // natural frequency

		if (_dampingRatio >= 0.999 && _dampingRatio <= 1.001)
		{
			// Exact critically-damped solution (unconditionally stable)
			double displacement = _current - _target;
			double c1 = displacement;
			double c2 = _velocity + omega * displacement;
			double expTerm = Math.Exp(-omega * deltaTime);

			_current = _target + (c1 + c2 * deltaTime) * expTerm;
			_velocity = (c2 * (1.0 - omega * deltaTime) - omega * c1) * expTerm;
		}
		else
		{
			// Sub-stepped semi-implicit Euler for non-critically-damped springs
			double damping = 2.0 * _dampingRatio * omega;
			double subDt = Math.Min(deltaTime, 1.0 / (4.0 * omega));
			int steps = Math.Max(1, (int)Math.Ceiling(deltaTime / subDt));
			subDt = deltaTime / steps;

			for (int i = 0; i < steps; i++)
			{
				double displacement = _current - _target;
				double acceleration = -_stiffness * displacement - damping * _velocity;
				_velocity += acceleration * subDt;
				_current += _velocity * subDt;
			}
		}

		// Snap when very close to avoid infinite settling
		if (Math.Abs(_current - _target) < 1e-8 && Math.Abs(_velocity) < 1e-8)
		{
			_current = _target;
			_velocity = 0;
		}
	}

	/// <summary>Immediately sets current to target, stopping all animation.</summary>
	public void SnapToTarget()
	{
		_current = _target;
		_velocity = 0;
	}

	/// <summary>Resets to a new value with no animation (sets both current and target).</summary>
	/// <param name="value">The new current and target value.</param>
	public void Reset(double value)
	{
		_current = value;
		_target = value;
		_velocity = 0;
	}
}
