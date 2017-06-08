using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SkiaSharp.Extended.Iconify
{
	public class SKTextRunLookupEntry : IDisposable
	{
		private readonly bool disposeTypeface;

		public SKTextRunLookupEntry(SKTypeface typeface, bool disposeTypeface, IReadOnlyDictionary<string, string> characters)
		{
			if (typeface == null)
				throw new ArgumentNullException(nameof(typeface));
			if (characters == null)
				throw new ArgumentNullException(nameof(characters));

			this.disposeTypeface = disposeTypeface;
			Typeface = typeface;
			Characters = characters;
		}

		public SKTextRunLookupEntry(SKTypeface typeface, IReadOnlyDictionary<string, string> characters)
		{
			if (typeface == null)
				throw new ArgumentNullException(nameof(typeface));
			if (characters == null)
				throw new ArgumentNullException(nameof(characters));

			Typeface = typeface;
			Characters = characters;
		}

		public SKTypeface Typeface { get; private set; }

		public IReadOnlyDictionary<string, string> Characters { get; private set; }

		public static Stream GetManifestFontStream(Type type, string resourceName)
		{
			var assembly = type.GetTypeInfo().Assembly;
			return assembly.GetManifestResourceStream(resourceName);
		}

		public Stream GetManifestFontStream(string resourceName)
		{
			return GetManifestFontStream(GetType(), resourceName);
		}

		public static SKTypeface GetManifestTypeface(Type type, string resourceName)
		{
			return SKTypeface.FromStream(GetManifestFontStream(type, resourceName));
		}

		public SKTypeface GetManifestTypeface(string resourceName)
		{
			return GetManifestTypeface(GetType(), resourceName);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposeTypeface)
			{
				Typeface?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
