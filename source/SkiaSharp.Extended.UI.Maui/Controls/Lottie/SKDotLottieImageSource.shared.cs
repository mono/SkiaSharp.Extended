using System.IO.Compression;
using System.Text.Json;

namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Loads a Lottie animation from a .lottie (dotLottie) file format.
/// The .lottie format is a ZIP archive containing animations and assets.
/// </summary>
public class SKDotLottieImageSource : SKLottieImageSource
{
	public static readonly BindableProperty FileProperty = BindableProperty.Create(
		nameof(File), typeof(string), typeof(SKDotLottieImageSource),
		propertyChanged: OnSourceChanged);

	public string? File
	{
		get => (string?)GetValue(FileProperty);
		set => SetValue(FileProperty, value);
	}

	public override bool IsEmpty =>
		string.IsNullOrEmpty(File);

	public override async Task<SKLottieAnimation> LoadAnimationAsync(CancellationToken cancellationToken = default)
	{
		if (IsEmpty || string.IsNullOrEmpty(File))
			return new SKLottieAnimation();

		try
		{
			// Load the .lottie file (ZIP archive)
			using var zipStream = await LoadFile(File);
			if (zipStream is null)
				throw new FileLoadException($"Unable to load .lottie file \"{File}\".");

			// Extract to temporary directory
			var tempDir = Path.Combine(Path.GetTempPath(), $"lottie_{Guid.NewGuid():N}");
			Directory.CreateDirectory(tempDir);

			try
			{
				// Extract ZIP contents
				using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: false))
				{
					archive.ExtractToDirectory(tempDir);
				}

				// Read manifest.json to find the initial animation
				var manifestPath = Path.Combine(tempDir, "manifest.json");
				if (!System.IO.File.Exists(manifestPath))
					throw new FileLoadException($".lottie file \"{File}\" does not contain a manifest.json file.");

				var manifestJson = await System.IO.File.ReadAllTextAsync(manifestPath, cancellationToken);
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
					throw new FileLoadException($".lottie file \"{File}\" does not contain any animations.");

				var fullAnimationPath = Path.Combine(tempDir, animationPath);
				if (!System.IO.File.Exists(fullAnimationPath))
					throw new FileLoadException($"Animation file \"{animationPath}\" not found in .lottie archive.");

				// Set ImageAssetsFolder to the temp directory so images can be loaded
				// Images in .lottie are in the 'i' subdirectory
				var imagesDir = Path.Combine(tempDir, "i");
				if (Directory.Exists(imagesDir))
				{
					ImageAssetsFolder = tempDir;
				}

				// Load the animation
				using var animStream = System.IO.File.OpenRead(fullAnimationPath);
				var animation = CreateAnimationBuilder().Build(animStream);
				if (animation is null)
					throw new FileLoadException($"Unable to parse Lottie animation in .lottie file \"{File}\".");

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
		catch (Exception ex)
		{
			throw new FileLoadException($"Error loading .lottie file \"{File}\".", ex);
		}
	}

	private static async Task<Stream> LoadFile(string filename)
	{
		try
		{
			return await FileSystem.OpenAppPackageFileAsync(filename).ConfigureAwait(false);
		}
		catch (NotImplementedException)
		{
			var root = AppContext.BaseDirectory;
			var path = Path.Combine(root, filename);
			return System.IO.File.OpenRead(path);
		}
	}
}
