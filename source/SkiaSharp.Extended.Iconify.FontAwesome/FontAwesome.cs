using System;
using System.IO;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class FontAwesome
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.fontawesome-webfont.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(FontAwesome), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(FontAwesome), ManifestResourceName);
	}

	public class FontAwesomeLookupEntry : SKTextRunLookupEntry
	{
		public FontAwesomeLookupEntry()
			: base(FontAwesome.GetTypeface(), true, FontAwesome.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<FontAwesomeLookupEntry> entry = new Lazy<FontAwesomeLookupEntry>(() => new FontAwesomeLookupEntry());

		public static void AddFontAwesome(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveFontAwesome(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
