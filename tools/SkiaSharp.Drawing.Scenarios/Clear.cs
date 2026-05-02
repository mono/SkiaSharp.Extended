using System.Drawing;

namespace SkiaSharp.Drawing.Scenarios;

public class Clear : ScenarioBase
{
    public Clear(string outputDir) : base(outputDir) { }

    public void Clear_Red() => Render(100, 100, g => g.Clear(Color.Red));
    public void Clear_White() => Render(100, 100, g => g.Clear(Color.White));
    public void Clear_Transparent() => Render(100, 100, g => g.Clear(Color.Transparent));
}
