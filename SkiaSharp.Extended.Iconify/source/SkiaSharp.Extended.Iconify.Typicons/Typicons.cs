using System;
using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class Typicons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.typicons.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(Typicons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(Typicons), ManifestResourceName);
	}

	public class TypiconsLookupEntry : SKTextRunLookupEntry
	{
		public TypiconsLookupEntry()
			: base(Typicons.GetTypeface(), true, Typicons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<TypiconsLookupEntry> entry = new Lazy<TypiconsLookupEntry>(() => new TypiconsLookupEntry());

		public static void AddTypicons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveTypicons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
