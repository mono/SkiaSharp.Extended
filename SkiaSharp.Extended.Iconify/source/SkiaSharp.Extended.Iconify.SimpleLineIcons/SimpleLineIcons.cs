using System;
using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class SimpleLineIcons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.Simple-Line-Icons.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(SimpleLineIcons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(SimpleLineIcons), ManifestResourceName);
	}

	public class SimpleLineIconsLookupEntry : SKTextRunLookupEntry
	{
		public SimpleLineIconsLookupEntry()
			: base(SimpleLineIcons.GetTypeface(), true, SimpleLineIcons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<SimpleLineIconsLookupEntry> entry = new Lazy<SimpleLineIconsLookupEntry>(() => new SimpleLineIconsLookupEntry());

		public static void AddSimpleLineIcons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveSimpleLineIcons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
