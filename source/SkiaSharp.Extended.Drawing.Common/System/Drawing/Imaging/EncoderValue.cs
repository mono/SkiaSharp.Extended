namespace System.Drawing.Imaging;

public enum EncoderValue
{
	ColorTypeCMYK = 0,
	ColorTypeYCCK = 1,
	CompressionLZW = 2,
	CompressionCCITT3 = 3,
	CompressionCCITT4 = 4,
	CompressionRle = 5,
	CompressionNone = 6,
	ScanMethodInterlaced = 7,
	ScanMethodNonInterlaced = 8,
	VersionGif87 = 9,
	VersionGif89 = 10,
	RenderProgressive = 11,
	RenderNonProgressive = 12,
	TransformRotate90 = 13,
	TransformRotate180 = 14,
	TransformRotate270 = 15,
	TransformFlipHorizontal = 16,
	TransformFlipVertical = 17,
	MultiFrame = 18,
	LastFrame = 19,
	Flush = 20,
	FrameDimensionTime = 21,
	FrameDimensionResolution = 22,
	FrameDimensionPage = 23,
}
