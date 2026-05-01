using System.Collections.Generic;

namespace SkiaSharp.Drawing.Scenarios;

public static class AllScenarios
{
    public static IReadOnlyList<TestScenario> GetAll() => new[]
    {
        // === CLEAR ===
        new TestScenario("Clear_Red", "Clear", 100, 100, s => { s.Clear(unchecked((int)0xFFFF0000)); }),
        new TestScenario("Clear_White", "Clear", 100, 100, s => { s.Clear(unchecked((int)0xFFFFFFFF)); }),
        new TestScenario("Clear_Transparent", "Clear", 100, 100, s => { s.Clear(0x00000000); }),

        // === LINES (no AA) ===
        new TestScenario("Line_Horizontal_1px", "Lines", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFF000000), 1, 10, 50, 90, 50);
        }),
        new TestScenario("Line_Vertical_1px", "Lines", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFF000000), 1, 50, 10, 50, 90);
        }),
        new TestScenario("Line_Diagonal_1px", "Lines", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFF000000), 1, 10, 10, 90, 90);
        }),
        new TestScenario("Line_Thick_5px", "Lines", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFFFF0000), 5, 10, 50, 90, 50);
        }),
        new TestScenario("Line_Colored_Blue", "Lines", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFF0000FF), 2, 10, 10, 90, 90);
        }),
        new TestScenario("Line_Multiple", "Lines", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFFFF0000), 1, 10, 10, 90, 10);
            s.DrawLine(unchecked((int)0xFF00FF00), 1, 10, 30, 90, 30);
            s.DrawLine(unchecked((int)0xFF0000FF), 1, 10, 50, 90, 50);
            s.DrawLine(unchecked((int)0xFF000000), 1, 10, 70, 90, 70);
            s.DrawLine(unchecked((int)0xFFFF00FF), 1, 10, 90, 90, 90);
        }),

        // === LINES (with AA) ===
        new TestScenario("Line_Diagonal_AA", "LinesAA", 100, 100, s => {
            s.SetAntiAlias(true); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFF000000), 1, 10, 10, 90, 90);
        }),
        new TestScenario("Line_Thick_AA", "LinesAA", 100, 100, s => {
            s.SetAntiAlias(true); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawLine(unchecked((int)0xFF0000FF), 3, 10, 80, 80, 20);
        }),

        // === RECTANGLES ===
        new TestScenario("Rect_Stroke_1px", "Rectangles", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawRectangle(unchecked((int)0xFF000000), 1, 10, 10, 80, 80);
        }),
        new TestScenario("Rect_Stroke_3px", "Rectangles", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawRectangle(unchecked((int)0xFF000000), 3, 10, 10, 80, 80);
        }),
        new TestScenario("Rect_Fill_Red", "Rectangles", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillRectangle(unchecked((int)0xFFFF0000), 10, 10, 80, 80);
        }),
        new TestScenario("Rect_Fill_Small", "Rectangles", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillRectangle(unchecked((int)0xFF00FF00), 40, 40, 20, 20);
        }),
        new TestScenario("Rect_StrokeAndFill", "Rectangles", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillRectangle(unchecked((int)0xFFFF0000), 10, 10, 80, 80);
            s.DrawRectangle(unchecked((int)0xFF000000), 2, 10, 10, 80, 80);
        }),
        new TestScenario("Rect_Multiple", "Rectangles", 200, 200, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillRectangle(unchecked((int)0xFFFF0000), 10, 10, 60, 60);
            s.FillRectangle(unchecked((int)0xFF00FF00), 50, 50, 60, 60);
            s.FillRectangle(unchecked((int)0xFF0000FF), 90, 90, 60, 60);
            s.DrawRectangle(unchecked((int)0xFF000000), 1, 130, 10, 60, 60);
        }),

        // === ELLIPSES ===
        new TestScenario("Ellipse_Stroke_Circle", "Ellipses", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawEllipse(unchecked((int)0xFF000000), 1, 10, 10, 80, 80);
        }),
        new TestScenario("Ellipse_Fill_Circle", "Ellipses", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFF0000FF), 10, 10, 80, 80);
        }),
        new TestScenario("Ellipse_Wide", "Ellipses", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFFFF00FF), 5, 25, 90, 50);
        }),
        new TestScenario("Ellipse_Tall", "Ellipses", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFF008080), 25, 5, 50, 90);
        }),
        new TestScenario("Ellipse_StrokeAndFill", "Ellipses", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFF00FF00), 10, 10, 80, 80);
            s.DrawEllipse(unchecked((int)0xFF000000), 2, 10, 10, 80, 80);
        }),
        new TestScenario("Ellipse_AA_Circle", "EllipsesAA", 100, 100, s => {
            s.SetAntiAlias(true); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFF0000FF), 10, 10, 80, 80);
        }),
        new TestScenario("Ellipse_AA_Wide", "EllipsesAA", 100, 100, s => {
            s.SetAntiAlias(true); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawEllipse(unchecked((int)0xFF000000), 2, 5, 25, 90, 50);
        }),

        // === ARCS ===
        new TestScenario("Arc_Quarter", "Arcs", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawArc(unchecked((int)0xFF000000), 2, 10, 10, 80, 80, 0, 90);
        }),
        new TestScenario("Arc_Half", "Arcs", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawArc(unchecked((int)0xFFFF0000), 2, 10, 10, 80, 80, 0, 180);
        }),
        new TestScenario("Arc_ThreeQuarter", "Arcs", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawArc(unchecked((int)0xFF0000FF), 2, 10, 10, 80, 80, 45, 270);
        }),
        new TestScenario("Arc_NegativeStart", "Arcs", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawArc(unchecked((int)0xFF008000), 2, 10, 10, 80, 80, -45, 90);
        }),
        new TestScenario("Arc_Thick", "Arcs", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawArc(unchecked((int)0xFFFF0000), 5, 10, 10, 80, 80, 0, 180);
        }),

        // === PIES ===
        new TestScenario("Pie_Fill_Quarter", "Pies", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPie(unchecked((int)0xFFFF0000), 10, 10, 80, 80, 0, 90);
        }),
        new TestScenario("Pie_Fill_Half", "Pies", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPie(unchecked((int)0xFF00FF00), 10, 10, 80, 80, -90, 180);
        }),
        new TestScenario("Pie_Fill_ThreeQuarter", "Pies", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPie(unchecked((int)0xFF0000FF), 10, 10, 80, 80, 0, 270);
        }),
        new TestScenario("Pie_Multiple", "Pies", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPie(unchecked((int)0xFFFF0000), 10, 10, 80, 80, 0, 120);
            s.FillPie(unchecked((int)0xFF00FF00), 10, 10, 80, 80, 120, 120);
            s.FillPie(unchecked((int)0xFF0000FF), 10, 10, 80, 80, 240, 120);
        }),

        // === POLYGONS ===
        new TestScenario("Polygon_Triangle_Stroke", "Polygons", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawPolygon(unchecked((int)0xFF000000), 1, new float[] { 50, 10, 10, 90, 90, 90 });
        }),
        new TestScenario("Polygon_Triangle_Fill", "Polygons", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPolygon(unchecked((int)0xFFFF0000), new float[] { 50, 10, 10, 90, 90, 90 });
        }),
        new TestScenario("Polygon_Square_Fill", "Polygons", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPolygon(unchecked((int)0xFF0000FF), new float[] { 20, 20, 80, 20, 80, 80, 20, 80 });
        }),
        new TestScenario("Polygon_Pentagon_Fill", "Polygons", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPolygon(unchecked((int)0xFF800080), new float[] { 50, 5, 95, 37, 77, 90, 23, 90, 5, 37 });
        }),
        new TestScenario("Polygon_Star_Stroke", "Polygons", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.DrawPolygon(unchecked((int)0xFFFF0000), 2, new float[] {
                50, 5, 61, 40, 98, 40, 68, 62, 79, 97, 50, 75, 21, 97, 32, 62, 2, 40, 39, 40
            });
        }),
        new TestScenario("Polygon_Diamond_StrokeAndFill", "Polygons", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillPolygon(unchecked((int)0xFF00FFFF), new float[] { 50, 5, 95, 50, 50, 95, 5, 50 });
            s.DrawPolygon(unchecked((int)0xFF000000), 2, new float[] { 50, 5, 95, 50, 50, 95, 5, 50 });
        }),

        // === COMPOSITES ===
        new TestScenario("Composite_RectOverEllipse", "Composites", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFF0000FF), 10, 10, 80, 80);
            s.FillRectangle(unchecked((int)0x80FF0000), 25, 25, 50, 50);
        }),
        new TestScenario("Composite_MultipleShapes", "Composites", 200, 200, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillRectangle(unchecked((int)0xFFFF0000), 10, 10, 80, 80);
            s.FillRectangle(unchecked((int)0xFF00FF00), 60, 60, 80, 80);
            s.FillEllipse(unchecked((int)0xFF0000FF), 110, 10, 80, 80);
            s.DrawLine(unchecked((int)0xFF000000), 3, 0, 0, 199, 199);
            s.DrawLine(unchecked((int)0xFF000000), 3, 199, 0, 0, 199);
        }),
        new TestScenario("Composite_ConcentricCircles", "Composites", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillEllipse(unchecked((int)0xFFFF0000), 5, 5, 90, 90);
            s.FillEllipse(unchecked((int)0xFF00FF00), 15, 15, 70, 70);
            s.FillEllipse(unchecked((int)0xFF0000FF), 25, 25, 50, 50);
            s.FillEllipse(unchecked((int)0xFFFFFF00), 35, 35, 30, 30);
        }),
        new TestScenario("Composite_Grid", "Composites", 100, 100, s => {
            s.SetAntiAlias(false); s.Clear(unchecked((int)0xFFFFFFFF));
            for (int i = 0; i < 10; i++)
            {
                s.DrawLine(unchecked((int)0xFFC0C0C0), 1, i * 10, 0, i * 10, 99);
                s.DrawLine(unchecked((int)0xFFC0C0C0), 1, 0, i * 10, 99, i * 10);
            }
            s.FillRectangle(unchecked((int)0x80FF0000), 20, 20, 30, 30);
            s.FillEllipse(unchecked((int)0x800000FF), 40, 40, 40, 40);
        }),

        // === COLORS (exact color precision) ===
        new TestScenario("Color_AllChannels", "Colors", 100, 100, s => {
            s.Clear(0x00000000);
            s.FillRectangle(unchecked((int)0xFFFF0000), 0, 0, 25, 100);
            s.FillRectangle(unchecked((int)0xFF00FF00), 25, 0, 25, 100);
            s.FillRectangle(unchecked((int)0xFF0000FF), 50, 0, 25, 100);
            s.FillRectangle(unchecked((int)0xFF000000), 75, 0, 25, 100);
        }),
        new TestScenario("Color_GrayLevels", "Colors", 100, 100, s => {
            s.Clear(0x00000000);
            for (int i = 0; i < 10; i++)
            {
                int gray = i * 28; if (gray > 255) gray = 255;
                int argb = unchecked((int)(0xFF000000u | (uint)(gray << 16) | (uint)(gray << 8) | (uint)gray));
                s.FillRectangle(argb, i * 10, 0, 10, 100);
            }
        }),
        new TestScenario("Color_AlphaBlending", "Colors", 100, 100, s => {
            s.Clear(unchecked((int)0xFFFFFFFF));
            s.FillRectangle(unchecked((int)0x40FF0000), 0, 0, 100, 25);
            s.FillRectangle(unchecked((int)0x80FF0000), 0, 25, 100, 25);
            s.FillRectangle(unchecked((int)0xC0FF0000), 0, 50, 100, 25);
            s.FillRectangle(unchecked((int)0xFFFF0000), 0, 75, 100, 25);
        }),
    };
}
