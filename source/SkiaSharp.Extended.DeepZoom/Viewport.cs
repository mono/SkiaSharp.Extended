using System;

namespace SkiaSharp.Extended.DeepZoom
{
    /// <summary>
    /// Manages the Deep Zoom viewport — the logical window into the image.
    /// Handles coordinate conversions between element (screen), logical (0-1 normalized),
    /// and image pixel spaces. Mirrors Silverlight's MultiScaleImage viewport behavior.
    /// </summary>
    public class Viewport
    {
        private double _viewportWidth;
        private double _viewportOriginX;
        private double _viewportOriginY;
        private double _controlWidth;
        private double _controlHeight;
        private double _aspectRatio;

        public Viewport()
        {
            _viewportWidth = 1.0;
            _viewportOriginX = 0.0;
            _viewportOriginY = 0.0;
            _controlWidth = 800;
            _controlHeight = 600;
            _aspectRatio = 1.0;
        }

        /// <summary>
        /// Logical width of the visible area. 1.0 = full image width fits in control.
        /// Less than 1.0 = zoomed in. Greater than 1.0 = zoomed out.
        /// Minimum width is clamped to prevent extreme zoom levels.
        /// </summary>
        public double ViewportWidth
        {
            get => _viewportWidth;
            set => _viewportWidth = Math.Max(MinViewportWidth, value);
        }

        /// <summary>Minimum viewport width to prevent extreme zoom (corresponds to ~1,000,000x zoom).</summary>
        public const double MinViewportWidth = 1e-6;

        /// <summary>Top-left X in logical coordinates (0-1 normalized to image width).</summary>
        public double ViewportOriginX
        {
            get => _viewportOriginX;
            set => _viewportOriginX = value;
        }

        /// <summary>Top-left Y in logical coordinates.</summary>
        public double ViewportOriginY
        {
            get => _viewportOriginY;
            set => _viewportOriginY = value;
        }

        /// <summary>Width of the control in screen pixels.</summary>
        public double ControlWidth
        {
            get => _controlWidth;
            set => _controlWidth = Math.Max(1, value);
        }

        /// <summary>Height of the control in screen pixels.</summary>
        public double ControlHeight
        {
            get => _controlHeight;
            set => _controlHeight = Math.Max(1, value);
        }

        /// <summary>Image aspect ratio (width/height).</summary>
        public double AspectRatio
        {
            get => _aspectRatio;
            set => _aspectRatio = Math.Max(double.Epsilon, value);
        }

        /// <summary>Pixels per logical unit — the effective scale factor.</summary>
        public double Scale => _controlWidth / _viewportWidth;

        /// <summary>The logical height visible, derived from ViewportWidth and control aspect ratio.</summary>
        public double ViewportHeight => _viewportWidth * _controlHeight / _controlWidth;

        /// <summary>Current zoom level (1.0 = fits width, higher = zoomed in).</summary>
        public double Zoom => 1.0 / _viewportWidth;

        /// <summary>Converts a screen-space point to logical coordinates.</summary>
        public (double X, double Y) ElementToLogicalPoint(double screenX, double screenY)
        {
            double scale = Scale;
            return (
                _viewportOriginX + screenX / scale,
                _viewportOriginY + screenY / scale
            );
        }

        /// <summary>Converts a logical point to screen-space coordinates.</summary>
        public (double X, double Y) LogicalToElementPoint(double logicalX, double logicalY)
        {
            double scale = Scale;
            return (
                (logicalX - _viewportOriginX) * scale,
                (logicalY - _viewportOriginY) * scale
            );
        }

        /// <summary>
        /// Zooms about a logical point. The point under the cursor stays fixed on screen.
        /// This is the core zoom algorithm from Silverlight's MultiScaleImage.ZoomAboutLogicalPoint.
        /// </summary>
        /// <param name="factor">Multiplicative zoom factor. &gt;1 = zoom in, &lt;1 = zoom out.</param>
        /// <param name="logicalX">Logical X of the anchor point.</param>
        /// <param name="logicalY">Logical Y of the anchor point.</param>
        public void ZoomAboutLogicalPoint(double factor, double logicalX, double logicalY)
        {
            if (factor <= 0) throw new ArgumentOutOfRangeException(nameof(factor));

            double newWidth = Math.Max(MinViewportWidth, _viewportWidth / factor);
            // Use effective factor to keep anchor point stable when zoom is clamped
            double effectiveFactor = _viewportWidth / newWidth;
            double newOriginX = logicalX - (logicalX - _viewportOriginX) / effectiveFactor;
            double newOriginY = logicalY - (logicalY - _viewportOriginY) / effectiveFactor;

            _viewportWidth = newWidth;
            _viewportOriginX = newOriginX;
            _viewportOriginY = newOriginY;
        }

