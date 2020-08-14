using System;
using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class MaterialIcons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.MaterialIcons-Regular.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(MaterialIcons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(MaterialIcons), ManifestResourceName);
	}

	public class MaterialIconsLookupEntry : SKTextRunLookupEntry
	{
		public MaterialIconsLookupEntry()
			: base(MaterialIcons.GetTypeface(), true, MaterialIcons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<MaterialIconsLookupEntry> entry = new Lazy<MaterialIconsLookupEntry>(() => new MaterialIconsLookupEntry());

		public static void AddMaterialIcons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveMaterialIcons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
