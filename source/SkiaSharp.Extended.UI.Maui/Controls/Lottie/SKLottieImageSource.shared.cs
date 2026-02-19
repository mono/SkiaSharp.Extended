using SkiaSharp.Resources;
using System.IO.Compression;
using System.Text.Json;

namespace SkiaSharp.Extended.UI.Controls;

[TypeConverter(typeof(Converters.SKLottieImageSourceConverter))]
public abstract class SKLottieImageSource : Element
{
	private readonly WeakEventManager weakEventManager = new();

	public static readonly BindableProperty ImageAssetsFolderProperty = BindableProperty.Create(
		nameof(ImageAssetsFolder),
		typeof(string),
		typeof(SKLottieImageSource),
		null,
		propertyChanged: OnSourceChanged);

	public virtual bool IsEmpty => true;

	public string? ImageAssetsFolder
	{
		get => (string?)GetValue(ImageAssetsFolderProperty);
		set => SetValue(ImageAssetsFolderProperty, value);
	}

	public abstract Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads an animation from a stream, automatically detecting if it's a .lottie (ZIP) file or JSON.
	/// </summary>
	protected async Task<SKLottieAnimation> LoadAnimationFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		if (stream is null)
			throw new ArgumentNullException(nameof(stream));

		// Check if this is a .lottie (ZIP) file by reading the first few bytes
		var isZip = await IsZipFileAsync(stream, cancellationToken);

