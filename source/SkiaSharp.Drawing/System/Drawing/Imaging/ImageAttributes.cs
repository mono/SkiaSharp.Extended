using System.Drawing.Drawing2D;

namespace System.Drawing.Imaging
{
	/// <summary>
	///  Contains information about how bitmap and metafile colors are manipulated during rendering.
	/// </summary>
	public sealed partial class ImageAttributes : System.ICloneable, System.IDisposable
	{
		private ColorMatrix? _colorMatrix;
		private ColorMatrix? _grayMatrix;
		private ColorMatrixFlag _colorMatrixFlag;
		private WrapMode _wrapMode = WrapMode.Clamp;
		private Color _wrapColor;
		private bool _wrapClamp;
		private float _gamma = 1.0f;
		private float _threshold;
		private bool _noOp;
		private Color _colorKeyLow;
		private Color _colorKeyHigh;
		private ColorMap[]? _remapTable;
		private ColorMap[]? _brushRemapTable;
		private ColorChannelFlag _outputChannel;
		private string? _outputChannelColorProfile;
		private bool _disposed;

		/// <summary>Initializes a new instance of the <see cref="ImageAttributes"/> class.</summary>
		public ImageAttributes() { }

		/// <summary>Clears the brush color-remap table of this <see cref="ImageAttributes"/> object.</summary>
		public void ClearBrushRemapTable() { _brushRemapTable = null; }
		/// <summary>Clears the color key (transparency range) for the default category.</summary>
		public void ClearColorKey() { _colorKeyLow = Color.Empty; _colorKeyHigh = Color.Empty; }
		/// <summary>Clears the color key (transparency range) for the specified category.</summary>
		public void ClearColorKey(System.Drawing.Imaging.ColorAdjustType type) { _colorKeyLow = Color.Empty; _colorKeyHigh = Color.Empty; }
		/// <summary>Clears the color-adjustment matrix for the default category.</summary>
		public void ClearColorMatrix() { _colorMatrix = null; _grayMatrix = null; }
		/// <summary>Clears the color-adjustment matrix for the specified category.</summary>
		public void ClearColorMatrix(System.Drawing.Imaging.ColorAdjustType type) { _colorMatrix = null; _grayMatrix = null; }
		/// <summary>Disables gamma correction for the default category.</summary>
		public void ClearGamma() { _gamma = 1.0f; }
		/// <summary>Disables gamma correction for the specified category.</summary>
		public void ClearGamma(System.Drawing.Imaging.ColorAdjustType type) { _gamma = 1.0f; }
		/// <summary>Clears the NoOp setting for the default category.</summary>
		public void ClearNoOp() { _noOp = false; }
		/// <summary>Clears the NoOp setting for the specified category.</summary>
		public void ClearNoOp(System.Drawing.Imaging.ColorAdjustType type) { _noOp = false; }
		/// <summary>Clears the output channel setting for the default category.</summary>
		public void ClearOutputChannel() { _outputChannel = default; }
		/// <summary>Clears the output channel setting for the specified category.</summary>
		public void ClearOutputChannel(System.Drawing.Imaging.ColorAdjustType type) { _outputChannel = default; }
		/// <summary>Clears the output channel color profile setting for the default category.</summary>
		public void ClearOutputChannelColorProfile() { _outputChannelColorProfile = null; }
		/// <summary>Clears the output channel color profile setting for the specified category.</summary>
		public void ClearOutputChannelColorProfile(System.Drawing.Imaging.ColorAdjustType type) { _outputChannelColorProfile = null; }
		/// <summary>Clears the color-remap table for the default category.</summary>
		public void ClearRemapTable() { _remapTable = null; }
		/// <summary>Clears the color-remap table for the specified category.</summary>
		public void ClearRemapTable(System.Drawing.Imaging.ColorAdjustType type) { _remapTable = null; }
		/// <summary>Clears the threshold value for the default category.</summary>
		public void ClearThreshold() { _threshold = 0f; }
		/// <summary>Clears the threshold value for the specified category.</summary>
		public void ClearThreshold(System.Drawing.Imaging.ColorAdjustType type) { _threshold = 0f; }

		/// <summary>Creates an exact copy of this <see cref="ImageAttributes"/> object.</summary>
		public object Clone()
		{
			var clone = new ImageAttributes();
			clone._colorMatrix = _colorMatrix;
			clone._grayMatrix = _grayMatrix;
			clone._colorMatrixFlag = _colorMatrixFlag;
			clone._wrapMode = _wrapMode;
			clone._wrapColor = _wrapColor;
			clone._wrapClamp = _wrapClamp;
			clone._gamma = _gamma;
			clone._threshold = _threshold;
			clone._noOp = _noOp;
			clone._colorKeyLow = _colorKeyLow;
			clone._colorKeyHigh = _colorKeyHigh;
			clone._remapTable = _remapTable != null ? (ColorMap[])_remapTable.Clone() : null;
			clone._brushRemapTable = _brushRemapTable != null ? (ColorMap[])_brushRemapTable.Clone() : null;
			clone._outputChannel = _outputChannel;
			clone._outputChannelColorProfile = _outputChannelColorProfile;
			return clone;
		}

		/// <summary>Releases all resources used by this <see cref="ImageAttributes"/> object.</summary>
		public void Dispose() { _disposed = true; GC.SuppressFinalize(this); }

		/// <summary>Adjusts the colors in a palette according to the adjustment settings of a specified category.</summary>
		public void GetAdjustedPalette(System.Drawing.Imaging.ColorPalette palette, System.Drawing.Imaging.ColorAdjustType type) { /* No-op for basic implementation */ }

