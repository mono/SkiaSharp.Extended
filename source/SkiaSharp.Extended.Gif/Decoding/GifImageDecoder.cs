using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp.Extended.Gif.Codec;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif.Decoding
{
	/// <summary>
	/// Internal class that handles the actual GIF decoding logic.
	/// Separates decoding implementation from public API.
	/// </summary>
	internal class GifImageDecoder
	{
		private readonly Stream stream;
		private GifHeader header;
		private LogicalScreenDescriptor screenDescriptor;
		private byte[]? globalColorTable;
		private readonly List<GifFrameData> frames = new List<GifFrameData>();
		private int loopCount = 1; // Default: play once

		public GifImageDecoder(Stream stream)
		{
			this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
		}

		/// <summary>
		/// Parses the entire GIF file and extracts metadata.
		/// </summary>
		public void Parse()
		{
			using var reader = new GifReader(stream);
			
			// Read header and logical screen descriptor
			header = reader.ReadHeader();
			screenDescriptor = reader.ReadLogicalScreenDescriptor();
			
			// Read global color table if present
			if (screenDescriptor.HasGlobalColorTable)
			{
				int colorTableSize = screenDescriptor.GlobalColorTableSize;
				globalColorTable = reader.ReadColorTableBytes(colorTableSize);
			}

			// Read all blocks until trailer
			GraphicsControlExtension? currentGCE = null;
			
			while (true)
			{
				var blockType = reader.ReadBlockType();
				
				if (blockType == BlockType.Trailer)
					break;
					
				switch (blockType)
				{
					case BlockType.ImageDescriptor:
						var imageDesc = reader.ReadImageDescriptor();
						byte[]? localColorTable = null;
						
						if (imageDesc.HasLocalColorTable)
						{
							localColorTable = reader.ReadColorTableBytes(imageDesc.LocalColorTableSize);
						}
						
						// Read image data
						var minCodeSize = reader.ReadByte();
						var compressedData = reader.ReadDataSubBlocks();
						
						frames.Add(new GifFrameData
						{
							ImageDescriptor = imageDesc,
							GraphicsControl = currentGCE,
							LocalColorTable = localColorTable,
							MinimumCodeSize = minCodeSize,
							CompressedData = compressedData
						});
						
						// Reset graphics control for next frame
						currentGCE = null;
						break;
						
					case BlockType.Extension:
						var extType = reader.ReadExtensionType();
						
						switch (extType)
						{
							case ExtensionType.GraphicsControl:
								currentGCE = reader.ReadGraphicsControlExtension();
								break;
								
							case ExtensionType.Application:
								var appExt = reader.ReadApplicationExtension();
								
								// Check for NETSCAPE loop extension
								if (appExt.IsNetscapeExtension && appExt.LoopCount.HasValue)
								{
									loopCount = appExt.LoopCount.Value;
								}
								break;
								
							case ExtensionType.Comment:
								// Read and discard comment data
								reader.ReadDataSubBlocks();
								break;
								
							case ExtensionType.PlainText:
								// Skip plain text extension
								reader.ReadByte(); // block size
								reader.ReadBytes(12); // plain text data
								reader.ReadDataSubBlocks(); // text data
								break;
								
							default:
								// Unknown extension - skip data sub-blocks
								reader.ReadDataSubBlocks();
								break;
						}
						break;
						
					default:
						throw new InvalidDataException($"Unexpected block type: {blockType}");
				}
			}
			
			if (frames.Count == 0)
				throw new InvalidDataException("GIF contains no image frames.");
		}

		/// <summary>
		/// Gets the logical screen width.
		/// </summary>
		public int Width => screenDescriptor.Width;

		/// <summary>
		/// Gets the logical screen height.
		/// </summary>
		public int Height => screenDescriptor.Height;

		/// <summary>
		/// Gets the background color index.
		/// </summary>
		public byte BackgroundColorIndex => screenDescriptor.BackgroundColorIndex;

		/// <summary>
		/// Gets the loop count (0 = infinite).
		/// </summary>
		public int LoopCount => loopCount;

		/// <summary>
		/// Gets the number of frames.
		/// </summary>
		public int FrameCount => frames.Count;

		/// <summary>
		/// Gets frame data for a specific frame.
		/// </summary>
		public GifFrameData GetFrameData(int index)
		{
			if (index < 0 || index >= frames.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
				
			return frames[index];
		}

		/// <summary>
		/// Gets the color table to use for a specific frame.
		/// Returns local color table if present, otherwise global color table.
		/// </summary>
		public byte[] GetColorTableForFrame(int index)
		{
			var frame = GetFrameData(index);
			
			if (frame.LocalColorTable != null)
				return frame.LocalColorTable;
				
			if (globalColorTable != null)
				return globalColorTable;
				
			throw new InvalidDataException($"Frame {index} has no color table.");
		}

		/// <summary>
		/// Decompresses the LZW-compressed image data for a frame.
		/// </summary>
		public byte[] DecompressFrameData(int index)
		{
			var frame = GetFrameData(index);
			var imageDesc = frame.ImageDescriptor;
			
			// Calculate expected output size
			int pixelCount = imageDesc.Width * imageDesc.Height;
			byte[] pixels = new byte[pixelCount];
			
			// Decompress using LZW
			using var compressedStream = new MemoryStream(frame.CompressedData);
			using var decoder = new LzwDecoder(compressedStream, frame.MinimumCodeSize);
			
			int bytesRead = decoder.Decompress(pixels, 0, pixelCount);
			
			if (bytesRead != pixelCount)
				throw new InvalidDataException($"Expected {pixelCount} pixels, got {bytesRead}.");
				
			return pixels;
		}
	}

	/// <summary>
	/// Internal structure holding all data for a single GIF frame.
	/// </summary>
	internal class GifFrameData
	{
		public ImageDescriptor ImageDescriptor { get; set; }
		public GraphicsControlExtension? GraphicsControl { get; set; }
		public byte[]? LocalColorTable { get; set; }
		public int MinimumCodeSize { get; set; }
		public byte[] CompressedData { get; set; } = Array.Empty<byte>();
	}
}
