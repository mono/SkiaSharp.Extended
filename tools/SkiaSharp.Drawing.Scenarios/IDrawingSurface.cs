using System;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// Abstraction over a drawing surface that can be backed by either
/// real System.Drawing.Graphics or SkiaSharp.Drawing.Graphics.
/// </summary>
public interface IDrawingSurface : IDisposable
{
    int Width { get; }
    int Height { get; }
    void Clear(int argb);
    void DrawLine(int penArgb, float penWidth, float x1, float y1, float x2, float y2);
    void DrawRectangle(int penArgb, float penWidth, float x, float y, float width, float height);
    void FillRectangle(int brushArgb, float x, float y, float width, float height);
    void DrawEllipse(int penArgb, float penWidth, float x, float y, float width, float height);
    void FillEllipse(int brushArgb, float x, float y, float width, float height);
    void DrawArc(int penArgb, float penWidth, float x, float y, float width, float height, float startAngle, float sweepAngle);
    void FillPie(int brushArgb, float x, float y, float width, float height, float startAngle, float sweepAngle);
    void DrawPolygon(int penArgb, float penWidth, float[] xyPairs);
    void FillPolygon(int brushArgb, float[] xyPairs);
    void SetAntiAlias(bool enabled);
    byte[] SaveAsPng();
}