		/// <summary>Sets the brush color-remap table.</summary>
		public void SetBrushRemapTable(System.Drawing.Imaging.ColorMap[] map) { _brushRemapTable = map; }
		/// <summary>Sets the color key for the default category.</summary>
		public void SetColorKey(System.Drawing.Color colorLow, System.Drawing.Color colorHigh) { _colorKeyLow = colorLow; _colorKeyHigh = colorHigh; }
		/// <summary>Sets the color key for the specified category.</summary>
		public void SetColorKey(System.Drawing.Color colorLow, System.Drawing.Color colorHigh, System.Drawing.Imaging.ColorAdjustType type) { _colorKeyLow = colorLow; _colorKeyHigh = colorHigh; }
		/// <summary>Sets the color-adjustment matrix and the grayscale-adjustment matrix for the default category.</summary>
		public void SetColorMatrices(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrix? grayMatrix) { _colorMatrix = newColorMatrix; _grayMatrix = grayMatrix; }
		/// <summary>Sets the color-adjustment matrix and the grayscale-adjustment matrix for the default category.</summary>
		public void SetColorMatrices(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrix? grayMatrix, System.Drawing.Imaging.ColorMatrixFlag flags) { _colorMatrix = newColorMatrix; _grayMatrix = grayMatrix; _colorMatrixFlag = flags; }
		/// <summary>Sets the color-adjustment matrix and the grayscale-adjustment matrix for the specified category.</summary>
		public void SetColorMatrices(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrix? grayMatrix, System.Drawing.Imaging.ColorMatrixFlag mode, System.Drawing.Imaging.ColorAdjustType type) { _colorMatrix = newColorMatrix; _grayMatrix = grayMatrix; _colorMatrixFlag = mode; }
		/// <summary>Sets the color-adjustment matrix for the default category.</summary>
		public void SetColorMatrix(System.Drawing.Imaging.ColorMatrix newColorMatrix) { _colorMatrix = newColorMatrix; }
		/// <summary>Sets the color-adjustment matrix for the default category with the specified flags.</summary>
		public void SetColorMatrix(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrixFlag flags) { _colorMatrix = newColorMatrix; _colorMatrixFlag = flags; }
		/// <summary>Sets the color-adjustment matrix for the specified category.</summary>
		public void SetColorMatrix(System.Drawing.Imaging.ColorMatrix newColorMatrix, System.Drawing.Imaging.ColorMatrixFlag mode, System.Drawing.Imaging.ColorAdjustType type) { _colorMatrix = newColorMatrix; _colorMatrixFlag = mode; }
		/// <summary>Sets the gamma value for the default category.</summary>
		public void SetGamma(float gamma) { _gamma = gamma; }
		/// <summary>Sets the gamma value for the specified category.</summary>
		public void SetGamma(float gamma, System.Drawing.Imaging.ColorAdjustType type) { _gamma = gamma; }
		/// <summary>Turns off color adjustment for the default category.</summary>
		public void SetNoOp() { _noOp = true; }
		/// <summary>Turns off color adjustment for the specified category.</summary>
		public void SetNoOp(System.Drawing.Imaging.ColorAdjustType type) { _noOp = true; }
		/// <summary>Sets the output channel for the default category.</summary>
		public void SetOutputChannel(System.Drawing.Imaging.ColorChannelFlag flags) { _outputChannel = flags; }
		/// <summary>Sets the output channel for the specified category.</summary>
		public void SetOutputChannel(System.Drawing.Imaging.ColorChannelFlag flags, System.Drawing.Imaging.ColorAdjustType type) { _outputChannel = flags; }
		/// <summary>Sets the output channel color profile file for the default category.</summary>
		public void SetOutputChannelColorProfile(string colorProfileFilename) { _outputChannelColorProfile = colorProfileFilename; }
		/// <summary>Sets the output channel color profile file for the specified category.</summary>
		public void SetOutputChannelColorProfile(string colorProfileFilename, System.Drawing.Imaging.ColorAdjustType type) { _outputChannelColorProfile = colorProfileFilename; }
		/// <summary>Sets the color-remap table for the default category.</summary>
		public void SetRemapTable(System.Drawing.Imaging.ColorMap[] map) { _remapTable = map; }
		/// <summary>Sets the color-remap table for the specified category.</summary>
		public void SetRemapTable(System.Drawing.Imaging.ColorMap[] map, System.Drawing.Imaging.ColorAdjustType type) { _remapTable = map; }
		/// <summary>Sets the threshold (transparency range) for the default category.</summary>
		public void SetThreshold(float threshold) { _threshold = threshold; }
		/// <summary>Sets the threshold (transparency range) for the specified category.</summary>
		public void SetThreshold(float threshold, System.Drawing.Imaging.ColorAdjustType type) { _threshold = threshold; }
		/// <summary>Sets the wrap mode that is used to decide how to tile a texture across a shape, or at shape boundaries.</summary>
		public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode) { _wrapMode = mode; }
		/// <summary>Sets the wrap mode and color used to decide how to tile a texture.</summary>
		public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode, System.Drawing.Color color) { _wrapMode = mode; _wrapColor = color; }
		/// <summary>Sets the wrap mode and color used to decide how to tile a texture.</summary>
		public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode, System.Drawing.Color color, bool clamp) { _wrapMode = mode; _wrapColor = color; _wrapClamp = clamp; }

		/// <summary>Allows an object to try to free resources before being reclaimed by garbage collection.</summary>
		~ImageAttributes() { Dispose(); }
	}
}
