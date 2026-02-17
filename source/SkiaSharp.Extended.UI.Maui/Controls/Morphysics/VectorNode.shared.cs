namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A scene node that renders vector paths with morphing support.
/// </summary>
public class VectorNode : SceneNode
{
	public static readonly BindableProperty PathDataProperty = BindableProperty.Create(
		nameof(PathData),
		typeof(string),
		typeof(VectorNode),
		string.Empty,
		propertyChanged: OnPathDataChanged);

	public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
		nameof(FillColor),
		typeof(Color),
		typeof(VectorNode),
		Colors.Black);

	public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
		nameof(StrokeColor),
		typeof(Color),
		typeof(VectorNode),
		Colors.Transparent);

	public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
		nameof(StrokeWidth),
		typeof(float),
		typeof(VectorNode),
		0f);

	public static readonly BindableProperty MorphProgressProperty = BindableProperty.Create(
		nameof(MorphProgress),
		typeof(float),
		typeof(VectorNode),
		0f,
		propertyChanged: OnMorphProgressChanged);

	private SKPath? originalPath;  // Keep original path for morphing
	private SKPath? currentPath;   // Current rendered path (may be morphed)
	private MorphTarget? currentMorphTarget;

	/// <summary>
	/// Gets or sets the SVG path data string.
	/// </summary>
	public string PathData
	{
		get => (string)GetValue(PathDataProperty);
		set => SetValue(PathDataProperty, value);
	}

	/// <summary>
	/// Gets or sets the fill color for the path.
	/// </summary>
	public Color FillColor
	{
		get => (Color)GetValue(FillColorProperty);
		set => SetValue(FillColorProperty, value);
	}

	/// <summary>
	/// Gets or sets the stroke color for the path.
	/// </summary>
	public Color StrokeColor
	{
		get => (Color)GetValue(StrokeColorProperty);
		set => SetValue(StrokeColorProperty, value);
	}

	/// <summary>
	/// Gets or sets the stroke width for the path.
	/// </summary>
	public float StrokeWidth
	{
		get => (float)GetValue(StrokeWidthProperty);
		set => SetValue(StrokeWidthProperty, value);
	}

	/// <summary>
	/// Gets or sets the morph progress (0.0 to 1.0).
	/// </summary>
	public float MorphProgress
	{
		get => (float)GetValue(MorphProgressProperty);
		set => SetValue(MorphProgressProperty, value);
	}

	/// <summary>
	/// Sets the morph target for this node.
	/// </summary>
	public void SetMorphTarget(MorphTarget? target)
	{
		currentMorphTarget = target;
		UpdateMorphedPath();
	}

	protected override void OnRender(SKCanvas canvas, SKSize size)
	{
		var pathToRender = currentPath ?? originalPath;
		if (pathToRender == null || pathToRender.PointCount == 0)
			return;

		// Fill
		if (FillColor.Alpha > 0)
		{
			using var fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = FillColor.ToSKColor(),
				IsAntialias = true
			};
			canvas.DrawPath(pathToRender, fillPaint);
		}

		// Stroke
		if (StrokeWidth > 0 && StrokeColor.Alpha > 0)
		{
			using var strokePaint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				Color = StrokeColor.ToSKColor(),
				StrokeWidth = StrokeWidth,
				IsAntialias = true
			};
			canvas.DrawPath(pathToRender, strokePaint);
		}
	}

	private static void OnPathDataChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is VectorNode node)
		{
			// Dispose old paths
			node.originalPath?.Dispose();
			node.currentPath?.Dispose();
			
			// Parse new path and keep it as original
			node.originalPath = string.IsNullOrEmpty(node.PathData) ? null : SKPath.ParseSvgPathData(node.PathData);
			node.currentPath = null;  // Will be set by UpdateMorphedPath if morphing
			
			node.UpdateMorphedPath();
		}
	}

	private static void OnMorphProgressChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is VectorNode node)
		{
			node.UpdateMorphedPath();
		}
	}

	private void UpdateMorphedPath()
	{
		// Dispose previous morphed path
		if (currentPath != null && currentPath != originalPath)
		{
			currentPath.Dispose();
			currentPath = null;
		}

		// If no morph target or no original path, use original as current
		if (currentMorphTarget == null || originalPath == null)
		{
			currentPath = originalPath;
			return;
		}

		// Apply morphing - ALWAYS from original path, never from previous morph!
		currentPath = currentMorphTarget.Interpolate(originalPath, MorphProgress);
	}
}
