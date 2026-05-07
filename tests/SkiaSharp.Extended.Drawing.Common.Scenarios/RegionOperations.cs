using System.Drawing;
using System.Drawing.Drawing2D;
using Xunit;

namespace SkiaSharp.Extended.Drawing.Common.Scenarios;

public partial class RegionOperations : ScenarioBase
{
    [Fact] public void Region_Intersect() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region(new Rectangle(10, 10, 60, 60));
        region.Intersect(new Rectangle(30, 30, 60, 60));
        g.FillRegion(Brushes.Blue, region);
    });

    [Fact] public void Region_Union() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region(new Rectangle(10, 10, 50, 50));
        region.Union(new Rectangle(40, 40, 50, 50));
        g.FillRegion(Brushes.Green, region);
    });

    [Fact] public void Region_Exclude() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region(new Rectangle(10, 10, 80, 80));
        using var path = new GraphicsPath();
        path.AddEllipse(30, 30, 40, 40);
        region.Exclude(path);
        g.FillRegion(Brushes.Red, region);
    });

    [Fact] public void Region_Xor() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region(new Rectangle(10, 10, 60, 60));
        region.Xor(new Rectangle(30, 30, 60, 60));
        g.FillRegion(Brushes.Purple, region);
    });

    [Fact] public void Region_Complement() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region(new Rectangle(10, 10, 50, 50));
        region.Complement(new Rectangle(30, 30, 60, 60));
        g.FillRegion(Brushes.Orange, region);
    });

    [Fact] public void Region_Complex() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region(new Rectangle(5, 5, 90, 90));
        region.Exclude(new Rectangle(20, 20, 30, 30));
        region.Union(new Rectangle(60, 60, 30, 30));
        region.Intersect(new Rectangle(10, 10, 80, 80));
        g.FillRegion(Brushes.DarkCyan, region);
    });

    [Fact] public void Region_FromPath() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var path = new GraphicsPath();
        path.AddPolygon(new PointF[] { new(50, 5), new(95, 50), new(50, 95), new(5, 50) });
        using var region = new Region(path);
        g.SetClip(region, CombineMode.Replace);
        g.FillRectangle(Brushes.Magenta, 0, 0, 100, 100);
    });

    [Fact] public void Region_InfiniteExclude() => Render(100, 100, g => {
        g.SmoothingMode = SmoothingMode.None; g.Clear(Color.White);
        using var region = new Region();
        region.Exclude(new Rectangle(20, 20, 60, 60));
        g.SetClip(region, CombineMode.Replace);
        g.FillRectangle(Brushes.DarkGreen, 0, 0, 100, 100);
    });
}
