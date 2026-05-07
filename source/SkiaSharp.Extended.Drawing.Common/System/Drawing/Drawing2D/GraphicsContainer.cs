namespace System.Drawing.Drawing2D;

/// <summary>
///  Represents the internal data of a graphics container. This class is used when saving the state of a <see cref="Graphics"/> object using the <see cref="Graphics.BeginContainer()"/> and <see cref="Graphics.EndContainer(GraphicsContainer)"/> methods.
/// </summary>
public sealed partial class GraphicsContainer : MarshalByRefObject
{
	internal int SaveCount;
	internal GraphicsContainer() {}
	internal GraphicsContainer(int saveCount) { SaveCount = saveCount; }
}
