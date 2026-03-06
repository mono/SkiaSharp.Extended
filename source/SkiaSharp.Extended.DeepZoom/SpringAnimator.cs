using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// A spring-physics animator that smoothly transitions between values.
    /// Used for viewport zoom and pan animations, matching Silverlight's
    /// spring behavior from VisualStateKeySpline.
    /// </summary>
    public class SpringAnimator
    {
        // Spring constants extracted from Silverlight's VisualStateKeySpline
        // Critical damping ratio for smooth deceleration
        private const double DefaultSpringStiffness = 100.0;
        private const double DefaultDampingRatio = 1.0; // critically damped

        private double _current;
        private double _target;
        private double _velocity;
        private double _stiffness;
        private double _dampingRatio;

        public SpringAnimator(double initialValue = 0.0)
        {
            _current = initialValue;
            _target = initialValue;
            _velocity = 0;
            _stiffness = DefaultSpringStiffness;
            _dampingRatio = DefaultDampingRatio;
        }

        /// <summary>Current animated value.</summary>
        public double Current
        {
            get => _current;
            set
            {
                _current = value;
                _velocity = 0;
            }
        }

        /// <summary>Target value the spring is animating toward.</summary>
        public double Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>Current velocity.</summary>
        public double Velocity => _velocity;

        /// <summary>Spring stiffness. Higher = faster snap.</summary>
        public double Stiffness
        {
            get => _stiffness;
            set => _stiffness = Math.Max(0.1, value);
        }

        /// <summary>Damping ratio. 1.0 = critically damped (no overshoot).</summary>
        public double DampingRatio
        {
            get => _dampingRatio;
            set => _dampingRatio = Math.Max(0.0, value);
        }

        /// <summary>Whether the spring has settled (within epsilon of target with near-zero velocity).</summary>
        public bool IsSettled => Math.Abs(_current - _target) < 1e-6 && Math.Abs(_velocity) < 1e-6;

        /// <summary>
        /// Updates the spring by a time step. Uses the exact critically-damped
        /// spring solution for unconditional stability at any time step.
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

            // Clamp delta to prevent extreme values
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

            // Snap to target when very close
            if (Math.Abs(_current - _target) < 1e-8 && Math.Abs(_velocity) < 1e-8)
            {
                _current = _target;
                _velocity = 0;
            }
        }

        /// <summary>Immediately sets current to target with no animation.</summary>
        public void SnapToTarget()
        {
            _current = _target;
            _velocity = 0;
        }

        /// <summary>Resets to a new value with no animation.</summary>
        public void Reset(double value)
        {
            _current = value;
            _target = value;
            _velocity = 0;
        }
    }

    /// <summary>
    /// Manages three springs (X, Y, width) for animating viewport transitions.
    /// </summary>
    public class ViewportSpring
    {
        public ViewportSpring()
        {
            OriginX = new SpringAnimator(0.0);
            OriginY = new SpringAnimator(0.0);
            Width = new SpringAnimator(1.0);
        }

        public SpringAnimator OriginX { get; }
        public SpringAnimator OriginY { get; }
        public SpringAnimator Width { get; }

        /// <summary>
        /// Spring stiffness applied to all three axes. Higher = faster snap, lower = slower/smoother.
        /// Default is 100.0.
        /// </summary>
        public double Stiffness
        {
            get => Width.Stiffness;
            set
            {
                OriginX.Stiffness = value;
                OriginY.Stiffness = value;
                Width.Stiffness = value;
            }
        }

        /// <summary>
        /// Damping ratio applied to all three axes.
        /// 1.0 = critically damped (no overshoot), &lt;1.0 = underdamped (bouncy), &gt;1.0 = overdamped (sluggish).
        /// Default is 1.0.
        /// </summary>
        public double DampingRatio
        {
            get => Width.DampingRatio;
            set
            {
                OriginX.DampingRatio = value;
                OriginY.DampingRatio = value;
                Width.DampingRatio = value;
            }
        }

        /// <summary>Whether all three springs have settled.</summary>
        public bool IsSettled => OriginX.IsSettled && OriginY.IsSettled && Width.IsSettled;

        /// <summary>Updates all three springs by the given time step.</summary>
        public void Update(double deltaTime)
        {
            OriginX.Update(deltaTime);
            OriginY.Update(deltaTime);
            Width.Update(deltaTime);
        }

        /// <summary>Immediately snaps all springs to their targets.</summary>
        public void SnapToTarget()
        {
            OriginX.SnapToTarget();
            OriginY.SnapToTarget();
            Width.SnapToTarget();
        }

        /// <summary>Sets the target viewport state for spring animation.</summary>
        public void SetTarget(double originX, double originY, double width)
        {
            OriginX.Target = originX;
            OriginY.Target = originY;
            Width.Target = width;
        }

        /// <summary>Resets all springs to a new state with no animation.</summary>
        public void Reset(double originX, double originY, double width)
        {
            OriginX.Reset(originX);
            OriginY.Reset(originY);
            Width.Reset(width);
        }

        /// <summary>Gets the current animated viewport state.</summary>
        public ViewportState GetCurrentState()
        {
            return new ViewportState(Width.Current, OriginX.Current, OriginY.Current);
        }
    }
}
