namespace SkiaSharp.Extended.UI.Controls.Converters;

#if XAMARIN_FORMS
[TypeConversion(typeof(SKLottieImageSource))]
#endif
public sealed class SKLottieImageSourceConverter : StringTypeConverter
{
	protected override object? Convert(string? value)
	{
		if (string.IsNullOrEmpty(value))
			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(SKLottieImageSource)}");

		if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme != "file")
			return SKLottieImageSource.FromUri(uri);

		return SKLottieImageSource.FromFile(value!);
	}

	protected override string? ConvertTo(object? value) =>
		value switch
		{
			SKFileLottieImageSource fis => fis.File,
			SKUriLottieImageSource uis => uis.Uri?.ToString(),
			_ => throw new NotSupportedException()
		};
}
