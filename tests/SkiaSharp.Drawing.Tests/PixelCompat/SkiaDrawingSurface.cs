using SD = System.Drawing;
using SkiaSharp.Drawing.Scenarios;

namespace SkiaSharp.Drawing.Tests.PixelCompat;

/// <summary>
/// IDrawingSurface backed by SkiaSharp.Drawing (our wrapper).
/// Used to compare against reference images from real System.Drawing.
/// </summary>
public sealed class SkiaDrawingSurface : IDrawingSurface
{
    private readonly SD.Bitmap _bitmap;
    private readonly SD.Graphics _graphics;

    public int Width => _bitmap.Width;
    public int Height => _bitmap.Height;

    public SkiaDrawingSurface(int width, int height)
    {
        _bitmap = new SD.Bitmap(width, height);
        _graphics = SD.Graphics.FromImage(_bitmap);
    }

    public void Clear(int argb) => _graphics.Clear(SD.Color.FromArgb(argb));

    public void DrawLine(int penArgb, float penWidth, float x1, float y1, float x2, float y2)
    {
        using var pen = new SD.Pen(SD.Color.FromArgb(penArgb), penWidth);
        _graphics.DrawLine(pen, x1, y1, x2, y2);
    }

    public void DrawRectangle(int penArgb, float penWidth, float x, float y, float width, float height)
    {
        using var pen = new SD.Pen(SD.Color.FromArgb(penArgb), penWidth);
        _graphics.DrawRectangle(pen, x, y, width, height);
    }

    public void FillRectangle(int brushArgb, float x, float y, float width, float height)
    {
        using var brush = new SD.SolidBrush(SD.Color.FromArgb(brushArgb));
        _graphics.FillRectangle(brush, x, y, width, height);
    }

    public void DrawEllipse(int penArgb, float penWidth, float x, float y, float width, float height)
    {
        using var pen = new SD.Pen(SD.Color.FromArgb(penArgb), penWidth);
        _graphics.DrawEllipse(pen, x, y, width, height);
    }

    public void FillEllipse(int brushArgb, float x, float y, float width, float height)
    {
        using var brush = new SD.SolidBrush(SD.Color.FromArgb(brushArgb));
        _graphics.FillEllipse(brush, x, y, width, height);
    }

    public void DrawArc(int penArgb, float penWidth, float x, float y, float width, float height, float startAngle, float sweepAngle)
    {
        using var pen = new SD.Pen(SD.Color.FromArgb(penArgb), penWidth);
        _graphics.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
    }

    public void FillPie(int brushArgb, float x, float y, float width, float height, float startAngle, float sweepAngle)
    {
        using var brush = new SD.SolidBrush(SD.Color.FromArgb(brushArgb));
        _graphics.FillPie(brush, x, y, width, height, startAngle, sweepAngle);
    }

    public void DrawPolygon(int penArgb, float penWidth, float[] xyPairs)
    {
        using var pen = new SD.Pen(SD.Color.FromArgb(penArgb), penWidth);
        var points = ToPointFArray(xyPairs);
        _graphics.DrawPolygon(pen, points);
    }

    public void FillPolygon(int brushArgb, float[] xyPairs)
    {
        using var brush = new SD.SolidBrush(SD.Color.FromArgb(brushArgb));
        var points = ToPointFArray(xyPairs);
        _graphics.FillPolygon(brush, points);
    }

    public void SetAntiAlias(bool enabled)
    {
        _graphics.SmoothingMode = enabled
            ? SD.Drawing2D.SmoothingMode.AntiAlias
            : SD.Drawing2D.SmoothingMode.None;
    }

    public byte[] SaveAsPng()
    {
        using var ms = new MemoryStream();
        _bitmap.Save(ms, SD.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    public void Dispose()
    {
        _graphics.Dispose();
        _bitmap.Dispose();
    }

    private static SD.PointF[] ToPointFArray(float[] xyPairs)
    {
        var points = new SD.PointF[xyPairs.Length / 2];
        for (int i = 0; i < points.Length; i++)
            points[i] = new SD.PointF(xyPairs[i * 2], xyPairs[i * 2 + 1]);
        return points;
    }
}
