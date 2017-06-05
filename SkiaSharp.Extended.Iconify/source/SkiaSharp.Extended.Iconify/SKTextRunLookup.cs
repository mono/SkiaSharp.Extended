using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.Iconify
{
	public class SKTextRunLookup : IDisposable
	{
		private readonly List<SKTextRunLookupEntry> entries;

		public SKTextRunLookup()
		{
			entries = new List<SKTextRunLookupEntry>();
		}

		public IEnumerable<SKTypeface> Typefaces => entries.Select(l => l.Typeface);

		public void AddTypeface(SKTypeface typeface, IReadOnlyDictionary<string, string> characters)
		{
			if (typeface == null)
				throw new ArgumentNullException(nameof(typeface));
			if (characters == null)
				throw new ArgumentNullException(nameof(characters));

			entries.Add(new SKTextRunLookupEntry(typeface, characters));
		}

		public void AddTypeface(SKTextRunLookupEntry entry)
		{
			entries.Add(entry);
		}

		public bool TryLookup(string template, out SKTypeface typeface, out string character)
		{
			foreach (var entry in entries)
			{
				if (entry.Characters.ContainsKey(template))
				{
					typeface = entry.Typeface;
					character = entry.Characters[template];
					return true;
				}
			}

			typeface = null;
			character = null;
			return false;
		}

		protected virtual void Dispose(bool disposing)
		{
			foreach (var entry in entries)
			{
				entry.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}

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
