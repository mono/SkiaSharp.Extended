using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Extended.Drawing.Common.Benchmarks;

/// <summary>
/// Base class for drawing benchmarks. Creates a reusable Bitmap+Graphics pair.
/// </summary>
public abstract partial class BenchmarkBase
{
    protected Bitmap Bitmap = null!;
    protected Graphics Graphics = null!;
    protected Pen BlackPen1 = null!;
    protected Pen BlackPen3 = null!;
    protected SolidBrush RedBrush = null!;
    protected SolidBrush BlueBrush = null!;
    protected Font DefaultFont = null!;

    [GlobalSetup]
    public virtual void Setup()
    {
        Bitmap = new Bitmap(500, 500, PixelFormat.Format32bppArgb);
        Graphics = Graphics.FromImage(Bitmap);
        Graphics.SmoothingMode = SmoothingMode.None;
        BlackPen1 = new Pen(Color.Black, 1);
        BlackPen3 = new Pen(Color.Black, 3);
        RedBrush = new SolidBrush(Color.Red);
        BlueBrush = new SolidBrush(Color.Blue);
        DefaultFont = new Font("Arial", 12);
    }

    [GlobalCleanup]
    public virtual void Cleanup()
    {
        DefaultFont?.Dispose();
        BlueBrush?.Dispose();
        RedBrush?.Dispose();
        BlackPen3?.Dispose();
        BlackPen1?.Dispose();
        Graphics?.Dispose();
        Bitmap?.Dispose();
    }
}
