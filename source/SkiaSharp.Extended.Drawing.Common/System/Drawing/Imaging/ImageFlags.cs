namespace System.Drawing.Imaging;

[System.FlagsAttribute]
public enum ImageFlags
{
	None = 0,
	Scalable = 1,
	HasAlpha = 2,
	HasTranslucent = 4,
	PartiallyScalable = 8,
	ColorSpaceRgb = 16,
	ColorSpaceCmyk = 32,
	ColorSpaceGray = 64,
	ColorSpaceYcbcr = 128,
	ColorSpaceYcck = 256,
	HasRealDpi = 4096,
	HasRealPixelSize = 8192,
	ReadOnly = 65536,
	Caching = 131072,
}
