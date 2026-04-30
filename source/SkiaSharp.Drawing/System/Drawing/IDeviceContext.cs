namespace System.Drawing
{
    public partial interface IDeviceContext : System.IDisposable
    {
        nint GetHdc();
        void ReleaseHdc();
    }
}
