namespace SkiaSharp.Extended
{
	/// <summary>
	/// Options for configuring pixel-by-pixel image comparison behavior.
	/// </summary>
	public class SKPixelComparerOptions
	{
		/// <summary>
		/// Gets or sets whether tolerance is applied independently to each color channel.
		/// When <c>true</c>, each channel (R, G, B) is checked separately against the tolerance value or mask channel value.
		/// When <c>false</c>, the sum of per-channel differences is checked against the tolerance (or sum of mask channel values).
		/// Default is <c>true</c>.
		/// </summary>
		public bool TolerancePerChannel { get; set; } = true;

		/// <summary>
		/// Gets or sets whether the alpha channel is included in the comparison.
		/// When <c>true</c>, alpha differences contribute to error metrics and pixel error detection.
		/// When <c>false</c>, only RGB channels are compared.
		/// Default is <c>false</c>.
		/// </summary>
		public bool CompareAlpha { get; set; }
	}
}
