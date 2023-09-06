using System.Windows.Input;

namespace SkiaSharp.Extended.UI.Controls;

public class SKSignatureSurfaceView : SKSurfaceView
{
	public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
		nameof(StrokeColor),
		typeof(Color),
		typeof(SKSignatureSurfaceView),
		Colors.Black);

	public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
		nameof(StrokeWidth),
		typeof(double),
		typeof(SKSignatureSurfaceView),
		2.0);

	public static readonly BindableProperty ClearedCommandProperty = BindableProperty.Create(
		nameof(ClearedCommand),
		typeof(ICommand),
		typeof(SKSignatureSurfaceView),
		default(ICommand));

	public static readonly BindableProperty ClearedCommandParameterProperty = BindableProperty.Create(
		nameof(ClearedCommandParameter),
		typeof(object),
		typeof(SKSignatureSurfaceView),
		null);

	public static readonly BindableProperty StrokeCompletedCommandProperty = BindableProperty.Create(
		nameof(StrokeCompletedCommand),
		typeof(ICommand),
		typeof(SKSignatureSurfaceView),
		default(ICommand));

	public static readonly BindableProperty StrokeCompletedCommandParameterProperty = BindableProperty.Create(
		nameof(StrokeCompletedCommandParameter),
		typeof(object),
		typeof(SKSignatureSurfaceView),
		null);

	internal static readonly BindablePropertyKey IsBlankPropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsBlank),
		typeof(bool),
		typeof(SKSignatureSurfaceView),
		true);

	public static readonly BindableProperty IsBlankProperty = IsBlankPropertyKey.BindableProperty;

	private readonly SKPaint inkPaint =
		new()
		{
			Color = SKColors.Black,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2,
		};

	private readonly List<SKInkPath> inkPaths = new();

	SKInkPath? currentInkPath;

	public SKSignatureSurfaceView()
	{
		ResourceLoader<Themes.SKSignatureSurfaceViewResources>.EnsureRegistered(this);

		EnableTouchEvents = true;
	}

	public bool IsBlank => inkPaths.Count == 0;

	public float StrokeWidth
	{
		get => (float)GetValue(StrokeWidthProperty);
		set => SetValue(StrokeWidthProperty, value);
	}

	public Color StrokeColor
	{
		get => (Color)GetValue(StrokeColorProperty);
		set => SetValue(StrokeColorProperty, value);
	}

	// public IEnumerable<Point> Points
	// {
	// 	get { return GetSignaturePoints(); }
	// 	set { SetSignaturePoints(value); }
	// }

	// public IEnumerable<IEnumerable<Point>> Strokes
	// {
	// 	get => GetSignatureStrokes();
	// 	set => SetSignatureStrokes(value);
	// }

	public ICommand ClearedCommand
	{
		get => (ICommand)GetValue(ClearedCommandProperty);
		set => SetValue(ClearedCommandProperty, value);
	}

	public object ClearedCommandParameter
	{
		get => GetValue(ClearedCommandParameterProperty);
		set => SetValue(ClearedCommandParameterProperty, value);
	}

	public ICommand StrokeCompletedCommand
	{
		get => (ICommand)GetValue(StrokeCompletedCommandProperty);
		set => SetValue(StrokeCompletedCommandProperty, value);
	}

	public object StrokeCompletedCommandParameter
	{
		get => GetValue(StrokeCompletedCommandParameterProperty);
		set => SetValue(StrokeCompletedCommandParameterProperty, value);
	}

	protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
		canvas.Clear(SKColors.White);

		foreach (var ink in inkPaths)
		{
			if (ink.Path is SKPath previous)
				canvas.DrawPath(previous, inkPaint);
		}

		if (currentInkPath?.Path is SKPath current)
			canvas.DrawPath(current, inkPaint);
	}

	protected override void OnTouch(SKTouchEventArgs e)
	{
		switch (e.ActionType)
		{
			case SKTouchAction.Pressed:
			case SKTouchAction.Moved:
				if (e.InContact)
				{
					DrawStroke(e.Location, false);
					e.Handled = true;
					Invalidate();
				}
				break;
			case SKTouchAction.Released:
				DrawStroke(e.Location, false);
				CompleteStroke();
				e.Handled = true;
				Invalidate();
				break;
			case SKTouchAction.Cancelled:
				ResetStroke();
				e.Handled = true;
				Invalidate();
				break;
		}

		base.OnTouch(e);
	}

	private void DrawStroke(SKPoint point, bool isLastPoint)
	{
		currentInkPath ??= new();
		currentInkPath.AddPoint(point, isLastPoint);
	}

	private void CompleteStroke()
	{
		if (currentInkPath is not null)
			inkPaths.Add(currentInkPath);

		ResetStroke();
	}

	private void ResetStroke()
	{
		currentInkPath = null;
	}
}
