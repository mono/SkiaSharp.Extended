using SkiaSharp.Extended;

namespace SkiaSharp.Extended.DeepZoom
{
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
