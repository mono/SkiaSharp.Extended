#if NETSTANDARD2_0
// Polyfill required for record types on netstandard2.0
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit { }
}
#endif
