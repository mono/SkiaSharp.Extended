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
            _smoothingFactor = _smoothingFactor
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
