using System;
using SkiaSharp.Extended.Gif.IO;

namespace SkiaSharp.Extended.Gif.Decoding
{
	/// <summary>
	/// Composes GIF frames applying disposal methods and transparency.
	/// Handles the stateful rendering of animated GIFs.
	/// </summary>
	internal class GifFrameCompositor
	{
		private readonly int width;
		private readonly int height;
		private readonly byte backgroundColorIndex;
		private SKBitmap? canvas;
		private SKBitmap? previousFrame;

		public GifFrameCompositor(int width, int height, byte backgroundColorIndex)
		{
			this.width = width;
			this.height = height;
			this.backgroundColorIndex = backgroundColorIndex;
		}

		/// <summary>
		/// Renders a frame with proper disposal method handling.
		/// </summary>
		public SKBitmap RenderFrame(
			int frameIndex,
			byte[] indexedPixels,
			ImageDescriptor imageDesc,
			byte[] colorTable,
			GraphicsControlExtension? gce)
		{
			// Initialize canvas on first frame
			if (canvas == null)
			{
				canvas = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
				canvas.Erase(SKColors.Transparent);
			}

			// Apply disposal method from previous frame
			if (frameIndex > 0 && gce != null)
			{
				ApplyDisposalMethod(gce.DisposalMethod, imageDesc);
			}

			// Render current frame onto canvas
			RenderFrameData(indexedPixels, imageDesc, colorTable, gce);

			// Create a copy to return
			var result = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
			using (var resultCanvas = new SKCanvas(result))
			{
				resultCanvas.DrawBitmap(canvas, 0, 0);
			}

			// Save for potential restore
			if (gce != null && gce.DisposalMethod == DisposalMethod.RestoreToPrevious)
			{
				previousFrame?.Dispose();
				previousFrame = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
				using (var prevCanvas = new SKCanvas(previousFrame))
				{
					prevCanvas.DrawBitmap(canvas, 0, 0);
				}
			}

			return result;
		}

		private void ApplyDisposalMethod(DisposalMethod disposal, ImageDescriptor imageDesc)
		{
			if (canvas == null)
				return;

			switch (disposal)
			{
				case DisposalMethod.None:
				case DisposalMethod.DoNotDispose:
					// Leave canvas as is
					break;

				case DisposalMethod.RestoreToBackground:
					// Clear the frame area to transparent
					using (var skCanvas = new SKCanvas(canvas))
					{
						var rect = new SKRectI(
							imageDesc.Left,
							imageDesc.Top,
							imageDesc.Left + imageDesc.Width,
							imageDesc.Top + imageDesc.Height);
						
						skCanvas.ClipRect(SKRect.Create(rect));
						skCanvas.Clear(SKColors.Transparent);
					}
					break;

				case DisposalMethod.RestoreToPrevious:
					// Restore to previous frame
					if (previousFrame != null)
					{
						using (var skCanvas = new SKCanvas(canvas))
						{
							skCanvas.DrawBitmap(previousFrame, 0, 0);
						}
					}
					break;
			}
		}

		private void RenderFrameData(
			byte[] indexedPixels,
			ImageDescriptor imageDesc,
			byte[] colorTable,
			GraphicsControlExtension? gce)
		{
			if (canvas == null)
				return;

			bool hasTransparency = gce?.HasTransparency ?? false;
			byte transparentIndex = gce?.TransparentColorIndex ?? 0;

			// Handle interlacing
			byte[] deinterlaced = imageDesc.IsInterlaced
				? DeinterlacePixels(indexedPixels, imageDesc.Width, imageDesc.Height)
				: indexedPixels;

			// Render pixels
			using (var skCanvas = new SKCanvas(canvas))
			{
				for (int y = 0; y < imageDesc.Height; y++)
				{
					for (int x = 0; x < imageDesc.Width; x++)
					{
						int pixelIndex = y * imageDesc.Width + x;
						byte colorIndex = deinterlaced[pixelIndex];

						// Skip transparent pixels
						if (hasTransparency && colorIndex == transparentIndex)
							continue;

						// Get RGB color from color table
						if (colorIndex * 3 + 2 < colorTable.Length)
						{
							byte r = colorTable[colorIndex * 3];
							byte g = colorTable[colorIndex * 3 + 1];
							byte b = colorTable[colorIndex * 3 + 2];

							var color = new SKColor(r, g, b, 255);
							int canvasX = imageDesc.Left + x;
							int canvasY = imageDesc.Top + y;

							if (canvasX >= 0 && canvasX < width && canvasY >= 0 && canvasY < height)
							{
								canvas.SetPixel(canvasX, canvasY, color);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Deinterlaces GIF pixels according to GIF89a specification.
		/// Interlaced images use 4 passes with specific row patterns.
		/// </summary>
		private byte[] DeinterlacePixels(byte[] interlaced, int width, int height)
		{
			byte[] deinterlaced = new byte[width * height];
			int sourceIndex = 0;

			// Pass 1: Every 8th row, starting with row 0
			for (int y = 0; y < height; y += 8)
			{
				Array.Copy(interlaced, sourceIndex, deinterlaced, y * width, width);
				sourceIndex += width;
			}

			// Pass 2: Every 8th row, starting with row 4
			for (int y = 4; y < height; y += 8)
			{
				Array.Copy(interlaced, sourceIndex, deinterlaced, y * width, width);
				sourceIndex += width;
			}

			// Pass 3: Every 4th row, starting with row 2
			for (int y = 2; y < height; y += 4)
			{
				Array.Copy(interlaced, sourceIndex, deinterlaced, y * width, width);
				sourceIndex += width;
			}

			// Pass 4: Every 2nd row, starting with row 1
			for (int y = 1; y < height; y += 2)
			{
				Array.Copy(interlaced, sourceIndex, deinterlaced, y * width, width);
				sourceIndex += width;
			}

			return deinterlaced;
		}

		public void Dispose()
		{
			canvas?.Dispose();
			previousFrame?.Dispose();
		}
	}
}