		if (isZip)
		{
			return await LoadDotLottieAnimationAsync(stream, cancellationToken);
		}
		else
		{
			return await LoadJsonAnimationAsync(stream, cancellationToken);
		}
	}

	private async Task<bool> IsZipFileAsync(Stream stream, CancellationToken cancellationToken)
	{
		if (!stream.CanSeek)
			return false;

		var position = stream.Position;
		try
		{
			var buffer = new byte[4];
			var bytesRead = await stream.ReadAsync(buffer, 0, 4, cancellationToken);
			
			// ZIP files start with PK\x03\x04 (0x04034b50 in little-endian)
			if (bytesRead >= 4 && buffer[0] == 0x50 && buffer[1] == 0x4B && 
			    buffer[2] == 0x03 && buffer[3] == 0x04)
			{
				return true;
			}
			
			return false;
		}
		finally
		{
			stream.Position = position;
		}
	}

	private async Task<SKLottieAnimation> LoadDotLottieAnimationAsync(Stream zipStream, CancellationToken cancellationToken)
	{
		// Extract to temporary directory
		var tempDir = Path.Combine(Path.GetTempPath(), $"lottie_{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);

		try
		{
			// Extract ZIP contents
			using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true))
			{
				archive.ExtractToDirectory(tempDir);
			}

			// Read manifest.json to find the initial animation
			var manifestPath = Path.Combine(tempDir, "manifest.json");
			if (!File.Exists(manifestPath))
				throw new FileLoadException(".lottie file does not contain a manifest.json file.");

			var manifestJson = await File.ReadAllTextAsync(manifestPath, cancellationToken);
			var manifest = JsonDocument.Parse(manifestJson);

			// Get the initial animation path from manifest
			string? animationPath = null;
			if (manifest.RootElement.TryGetProperty("initial", out var initial) &&
				initial.TryGetProperty("animation", out var animationId))
			{
				var id = animationId.GetString();
				if (manifest.RootElement.TryGetProperty("animations", out var animations))
				{
					foreach (var anim in animations.EnumerateArray())
					{
						if (anim.TryGetProperty("id", out var aid) && aid.GetString() == id &&
							anim.TryGetProperty("path", out var path))
						{
							animationPath = path.GetString();
							break;
						}
					}
				}
			}

			// Fallback: use first animation in manifest
			if (string.IsNullOrEmpty(animationPath) &&
				manifest.RootElement.TryGetProperty("animations", out var anims) &&
				anims.GetArrayLength() > 0)
			{
				var firstAnim = anims.EnumerateArray().First();
				if (firstAnim.TryGetProperty("path", out var path))
				{
					animationPath = path.GetString();
				}
			}

			// Fallback: look for JSON files in 'a' directory
			if (string.IsNullOrEmpty(animationPath))
			{
				var animDir = Path.Combine(tempDir, "a");
				if (Directory.Exists(animDir))
				{
					var jsonFiles = Directory.GetFiles(animDir, "*.json");
					if (jsonFiles.Length > 0)
					{
						animationPath = Path.Combine("a", Path.GetFileName(jsonFiles[0]));
					}
				}
			}

			if (string.IsNullOrEmpty(animationPath))
				throw new FileLoadException(".lottie file does not contain any animations.");

			var fullAnimationPath = Path.Combine(tempDir, animationPath);
			if (!File.Exists(fullAnimationPath))
				throw new FileLoadException($"Animation file \"{animationPath}\" not found in .lottie archive.");

			// Set ImageAssetsFolder to the temp directory so images can be loaded
			// Images in .lottie are in the 'i' subdirectory
			// Save the original ImageAssetsFolder to restore it later
			var originalImageAssetsFolder = ImageAssetsFolder;
			var imagesDir = Path.Combine(tempDir, "i");
			if (Directory.Exists(imagesDir))
			{
				ImageAssetsFolder = tempDir;
			}

			// Load the animation
			using var animStream = File.OpenRead(fullAnimationPath);
			var animation = CreateAnimationBuilder().Build(animStream);
			
			// Restore original ImageAssetsFolder
			ImageAssetsFolder = originalImageAssetsFolder;

			if (animation is null)
				throw new FileLoadException("Unable to parse Lottie animation in .lottie file.");

			return new SKLottieAnimation(animation);
		}
		finally
		{
			// Clean up temporary directory
			try
			{
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, recursive: true);
				}
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}

	private async Task<SKLottieAnimation> LoadJsonAnimationAsync(Stream stream, CancellationToken cancellationToken)
	{
		var animation = CreateAnimationBuilder().Build(stream);
		if (animation is null)
			throw new FileLoadException("Unable to parse Lottie animation.");

		return new SKLottieAnimation(animation);
	}

	internal Skottie.AnimationBuilder CreateAnimationBuilder()
	{
		var builder = Skottie.Animation.CreateBuilder();

		// Create the resource provider chain
		ResourceProvider resourceProvider;
		if (!string.IsNullOrEmpty(ImageAssetsFolder))
		{
			// Chain DataUriResourceProvider with FileResourceProvider
			// DataUriResourceProvider first handles base64 embedded images (data: URIs)
			// FileResourceProvider (fallback) loads external image files from the specified folder
			// This allows animations to use both embedded and external images
			//
			// LIMITATION: FileResourceProvider uses standard file system I/O and cannot access
			// MAUI app package resources (FileSystem.OpenAppPackageFileAsync). Images must be
			// in an accessible file system location. A custom ResourceProvider would be needed
			// to support app package resources, but ResourceProvider does not currently allow
			// inheritance (Load methods are not virtual/abstract).
			// TODO: Once SkiaSharp.Resources.ResourceProvider supports inheritance, create a
			// custom provider that uses FileSystem.OpenAppPackageFileAsync for MAUI apps.
			resourceProvider = new CachingResourceProvider(
				new DataUriResourceProvider(
					new FileResourceProvider(ImageAssetsFolder)));
		}
		else
		{
			// Default: only handle base64 embedded images
			resourceProvider = new CachingResourceProvider(new DataUriResourceProvider());
		}

		return builder
			.SetResourceProvider(resourceProvider)
			.SetFontManager(SKFontManager.Default);
	}

	public static object FromUri(Uri uri) =>
		new SKUriLottieImageSource { Uri = uri };

	public static object FromFile(string file) =>
		new SKFileLottieImageSource { File = file };

	public static object FromStream(Func<CancellationToken, Task<Stream?>> getter) =>
		new SKStreamLottieImageSource { Stream = getter };

	public static object FromStream(Stream stream) =>
		FromStream(token => Task.FromResult<Stream?>(stream));

	public event EventHandler SourceChanged
	{
		add => weakEventManager.AddEventHandler(value);
		remove => weakEventManager.RemoveEventHandler(value);
	}

	protected static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SKLottieImageSource source)
			source.weakEventManager.HandleEvent(source, EventArgs.Empty, nameof(SourceChanged));
	}
}
