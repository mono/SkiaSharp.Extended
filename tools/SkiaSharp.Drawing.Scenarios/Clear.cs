using System.Drawing;
using Xunit;

namespace SkiaSharp.Drawing.Scenarios;

public class Clear : ScenarioBase
{
    [Fact] public void Clear_Red() => Render(100, 100, g => g.Clear(Color.Red));
    [Fact] public void Clear_White() => Render(100, 100, g => g.Clear(Color.White));
    [Fact] public void Clear_Transparent() => Render(100, 100, g => g.Clear(Color.Transparent));
}