        /// <summary>Pans by a delta in screen pixels.</summary>
        public void PanByScreenDelta(double deltaX, double deltaY)
        {
            double scale = Scale;
            _viewportOriginX -= deltaX / scale;
            _viewportOriginY -= deltaY / scale;
        }

        /// <summary>
        /// Returns the logical rectangle visible at a given ViewportWidth.
        /// Mirrors Silverlight's MultiScaleImage.GetZoomRect().
        /// </summary>
        public (double X, double Y, double Width, double Height) GetZoomRect(double viewportWidth)
        {
            double height = viewportWidth / AspectRatio;
            return (ViewportOriginX, ViewportOriginY, viewportWidth, height);
        }

        /// <summary>Gets the logical bounds of the current viewport.</summary>
        public (double Left, double Top, double Right, double Bottom) GetLogicalBounds()
        {
            double height = ViewportHeight;
            return (
                _viewportOriginX,
                _viewportOriginY,
                _viewportOriginX + _viewportWidth,
                _viewportOriginY + height
            );
        }

        /// <summary>
        /// Maximum viewport width (minimum zoom level). Default 1.0 means the image fills the control.
        /// Set higher (e.g., 10.0) to allow zooming out so the image appears as a small thumbnail.
        /// </summary>
        public double MaxViewportWidth { get; set; } = double.MaxValue;

        /// <summary>
        /// Constrains the viewport. When zoomed out past the image,
        /// the image is centered within the viewport.
        /// </summary>
        public void Constrain()
        {
            double imageLogicalHeight = 1.0 / _aspectRatio;

            // Clamp to max zoom-out
            if (_viewportWidth > MaxViewportWidth)
                _viewportWidth = MaxViewportWidth;

            double vpHeight = ViewportHeight;

            if (_viewportWidth <= 1.0)
            {
                // Zoomed in or exactly fitting: clamp origin to keep image in view
                if (_viewportOriginX < 0) _viewportOriginX = 0;
                if (_viewportOriginY < 0) _viewportOriginY = 0;
                if (_viewportOriginX + _viewportWidth > 1.0)
                    _viewportOriginX = 1.0 - _viewportWidth;
                if (_viewportOriginY + vpHeight > imageLogicalHeight)
                    _viewportOriginY = imageLogicalHeight - vpHeight;
                if (_viewportOriginX < 0) _viewportOriginX = 0;
                if (_viewportOriginY < 0) _viewportOriginY = 0;
            }
            else
            {
                // Zoomed out: center the image in the viewport
                _viewportOriginX = (1.0 - _viewportWidth) / 2.0;
                _viewportOriginY = (imageLogicalHeight - vpHeight) / 2.0;
            }
        }

        /// <summary>Creates a snapshot of the current viewport state.</summary>
        public ViewportState GetState()
        {
            return new ViewportState(_viewportWidth, _viewportOriginX, _viewportOriginY);
        }

        /// <summary>Restores a previously captured viewport state.</summary>
        public void SetState(ViewportState state)
        {
            _viewportWidth = state.ViewportWidth;
            _viewportOriginX = state.OriginX;
            _viewportOriginY = state.OriginY;
        }
    }

    /// <summary>Immutable snapshot of viewport position and zoom.</summary>
    public readonly struct ViewportState : IEquatable<ViewportState>
    {
        public ViewportState(double viewportWidth, double originX, double originY)
        {
            ViewportWidth = viewportWidth;
            OriginX = originX;
            OriginY = originY;
        }

        public double ViewportWidth { get; }
        public double OriginX { get; }
        public double OriginY { get; }

        public bool Equals(ViewportState other)
            => ViewportWidth == other.ViewportWidth && OriginX == other.OriginX && OriginY == other.OriginY;

        public override bool Equals(object? obj) => obj is ViewportState s && Equals(s);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ViewportWidth.GetHashCode();
                hash = hash * 31 + OriginX.GetHashCode();
                hash = hash * 31 + OriginY.GetHashCode();
                return hash;
            }
        }
        public static bool operator ==(ViewportState left, ViewportState right) => left.Equals(right);
        public static bool operator !=(ViewportState left, ViewportState right) => !left.Equals(right);
    }
}
