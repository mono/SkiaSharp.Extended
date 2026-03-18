#nullable enable

using System;

namespace SkiaSharp.Extended;

/// <summary>
/// Manages the Deep Zoom viewport — the logical window into the image.
/// Handles coordinate conversions between element (screen), logical (0-1 normalized),
/// and image pixel spaces. Mirrors Silverlight's MultiScaleImage viewport behavior.
/// </summary>
public class SKImagePyramidViewport
{
    private double _viewportWidth;
    private double _viewportOriginX;
    private double _viewportOriginY;
    private double _controlWidth;
    private double _controlHeight;
    private double _aspectRatio;

    public SKImagePyramidViewport()
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
    public Point<double> ElementToLogicalPoint(double screenX, double screenY)
    {
        double scale = Scale;
        return new Point<double>(
            _viewportOriginX + screenX / scale,
            _viewportOriginY + screenY / scale
        );
    }

    /// <summary>Converts a logical point to screen-space coordinates.</summary>
    public Point<double> LogicalToElementPoint(double logicalX, double logicalY)
    {
        double scale = Scale;
        return new Point<double>(
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
    public Rect<double> GetZoomRect(double viewportWidth)
    {
        double height = viewportWidth / AspectRatio;
        return new Rect<double>(ViewportOriginX, ViewportOriginY, viewportWidth, height);
    }

    /// <summary>Gets the logical bounds of the current viewport (Left, Top, Right, Bottom).</summary>
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
            if (_viewportOriginX + _viewportWidth > 1.0)
                _viewportOriginX = 1.0 - _viewportWidth;
            if (_viewportOriginX < 0) _viewportOriginX = 0;

            // Vertical: center if viewport is taller than image, otherwise clamp
            if (vpHeight >= imageLogicalHeight)
            {
                _viewportOriginY = (imageLogicalHeight - vpHeight) / 2.0;
            }
            else
            {
                if (_viewportOriginY < 0) _viewportOriginY = 0;
                if (_viewportOriginY + vpHeight > imageLogicalHeight)
                    _viewportOriginY = imageLogicalHeight - vpHeight;
                if (_viewportOriginY < 0) _viewportOriginY = 0;
            }
        }
        else
        {
            // Zoomed out: center the image in the viewport
            _viewportOriginX = (1.0 - _viewportWidth) / 2.0;
            _viewportOriginY = (imageLogicalHeight - vpHeight) / 2.0;
        }
    }

    /// <summary>
    /// Sets the viewport to "fit" mode — the entire image is visible and centered.
    /// Does not alter <see cref="MaxViewportWidth"/>; callers may zoom out past fit.
    /// </summary>
    public void FitToView()
    {
        // Image in logical space: width = 1.0, height = 1.0 / AspectRatio
        double imageLogicalHeight = 1.0 / _aspectRatio;

        // Fit ViewportWidth = max(1.0, imageLogicalHeight * controlWidth / controlHeight)
        // This ensures both dimensions are fully visible.
        double fitWidth = (_controlHeight > 0)
            ? Math.Max(1.0, imageLogicalHeight * _controlWidth / _controlHeight)
            : 1.0;

        _viewportWidth = fitWidth;

        if (fitWidth > 1.0)
        {
            // Image is taller than control — center horizontally (image narrower than viewport)
            _viewportOriginX = (1.0 - fitWidth) / 2.0;
            double vpHeight = fitWidth * _controlHeight / _controlWidth;
            _viewportOriginY = (imageLogicalHeight - vpHeight) / 2.0;
        }
        else
        {
            // Image fits in width (fitWidth = 1.0). Center vertically if viewport is taller than image.
            _viewportOriginX = 0;
            double vpHeight = _controlHeight / _controlWidth;
            _viewportOriginY = (imageLogicalHeight - vpHeight) / 2.0; // negative when image is shorter
        }
    }

    /// <summary>Creates a snapshot of the current viewport state.</summary>
    public SKImagePyramidViewportState GetState()
    {
        return new SKImagePyramidViewportState(_viewportWidth, _viewportOriginX, _viewportOriginY);
    }

    /// <summary>Restores a previously captured viewport state.</summary>
    public void SetState(SKImagePyramidViewportState state)
    {
        _viewportWidth = state.ViewportWidth;
        _viewportOriginX = state.OriginX;
        _viewportOriginY = state.OriginY;
    }
}
