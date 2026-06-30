// Polyfill required for C# 9+ record types (and C# 10+ record structs) when targeting
// netstandard2.0, which does not define System.Runtime.CompilerServices.IsExternalInit.
// The compiler emits references to this marker type for init-only setters.
#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
