namespace System.Drawing.Imaging;

/// <summary>
///  Encapsulates an array of <see cref="EncoderParameter"/> objects.
/// </summary>
public sealed partial class EncoderParameters : System.IDisposable
{
	/// <summary>Initializes a new instance with one <see cref="EncoderParameter"/> object.</summary>
	public EncoderParameters() { Param = new EncoderParameter[1]; }
	/// <summary>Initializes a new instance that can contain the specified number of <see cref="EncoderParameter"/> objects.</summary>
	/// <param name="count">The number of <see cref="EncoderParameter"/> objects that the array can hold.</param>
	public EncoderParameters(int count) { Param = new EncoderParameter[count]; }
	/// <summary>Gets or sets an array of <see cref="EncoderParameter"/> objects.</summary>
	public System.Drawing.Imaging.EncoderParameter[] Param { get; set; }
	/// <summary>Releases all resources used by this <see cref="EncoderParameters"/> object.</summary>
	public void Dispose() { }
}
