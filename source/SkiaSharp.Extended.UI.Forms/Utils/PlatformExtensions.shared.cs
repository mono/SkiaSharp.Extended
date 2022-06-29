namespace SkiaSharp.Extended.UI;

#if NETSTANDARD
using IVisualElementRenderer = System.Object;
#endif

internal static class PlatformExtensions
{
	internal static void StartTimer(this IDispatcher dispatcher, TimeSpan interval, Func<bool> callback) =>
		Device.StartTimer(interval, callback);

	internal static IVisualElementRenderer? GetRenderer(this VisualElement element) =>
#if NETSTANDARD
		default;
#else
		Platform.GetRenderer(element);
#endif

	internal static bool IsLoadedEx(this VisualElement element) =>
		element.GetRenderer() is not null;

	internal static void RegisterLoadedUnloaded(this VisualElement element, Action? loaded, Action? unloaded)
	{
		element.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName != "Renderer")
				return;

			if (element.GetRenderer() is null)
				unloaded?.Invoke();
			else
				loaded?.Invoke();
		};
	}

	internal static Task<Stream> ReadAsStreamAsync(this HttpContent httpContent, CancellationToken cancellationToken) =>
		httpContent.ReadAsStreamAsync();
}

internal static class FileSystem
{
	internal static Task<Stream> OpenAppPackageFileAsync(string filename)
	{
		if (string.IsNullOrEmpty(filename))
			throw new ArgumentNullException(nameof(filename));

		filename = NormalizePath(filename);

#if __ANDROID__
		try
		{
			var stream = Android.App.Application.Context.Assets!.Open(filename);
			return Task.FromResult(stream);
		}
		catch (Java.IO.FileNotFoundException ex)
		{
			throw new FileNotFoundException(ex.Message, filename, ex);
		}
#elif __IOS__ || __MACOS__
		var root = Foundation.NSBundle.MainBundle.BundlePath;
#if __MACOS__
		root = Path.Combine(root, "Contents", "Resources");
#endif
		var file = Path.Combine(root, filename);
		return Task.FromResult((Stream)System.IO.File.OpenRead(file));
#elif __TIZEN__ || TIZEN
		var root = Tizen.Applications.Application.Current.DirectoryInfo.Resource;
		var file = Path.Combine(root, filename);
		return Task.FromResult((Stream)System.IO.File.OpenRead(file));
#elif WINDOWS_UWP
		var package = Windows.ApplicationModel.Package.Current;
		return package.InstalledLocation.OpenStreamForReadAsync(filename);
#elif NETCOREAPP || NET45_OR_GREATER
		return Task.FromResult((Stream)System.IO.File.OpenRead(filename));
#elif NETSTANDARD
		throw new NotImplementedException();
#endif
	}

	private static string NormalizePath(string filename) =>
		filename
			.Replace('\\', Path.DirectorySeparatorChar)
			.Replace('/', Path.DirectorySeparatorChar);
}
