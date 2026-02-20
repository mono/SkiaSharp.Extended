using Xunit;

namespace SkiaSharp.Extended.UI.Controls.Tests;

/// <summary>
/// Tests for .lottie format detection and loading across all image source types.
/// </summary>
public class SKLottieDotLottieFormatTest
{
	private const string TestLottieFile = "TestAssets/Lottie/test.lottie";
	private const string TrophyJson = "TestAssets/Lottie/trophy.json";

	[Fact]
	public async Task FileSourceAutoDetectsLottieZipFormat()
	{
		var source = new SKFileLottieImageSource { File = TestLottieFile };
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task FileSourceAutoDetectsJsonFormat()
	{
		var source = new SKFileLottieImageSource { File = TrophyJson };
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}

	[Fact]
	public async Task StreamSourceAutoDetectsLottieZipFormat()
	{
		var source = new SKStreamLottieImageSource
		{
			Stream = async ct =>
			{
				var root = AppContext.BaseDirectory;
				var path = Path.Combine(root, TestLottieFile);
				return File.OpenRead(path);
			}
		};
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task StreamSourceAutoDetectsJsonFormat()
	{
		var source = new SKStreamLottieImageSource
		{
			Stream = async ct =>
			{
				var root = AppContext.BaseDirectory;
				var path = Path.Combine(root, TrophyJson);
				return File.OpenRead(path);
			}
		};
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}

	[Fact]
	public async Task LottieFileWithEmbeddedImagesLoadsSuccessfully()
	{
		// .lottie files have images in 'i/' subdirectory which should be auto-detected
		var source = new SKFileLottieImageSource { File = TestLottieFile };
		var animation = await source.LoadAnimationAsync();

		Assert.True(animation.IsLoaded);
		Assert.NotNull(animation.Animation);
	}

	[Fact]
	public async Task LottieFileCanBeLoadedMultipleTimes()
	{
		var source = new SKFileLottieImageSource { File = TestLottieFile };
		
		var animation1 = await source.LoadAnimationAsync();
		Assert.True(animation1.IsLoaded);

		var animation2 = await source.LoadAnimationAsync();
		Assert.True(animation2.IsLoaded);
	}

	[Fact]
	public async Task ImageAssetsFolderIsPreservedAfterLoadingLottieFile()
	{
		var source = new SKFileLottieImageSource
		{
			File = TestLottieFile,
			ImageAssetsFolder = "original/path"
		};

		await source.LoadAnimationAsync();

		// ImageAssetsFolder should not be modified by .lottie loading
		Assert.Equal("original/path", source.ImageAssetsFolder);
	}

	[Fact]
	public async Task LoadingNonSeekableStreamWithLottieFormat()
	{
		var source = new SKStreamLottieImageSource
		{
			Stream = async ct =>
			{
				// Create a non-seekable stream wrapper
				var root = AppContext.BaseDirectory;
				var path = Path.Combine(root, TestLottieFile);
				var fileStream = File.OpenRead(path);
				return new NonSeekableStreamWrapper(fileStream);
			}
		};
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(1), lottie.Duration);
	}

	[Fact]
	public async Task LoadingNonSeekableStreamWithJsonFormat()
	{
		var source = new SKStreamLottieImageSource
		{
			Stream = async ct =>
			{
				// Create a non-seekable stream wrapper
				var root = AppContext.BaseDirectory;
				var path = Path.Combine(root, TrophyJson);
				var fileStream = File.OpenRead(path);
				return new NonSeekableStreamWrapper(fileStream);
			}
		};
		var lottie = new WaitingLottieView { Source = source };

		await lottie.LoadedTask;
		Assert.Equal(TimeSpan.FromSeconds(2.3666665), lottie.Duration);
	}

	[Fact]
	public async Task InvalidZipFileThrowsException()
	{
		var source = new SKStreamLottieImageSource
		{
			Stream = async ct =>
			{
				// Create a stream that starts with ZIP signature but is invalid
				var ms = new MemoryStream();
				ms.Write(new byte[] { 0x50, 0x4B, 0x03, 0x04 }); // ZIP signature
				ms.Write(new byte[100]); // Random data
				ms.Position = 0;
				return ms;
			}
		};

		await Assert.ThrowsAsync<FileLoadException>(() => source.LoadAnimationAsync());
	}

	[Fact]
	public async Task LottieFileWithoutManifestThrowsException()
	{
		// Create a ZIP without manifest.json
		var tempZip = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.zip");
		try
		{
			using (var zip = System.IO.Compression.ZipFile.Open(tempZip, System.IO.Compression.ZipArchiveMode.Create))
			{
				var entry = zip.CreateEntry("test.txt");
				using var writer = new StreamWriter(entry.Open());
				writer.Write("test");
			}

			var source = new SKFileLottieImageSource { File = tempZip };
			await Assert.ThrowsAsync<FileLoadException>(() => source.LoadAnimationAsync());
		}
		finally
		{
			if (File.Exists(tempZip))
				File.Delete(tempZip);
		}
	}

	// Helper class for testing non-seekable streams
	private class NonSeekableStreamWrapper : Stream
	{
		private readonly Stream _inner;

		public NonSeekableStreamWrapper(Stream inner) => _inner = inner;

		public override bool CanRead => _inner.CanRead;
		public override bool CanSeek => false; // Force non-seekable
		public override bool CanWrite => _inner.CanWrite;
		public override long Length => throw new NotSupportedException();
		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override void Flush() => _inner.Flush();
		public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_inner.Dispose();
			base.Dispose(disposing);
		}
	}
}
