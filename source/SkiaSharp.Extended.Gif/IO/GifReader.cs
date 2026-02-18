using System;
using System.IO;
using System.Text;

namespace SkiaSharp.Extended.Gif.IO
{
	/// <summary>
	/// Low-level GIF block reader following the GIF specification.
	/// Reads and parses GIF data structures from a stream.
	/// </summary>
	internal class GifReader : IDisposable
	{
		private readonly Stream stream;
		private readonly BinaryReader reader;
		private bool disposed;

		public GifReader(Stream stream)
		{
			this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
			this.reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
		}

		/// <summary>
		/// Reads the GIF header (6 bytes: signature + version).
		/// </summary>
		public GifHeader ReadHeader()
		{
			var bytes = reader.ReadBytes(6);
			if (bytes.Length < 6)
				throw new InvalidDataException("Unexpected end of stream reading GIF header.");

			var signature = Encoding.ASCII.GetString(bytes, 0, 3);
			var version = Encoding.ASCII.GetString(bytes, 3, 3);

			var header = new GifHeader
			{
				Signature = signature,
				Version = version
			};

			if (!header.IsValid)
				throw new InvalidDataException($"Invalid GIF header. Got '{signature}{version}', expected 'GIF87a' or 'GIF89a'.");

			return header;
		}

		/// <summary>
		/// Reads the Logical Screen Descriptor (7 bytes).
		/// </summary>
		public LogicalScreenDescriptor ReadLogicalScreenDescriptor()
		{
			var width = reader.ReadUInt16();
			var height = reader.ReadUInt16();
			var packed = reader.ReadByte();
			var backgroundColorIndex = reader.ReadByte();
			var pixelAspectRatio = reader.ReadByte();

			return new LogicalScreenDescriptor
			{
				Width = width,
				Height = height,
				HasGlobalColorTable = (packed & 0x80) != 0,
				ColorResolution = (byte)((packed >> 4) & 0x07),
				SortFlag = (packed & 0x08) != 0,
				GlobalColorTableSize = (byte)(packed & 0x07),
				BackgroundColorIndex = backgroundColorIndex,
				PixelAspectRatio = pixelAspectRatio
			};
		}

		/// <summary>
		/// Reads a color table (RGB triplets).
		/// </summary>
		public SKColor[] ReadColorTable(int count)
		{
			if (count <= 0 || count > 256)
				throw new ArgumentOutOfRangeException(nameof(count), "Color table size must be 1-256.");

			var colors = new SKColor[count];
			for (int i = 0; i < count; i++)
			{
				var r = reader.ReadByte();
				var g = reader.ReadByte();
				var b = reader.ReadByte();
				colors[i] = new SKColor(r, g, b);
			}
			return colors;
		}

		/// <summary>
		/// Reads an Image Descriptor (10 bytes).
		/// </summary>
		public ImageDescriptor ReadImageDescriptor()
		{
			var left = reader.ReadUInt16();
			var top = reader.ReadUInt16();
			var width = reader.ReadUInt16();
			var height = reader.ReadUInt16();
			var packed = reader.ReadByte();

			return new ImageDescriptor
			{
				Left = left,
				Top = top,
				Width = width,
				Height = height,
				HasLocalColorTable = (packed & 0x80) != 0,
				InterlaceFlag = (packed & 0x40) != 0,
				SortFlag = (packed & 0x20) != 0,
				LocalColorTableSize = (byte)(packed & 0x07)
			};
		}

		/// <summary>
		/// Reads a Graphics Control Extension.
		/// </summary>
		public GraphicsControlExtension ReadGraphicsControlExtension()
		{
			var blockSize = reader.ReadByte();
			if (blockSize != 4)
				throw new InvalidDataException($"Graphics Control Extension block size must be 4, got {blockSize}.");

			var packed = reader.ReadByte();
			var delayTime = reader.ReadUInt16();
			var transparentColorIndex = reader.ReadByte();
			
			// Block terminator
			var terminator = reader.ReadByte();
			if (terminator != 0)
				throw new InvalidDataException($"Expected block terminator (0), got {terminator}.");

			return new GraphicsControlExtension
			{
				DisposalMethod = (byte)((packed >> 2) & 0x07),
				UserInputFlag = (packed & 0x02) != 0,
				TransparencyFlag = (packed & 0x01) != 0,
				DelayTime = delayTime,
				TransparentColorIndex = transparentColorIndex
			};
		}

		/// <summary>
		/// Reads an Application Extension.
		/// </summary>
		public ApplicationExtension ReadApplicationExtension()
		{
			var blockSize = reader.ReadByte();
			if (blockSize != 11)
				throw new InvalidDataException($"Application Extension block size must be 11, got {blockSize}.");

			var appId = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
			var authCode = reader.ReadBytes(3);

			// Read application data sub-blocks
			var data = ReadDataSubBlocks();

			return new ApplicationExtension
			{
				ApplicationIdentifier = appId,
				AuthenticationCode = authCode,
				Data = data
			};
		}

