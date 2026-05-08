namespace System.Drawing.Drawing2D;

public sealed partial class GraphicsState : MarshalByRefObject
{
	internal int SaveCount;
	internal GraphicsState() {}
	internal GraphicsState(int saveCount) { SaveCount = saveCount; }
}
