using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Extended.Iconify
{
	public class SKTextRun
	{
		private const string IconTemplateBegin = "{{";
		private const string IconTemplateEnd = "}}";

		public SKTextRun(string text)
			: this(text, SKTextEncoding.Utf8)
		{
		}

		public SKTextRun(string text, SKTextEncoding encoding)
		{
			Text = StringUtilities.GetEncodedText(text, encoding);
			TextEncoding = encoding;
		}

		public SKTextRun(byte[] text, SKTextEncoding encoding)
		{
			Text = text;
			TextEncoding = encoding;
		}

		public byte[] Text { get; }
		public SKTextEncoding? TextEncoding { get; }

		public SKTypeface Typeface { get; set; }
		public float? TextSize { get; set; }
		public SKPoint Offset { get; set; }
		public SKColor? Color { get; set; }

		public override string ToString()
		{
			return StringUtilities.GetString(Text, TextEncoding ?? SKTextEncoding.Utf8);
		}

		public static IEnumerable<SKTextRun> Create(string text, SKTextRunLookup lookup)
		{
			var runs = new List<SKTextRun>();

			if (string.IsNullOrEmpty(text))
				return runs;

			var start = 0;

			while (start < text.Length)
			{
				var startIndex = text.IndexOf(IconTemplateBegin, start, StringComparison.Ordinal);
				if (startIndex == -1)
					break;

				var endIndex = text.IndexOf(IconTemplateEnd, startIndex, StringComparison.Ordinal);
				if (endIndex == -1)
					break;

				var pre = text.Substring(start, startIndex - start);
				var post = text.Substring(endIndex + IconTemplateEnd.Length);

				var expression = text.Substring(startIndex + IconTemplateBegin.Length, endIndex - startIndex - IconTemplateEnd.Length);
				var segments = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				expression = segments.FirstOrDefault();

				SKColor? color = null;
				foreach (var item in segments)
				{
					var pair = item.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					switch (pair[0].ToLower())
					{
						case "color":
							if (pair.Length > 1 && !string.IsNullOrWhiteSpace(pair[1]))
							{
								color = SKColor.Parse(pair[1]);
							}
							break;
					}
				}

				runs.Add(new SKTextRun(pre));
				SKTypeface typeface;
				string character;
				if (lookup.TryLookup(expression, out typeface, out character))
				{
					runs.Add(new SKTextRun(character) { Typeface = typeface, Color = color });
				}
				else
				{
					runs.Add(new SKTextRun(IconTemplateBegin + expression + IconTemplateEnd));
				}

				start = endIndex + IconTemplateEnd.Length;
			}

			if (start < text.Length)
			{
				runs.Add(new SKTextRun(text.Substring(start)));
			}

			return runs;
		}
	}
}
