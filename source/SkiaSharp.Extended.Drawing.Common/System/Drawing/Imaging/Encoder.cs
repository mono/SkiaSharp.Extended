namespace System.Drawing.Imaging
{
	/// <summary>
	///  An <see cref="Encoder"/> object encapsulates a globally unique identifier (GUID) that identifies the category of an image encoder parameter.
	/// </summary>
	public sealed partial class Encoder
	{
		/// <summary>
		///  Initializes a new instance of the <see cref="Encoder"/> class from the specified GUID.
		/// </summary>
		/// <param name="guid">A globally unique identifier that identifies an image encoder parameter category.</param>
		public Encoder(System.Guid guid) { Guid = guid; }

		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the chrominance table parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder ChrominanceTable = new Encoder(new Guid("f2e455dc-09b3-4316-8260-676ada32481c"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the color depth parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder ColorDepth = new Encoder(new Guid("66087055-ad66-4c7c-9a18-38a2310b8337"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the color space parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder ColorSpace = new Encoder(new Guid("ae7a62a0-ee2c-49d8-9d07-1ba8a927596e"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the compression parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder Compression = new Encoder(new Guid("e09d739d-ccd4-44ee-8eba-3fbf8be4fc58"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the image items parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder ImageItems = new Encoder(new Guid("63875e13-1f1d-45ab-9195-a29b6066a650"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the luminance table parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder LuminanceTable = new Encoder(new Guid("edb33bce-0266-4a77-b904-27216099e717"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the quality parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder Quality = new Encoder(new Guid("1d5be4b5-fa4a-452d-9cdd-5db35105e7eb"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the render method parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder RenderMethod = new Encoder(new Guid("6d42c53a-229a-4825-8bb7-5c99e2b9a8b8"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the save as CMYK parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder SaveAsCmyk = new Encoder(new Guid("a219bbc9-0a9d-4f3f-bef2-2cbb5c5f9f1b"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the save flag parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder SaveFlag = new Encoder(new Guid("292266fc-ac40-47bf-8cfc-a85b89a655de"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the scan method parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder ScanMethod = new Encoder(new Guid("3a4e2661-3109-4e56-8536-42c156e7dcfa"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the transformation parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder Transformation = new Encoder(new Guid("8d0eb2d1-a58e-4ea8-aa14-108074b7b6f9"));
		/// <summary>An <see cref="Encoder"/> object that is initialized with the GUID for the version parameter category.</summary>
		public static readonly System.Drawing.Imaging.Encoder Version = new Encoder(new Guid("24d18c76-814a-41a4-bf53-1c219cccf797"));

		/// <summary>
		///  Gets a globally unique identifier (GUID) that identifies an image encoder parameter category.
		/// </summary>
		public System.Guid Guid { get; }
	}
}
