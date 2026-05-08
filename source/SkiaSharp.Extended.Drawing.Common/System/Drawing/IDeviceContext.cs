namespace System.Drawing;

public partial interface IDeviceContext : IDisposable
{
	nint GetHdc();
	void ReleaseHdc();
}
