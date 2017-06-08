using System;
using System.IO;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class MaterialDesignIcons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.materialdesignicons-webfont.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(MaterialDesignIcons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(MaterialDesignIcons), ManifestResourceName);
	}

	public class MaterialDesignIconsLookupEntry : SKTextRunLookupEntry
	{
		public MaterialDesignIconsLookupEntry()
			: base(MaterialDesignIcons.GetTypeface(), true, MaterialDesignIcons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<MaterialDesignIconsLookupEntry> entry = new Lazy<MaterialDesignIconsLookupEntry>(() => new MaterialDesignIconsLookupEntry());

		public static void AddMaterialDesignIcons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveMaterialDesignIcons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
