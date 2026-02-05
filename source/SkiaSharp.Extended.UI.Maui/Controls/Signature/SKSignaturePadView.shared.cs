using System.Windows.Input;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// A signature pad control that provides fluid, pressure-sensitive ink rendering.
/// Supports stylus/pen pressure for variable stroke width (harder pressure = thicker lines).
/// Uses quadratic Bezier curves to smooth jagged input from finger or pen movement.
/// </summary>
public class SKSignaturePadView : SKSurfaceView, IDisposable
{
	/// <summary>
	/// Bindable property for the stroke color.
	/// </summary>
	public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
		nameof(StrokeColor),
		typeof(Color),
		typeof(SKSignaturePadView),
		Colors.Black,
		propertyChanged: OnStrokeColorChanged);

	/// <summary>
	/// Bindable property for the minimum stroke width (at zero pressure).
	/// </summary>
	public static readonly BindableProperty MinStrokeWidthProperty = BindableProperty.Create(
		nameof(MinStrokeWidth),
		typeof(float),
		typeof(SKSignaturePadView),
		1f,
		propertyChanged: OnStrokeWidthChanged);

	/// <summary>
	/// Bindable property for the maximum stroke width (at full pressure).
	/// </summary>
	public static readonly BindableProperty MaxStrokeWidthProperty = BindableProperty.Create(
		nameof(MaxStrokeWidth),
		typeof(float),
		typeof(SKSignaturePadView),
		8f,
		propertyChanged: OnStrokeWidthChanged);

	/// <summary>
	/// Bindable property for the pad background color.
	/// </summary>
	public static readonly BindableProperty PadBackgroundColorProperty = BindableProperty.Create(
		nameof(PadBackgroundColor),
		typeof(Color),
		typeof(SKSignaturePadView),
		Colors.White,
		propertyChanged: OnPadBackgroundColorChanged);

	/// <summary>
	/// Bindable property for the command executed when strokes are cleared.
	/// </summary>
	public static readonly BindableProperty ClearedCommandProperty = BindableProperty.Create(
		nameof(ClearedCommand),
		typeof(ICommand),
		typeof(SKSignaturePadView),
		default(ICommand));

	/// <summary>
	/// Bindable property for the parameter passed to the ClearedCommand.
	/// </summary>
	public static readonly BindableProperty ClearedCommandParameterProperty = BindableProperty.Create(
		nameof(ClearedCommandParameter),
		typeof(object),
		typeof(SKSignaturePadView),
		null);

	/// <summary>
	/// Bindable property for the command executed when a stroke is completed.
	/// </summary>
	public static readonly BindableProperty StrokeCompletedCommandProperty = BindableProperty.Create(
		nameof(StrokeCompletedCommand),
		typeof(ICommand),
		typeof(SKSignaturePadView),
		default(ICommand));

	/// <summary>
	/// Bindable property for the parameter passed to the StrokeCompletedCommand.
	/// </summary>
	public static readonly BindableProperty StrokeCompletedCommandParameterProperty = BindableProperty.Create(
		nameof(StrokeCompletedCommandParameter),
		typeof(object),
		typeof(SKSignaturePadView),
		null);

	/// <summary>
	/// Internal bindable property key for the read-only IsBlank property.
	/// </summary>
	internal static readonly BindablePropertyKey IsBlankPropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsBlank),
		typeof(bool),
		typeof(SKSignaturePadView),
		true);

	/// <summary>
	/// Bindable property for the IsBlank property.
	/// </summary>
	public static readonly BindableProperty IsBlankProperty = IsBlankPropertyKey.BindableProperty;

	private readonly List<SKInkStroke> strokes = new();
	private readonly SKPaint strokePaint;
	private SKInkStroke? currentStroke;
	private SKColor skStrokeColor = SKColors.Black;
	private SKColor skBackgroundColor = SKColors.White;
	private SKCanvasView? canvasView;
	private SKGLView? glView;
	private long? activeTouchId;
	private bool isDisposed;

	/// <summary>
	/// Creates a new signature pad view.
	/// </summary>
	public SKSignaturePadView()
	{
		ResourceLoader<Themes.SKSignaturePadViewResources>.EnsureRegistered(this);

		strokePaint = new SKPaint
		{
			Color = skStrokeColor,
			Style = SKPaintStyle.Fill,
			IsAntialias = true
		};
	}

	/// <summary>
	/// Occurs when all strokes are cleared from the pad.
	/// </summary>
	public event EventHandler? Cleared;

	/// <summary>
	/// Occurs when a stroke is completed.
	/// </summary>
	public event EventHandler<SKSignatureStrokeCompletedEventArgs>? StrokeCompleted;

	/// <summary>
	/// Gets whether the signature pad is blank (has no strokes).
	/// </summary>
	public bool IsBlank
	{
		get => (bool)GetValue(IsBlankProperty);
		private set => SetValue(IsBlankPropertyKey, value);
	}

	/// <summary>
	/// Gets or sets the stroke color.
	/// </summary>
	public Color StrokeColor
	{
		get => (Color)GetValue(StrokeColorProperty);
		set => SetValue(StrokeColorProperty, value);
	}

	/// <summary>
	/// Gets or sets the minimum stroke width (at zero pressure).
	/// </summary>
	public float MinStrokeWidth
	{
		get => (float)GetValue(MinStrokeWidthProperty);
		set => SetValue(MinStrokeWidthProperty, value);
	}

	/// <summary>
	/// Gets or sets the maximum stroke width (at full pressure).
	/// </summary>
	public float MaxStrokeWidth
	{
		get => (float)GetValue(MaxStrokeWidthProperty);
		set => SetValue(MaxStrokeWidthProperty, value);
	}

	/// <summary>
	/// Gets or sets the background color of the signature pad.
	/// </summary>
	public Color PadBackgroundColor
	{
		get => (Color)GetValue(PadBackgroundColorProperty);
		set => SetValue(PadBackgroundColorProperty, value);
	}

	/// <summary>
	/// Gets or sets the command executed when strokes are cleared.
	/// </summary>
	public ICommand? ClearedCommand
	{
		get => (ICommand?)GetValue(ClearedCommandProperty);
		set => SetValue(ClearedCommandProperty, value);
	}

	/// <summary>
	/// Gets or sets the parameter passed to the ClearedCommand.
	/// </summary>
	public object? ClearedCommandParameter
	{
		get => GetValue(ClearedCommandParameterProperty);
		set => SetValue(ClearedCommandParameterProperty, value);
	}

	/// <summary>
	/// Gets or sets the command executed when a stroke is completed.
	/// </summary>
	public ICommand? StrokeCompletedCommand
	{
		get => (ICommand?)GetValue(StrokeCompletedCommandProperty);
		set => SetValue(StrokeCompletedCommandProperty, value);
	}

	/// <summary>
	/// Gets or sets the parameter passed to the StrokeCompletedCommand.
	/// </summary>
	public object? StrokeCompletedCommandParameter
	{
		get => GetValue(StrokeCompletedCommandParameterProperty);
		set => SetValue(StrokeCompletedCommandParameterProperty, value);
	}

	/// <summary>
	/// Gets the number of strokes in the signature.
	/// </summary>
	public int StrokeCount => strokes.Count;

	/// <summary>
	/// Clears all strokes from the signature pad.
	/// </summary>
	public void Clear()
	{
		foreach (var stroke in strokes)
		{
			stroke.Dispose();
		}
		strokes.Clear();

		currentStroke?.Dispose();
		currentStroke = null;

		UpdateIsBlank();
		Invalidate();

		Cleared?.Invoke(this, EventArgs.Empty);

		if (ClearedCommand?.CanExecute(ClearedCommandParameter) == true)
		{
			ClearedCommand.Execute(ClearedCommandParameter);
		}
	}

	/// <summary>
	/// Removes the last stroke from the signature pad (undo).
	/// </summary>
	/// <returns>True if a stroke was removed, false if there were no strokes.</returns>
	public bool Undo()
	{
		if (strokes.Count == 0)
			return false;

		var lastStroke = strokes[^1];
		strokes.RemoveAt(strokes.Count - 1);
		lastStroke.Dispose();

		UpdateIsBlank();
		Invalidate();

		return true;
	}

	/// <summary>
	/// Gets a combined path of all strokes.
	/// </summary>
	/// <returns>An SKPath containing all strokes, or null if the pad is blank.</returns>
	public SKPath? ToPath()
	{
		if (strokes.Count == 0)
			return null;

		var combinedPath = new SKPath();
		foreach (var stroke in strokes)
		{
			if (stroke.Path is SKPath path)
			{
				combinedPath.AddPath(path);
			}
		}

		return combinedPath;
	}

	/// <summary>
	/// Renders the signature to an SKImage.
	/// </summary>
	/// <param name="width">The width of the output image.</param>
	/// <param name="height">The height of the output image.</param>
	/// <param name="backgroundColor">The background color, or null for transparent.</param>
	/// <returns>An SKImage containing the rendered signature.</returns>
	public SKImage? ToImage(int width, int height, SKColor? backgroundColor = null)
	{
		if (strokes.Count == 0)
			return null;

		var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
		using var surface = SKSurface.Create(info);

		if (surface == null)
			return null;

		var canvas = surface.Canvas;

		if (backgroundColor.HasValue)
		{
			canvas.Clear(backgroundColor.Value);
		}
		else
		{
			canvas.Clear(SKColors.Transparent);
		}

		// Calculate scale to fit the signature in the image
		var bounds = GetStrokeBounds();
		if (bounds.IsEmpty)
			return null;

		var scale = Math.Min(width / bounds.Width, height / bounds.Height) * 0.9f;
		var offsetX = (width - bounds.Width * scale) / 2f - bounds.Left * scale;
		var offsetY = (height - bounds.Height * scale) / 2f - bounds.Top * scale;

		canvas.Translate(offsetX, offsetY);
		canvas.Scale(scale);

		// Draw all strokes
		foreach (var stroke in strokes)
		{
			if (stroke.Path is SKPath path)
			{
				canvas.DrawPath(path, strokePaint);
			}
		}

		return surface.Snapshot();
	}

	/// <summary>
	/// Gets the bounding rectangle of all strokes.
	/// </summary>
	/// <returns>The bounding rectangle.</returns>
	public SKRect GetStrokeBounds()
	{
		if (strokes.Count == 0)
			return SKRect.Empty;

		var bounds = SKRect.Empty;
		foreach (var stroke in strokes)
		{
			if (stroke.Path is SKPath path)
			{
				var pathBounds = path.Bounds;
				if (bounds.IsEmpty)
				{
					bounds = pathBounds;
				}
				else
				{
					bounds = SKRect.Union(bounds, pathBounds);
				}
			}
		}

		return bounds;
	}

	/// <inheritdoc/>
	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		// Unsubscribe from previous views to prevent event handler accumulation
		if (canvasView is not null)
		{
			canvasView.Touch -= OnCanvasTouch;
			canvasView = null;
		}

		if (glView is not null)
		{
			glView.Touch -= OnGLViewTouch;
			glView = null;
		}

		// Get access to the underlying drawing surface to enable touch events
		var templateChild = GetTemplateChild("PART_DrawingSurface");

		if (templateChild is SKCanvasView view)
		{
			canvasView = view;
			canvasView.EnableTouchEvents = true;
			canvasView.Touch += OnCanvasTouch;
		}
		else if (templateChild is SKGLView gl)
		{
			glView = gl;
			glView.EnableTouchEvents = true;
			glView.Touch += OnGLViewTouch;
		}
	}

	/// <inheritdoc/>
	protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
		// Clear with background color
		canvas.Clear(skBackgroundColor);

		// Draw all completed strokes
		foreach (var stroke in strokes)
		{
			if (stroke.Path is SKPath path)
			{
				canvas.DrawPath(path, strokePaint);
			}
		}

		// Draw the current stroke being drawn
		if (currentStroke?.Path is SKPath currentPath)
		{
			canvas.DrawPath(currentPath, strokePaint);
		}
	}

	/// <summary>
	/// Handles touch events on the canvas view.
	/// </summary>
	private void OnCanvasTouch(object? sender, SKTouchEventArgs e)
	{
		var scale = Width > 0 ? (float)(canvasView?.CanvasSize.Width / Width ?? 1) : 1f;
		HandleTouchEvent(e, scale);
	}

	/// <summary>
	/// Handles touch events on the GL view.
	/// </summary>
	private void OnGLViewTouch(object? sender, SKTouchEventArgs e)
	{
		var scale = Width > 0 ? (float)(glView?.CanvasSize.Width / Width ?? 1) : 1f;
		HandleTouchEvent(e, scale);
	}

	/// <summary>
	/// Common touch event handling logic.
	/// </summary>
	private void HandleTouchEvent(SKTouchEventArgs e, float scale)
	{
		var location = new SKPoint(e.Location.X / scale, e.Location.Y / scale);
		var pressure = e.Pressure;
		var touchId = e.Id;

		switch (e.ActionType)
		{
			case SKTouchAction.Pressed:
				// Only accept the first touch - reject multi-touch
				if (activeTouchId.HasValue)
				{
					// Already have an active touch, ignore this one but mark as handled
					e.Handled = true;
					return;
				}
				activeTouchId = touchId;
				StartStroke(location, pressure);
				e.Handled = true;
				break;

			case SKTouchAction.Moved:
				// Only process if this is our active touch
				if (activeTouchId != touchId)
				{
					e.Handled = true;
					return;
				}
				if (e.InContact)
				{
					ContinueStroke(location, pressure);
					e.Handled = true;
				}
				break;

			case SKTouchAction.Released:
				// Only process if this is our active touch
				if (activeTouchId != touchId)
				{
					e.Handled = true;
					return;
				}
				activeTouchId = null;
				EndStroke(location, pressure);
				e.Handled = true;
				break;

			case SKTouchAction.Cancelled:
				// Only process if this is our active touch
				if (activeTouchId != touchId)
				{
					e.Handled = true;
					return;
				}
				activeTouchId = null;
				CancelStroke();
				e.Handled = true;
				break;
		}
	}

	private void StartStroke(SKPoint point, float pressure)
	{
		currentStroke = new SKInkStroke(MinStrokeWidth, MaxStrokeWidth);
		currentStroke.AddPoint(point, pressure);
		Invalidate();
	}

	private void ContinueStroke(SKPoint point, float pressure)
	{
		currentStroke?.AddPoint(point, pressure);
		Invalidate();
	}

	private void EndStroke(SKPoint point, float pressure)
	{
		if (currentStroke != null)
		{
			currentStroke.AddPoint(point, pressure, isLastPoint: true);

			if (!currentStroke.IsEmpty)
			{
				strokes.Add(currentStroke);
				UpdateIsBlank();

				StrokeCompleted?.Invoke(this, new SKSignatureStrokeCompletedEventArgs(strokes.Count));

				if (StrokeCompletedCommand?.CanExecute(StrokeCompletedCommandParameter) == true)
				{
					StrokeCompletedCommand.Execute(StrokeCompletedCommandParameter);
				}
			}
			else
			{
				currentStroke.Dispose();
			}

			currentStroke = null;
		}

		Invalidate();
	}

	private void CancelStroke()
	{
		currentStroke?.Dispose();
		currentStroke = null;
		Invalidate();
	}

	private void UpdateIsBlank()
	{
		IsBlank = strokes.Count == 0;
	}

	private static void OnStrokeColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKSignaturePadView view && newValue is Color color)
		{
			view.skStrokeColor = new SKColor(
				(byte)(color.Red * 255),
				(byte)(color.Green * 255),
				(byte)(color.Blue * 255),
				(byte)(color.Alpha * 255));
			view.strokePaint.Color = view.skStrokeColor;
			view.Invalidate();
		}
	}

	private static void OnStrokeWidthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		// Stroke width changes will affect new strokes only
		// No need to invalidate existing strokes
	}

	private static void OnPadBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKSignaturePadView view && newValue is Color color)
		{
			view.skBackgroundColor = new SKColor(
				(byte)(color.Red * 255),
				(byte)(color.Green * 255),
				(byte)(color.Blue * 255),
				(byte)(color.Alpha * 255));
			view.Invalidate();
		}
	}

	/// <summary>
	/// Releases all resources used by this control.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases the unmanaged resources used by the control and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
			return;

		if (disposing)
		{
			// Unsubscribe from touch events
			if (canvasView is not null)
			{
				canvasView.Touch -= OnCanvasTouch;
				canvasView = null;
			}

			if (glView is not null)
			{
				glView.Touch -= OnGLViewTouch;
				glView = null;
			}

			// Dispose all strokes
			foreach (var stroke in strokes)
			{
				stroke.Dispose();
			}
			strokes.Clear();

			// Dispose current stroke
			currentStroke?.Dispose();
			currentStroke = null;

			// Dispose paint
			strokePaint.Dispose();
		}

		isDisposed = true;
	}
}
