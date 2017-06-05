using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Svg.Tests
{
	public abstract class SKTest
	{
		protected static readonly string PathToAssembly = Path.GetDirectoryName(typeof(SKTest).GetTypeInfo().Assembly.Location);
		protected static readonly string PathToFonts = Path.Combine(PathToAssembly, "fonts");
		protected static readonly string PathToImages = Path.Combine(PathToAssembly, "images");
	}
}
