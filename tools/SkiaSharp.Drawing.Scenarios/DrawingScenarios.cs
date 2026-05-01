using System.Drawing;
using System.Drawing.Drawing2D;

namespace SkiaSharp.Drawing.Scenarios;

/// <summary>
/// Drawing test scenarios using System.Drawing API directly.
/// These files are compiled against both real System.Drawing.Common (GDI+)
/// and SkiaSharp.Drawing to produce comparable output.
/// </summary>
public static class DrawingScenarios
{
    public static IReadOnlyList<(string Name, string Category, int Width, int Height, Action<Graphics> Draw)> GetAll() => new (string, string, int, int, Action<Graphics>)[]
    {
        // === CLEAR ===
        ("Clear_Red", "Clear", 100, 100, g => { g.Clear(Color.Red); }),
        ("Clear_White", "Clear", 100, 100, g => { g.Clear(Color.White); }),
        ("Clear_Transparent", "Clear", 100, 100, g => { g.Clear(Color.Transparent); }),

        // === LINES (no AA) ===
        ("Line_Horizontal_1px", "Lines", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, 10, 50, 90, 50);
        }),
        ("Line_Vertical_1px", "Lines", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, 50, 10, 50, 90);
        }),
        ("Line_Diagonal_1px", "Lines", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, 10, 10, 90, 90);
        }),
        ("Line_Thick_5px", "Lines", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Red, 5);
            g.DrawLine(pen, 10, 50, 90, 50);
        }),
        ("Line_Colored_Blue", "Lines", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Blue, 2);
            g.DrawLine(pen, 10, 10, 90, 90);
        }),
        ("Line_Multiple", "Lines", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, 10, 10, 90, 10);
            g.DrawLine(pen, 10, 30, 90, 30);
            g.DrawLine(pen, 10, 50, 90, 50);
            g.DrawLine(pen, 10, 70, 90, 70);
            g.DrawLine(pen, 10, 90, 90, 90);
        }),

        // === LINES (with AA) ===
        ("Line_Diagonal_AA", "LinesAA", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, 10, 10, 90, 90);
        }),
        ("Line_Thick_AA", "LinesAA", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Red, 5);
            g.DrawLine(pen, 10, 10, 90, 90);
        }),

        // === RECTANGLES ===
        ("Rect_Stroke_1px", "Rectangles", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawRectangle(pen, 10, 10, 80, 80);
        }),
        ("Rect_Stroke_3px", "Rectangles", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 3);
            g.DrawRectangle(pen, 10, 10, 80, 80);
        }),
        ("Rect_Fill_Red", "Rectangles", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Red);
            g.FillRectangle(brush, 10, 10, 80, 80);
        }),
        ("Rect_Fill_Small", "Rectangles", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Green);
            g.FillRectangle(brush, 40, 40, 20, 20);
        }),
        ("Rect_StrokeAndFill", "Rectangles", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Red);
            g.FillRectangle(brush, 10, 10, 80, 80);
            using var pen = new Pen(Color.Black, 2);
            g.DrawRectangle(pen, 10, 10, 80, 80);
        }),
        ("Rect_Multiple", "Rectangles", 200, 200, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var redBrush = new SolidBrush(Color.Red);
            using var greenBrush = new SolidBrush(Color.Green);
            using var blueBrush = new SolidBrush(Color.Blue);
            g.FillRectangle(redBrush, 10, 10, 80, 80);
            g.FillRectangle(greenBrush, 60, 60, 80, 80);
            g.FillRectangle(blueBrush, 110, 110, 80, 80);
        }),

        // === ELLIPSES ===
        ("Ellipse_Stroke_Circle", "Ellipses", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawEllipse(pen, 10, 10, 80, 80);
        }),
        ("Ellipse_Fill_Circle", "Ellipses", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Blue);
            g.FillEllipse(brush, 10, 10, 80, 80);
        }),
        ("Ellipse_Wide", "Ellipses", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Magenta);
            g.FillEllipse(brush, 5, 25, 90, 50);
        }),
        ("Ellipse_Tall", "Ellipses", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Cyan);
            g.FillEllipse(brush, 25, 5, 50, 90);
        }),
        ("Ellipse_StrokeAndFill", "Ellipses", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Yellow);
            g.FillEllipse(brush, 10, 10, 80, 80);
            using var pen = new Pen(Color.Black, 2);
            g.DrawEllipse(pen, 10, 10, 80, 80);
        }),

        // === ELLIPSES (AA) ===
        ("Ellipse_AA_Circle", "EllipsesAA", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Blue);
            g.FillEllipse(brush, 10, 10, 80, 80);
        }),
        ("Ellipse_AA_Wide", "EllipsesAA", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Magenta);
            g.FillEllipse(brush, 5, 25, 90, 50);
        }),

        // === ARCS ===
        ("Arc_Quarter", "Arcs", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 2);
            g.DrawArc(pen, 10, 10, 80, 80, 0, 90);
        }),
        ("Arc_Half", "Arcs", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Red, 2);
            g.DrawArc(pen, 10, 10, 80, 80, 0, 180);
        }),
        ("Arc_ThreeQuarter", "Arcs", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Blue, 2);
            g.DrawArc(pen, 10, 10, 80, 80, 45, 270);
        }),
        ("Arc_NegativeStart", "Arcs", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Green, 2);
            g.DrawArc(pen, 10, 10, 80, 80, -45, 180);
        }),
        ("Arc_Thick", "Arcs", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.DarkRed, 5);
            g.DrawArc(pen, 10, 10, 80, 80, 30, 120);
        }),

        // === PIES ===
        ("Pie_Fill_Quarter", "Pies", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Red);
            g.FillPie(brush, 10, 10, 80, 80, 0, 90);
        }),
        ("Pie_Fill_Half", "Pies", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Green);
            g.FillPie(brush, 10, 10, 80, 80, -90, 180);
        }),
        ("Pie_Fill_ThreeQuarter", "Pies", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Blue);
            g.FillPie(brush, 10, 10, 80, 80, 0, 270);
        }),
        ("Pie_Multiple", "Pies", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var r = new SolidBrush(Color.Red);
            using var gr = new SolidBrush(Color.Green);
            using var b = new SolidBrush(Color.Blue);
            using var y = new SolidBrush(Color.Yellow);
            g.FillPie(r, 10, 10, 80, 80, 0, 90);
            g.FillPie(gr, 10, 10, 80, 80, 90, 90);
            g.FillPie(b, 10, 10, 80, 80, 180, 90);
            g.FillPie(y, 10, 10, 80, 80, 270, 90);
        }),

        // === POLYGONS ===
        ("Polygon_Triangle_Stroke", "Polygons", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Black, 1);
            g.DrawPolygon(pen, new PointF[] { new(50, 10), new(10, 90), new(90, 90) });
        }),
        ("Polygon_Triangle_Fill", "Polygons", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Red);
            g.FillPolygon(brush, new PointF[] { new(50, 10), new(10, 90), new(90, 90) });
        }),
        ("Polygon_Square_Fill", "Polygons", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Blue);
            g.FillPolygon(brush, new PointF[] { new(20, 20), new(80, 20), new(80, 80), new(20, 80) });
        }),
        ("Polygon_Pentagon_Fill", "Polygons", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Purple);
            g.FillPolygon(brush, new PointF[] { new(50,5), new(95,37), new(77,90), new(23,90), new(5,37) });
        }),
        ("Polygon_Star_Stroke", "Polygons", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Red, 2);
            g.DrawPolygon(pen, new PointF[] {
                new(50,5), new(61,40), new(98,40), new(68,62), new(79,97),
                new(50,75), new(21,97), new(32,62), new(2,40), new(39,40)
            });
        }),
        ("Polygon_Diamond_StrokeAndFill", "Polygons", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var brush = new SolidBrush(Color.Orange);
            using var pen = new Pen(Color.Black, 2);
            var points = new PointF[] { new(50, 10), new(90, 50), new(50, 90), new(10, 50) };
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }),

        // === COMPOSITES ===
        ("Composite_RectOverEllipse", "Composites", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var blueBrush = new SolidBrush(Color.Blue);
            g.FillEllipse(blueBrush, 10, 10, 80, 80);
            using var redBrush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            g.FillRectangle(redBrush, 25, 25, 50, 50);
        }),
        ("Composite_MultipleShapes", "Composites", 200, 200, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var redBrush = new SolidBrush(Color.Red);
            using var greenBrush = new SolidBrush(Color.Green);
            using var blueBrush = new SolidBrush(Color.Blue);
            using var pen = new Pen(Color.Black, 3);
            g.FillRectangle(redBrush, 10, 10, 80, 80);
            g.FillRectangle(greenBrush, 60, 60, 80, 80);
            g.FillEllipse(blueBrush, 110, 10, 80, 80);
            g.DrawLine(pen, 0, 0, 199, 199);
            g.DrawLine(pen, 199, 0, 0, 199);
        }),
        ("Composite_ConcentricCircles", "Composites", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var r = new SolidBrush(Color.Red);
            using var gr = new SolidBrush(Color.Green);
            using var b = new SolidBrush(Color.Blue);
            using var y = new SolidBrush(Color.Yellow);
            g.FillEllipse(r, 5, 5, 90, 90);
            g.FillEllipse(gr, 15, 15, 70, 70);
            g.FillEllipse(b, 25, 25, 50, 50);
            g.FillEllipse(y, 35, 35, 30, 30);
        }),
        ("Composite_Grid", "Composites", 100, 100, g => {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.White);
            using var pen = new Pen(Color.Gray, 1);
            for (int i = 0; i <= 100; i += 10) {
                g.DrawLine(pen, i, 0, i, 100);
                g.DrawLine(pen, 0, i, 100, i);
            }
        }),

        // === COLORS (exact color precision) ===
        ("Color_AllChannels", "Colors", 100, 100, g => {
            g.Clear(Color.Transparent);
            using var r = new SolidBrush(Color.Red);
            using var gr = new SolidBrush(Color.Lime);
            using var b = new SolidBrush(Color.Blue);
            using var bl = new SolidBrush(Color.Black);
            g.FillRectangle(r, 0, 0, 25, 100);
            g.FillRectangle(gr, 25, 0, 25, 100);
            g.FillRectangle(b, 50, 0, 25, 100);
            g.FillRectangle(bl, 75, 0, 25, 100);
        }),
        ("Color_GrayLevels", "Colors", 100, 100, g => {
            g.Clear(Color.Transparent);
            for (int i = 0; i < 10; i++) {
                int gray = Math.Min(255, i * 28);
                using var brush = new SolidBrush(Color.FromArgb(255, gray, gray, gray));
                g.FillRectangle(brush, i * 10, 0, 10, 100);
            }
        }),
        ("Color_AlphaBlending", "Colors", 100, 100, g => {
            g.Clear(Color.White);
            for (int i = 0; i < 5; i++) {
                int alpha = 50 + i * 50;
                using var brush = new SolidBrush(Color.FromArgb(alpha, 255, 0, 0));
                g.FillRectangle(brush, i * 20, 0, 20, 100);
            }
        }),
    };
}
