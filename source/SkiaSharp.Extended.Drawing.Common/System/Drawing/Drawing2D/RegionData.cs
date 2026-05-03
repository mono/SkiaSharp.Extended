namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Encapsulates the data that makes up a <see cref="System.Drawing.Region"/> object.
	/// </summary>
	public sealed partial class RegionData
	{
		internal RegionData() {}
		/// <summary>Gets or sets an array of bytes that specify the <see cref="System.Drawing.Region"/> object.</summary>
		public byte[] Data { get; set; } = Array.Empty<byte>();
	}
}
