using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

/// <summary>
/// Scaled-down (0.5×) variants of representative scenarios from different categories.
/// Each test renders at half size with ScaleTransform(0.5f, 0.5f) applied.
/// </summary>
public class ScaledHalf : ScenarioBase
{
    // === Basic Shapes ===

    [Fact] public void Scaled05_Ellipse_Fill_Circle() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new SolidBrush(Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    [Fact] public void Scaled05_Rect_Stroke_1px() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, 10, 10, 80, 80);
    });

    [Fact] public void Scaled05_Line_Diagonal_1px() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, 10, 10, 90, 90);
    });

    [Fact] public void Scaled05_Polygon_Triangle_Fill() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new SolidBrush(Color.Red);
        g.FillPolygon(brush, new PointF[] { new(50, 10), new(10, 90), new(90, 90) });
    });

    [Fact] public void Scaled05_Pie_Fill_Quarter() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new SolidBrush(Color.Red);
        g.FillPie(brush, 10, 10, 80, 80, 0, 90);
    });

    // === Hatches ===

    [Fact] public void Scaled05_Hatch_Horizontal() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new HatchBrush(HatchStyle.Horizontal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Scaled05_Hatch_ForwardDiagonal() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Scaled05_Hatch_Cross() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new HatchBrush(HatchStyle.Cross, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Scaled05_Hatch_Percent50() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Scaled05_Hatch_SmallCheckerBoard() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new HatchBrush(HatchStyle.SmallCheckerBoard, Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    // === Gradients ===

    [Fact] public void Scaled05_Gradient_Horizontal() => Render(50, 50, g => {
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(99, 0), Color.Red, Color.Blue);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Scaled05_Gradient_Diagonal() => Render(50, 50, g => {
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new LinearGradientBrush(new Point(0, 0), new Point(99, 99), Color.Black, Color.White);
        g.FillRectangle(brush, 0, 0, 100, 100);
    });

    [Fact] public void Scaled05_Gradient_InEllipse() => Render(50, 50, g => {
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var brush = new LinearGradientBrush(new Point(10, 10), new Point(90, 90), Color.Red, Color.Blue);
        g.FillEllipse(brush, 10, 10, 80, 80);
    });

    // === Text ===

    [Fact] public void Scaled05_Text_Simple() => Render(100, 25, g => {
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var font = new Font("Arial", 20);
        g.DrawString("Hello World", font, Brushes.Black, 10, 10);
    });

    [Fact] public void Scaled05_Text_Bold() => Render(100, 25, g => {
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var font = new Font("Arial", 20, FontStyle.Bold);
        g.DrawString("Bold Text", font, Brushes.Black, 10, 10);
    });

    [Fact] public void Scaled05_Text_InRectangle() => Render(100, 50, g => {
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var font = new Font("Arial", 12);
        var rect = new RectangleF(10, 10, 180, 80);
        g.DrawRectangle(Pens.Gray, Rectangle.Round(rect));
        g.DrawString("This text should wrap inside the rectangle bounds", font, Brushes.Black, rect);
    });

    // === Pens ===

    [Fact] public void Scaled05_Pen_Dash() => Render(50, 20, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 2) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, 5, 20, 95, 20);
    });

    [Fact] public void Scaled05_Pen_RoundCap() => Render(50, 20, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, 20, 20, 80, 20);
    });

    [Fact] public void Scaled05_Pen_MiterJoin() => Render(30, 30, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Miter };
        g.DrawRectangle(pen, 10, 10, 40, 40);
    });

    // === Curves/Paths ===

    [Fact] public void Scaled05_Curve_Open() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 2);
        g.DrawCurve(pen, new PointF[] { new(10, 50), new(30, 20), new(50, 70), new(70, 30), new(90, 50) });
    });

    [Fact] public void Scaled05_Path_MultiShape() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var path = new GraphicsPath();
        path.AddRectangle(new RectangleF(10, 10, 30, 30));
        path.AddEllipse(50, 10, 40, 40);
        path.AddLine(10, 60, 90, 90);
        using var pen = new Pen(Color.Black, 2);
        g.DrawPath(pen, path);
    });

    [Fact] public void Scaled05_Bezier_Simple() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var pen = new Pen(Color.Black, 2);
        g.DrawBezier(pen, 10f, 50f, 30f, 10f, 70f, 90f, 90f, 50f);
    });

    // === Transforms ===

    [Fact] public void Scaled05_Transform_Rotate() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        g.TranslateTransform(50, 50);
        g.RotateTransform(45);
        g.FillRectangle(Brushes.Green, -20, -20, 40, 40);
    });

    [Fact] public void Scaled05_Transform_Scale() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        g.ScaleTransform(2, 2);
        g.FillRectangle(Brushes.Blue, 5, 5, 20, 20);
    });

    [Fact] public void Scaled05_Transform_Combined() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        g.TranslateTransform(50, 50);
        g.ScaleTransform(1.5f, 1.5f);
        g.RotateTransform(30);
        g.FillRectangle(Brushes.Purple, -15, -15, 30, 30);
    });

    // === Images ===

    private static Bitmap CreateTestImage(int width, int height)
    {
        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var redBrush = new SolidBrush(Color.Red);
        g.FillRectangle(redBrush, 0, 0, width / 2, height / 2);
        using var blueBrush = new SolidBrush(Color.Blue);
        g.FillRectangle(blueBrush, width / 2, 0, width / 2, height / 2);
        using var greenBrush = new SolidBrush(Color.Green);
        g.FillRectangle(greenBrush, 0, height / 2, width / 2, height / 2);
        using var yellowBrush = new SolidBrush(Color.Yellow);
        g.FillRectangle(yellowBrush, width / 2, height / 2, width / 2, height / 2);
        return bmp;
    }

    [Fact] public void Scaled05_DrawImage_Scaled() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var src = CreateTestImage(20, 20);
        g.DrawImage(src, 10, 10, 80, 80);
    });

    [Fact] public void Scaled05_DrawImage_Stretched() => Render(75, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var src = CreateTestImage(30, 30);
        g.DrawImage(src, 5, 5, 140, 90);
    });

    [Fact] public void Scaled05_DrawImage_SmallToLarge() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var src = CreateTestImage(10, 10);
        g.DrawImage(src, 0, 0, 100, 100);
    });

    // === Regions ===

    [Fact] public void Scaled05_Region_Intersect() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var region = new Region(new Rectangle(10, 10, 60, 60));
        region.Intersect(new Rectangle(30, 30, 60, 60));
        g.FillRegion(Brushes.Blue, region);
    });

    [Fact] public void Scaled05_Region_Exclude() => Render(50, 50, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        using var region = new Region(new Rectangle(10, 10, 80, 80));
        using var path = new GraphicsPath();
        path.AddEllipse(30, 30, 40, 40);
        region.Exclude(path);
        g.FillRegion(Brushes.Red, region);
    });
}
