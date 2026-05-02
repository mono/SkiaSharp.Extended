using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Scenarios;

public class Polygons : ScenarioBase
{
    public Polygons(string outputDir) : base(outputDir) { }

    public void Polygon_Triangle_Stroke() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Black, 1);
        g.DrawPolygon(pen, new PointF[] { new(50, 10), new(10, 90), new(90, 90) });
    });

    public void Polygon_Triangle_Fill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Red);
        g.FillPolygon(brush, new PointF[] { new(50, 10), new(10, 90), new(90, 90) });
    });

    public void Polygon_Square_Fill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Blue);
        g.FillPolygon(brush, new PointF[] { new(20, 20), new(80, 20), new(80, 80), new(20, 80) });
    });

    public void Polygon_Pentagon_Fill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Purple);
        g.FillPolygon(brush, new PointF[] { new(50,5), new(95,37), new(77,90), new(23,90), new(5,37) });
    });

    public void Polygon_Star_Stroke() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var pen = new Pen(Color.Red, 2);
        g.DrawPolygon(pen, new PointF[] {
            new(50,5), new(61,40), new(98,40), new(68,62), new(79,97),
            new(50,75), new(21,97), new(32,62), new(2,40), new(39,40)
        });
    });

    public void Polygon_Diamond_StrokeAndFill() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None;
        g.Clear(Color.White);
        using var brush = new SolidBrush(Color.Orange);
        using var pen = new Pen(Color.Black, 2);
        var points = new PointF[] { new(50, 10), new(90, 50), new(50, 90), new(10, 50) };
        g.FillPolygon(brush, points);
        g.DrawPolygon(pen, points);
    });
}
