using System;
using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class WeatherIcons
	{
		public const string ManifestResourceName = "SkiaSharp.Extended.Iconify.weathericons-regular-webfont.ttf";

		public static Stream GetFontStream() => SKTextRunLookupEntry.GetManifestFontStream(typeof(WeatherIcons), ManifestResourceName);

		public static SKTypeface GetTypeface() => SKTextRunLookupEntry.GetManifestTypeface(typeof(WeatherIcons), ManifestResourceName);
	}

	public class WeatherIconsLookupEntry : SKTextRunLookupEntry
	{
		public WeatherIconsLookupEntry()
			: base(WeatherIcons.GetTypeface(), true, WeatherIcons.Characters)
		{
		}
	}

	public static class SKTextRunLookupExtensions
	{
		private static readonly Lazy<WeatherIconsLookupEntry> entry = new Lazy<WeatherIconsLookupEntry>(() => new WeatherIconsLookupEntry());

		public static void AddWeatherIcons(this SKTextRunLookup lookup) => lookup.AddTypeface(entry.Value);

		public static void RemoveWeatherIcons(this SKTextRunLookup lookup) => lookup.RemoveTypeface(entry.Value);
	}
}
