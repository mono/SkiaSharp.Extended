using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif.Decoding
{
/// <summary>
/// Parses the complete structure of a GIF file and extracts all frames and metadata.
/// </summary>
internal class GifParser
{
private readonly GifReader reader;
private readonly Stream stream;

public GifParser(Stream stream)
{
this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
this.reader = new GifReader(stream);
}

/// <summary>
/// Parses the complete GIF file and returns all frame data.
/// </summary>
public ParsedGif Parse()
{
// Read header
var header = reader.ReadHeader();

// Read logical screen descriptor
var screenDescriptor = reader.ReadLogicalScreenDescriptor();

// Read global color table if present
SKColor[]? globalColorTable = null;
if (screenDescriptor.HasGlobalColorTable)
{
globalColorTable = reader.ReadColorTable(screenDescriptor.GlobalColorTableLength);
}

// Parse all blocks
var frames = new List<ParsedFrame>();
GraphicsControlExtension? currentGCE = null;
int loopCount = -1; // -1 means no loop extension found

while (true)
{
var blockType = reader.ReadBlockType();

if (blockType == BlockType.Trailer)
{
// End of file
break;
}
else if (blockType == BlockType.Extension)
{
var extensionType = reader.ReadExtensionType();

if (extensionType == ExtensionType.GraphicsControl)
{
currentGCE = reader.ReadGraphicsControlExtension();
}
else if (extensionType == ExtensionType.Application)
{
var appExt = reader.ReadApplicationExtension();
if (appExt.IsNetscapeExtension && appExt.LoopCount.HasValue)
{
loopCount = appExt.LoopCount.Value;
}
}
else if (extensionType == ExtensionType.Comment)
{
// Read and discard comment
reader.ReadCommentExtension();
}
else if (extensionType == ExtensionType.PlainText)
{
// Skip plain text extension
reader.ReadDataSubBlocks();
}
else
{
// Unknown extension, skip it
reader.ReadDataSubBlocks();
}
}
else if (blockType == BlockType.ImageDescriptor)
{
var imageDescriptor = reader.ReadImageDescriptor();

// Read local color table if present
SKColor[]? localColorTable = null;
if (imageDescriptor.HasLocalColorTable)
{
localColorTable = reader.ReadColorTable(imageDescriptor.LocalColorTableLength);
}

// Read LZW minimum code size
var lzwMinCodeSize = reader.ReadByte();

// Read compressed image data
var compressedData = reader.ReadDataSubBlocks();

// Create frame
var frame = new ParsedFrame
{
ImageDescriptor = imageDescriptor,
GraphicsControlExtension = currentGCE,
LocalColorTable = localColorTable,
GlobalColorTable = globalColorTable,
LzwMinimumCodeSize = lzwMinCodeSize,
CompressedData = compressedData
};

frames.Add(frame);

// Reset GCE for next frame
currentGCE = null;
}
else
{
throw new InvalidDataException($"Unexpected block type: {blockType}");
}
}

return new ParsedGif
{
Header = header,
ScreenDescriptor = screenDescriptor,
GlobalColorTable = globalColorTable,
Frames = frames.ToArray(),
LoopCount = loopCount
};
}
}

/// <summary>
/// Represents a completely parsed GIF file.
/// </summary>
internal class ParsedGif
{
public GifHeader Header { get; set; }
public LogicalScreenDescriptor ScreenDescriptor { get; set; }
public SKColor[]? GlobalColorTable { get; set; }
public ParsedFrame[] Frames { get; set; } = Array.Empty<ParsedFrame>();
public int LoopCount { get; set; }
}

/// <summary>
/// Represents a single parsed frame with all its data.
/// </summary>
internal class ParsedFrame
{
public ImageDescriptor ImageDescriptor { get; set; }
public GraphicsControlExtension? GraphicsControlExtension { get; set; }
public SKColor[]? LocalColorTable { get; set; }
public SKColor[]? GlobalColorTable { get; set; }
public byte LzwMinimumCodeSize { get; set; }
public byte[] CompressedData { get; set; } = Array.Empty<byte>();

/// <summary>
/// Gets the color table to use for this frame (local takes precedence over global).
/// </summary>
public SKColor[] GetColorTable()
{
if (LocalColorTable != null)
return LocalColorTable;
if (GlobalColorTable != null)
return GlobalColorTable;

// Fallback for broken GIFs with no color table
// Return a simple black and white palette
return new[] { SKColors.Black, SKColors.White };
}
}
}
