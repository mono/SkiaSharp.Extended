using System;

namespace SkiaSharp.Extended.Gif.IO
{
	/// <summary>
	/// GIF file signature and version.
	/// </summary>
	internal struct GifHeader
	{
		/// <summary>
		/// GIF signature ("GIF")
		/// </summary>
		public string Signature { get; set; }

		/// <summary>
		/// GIF version ("87a" or "89a")
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// Gets whether this is a valid GIF header.
		/// </summary>
		public bool IsValid => Signature == "GIF" && (Version == "87a" || Version == "89a");

		/// <summary>
		/// Gets whether this is GIF89a format.
		/// </summary>
		public bool IsGif89a => Version == "89a";
	}

	/// <summary>
	/// Logical Screen Descriptor from GIF specification.
	/// </summary>
	internal struct LogicalScreenDescriptor
	{
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public bool HasGlobalColorTable { get; set; }
		public byte ColorResolution { get; set; }
		public bool SortFlag { get; set; }
		public byte GlobalColorTableSize { get; set; }
		public byte BackgroundColorIndex { get; set; }
		public byte PixelAspectRatio { get; set; }

		/// <summary>
		/// Calculates the number of colors in the global color table.
		/// </summary>
		public int GlobalColorTableLength => HasGlobalColorTable ? (1 << (GlobalColorTableSize + 1)) : 0;
	}

	/// <summary>
	/// Image Descriptor from GIF specification.
	/// </summary>
	internal struct ImageDescriptor
	{
		public ushort Left { get; set; }
		public ushort Top { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public bool HasLocalColorTable { get; set; }
		public bool InterlaceFlag { get; set; }
		public bool SortFlag { get; set; }
		public byte LocalColorTableSize { get; set; }

		/// <summary>
		/// Calculates the number of colors in the local color table.
		/// </summary>
		public int LocalColorTableLength => HasLocalColorTable ? (1 << (LocalColorTableSize + 1)) : 0;

		/// <summary>
		/// Gets whether this image is interlaced.
		/// </summary>
		public bool IsInterlaced => InterlaceFlag;
	}

	/// <summary>
	/// Graphics Control Extension from GIF specification.
	/// </summary>
	internal struct GraphicsControlExtension
	{
		public byte DisposalMethod { get; set; }
		public bool UserInputFlag { get; set; }
		public bool TransparencyFlag { get; set; }
		public ushort DelayTime { get; set; } // In centiseconds (1/100 sec)
		public byte TransparentColorIndex { get; set; }

		/// <summary>
		/// Gets the delay in milliseconds.
		/// </summary>
		public int DelayMs => DelayTime * 10;

		/// <summary>
		/// Gets whether this frame has transparency.
		/// </summary>
		public bool HasTransparency => TransparencyFlag;
	}

	/// <summary>
	/// Application Extension (e.g., NETSCAPE for looping).
	/// </summary>
	internal struct ApplicationExtension
	{
		public string ApplicationIdentifier { get; set; }
		public byte[] AuthenticationCode { get; set; }
		public byte[] Data { get; set; }

		/// <summary>
		/// Gets whether this is the NETSCAPE looping extension.
		/// </summary>
		public bool IsNetscapeExtension => 
			ApplicationIdentifier == "NETSCAPE" && 
			AuthenticationCode != null && 
			AuthenticationCode.Length == 3 &&
			AuthenticationCode[0] == '2' &&
			AuthenticationCode[1] == '.' &&
			AuthenticationCode[2] == '0';

		/// <summary>
		/// Gets the loop count from NETSCAPE extension (0 = infinite).
		/// Returns null if not a NETSCAPE extension or invalid.
		/// </summary>
		public int? LoopCount
		{
			get
			{
				if (!IsNetscapeExtension || Data == null || Data.Length < 3)
					return null;
				
				// NETSCAPE format: byte 0 = 1 (sub-block ID), bytes 1-2 = loop count (little endian)
				if (Data[0] != 1)
					return null;
				
				return Data[1] | (Data[2] << 8);
			}
		}
	}

	/// <summary>
	/// GIF block types.
	/// </summary>
	internal enum GifBlockType : byte
	{
		Extension = 0x21,
		ImageDescriptor = 0x2C,
		Trailer = 0x3B
	}

	/// <summary>
	/// GIF extension types.
	/// </summary>
	internal enum GifExtensionType : byte
	{
		GraphicsControl = 0xF9,
		Comment = 0xFE,
		PlainText = 0x01,
		ApplicationExtension = 0xFF
	}
}
