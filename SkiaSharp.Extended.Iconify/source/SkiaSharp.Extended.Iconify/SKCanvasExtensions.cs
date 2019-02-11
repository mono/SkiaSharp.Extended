using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Iconify
{
	public static class SKCanvasExtensions
	{
		public static void DrawText(this SKCanvas canvas, IEnumerable<SKTextRun> runs, float x, float y, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException(nameof(canvas));
			if (runs == null)
				throw new ArgumentNullException(nameof(runs));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			foreach (var run in runs)
			{
				using (var newPaint = paint.Clone())
				{
					if (run.Typeface != null)
						newPaint.Typeface = run.Typeface;
					if (run.TextSize != null)
						newPaint.TextSize = run.TextSize.Value;
					if (run.Color != null)
						newPaint.Color = run.Color.Value;
					if (run.TextEncoding != null)
						newPaint.TextEncoding = run.TextEncoding.Value;

					if (run.Text?.Length > 0)
					{
						canvas.DrawText(run.Text, x + run.Offset.X, y + run.Offset.Y, newPaint);
						x += newPaint.MeasureText(run.Text);
					}
				}
			}
		}

		public static void DrawIconifiedText(this SKCanvas canvas, string text, float x, float y, SKPaint paint)
		{
			canvas.DrawIconifiedText(text, x, y, SKTextRunLookup.Instance, paint);
		}

		public static void DrawIconifiedText(this SKCanvas canvas, string text, float x, float y, SKTextRunLookup lookup, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException(nameof(canvas));
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			var runs = SKTextRun.Create(text, lookup);
			canvas.DrawText(runs, x, y, paint);
		}
	}
}
