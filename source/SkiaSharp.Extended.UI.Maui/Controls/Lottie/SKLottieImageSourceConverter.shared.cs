namespace SkiaSharp.Extended.UI.Controls.Converters;

public sealed class SKLottieImageSourceConverter : StringTypeConverter
{
	protected override object? ConvertFromStringCore(string? value)
	{
		if (string.IsNullOrEmpty(value))
			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(SKLottieImageSource)}");

		if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme != "file")
			return SKLottieImageSource.FromUri(uri);

		return SKLottieImageSource.FromFile(value!);
	}

	protected override string? ConvertToStringCore(object? value) =>
		value switch
		{
			SKFileLottieImageSource fis => fis.File,
			SKUriLottieImageSource uis => uis.Uri?.ToString(),
			_ => throw new NotSupportedException()
		};
}
