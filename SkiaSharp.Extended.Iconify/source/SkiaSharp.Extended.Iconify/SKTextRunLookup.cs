using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.Iconify
{
	public class SKTextRunLookup : IDisposable
	{
		private static readonly Lazy<SKTextRunLookup> instance = new Lazy<SKTextRunLookup>(() => new SKTextRunLookup(true));

		private readonly List<SKTextRunLookupEntry> entries;
		private readonly bool disposeEntries;

		public SKTextRunLookup()
			: this(false)
		{
		}

		public SKTextRunLookup(bool disposeEntries)
		{
			this.disposeEntries = disposeEntries;
			entries = new List<SKTextRunLookupEntry>();
		}

		public static SKTextRunLookup Instance => instance.Value;

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
			if (entry == null)
				throw new ArgumentNullException(nameof(entry));

			if (!entries.Contains(entry))
			{
				entries.Add(entry);
			}
		}

		public void RemoveTypeface(SKTextRunLookupEntry entry)
		{
			if (entry == null)
				throw new ArgumentNullException(nameof(entry));

			if (entries.Contains(entry))
			{
				entries.Remove(entry);
			}
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
			if (disposeEntries)
			{
				foreach (var entry in entries)
				{
					entry.Dispose();
				}
			}
			entries.Clear();
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