		/// <summary>
		/// Reads comment extension data.
		/// </summary>
		public string ReadCommentExtension()
		{
			var data = ReadDataSubBlocks();
			return Encoding.ASCII.GetString(data);
		}

		/// <summary>
		/// Reads data sub-blocks (used by extensions and image data).
		/// Format: [size][data...] repeated, terminated by 0.
		/// </summary>
		public byte[] ReadDataSubBlocks()
		{
			using var ms = new MemoryStream();
			
			while (true)
			{
				var blockSize = reader.ReadByte();
				if (blockSize == 0)
					break; // Block terminator

				var data = reader.ReadBytes(blockSize);
				if (data.Length < blockSize)
					throw new InvalidDataException($"Unexpected end of stream reading data sub-block (expected {blockSize} bytes, got {data.Length}).");

				ms.Write(data, 0, data.Length);
			}

			return ms.ToArray();
		}

		/// <summary>
		/// Skips an unknown extension.
		/// </summary>
		public void SkipExtension()
		{
			// Skip block size
			var blockSize = reader.ReadByte();
			stream.Seek(blockSize, SeekOrigin.Current);
			
			// Skip data sub-blocks
			ReadDataSubBlocks();
		}

		/// <summary>
		/// Peeks the next byte without advancing the stream.
		/// </summary>
		public byte PeekByte()
		{
			var b = reader.ReadByte();
			stream.Seek(-1, SeekOrigin.Current);
			return b;
		}

		/// <summary>
		/// Reads a single byte.
		/// </summary>
		public byte ReadByte()
		{
			return reader.ReadByte();
		}

		/// <summary>
		/// Reads the LZW minimum code size for image data.
		/// </summary>
		public byte ReadLzwMinimumCodeSize()
		{
			return reader.ReadByte();
		}

		/// <summary>
		/// Reads a color table as byte array (RGB triplets).
		/// </summary>
		/// <param name="colorTableSize">The size field from the GIF header (0-7).</param>
		public byte[] ReadColorTableBytes(int colorTableSize)
		{
			int count = 1 << (colorTableSize + 1);
			int byteCount = count * 3; // RGB triplets
			
			var data = reader.ReadBytes(byteCount);
			if (data.Length < byteCount)
				throw new InvalidDataException($"Unexpected end of stream reading color table (expected {byteCount} bytes, got {data.Length}).");
			
			return data;
		}

		/// <summary>
		/// Reads multiple bytes.
		/// </summary>
		public byte[] ReadBytes(int count)
		{
			var bytes = reader.ReadBytes(count);
			if (bytes.Length < count)
				throw new InvalidDataException($"Unexpected end of stream (expected {count} bytes, got {bytes.Length}).");
			return bytes;
		}

		/// <summary>
		/// Reads the next block type.
		/// </summary>
		public BlockType ReadBlockType()
		{
			var b = reader.ReadByte();
			return b switch
			{
				0x21 => BlockType.Extension,
				0x2C => BlockType.ImageDescriptor,
				0x3B => BlockType.Trailer,
				_ => throw new InvalidDataException($"Unknown block type: 0x{b:X2}")
			};
		}

		/// <summary>
		/// Reads the extension type (after reading Extension block type).
		/// </summary>
		public ExtensionType ReadExtensionType()
		{
			var b = reader.ReadByte();
			return b switch
			{
				0xF9 => ExtensionType.GraphicsControl,
				0xFE => ExtensionType.Comment,
				0x01 => ExtensionType.PlainText,
				0xFF => ExtensionType.Application,
				_ => ExtensionType.Unknown
			};
		}

		/// <summary>
		/// Gets the current stream position.
		/// </summary>
		public long Position => stream.Position;

		public void Dispose()
		{
			if (!disposed)
			{
				reader?.Dispose();
				disposed = true;
			}
		}
	}

	/// <summary>
	/// GIF block types.
	/// </summary>
	internal enum BlockType
	{
		Extension,
		ImageDescriptor,
		Trailer
	}

	/// <summary>
	/// GIF extension types.
	/// </summary>
	internal enum ExtensionType
	{
		GraphicsControl,
		Comment,
		PlainText,
		Application,
		Unknown
	}

	/// <summary>
	/// GIF disposal methods.
	/// </summary>
	internal enum DisposalMethod : byte
	{
		None = 0,
		DoNotDispose = 1,
		RestoreToBackground = 2,
		RestoreToPrevious = 3
	}
}
