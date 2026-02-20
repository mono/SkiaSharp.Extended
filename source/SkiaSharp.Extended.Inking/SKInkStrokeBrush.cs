using System;
using SkiaSharp;

namespace SkiaSharp.Extended.Inking;

/// <summary>
/// Defines the visual appearance properties for ink strokes.
/// </summary>
public class SKInkStrokeBrush
{
    private SKColor _color = SKColors.Black;
    private SKSize _minSize = new SKSize(2, 2);
    private SKSize _maxSize = new SKSize(6, 6);
    private SKStrokeCapStyle _capStyle = SKStrokeCapStyle.Round;
    private SKSmoothingAlgorithm _smoothingAlgorithm = SKSmoothingAlgorithm.CatmullRom;
    private int _smoothingFactor = 4;
    private SKVelocityMode _velocityMode = SKVelocityMode.None;
    private float _velocityScale = 0.5f;

    /// <summary>
    /// Gets or sets the stroke color.
    /// </summary>
    public SKColor Color
    {
        get => _color;
        set => _color = value;
    }

    /// <summary>
    /// Gets or sets the minimum stroke size (at zero pressure).
    /// The width is used for horizontal strokes, height for vertical.
    /// For isotropic strokes, use the same value for both.
    /// </summary>
    public SKSize MinSize
    {
        get => _minSize;
        set
        {
            if (value.Width < 0 || value.Height < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Size cannot be negative");
            _minSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum stroke size (at full pressure).
    /// </summary>
    public SKSize MaxSize
    {
        get => _maxSize;
        set
        {
            if (value.Width < 0 || value.Height < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Size cannot be negative");
            _maxSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the cap style for stroke endpoints.
    /// </summary>
    public SKStrokeCapStyle CapStyle
    {
        get => _capStyle;
        set => _capStyle = value;
    }

    /// <summary>
    /// Gets or sets the smoothing algorithm for curve interpolation.
    /// </summary>
    public SKSmoothingAlgorithm SmoothingAlgorithm
    {
        get => _smoothingAlgorithm;
        set => _smoothingAlgorithm = value;
    }

    /// <summary>
    /// Gets or sets the smoothing factor (1-10). Higher values produce smoother curves.
    /// </summary>
    public int SmoothingFactor
    {
        get => _smoothingFactor;
        set
        {
            if (value < 1 || value > 10)
                throw new ArgumentOutOfRangeException(nameof(value), "SmoothingFactor must be between 1 and 10");
            _smoothingFactor = value;
        }
    }

    /// <summary>
    /// Gets or sets the velocity mode, which determines how pen velocity affects stroke appearance.
    /// </summary>
    public SKVelocityMode VelocityMode
    {
        get => _velocityMode;
        set => _velocityMode = value;
    }

    /// <summary>
    /// Gets or sets the velocity scale factor (0.0 to 1.0).
    /// Controls how strongly velocity affects stroke width.
    /// 0 = no velocity effect, 1 = maximum velocity effect.
    /// Default is 0.5.
    /// </summary>
    public float VelocityScale
    {
        get => _velocityScale;
        set => _velocityScale = Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Creates a default brush with black color and standard sizes.
    /// </summary>
    public SKInkStrokeBrush()
    {
    }

    /// <summary>
    /// Creates a brush with the specified color.
    /// </summary>
    public SKInkStrokeBrush(SKColor color)
    {
        _color = color;
    }

    /// <summary>
    /// Creates a brush with the specified color and sizes.
    /// </summary>
    public SKInkStrokeBrush(SKColor color, SKSize minSize, SKSize maxSize)
    {
        _color = color;
        MinSize = minSize;
        MaxSize = maxSize;
    }

    /// <summary>
    /// Creates a brush with isotropic (equal width/height) sizes.
    /// </summary>
    public SKInkStrokeBrush(SKColor color, float minWidth, float maxWidth)
    {
        _color = color;
        MinSize = new SKSize(minWidth, minWidth);
        MaxSize = new SKSize(maxWidth, maxWidth);
    }

    /// <summary>
    /// Creates an independent copy of this brush.
    /// </summary>
    public SKInkStrokeBrush Clone()
    {
        return new SKInkStrokeBrush
        {
            _color = _color,
            _minSize = _minSize,
            _maxSize = _maxSize,
            _capStyle = _capStyle,
            _smoothingAlgorithm = _smoothingAlgorithm,
            _smoothingFactor = _smoothingFactor,
            _velocityMode = _velocityMode,
            _velocityScale = _velocityScale
        };
    }

    /// <summary>
    /// Calculates the stroke width for a given pressure value.
    /// </summary>
    /// <param name="pressure">Pressure from 0.0 to 1.0</param>
    /// <returns>The interpolated width based on pressure</returns>
    public float GetWidthForPressure(float pressure)
    {
        pressure = Clamp(pressure, 0f, 1f);
        return _minSize.Width + (_maxSize.Width - _minSize.Width) * pressure;
    }

    /// <summary>
    /// Calculates the stroke width for given pressure and velocity values.
    /// The velocity effect depends on the VelocityMode setting.
    /// </summary>
    /// <param name="pressure">Pressure from 0.0 to 1.0</param>
    /// <param name="velocity">Velocity in pixels per millisecond</param>
    /// <returns>The interpolated width based on pressure and velocity</returns>
    public float GetWidthForPressureAndVelocity(float pressure, float velocity)
    {
        // Base width from pressure
        var baseWidth = GetWidthForPressure(pressure);

        if (_velocityMode == SKVelocityMode.None || _velocityScale == 0f)
            return baseWidth;

        // Normalize velocity to a 0-1 range.
        // Typical handwriting velocity ranges from ~0.1 to ~5 pixels/ms
        // We'll use a reference velocity of 2 pixels/ms as "normal"
        const float ReferenceVelocity = 2.0f;
        var normalizedVelocity = Clamp(velocity / ReferenceVelocity, 0f, 2f) / 2f; // 0 to 1

        // Calculate velocity factor based on mode
        float velocityFactor;
        switch (_velocityMode)
        {
            case SKVelocityMode.BallpointPen:
                // Faster = thinner: at max velocity, reduce width by velocityScale
                velocityFactor = 1f - normalizedVelocity * _velocityScale;
                break;
            case SKVelocityMode.Pencil:
                // Pencil mode: same as ballpoint for width
                velocityFactor = 1f - normalizedVelocity * _velocityScale;
                break;
            default:
                velocityFactor = 1f;
                break;
        }

        // Clamp factor to reasonable range
        velocityFactor = Clamp(velocityFactor, 0.2f, 1f);

        return baseWidth * velocityFactor;
    }

    /// <summary>
    /// Calculates the color for given velocity in Pencil mode.
    /// Returns a lighter color for faster strokes.
    /// </summary>
    /// <param name="velocity">Velocity in pixels per millisecond</param>
    /// <returns>The adjusted color based on velocity</returns>
    public SKColor GetColorForVelocity(float velocity)
    {
        if (_velocityMode != SKVelocityMode.Pencil || _velocityScale == 0f)
            return _color;

        // Normalize velocity (same as width calculation)
        const float ReferenceVelocity = 2.0f;
        var normalizedVelocity = Clamp(velocity / ReferenceVelocity, 0f, 2f) / 2f;

        // Faster = lighter (reduce alpha)
        var alphaFactor = 1f - normalizedVelocity * _velocityScale * 0.5f; // Max 50% lighter
        alphaFactor = Clamp(alphaFactor, 0.5f, 1f);

        return _color.WithAlpha((byte)(_color.Alpha * alphaFactor));
    }

    /// <summary>
    /// Calculates the stroke height for a given pressure value.
    /// </summary>
    /// <param name="pressure">Pressure from 0.0 to 1.0</param>
    /// <returns>The interpolated height based on pressure</returns>
    public float GetHeightForPressure(float pressure)
    {
        pressure = Clamp(pressure, 0f, 1f);
        return _minSize.Height + (_maxSize.Height - _minSize.Height) * pressure;
    }

    /// <summary>
    /// Calculates the stroke size for a given pressure value.
    /// </summary>
    /// <param name="pressure">Pressure from 0.0 to 1.0</param>
    /// <returns>The interpolated size based on pressure</returns>
    public SKSize GetSizeForPressure(float pressure)
    {
        return new SKSize(GetWidthForPressure(pressure), GetHeightForPressure(pressure));
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
