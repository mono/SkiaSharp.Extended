using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Topten.RichTextKit;

namespace SkiaSharpDemo.Views;

public class DemoListItem : SKCanvasView
{
	public static readonly BindableProperty TitleProperty = BindableProperty.Create(
		nameof(Title), typeof(string), typeof(DemoListItem), null,
		propertyChanged: OnTitleUpdated);

	public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
		nameof(Description), typeof(string), typeof(DemoListItem), null,
		propertyChanged: OnDescriptionUpdated);

	public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
		nameof(Spacing), typeof(double), typeof(DemoListItem), 12.0,
		propertyChanged: OnInvalidate);

	public static readonly BindableProperty ShadowSizeProperty = BindableProperty.Create(
		nameof(ShadowSize), typeof(double), typeof(DemoListItem), 6.0,
		propertyChanged: OnInvalidate);

	public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(
		nameof(ShadowColor), typeof(Color), typeof(DemoListItem), Colors.Black.MultiplyAlpha(0.2f),
		propertyChanged: OnInvalidate);

	public static readonly BindableProperty ColorProperty = BindableProperty.Create(
		nameof(Color), typeof(Color), typeof(DemoListItem), Colors.Gray,
		propertyChanged: OnInvalidate);

	public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
		nameof(FontSize), typeof(double), typeof(DemoListItem), 16.0,
		propertyChanged: OnInvalidate);

	// TODO: make these bindable properties
	private const float CornerRadius = 12f;
	private const int LineCount = 3;
	private const float BorderWidth = 1f;

	private RichString? descString;
	private RichString? titleString;
	private float descriptionHeight;

	public DemoListItem()
	{
		IgnorePixelScaling = true;
	}

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string Description
	{
		get => (string)GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	public double Spacing
	{
		get => (double)GetValue(SpacingProperty);
		set => SetValue(SpacingProperty, value);
	}

	public double ShadowSize
	{
		get => (double)GetValue(ShadowSizeProperty);
		set => SetValue(ShadowSizeProperty, value);
	}

	public Color ShadowColor
	{
		get => (Color)GetValue(ShadowColorProperty);
		set => SetValue(ShadowColorProperty, value);
	}

	public Color Color
	{
		get => (Color)GetValue(ColorProperty);
		set => SetValue(ColorProperty, value);
	}

	public double FontSize
	{
		get => (double)GetValue(FontSizeProperty);
		set => SetValue(FontSizeProperty, value);
	}

	protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;

		var padding = (float)Spacing;
		var shadow = (float)ShadowSize;

		var rect = (SKRect)e.Info.Rect;
		rect.Right -= shadow + BorderWidth * 2;
		rect.Bottom -= shadow + BorderWidth * 2;

		canvas.Clear(SKColors.Transparent);

		canvas.Translate(BorderWidth, BorderWidth);

		// background parts
		using var rrect = new SKRoundRect(rect, CornerRadius);
		using var paint = new SKPaint
		{
			IsAntialias = true,
		};

		// shadow
		using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadow / 4);
		paint.MaskFilter = blur;
		paint.Color = ShadowColor.ToSKColor();
		canvas.Save();
		canvas.Translate(shadow / 2, shadow / 2);
		canvas.DrawRoundRect(rrect, paint);
		canvas.Restore();

		// background
		paint.Color = SKColors.White;
		paint.MaskFilter = null;
		canvas.DrawRoundRect(rrect, paint);

		// border
		if (BorderWidth > 0)
		{
			paint.Color = Color.ToSKColor();
			paint.StrokeWidth = BorderWidth;
			paint.Style = SKPaintStyle.Stroke;
			canvas.DrawRoundRect(rrect, paint);
		}

		// clip all the contents
		canvas.ClipRoundRect(rrect);

		// image
		using var imagePaint = new SKPaint
		{
			Color = Color.ToSKColor(),
		};
		var imageRect = SKRect.Create(100, rect.Height);
		canvas.DrawRect(imageRect, imagePaint);

		if (titleString != null)
		{
			// title
			var titlePos = new SKPoint(imageRect.Right + padding, padding);
			titleString.Paint(canvas, titlePos);

			if (descString != null)
			{
				// description
				descString.MaxWidth = rect.Width - padding - padding - imageRect.Right;
				var descPos = new SKPoint(titlePos.X, titlePos.Y + titleString.MeasuredHeight + padding / 2);
				descString.Paint(canvas, descPos);
			}
		}
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		heightConstraint =
			BorderWidth +
			Spacing +
			(titleString?.MeasuredHeight ?? 0) +
			Spacing / 2 +
			descriptionHeight +
			ShadowSize +
			Spacing +
			BorderWidth;

		return new Size(widthConstraint, heightConstraint);
	}

	private static void OnTitleUpdated(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is DemoListItem item)
		{
			item.titleString = new RichString()
				.FontFamily("Segoe UI")
				.Bold()
				.FontSize((float)item.FontSize)
				.Add(item.Title);
		}

		OnInvalidate(bindable, oldValue, newValue);
	}

	private static void OnDescriptionUpdated(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is DemoListItem item)
		{
			item.descString = new RichString()
				.FontFamily("Segoe UI")
				.LineHeight(1.1f)
				.FontSize((float)item.FontSize)
				.Add(item.Description);
			item.descString.MaxLines = LineCount;
			item.descriptionHeight = item.descString.MeasuredHeight * LineCount;
		}

		OnInvalidate(bindable, oldValue, newValue);
	}

	private static void OnInvalidate(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is DemoListItem item)
		{
			item.InvalidateMeasure();
			item.InvalidateSurface();
		}
	}
}
