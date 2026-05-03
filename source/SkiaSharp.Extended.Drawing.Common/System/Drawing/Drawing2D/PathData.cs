namespace System.Drawing.Drawing2D
{
	/// <summary>
	///  Contains the graphical data that makes up a <see cref="GraphicsPath"/> object.
	/// </summary>
	public sealed partial class PathData
	{
		/// <summary>Initializes a new instance of the <see cref="PathData"/> class.</summary>
		public PathData() { }
		/// <summary>Gets or sets an array of <see cref="PointF"/> structures that represents the points through which the path is constructed.</summary>
		public System.Drawing.PointF[]? Points { get; set; }
		/// <summary>Gets or sets the types of the corresponding points in the path.</summary>
		public byte[]? Types { get; set; }
	}
}
