using System.Collections.Generic;

namespace SkiaSharp.Extended.Iconify
{
	public static partial class Meteocons
	{
		public static readonly IReadOnlyDictionary<string, string> Characters;

		static Meteocons()
		{
			Characters = new Dictionary<string, string>
			{
				{ "mc-sunrise-o", "A" },
				{ "mc-sun-o", "B" },
				{ "mc-moon-o", "C" },
				{ "mc-eclipse", "D" },
				{ "mc-cloudy-o", "E" },
				{ "mc-wind-o", "F" },
				{ "mc-snow", "G" },
				{ "mc-sun-cloud-o", "H" },
				{ "mc-moon-cloud-o", "I" },
				{ "mc-sunrise-sea-o", "J" },
				{ "mc-moonrise-sea-o", "K" },
				{ "mc-cloud-sea-o", "L" },
				{ "mc-sea-o", "M" },
				{ "mc-cloud-o", "N" },
				{ "mc-cloud-thunder-o", "O" },
				{ "mc-cloud-thunder2-o", "P" },
				{ "mc-cloud-drop-o", "Q" },
				{ "mc-cloud-rain-o", "R" },
				{ "mc-cloud-wind-o", "S" },
				{ "mc-cloud-wind-rain-o", "T" },
				{ "mc-cloud-snow-o", "U" },
				{ "mc-cloud-snow2-o", "V" },
				{ "mc-cloud-snow3-o", "W" },
				{ "mc-cloud-rain2-o", "X" },
				{ "mc-cloud-double-o", "Y" },
				{ "mc-cloud-double-thunder-o", "Z" },
				{ "mc-cloud-double-thunder2-o", "0" },
				{ "mc-sun", "1" },
				{ "mc-moon", "2" },
				{ "mc-sun-cloud", "3" },
				{ "mc-moon-cloud", "4" },
				{ "mc-cloud", "5" },
				{ "mc-cloud-thunder", "6" },
				{ "mc-cloud-drop", "7" },
				{ "mc-cloud-rain", "8" },
				{ "mc-cloud-wind", "9" },
				{ "mc-cloud-wind-rain", "!" },
				{ "mc-cloud-snow", "\"" },
				{ "mc-cloud-snow2", "#" },
				{ "mc-cloud-rain2", "$" },
				{ "mc-cloud-double", "%" },
				{ "mc-cloud-double-thunder", "&" },
				{ "mc-thermometer", "\'" },
				{ "mc-compass", "(" },
				{ "mc-not-applicable", ")" },
				{ "mc-celsius", "*" },
				{ "mc-fahrenheit", "+"}
			};
		}
	}
}
