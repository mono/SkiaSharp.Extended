using System;
using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class Meteocons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.meteocons-webfont.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(Meteocons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(Meteocons), ManifestResourceName);
	}

	public class MeteoconsLookupEntry : SKTextRunLookupEntry
	{
		public MeteoconsLookupEntry()
			: base(Meteocons.GetTypeface(), true, Meteocons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<MeteoconsLookupEntry> entry = new Lazy<MeteoconsLookupEntry>(() => new MeteoconsLookupEntry());

		public static void AddMeteocons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveMeteocons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
