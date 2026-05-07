namespace System.Drawing.Imaging;

[Flags]
public enum ImageCodecFlags
{
	Encoder = 1,
	Decoder = 2,
	SupportBitmap = 4,
	SupportVector = 8,
	SeekableEncode = 16,
	BlockingDecode = 32,
	Builtin = 65536,
	System = 131072,
	User = 262144,
}
