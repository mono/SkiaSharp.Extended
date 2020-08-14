using System;
using System.IO;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class IonIcons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.ionicons.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(IonIcons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(IonIcons), ManifestResourceName);
	}

	public class IonIconsLookupEntry : SKTextRunLookupEntry
	{
		public IonIconsLookupEntry()
			: base(IonIcons.GetTypeface(), true, IonIcons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<IonIconsLookupEntry> entry = new Lazy<IonIconsLookupEntry>(() => new IonIconsLookupEntry());

		public static void AddIonIcons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveIonIcons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
