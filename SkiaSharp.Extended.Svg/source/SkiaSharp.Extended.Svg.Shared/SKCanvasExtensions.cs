using System;
using System.Linq;

namespace SkiaSharp.Extended.Svg
{
    internal static class SKCanvasExtensions
    {
        public static void DrawRoundRect(this SKCanvas canvas, SKRoundedRect rect, SKPaint paint)
        {
            canvas.DrawRoundRect(rect.Rect, rect.RadiusX, rect.RadiusY, paint);
        }

        public static void DrawOval(this SKCanvas canvas, SKOval oval, SKPaint paint)
        {
            canvas.DrawOval(oval.Center.X, oval.Center.Y, oval.RadiusX, oval.RadiusY, paint);
        }

        public static void DrawCircle(this SKCanvas canvas, SKCircle circle, SKPaint paint)
        {
            canvas.DrawCircle(circle.Center.X, circle.Center.Y, circle.Radius, paint);
        }

        public static void DrawLine(this SKCanvas canvas, SKLine line, SKPaint paint)
        {
            canvas.DrawLine(line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, paint);
        }
    }
}
